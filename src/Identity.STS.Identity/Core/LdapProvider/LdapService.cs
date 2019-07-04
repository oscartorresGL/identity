using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Identity.STS.Identity.Core.LdapProvider.Abstract;
using Identity.STS.Identity.Core.LdapProvider.Models;
using Novell.Directory.Ldap;

namespace Identity.STS.Identity.Core.LdapProvider
{
    public class LdapService : ILdapService
    {
        private readonly LdapConfiguration _configuration;
        private readonly string[] _attributes =
        {
            "objectSid", "objectGUID", "objectCategory", "objectClass", "memberOf", "name", "cn", "distinguishedName",
            "sAMAccountName", "sAMAccountName", "userPrincipalName", "displayName", "givenName", "sn", "description",
            "telephoneNumber", "mail", "streetAddress", "postalCode", "l", "st", "co", "c"
        };

        public LdapService(LdapConfiguration configuration)
        {
            _configuration = configuration;
        }

        public bool Authenticate(string distinguishedName, string password)
        {
            using (var ldapConnection = new LdapConnection() { SecureSocketLayer = true })
            {
                ldapConnection.Connect(_configuration.ServerName, _configuration.ServerPort);

                try
                {
                    ldapConnection.Bind(WrapDomainName(distinguishedName), password);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        private string WrapDomainName(string distinguishedName)
        {
            string result = distinguishedName;
            if (!result.Contains("\\"))
            {
                result = _configuration.DomainName + "\\" + distinguishedName;
            }

            return result;
        }

        public LdapUser GetUserByUserName(string userName)
        {
            LdapUser user = null;
            var filter = $"(&(objectClass=user)(sAMAccountName={userName}))";

            using (var ldapConnection = GetConnection())
            {
                var search = ldapConnection.Search(
                    _configuration.SearchBase,
                    LdapConnection.SCOPE_SUB,
                    filter,
                    _attributes,
                    false,
                    null,
                    null);

                LdapMessage message;

                while ((message = search.getResponse()) != null)
                {
                    if (!(message is LdapSearchResult searchResultMessage))
                    {
                        continue;
                    }
                    user = CreateUserFromAttributes(_configuration.SearchBase, searchResultMessage.Entry.getAttributeSet());
                }
            }
            return user;
        }

        private ILdapConnection GetConnection()
        {
            var ldapConnection = new LdapConnection() { SecureSocketLayer = _configuration.UseSSL };
            //Connect function will create a socket connection to the server - Port 389 for insecure and 3269 for secure    
            ldapConnection.Connect(_configuration.ServerName, _configuration.ServerPort);
            //Bind function with null user dn and password value will perform anonymous bind to LDAP server 
            ldapConnection.Bind(_configuration.Credentials.DomainUserName, _configuration.Credentials.Password);
            return ldapConnection;
        }

        private LdapUser CreateUserFromAttributes(string distinguishedName, LdapAttributeSet attributeSet)
        {
            var ldapUser = new LdapUser
            {
                //ObjectSid = attributeSet.getAttribute("objectSid")?.StringValue,
                //ObjectGuid = attributeSet.getAttribute("objectGUID")?.StringValue,
                //ObjectCategory = attributeSet.getAttribute("objectCategory")?.StringValue,
                //ObjectClass = attributeSet.getAttribute("objectClass")?.StringValue,
                //IsDomainAdmin = attributeSet.getAttribute("memberOf") != null && attributeSet.getAttribute("memberOf").StringValueArray.Contains("CN=Domain Admins," + this._ldapSettings.SearchBase),
                //MemberOf = attributeSet.getAttribute("memberOf")?.StringValueArray,
                //CommonName = attributeSet.getAttribute("cn")?.StringValue,
                FullName = attributeSet.getAttribute("name")?.StringValue,
                UserName = attributeSet.getAttribute("sAMAccountName")?.StringValue,
                //UserPrincipalName = attributeSet.getAttribute("userPrincipalName")?.StringValue,
                //Name = attributeSet.getAttribute("name")?.StringValue,
                //DistinguishedName = attributeSet.getAttribute("distinguishedName")?.StringValue ?? distinguishedName,
                //DisplayName = attributeSet.getAttribute("displayName")?.StringValue,
                FirstName = attributeSet.getAttribute("givenName")?.StringValue,
                LastName = attributeSet.getAttribute("sn")?.StringValue,
                //Description = attributeSet.getAttribute("description")?.StringValue,
                Phone = attributeSet.getAttribute("telephoneNumber")?.StringValue,
                Email = attributeSet.getAttribute("mail")?.StringValue,
                //Address = new LdapAddress
                //{
                //    Street = attributeSet.getAttribute("streetAddress")?.StringValue,
                //    City = attributeSet.getAttribute("l")?.StringValue,
                //    PostalCode = attributeSet.getAttribute("postalCode")?.StringValue,
                //    StateName = attributeSet.getAttribute("st")?.StringValue,
                //    CountryName = attributeSet.getAttribute("co")?.StringValue,
                //    CountryCode = attributeSet.getAttribute("c")?.StringValue
                //},

                //SamAccountType = int.Parse(attributeSet.getAttribute("sAMAccountType")?.StringValue ?? "0"),
            };
            //ldapUser.Id = ldapUser.SamAccountName;

            return ldapUser;
        }
    }
}
