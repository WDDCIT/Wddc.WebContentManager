namespace Wddc.WebContentManager.Core.Configuration
{
    public interface ISecondaryApiConfiguration
    {
        string BaseUrl { get; }
        string ApiKey { get; }
    }
}
