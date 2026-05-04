namespace Wddc.WebContentManager.Core.Configuration
{
    public class SecondaryApiConfiguration : ISecondaryApiConfiguration
    {
        public string BaseUrl { get; set; }
        public string ApiKey { get; set; }
    }
}
