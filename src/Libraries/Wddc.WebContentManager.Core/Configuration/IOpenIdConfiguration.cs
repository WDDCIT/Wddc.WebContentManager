namespace Wddc.WebContentManager.Core.Configuration
{
    public interface IOpenIdConfiguration
    {
        string IdentityAdminRedirectUri { get; }
        string IdentityServerBaseUrl { get; }
        string IdentityAdminBaseUrl { get; }
        string ClientId { get; }
        string ClientSecret { get; }
        string OidcResponseType { get; }
        string[] Scopes { get; }
    }
}