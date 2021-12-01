namespace Wddc.WebContentManager.Core.Configuration
{
    public interface IDynamicsGPConfiguration
    {
        string Url { get; }
        IServiceConfiguration ServiceConfiguration { get; }
        IWindowsConfiguration WindowsConfiguration { get; }
    }
}