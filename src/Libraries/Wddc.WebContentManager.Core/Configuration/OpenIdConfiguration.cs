namespace Wddc.WebContentManager.Core.Configuration
{
    public class OpenIdConfiguration : IOpenIdConfiguration
    {
        public string IdentityAdminRedirectUri { get; set; }

        public string IdentityServerBaseUrl { get; set; }

        public string IdentityAdminBaseUrl { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string OidcResponseType { get; set; }

        public string[] Scopes { get; set; }
    }
}