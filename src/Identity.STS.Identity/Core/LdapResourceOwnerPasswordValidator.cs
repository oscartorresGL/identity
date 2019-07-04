using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Identity.STS.Identity.Core.LdapProvider.Abstract;
using IdentityServer4.AspNetIdentity;
using IdentityServer4.Events;
using IdentityServer4.Services;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Identity.STS.Identity.Core
{
    public class LdapResourceOwnerPasswordValidator<TUser, TKey> : ResourceOwnerPasswordValidator<TUser>
        where TUser : IdentityUser<TKey>, new()
        where TKey : IEquatable<TKey>
    {
        private readonly ILdapService _ldapService;
        private readonly UserManager<TUser> _userManager;
        private readonly IEventService _events;

        public LdapResourceOwnerPasswordValidator(
            UserManager<TUser> userManager,
            SignInManager<TUser> signInManager,
            IEventService events,
            ILogger<ResourceOwnerPasswordValidator<TUser>> logger,
            ILdapService ldapService) : base(userManager, signInManager, events, logger)
        {
            _ldapService = ldapService;
            _userManager = userManager;
            _events = events;
        }

        public override async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            var user = await _userManager.FindByNameAsync(context.UserName);
            if (user == default(TUser)
                && _ldapService.Authenticate(context.UserName, context.Password))
            {
                var ldapUser = _ldapService.GetUserByUserName(context.UserName);
                if (ldapUser != null)
                {
                    var newUser = new TUser()
                    {
                        UserName = ldapUser.UserName,
                        Email = ldapUser.Email,
                        EmailConfirmed = true,
                        PhoneNumber = ldapUser.Phone,
                        PhoneNumberConfirmed = true
                    };

                    var createResult = await _userManager.CreateAsync(newUser, context.Password);
                    if (!createResult.Succeeded)
                    {
                        await _events.RaiseAsync(new UserLoginFailureEvent(context.UserName,
                            string.Join(';', createResult.Errors.SelectMany(r => r.Description))));
                    }
                }
            }
            await base.ValidateAsync(context);
        }
    }
}
