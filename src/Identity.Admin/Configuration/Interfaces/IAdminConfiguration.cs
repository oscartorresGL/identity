namespace Identity.Admin.Configuration.Interfaces
{
    public interface IAdminConfiguration
    {
        string IdentityAdminRedirectUri { get; }
        string IdentityServerBaseUrl { get; }
        string IdentityServerBaseLocalUrl { get; }
        string IdentityAdminBaseUrl { get; }
        string ClientId { get; }
        string ClientSecret { get; }
        string OidcResponseType { get; }
        string[] Scopes { get; }
        string IdentityAdminApiSwaggerUIClientId { get; }
        string IdentityAdminApiSwaggerUIRedirectUrl { get; }
        string IdentityAdminApiScope { get; }
    }
}