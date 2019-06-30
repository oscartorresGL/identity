using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Identity.Admin.EntityFramework.Shared.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace Identity.STS.Identity.Helpers.Stores
{
    public class LdapUserStore : 
            IUserStore<UserIdentity>,
            IUserPasswordStore<UserIdentity>,
            IUserRoleStore<UserIdentity>
    {
        public void Dispose()
        {
        }

        public Task<string> GetUserIdAsync(UserIdentity user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Id);
        }

        public Task<string> GetUserNameAsync(UserIdentity user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.UserName);
        }

        public Task SetUserNameAsync(UserIdentity user, string userName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetNormalizedUserNameAsync(UserIdentity user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetNormalizedUserNameAsync(UserIdentity user, string normalizedName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> CreateAsync(UserIdentity user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> UpdateAsync(UserIdentity user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> DeleteAsync(UserIdentity user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<UserIdentity> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            var result = new UserIdentity()
            {
                Id = userId,
                UserName = userId
            };
            return Task.FromResult(result);
        }

        public Task<UserIdentity> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            var result = new UserIdentity()
            {
                Id = normalizedUserName,
                UserName = normalizedUserName
            };
            return Task.FromResult(result);
        }

        public Task SetPasswordHashAsync(UserIdentity user, string passwordHash, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetPasswordHashAsync(UserIdentity user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<bool> HasPasswordAsync(UserIdentity user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task AddToRoleAsync(UserIdentity user, string roleName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task RemoveFromRoleAsync(UserIdentity user, string roleName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IList<string>> GetRolesAsync(UserIdentity user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsInRoleAsync(UserIdentity user, string roleName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IList<UserIdentity>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
