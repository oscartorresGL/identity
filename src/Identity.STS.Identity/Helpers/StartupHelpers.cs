﻿using System;
using System.Globalization;
using System.Reflection;
using Identity.Admin.EntityFramework.Shared.Entities.Identity;
using IdentityServer4.EntityFramework.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using Serilog;
using Identity.STS.Identity.Configuration;
using Identity.STS.Identity.Configuration.ApplicationParts;
using Identity.STS.Identity.Configuration.Constants;
using Identity.STS.Identity.Configuration.Intefaces;
using Identity.STS.Identity.Core;
using Identity.STS.Identity.Core.LdapProvider;
using Identity.STS.Identity.Core.LdapProvider.Abstract;
using Identity.STS.Identity.Core.LdapProvider.Models;
using Identity.STS.Identity.Helpers.Localization;
using Identity.STS.Identity.Helpers.Stores;
using Identity.STS.Identity.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Identity.STS.Identity.Helpers
{
    public static class StartupHelpers
    {
        /// <summary>
        /// Register services for MVC and localization including available languages
        /// </summary>
        /// <param name="services"></param>
        public static void AddMvcWithLocalization<TUser, TKey>(this IServiceCollection services)
            where TUser : IdentityUser<TKey>
            where TKey : IEquatable<TKey>
        {
            services.AddLocalization(opts => { opts.ResourcesPath = ConfigurationConsts.ResourcesPath; });

            services.TryAddTransient(typeof(IGenericControllerLocalizer<>), typeof(GenericControllerLocalizer<>));

            services.AddMvc(o =>
                {
                    o.Conventions.Add(new GenericControllerRouteConvention());
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddViewLocalization(
                    LanguageViewLocationExpanderFormat.Suffix,
                    opts => { opts.ResourcesPath = ConfigurationConsts.ResourcesPath; })
                .AddDataAnnotationsLocalization()
                .ConfigureApplicationPartManager(m =>
                {
                    m.FeatureProviders.Add(new GenericTypeControllerFeatureProvider<TUser, TKey>());
                });

            services.Configure<RequestLocalizationOptions>(
                opts =>
                {
                    var supportedCultures = new[]
                    {
                        new CultureInfo("en"),
                        new CultureInfo("fa"),
                        new CultureInfo("ru"),
                        new CultureInfo("sv"),
                        new CultureInfo("zh")
                    };

                    opts.DefaultRequestCulture = new RequestCulture("en");
                    opts.SupportedCultures = supportedCultures;
                    opts.SupportedUICultures = supportedCultures;
                });
        }

        /// <summary>
        /// Using of Forwarded Headers and Referrer Policy
        /// </summary>
        /// <param name="app"></param>
        public static void UseSecurityHeaders(this IApplicationBuilder app)
        {
            app.UseForwardedHeaders(new ForwardedHeadersOptions()
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseHsts(options => options.MaxAge(days: 365));
            app.UseReferrerPolicy(options => options.NoReferrer());
        }

        /// <summary>
        /// Add email senders - configuration of sendgrid, smtp senders
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddEmailSenders(this IServiceCollection services, IConfiguration configuration)
        {
            var smtpConfiguration = configuration.GetSection(nameof(SmtpConfiguration)).Get<SmtpConfiguration>();
            var sendGridConfiguration = configuration.GetSection(nameof(SendgridConfiguration)).Get<SendgridConfiguration>();

            if (sendGridConfiguration != null && !string.IsNullOrWhiteSpace(sendGridConfiguration.ApiKey))
            {
                services.AddSingleton<ISendGridClient>(_ => new SendGridClient(sendGridConfiguration.ApiKey));
                services.AddSingleton(sendGridConfiguration);
                services.AddTransient<IEmailSender, SendgridEmailSender>();
            }
            else if (smtpConfiguration != null && !string.IsNullOrWhiteSpace(smtpConfiguration.Host))
            {
                services.AddSingleton(smtpConfiguration);
                services.AddTransient<IEmailSender, SmtpEmailSender>();
            }
            else
            {
                services.AddSingleton<IEmailSender, EmailSender>();
            }
        }

        /// <summary>
        /// Add services for authentication, including Identity model, IdentityServer4 and external providers
        /// </summary>
        /// <typeparam name="TIdentityDbContext">DbContext for Identity</typeparam>
        /// <typeparam name="TUserIdentity">User Identity class</typeparam>
        /// <typeparam name="TUserIdentityRole">User Identity Role class</typeparam>
        /// <typeparam name="TConfigurationDbContext"></typeparam>
        /// <typeparam name="TPersistedGrantDbContext"></typeparam>
        /// <param name="services"></param>
        /// <param name="hostingEnvironment"></param>
        /// <param name="configuration"></param>
        /// <param name="logger"></param>
        public static void AddAuthenticationServices<TConfigurationDbContext, TPersistedGrantDbContext, TIdentityDbContext, TUserIdentity, TUserIdentityRole, TKey>(this IServiceCollection services, IHostingEnvironment hostingEnvironment, IConfiguration configuration, ILogger logger)
            where TPersistedGrantDbContext : DbContext, IPersistedGrantDbContext
            where TConfigurationDbContext : DbContext, IConfigurationDbContext
            where TIdentityDbContext : DbContext
            where TUserIdentity : IdentityUser<TKey>, new()
            where TUserIdentityRole : class
            where TKey : IEquatable<TKey>
        {
            var loginConfiguration = GetLoginConfiguration(configuration);
            var registrationConfiguration = GetRegistrationConfiguration(configuration);

            var ldapConfiguration = configuration.GetSection(nameof(LdapConfiguration)).Get<LdapConfiguration>();

            services.AddScoped<ILdapService, LdapService>();

            services
                .AddSingleton(registrationConfiguration)
                .AddSingleton(loginConfiguration)
                .AddSingleton(ldapConfiguration)
                .AddScoped<UserResolver<TUserIdentity>>()
                .AddIdentity<TUserIdentity, TUserIdentityRole>(options =>
                {
                    options.User.RequireUniqueEmail = false;
                })
                .AddEntityFrameworkStores<TIdentityDbContext>()
                //.AddUserManager<LdapUserManager<TUserIdentity>>()
                //.AddPasswordValidator<LdapResourceOwnerPasswordValidator<TUserIdentity, TKey>>()
                .AddDefaultTokenProviders();

            //services.AddScoped<IUserStore<UserIdentity>, LdapUserStore>();
            //services.AddScoped<IRoleStore<UserIdentityRole>, LdapRoleStore>();
            

            services.Configure<IISOptions>(iis =>
            {
                iis.AuthenticationDisplayName = "Windows";
                iis.AutomaticAuthentication = false;
            });

            var authenticationBuilder = services.AddAuthentication();

            AddExternalProviders(authenticationBuilder, configuration);

            AddIdentityServer<TConfigurationDbContext, TPersistedGrantDbContext, TUserIdentity>(services, configuration, logger, hostingEnvironment);
        }

        /// <summary>
        /// Get configuration for login
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        private static LoginConfiguration GetLoginConfiguration(IConfiguration configuration)
        {
            var loginConfiguration = configuration.GetSection(nameof(LoginConfiguration)).Get<LoginConfiguration>();

            // Cannot load configuration - use default configuration values
            if (loginConfiguration == null)
            {
                return new LoginConfiguration();
            }

            return loginConfiguration;
        }

        /// <summary>
        /// Get configuration for registration
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        private static RegisterConfiguration GetRegistrationConfiguration(IConfiguration configuration)
        {
            var registerConfiguration = configuration.GetSection(nameof(RegisterConfiguration)).Get<RegisterConfiguration>();

            // Cannot load configuration - use default configuration values
            if (registerConfiguration == null)
            {
                return new RegisterConfiguration();
            }

            return registerConfiguration;
        }

        /// <summary>
        /// Configuration root configuration
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection ConfigureRootConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();

            services.Configure<AdminConfiguration>(configuration.GetSection(ConfigurationConsts.AdminConfigurationKey));
            services.Configure<RegisterConfiguration>(configuration.GetSection(ConfigurationConsts.RegisterConfiguration));

            services.TryAddSingleton<IRootConfiguration, RootConfiguration>();

            return services;
        }

        /// <summary>
        /// Add configuration for IdentityServer4
        /// </summary>
        /// <typeparam name="TUserIdentity"></typeparam>
        /// <typeparam name="TConfigurationDbContext"></typeparam>
        /// <typeparam name="TPersistedGrantDbContext"></typeparam>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="logger"></param>
        /// <param name="hostingEnvironment"></param>
        private static void AddIdentityServer<TConfigurationDbContext, TPersistedGrantDbContext, TUserIdentity>(
            IServiceCollection services,
            IConfiguration configuration, ILogger logger, IHostingEnvironment hostingEnvironment)
            where TUserIdentity : class
            where TPersistedGrantDbContext : DbContext, IPersistedGrantDbContext
            where TConfigurationDbContext : DbContext, IConfigurationDbContext
        {
            var builder = services.AddIdentityServer(options =>
                {
                    options.Events.RaiseErrorEvents = true;
                    options.Events.RaiseInformationEvents = true;
                    options.Events.RaiseFailureEvents = true;
                    options.Events.RaiseSuccessEvents = true;
                })
                .AddAspNetIdentity<TUserIdentity>()
                .AddIdentityServerStoresWithDbContexts<TConfigurationDbContext, TPersistedGrantDbContext>(configuration, hostingEnvironment);

            builder.AddCustomSigningCredential(configuration, logger);
            builder.AddCustomValidationKey(configuration, logger);
        }

        /// <summary>
        /// Add external providers
        /// </summary>
        /// <param name="authenticationBuilder"></param>
        /// <param name="configuration"></param>
        private static void AddExternalProviders(AuthenticationBuilder authenticationBuilder,
            IConfiguration configuration)
        {
            var externalProviderConfiguration = configuration.GetSection(nameof(ExternalProvidersConfiguration)).Get<ExternalProvidersConfiguration>();

            if (externalProviderConfiguration.UseGitHubProvider)
            {
                authenticationBuilder.AddGitHub(options =>
                {
                    options.ClientId = externalProviderConfiguration.GitHubClientId;
                    options.ClientSecret = externalProviderConfiguration.GitHubClientSecret;
                    options.Scope.Add("user:email");
                });
            }
        }

        /// <summary>
        /// Add DbContext for Identity
        /// </summary>
        /// <typeparam name="TContext"></typeparam>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="hostingEnvironment"></param>
        public static void AddIdentityDbContext<TContext>(this IServiceCollection services,
            IConfiguration configuration, IHostingEnvironment hostingEnvironment)
            where TContext : DbContext
        {
            if (hostingEnvironment.IsStaging())
            {
                RegisterIdentityDbContextStaging<TContext>(services);
            }
            else
            {
                RegisterIdentityDbContext<TContext>(services, configuration);
            }
        }

        private static void RegisterIdentityDbContextStaging<TContext>(IServiceCollection services) where TContext : DbContext
        {
            var identityDatabaseName = Guid.NewGuid().ToString();

            services.AddDbContext<TContext>(optionsBuilder => optionsBuilder.UseInMemoryDatabase(identityDatabaseName));
        }

        private static void RegisterIdentityDbContext<TContext>(IServiceCollection services, IConfiguration configuration)
            where TContext : DbContext
        {
            var connectionString = configuration.GetConnectionString(ConfigurationConsts.IdentityDbConnectionStringKey);
            services.AddDbContext<TContext>(options => options.UseNpgsql(connectionString));
        }

        /// <summary>
        /// Add shared DbContext for Identity and IdentityServer4 stores
        /// </summary>
        /// <typeparam name="TContext"></typeparam>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddDbContexts<TContext>(this IServiceCollection services, IConfiguration configuration)
            where TContext : DbContext
        {
            var connectionString = configuration.GetConnectionString(ConfigurationConsts.AdminConnectionStringKey);
            services.AddDbContext<TContext>(options => options.UseNpgsql(connectionString));
        }

        /// <summary>
        /// Register DbContexts and configure stores for IdentityServer4
        /// </summary>
        /// <typeparam name="TConfigurationDbContext"></typeparam>
        /// <typeparam name="TPersistedGrantDbContext"></typeparam>
        /// <param name="builder"></param>
        /// <param name="configuration"></param>
        /// <param name="hostingEnvironment"></param>
        public static IIdentityServerBuilder AddIdentityServerStoresWithDbContexts<TConfigurationDbContext,
            TPersistedGrantDbContext>(this IIdentityServerBuilder builder, IConfiguration configuration,
            IHostingEnvironment hostingEnvironment)
            where TPersistedGrantDbContext : DbContext, IPersistedGrantDbContext
            where TConfigurationDbContext : DbContext, IConfigurationDbContext
        {
            if (hostingEnvironment.IsStaging())
            {
                return RegisterIdentityServerStoresWithDbContextsStaging<TConfigurationDbContext, TPersistedGrantDbContext>(builder, configuration);
            }
            else
            {
                return RegisterIdentityServerStoresWithDbContexts<TConfigurationDbContext, TPersistedGrantDbContext>(builder, configuration);
            }
        }

        private static IIdentityServerBuilder
            RegisterIdentityServerStoresWithDbContextsStaging<TConfigurationDbContext, TPersistedGrantDbContext>(
                IIdentityServerBuilder builder, IConfiguration configuration)
            where TPersistedGrantDbContext : DbContext, IPersistedGrantDbContext
            where TConfigurationDbContext : DbContext, IConfigurationDbContext
        {
            var configurationDatabaseName = Guid.NewGuid().ToString();
            var operationalDatabaseName = Guid.NewGuid().ToString();

            builder.AddConfigurationStore<TConfigurationDbContext>(options =>
            {
                options.ConfigureDbContext = b => b.UseInMemoryDatabase(configurationDatabaseName);
            });

            builder.AddOperationalStore<TPersistedGrantDbContext>(options =>
            {
                options.ConfigureDbContext = b => b.UseInMemoryDatabase(operationalDatabaseName);
            });

            return builder;
        }

        private static IIdentityServerBuilder
            RegisterIdentityServerStoresWithDbContexts<TConfigurationDbContext, TPersistedGrantDbContext>(
                IIdentityServerBuilder builder, IConfiguration configuration)
            where TPersistedGrantDbContext : DbContext, IPersistedGrantDbContext
            where TConfigurationDbContext : DbContext, IConfigurationDbContext
        {
            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            // Config DB from existing connection
            builder.AddConfigurationStore<TConfigurationDbContext>(options =>
            {
                options.ConfigureDbContext = b =>
                    b.UseNpgsql(
                        configuration.GetConnectionString(ConfigurationConsts.ConfigurationDbConnectionStringKey),
                        sql => sql.MigrationsAssembly(migrationsAssembly));
            });

            // Operational DB from existing connection
            builder.AddOperationalStore<TPersistedGrantDbContext>(options =>
            {
                options.EnableTokenCleanup = true;
                options.ConfigureDbContext = b =>
                    b.UseNpgsql(
                        configuration.GetConnectionString(ConfigurationConsts.PersistedGrantDbConnectionStringKey),
                        sql => sql.MigrationsAssembly(migrationsAssembly));
            });

            return builder;
        }

        /// <summary>
        /// Register middleware for localization
        /// </summary>
        /// <param name="app"></param>
        public static void UseMvcLocalizationServices(this IApplicationBuilder app)
        {
            var options = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
            app.UseRequestLocalization(options.Value);
        }

        /// <summary>
        /// Add configuration for logging
        /// </summary>
        /// <param name="app"></param>
        /// <param name="loggerFactory"></param>
        /// <param name="configuration"></param>
        public static void AddLogging(this IApplicationBuilder app, ILoggerFactory loggerFactory, IConfiguration configuration)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();
        }

        /// <summary>
        /// Add authorization policies
        /// </summary>
        /// <param name="services"></param>
        public static void AddAuthorizationPolicies(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy(AuthorizationConsts.AdministrationPolicy,
                    policy => policy.RequireRole(AuthorizationConsts.AdministrationRole));
            });
        }

        public static IServiceCollection AddCustomHealthCheck(this IServiceCollection services, IConfiguration configuration)
        {

            var hcBuilder = services.AddHealthChecks();
            var identityDbCS = configuration.GetConnectionString(ConfigurationConsts.IdentityDbConnectionStringKey);
            hcBuilder
                .AddCheck("self", () => HealthCheckResult.Healthy())
                .AddNpgSql(identityDbCS,
                    name: "IdentityDB-check",
                    tags: new[] { "IdentityDB" });
            return services;
        }
    }
}
