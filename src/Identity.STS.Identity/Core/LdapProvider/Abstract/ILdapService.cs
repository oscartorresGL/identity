using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Identity.STS.Identity.Core.LdapProvider.Models;

namespace Identity.STS.Identity.Core.LdapProvider.Abstract
{
    public interface ILdapService
    {
        bool Authenticate(string distinguishedName, string password);
        LdapUser GetUserByUserName(string userName);
    }
}
