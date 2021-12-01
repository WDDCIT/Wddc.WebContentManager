namespace Wddc.WebContentManager.Core.Configuration
{
    public interface IWindowsConfiguration
    {
        string Domain { get; }
        string Username { get; }
        string Password { get; }
    }
}