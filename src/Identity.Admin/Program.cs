using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using Identity.Admin.EntityFramework.Shared.DbContexts;
using Identity.Admin.EntityFramework.Shared.Entities.Identity;
using Identity.Admin.Helpers;

namespace Identity.Admin
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var seed = Environment.GetEnvironmentVariable("IDENTITY_ADMIN_SEED");
            var host = BuildWebHost(args);

            if (seed != null)
            {
                await DbMigrationHelpers.EnsureSeedData<IdentityServerConfigurationDbContext, AdminIdentityDbContext, IdentityServerPersistedGrantDbContext, AdminLogDbContext, UserIdentity, UserIdentityRole>(host);
            }

            host.Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                   .UseKestrel(c => c.AddServerHeader = false)
                   .UseStartup<Startup>()
                   .UseSerilog()
                   .Build();
    }
}