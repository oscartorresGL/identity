using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Identity.Admin.EntityFramework.Shared.Entities.Identity;
using Identity.STS.Identity.Core.LdapProvider.Abstract;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Identity.STS.Identity.Core.LdapProvider
{
    public class LdapUserManager<TUserIdentity> : UserManager<TUserIdentity> where TUserIdentity: class
    {
        private readonly ILdapService _ldapService;

        public LdapUserManager(
            IUserStore<TUserIdentity> store,
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<TUserIdentity> passwordHasher,
            IEnumerable<IUserValidator<TUserIdentity>> userValidators,
            IEnumerable<IPasswordValidator<TUserIdentity>> passwordValidators,
            ILookupNormalizer keyNormalizer, 
            IdentityErrorDescriber errors, 
            IServiceProvider services, 
            ILogger<UserManager<TUserIdentity>> logger) 
            : base(
                store, 
                optionsAccessor, 
                passwordHasher, 
                userValidators, 
                passwordValidators, 
                keyNormalizer, 
                errors, 
                services, 
                logger)
        {
        }

        public override Task<bool> CheckPasswordAsync(TUserIdentity user, string password)
        {

            return base.CheckPasswordAsync(user, password);
        }
    }
}
