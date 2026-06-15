// Fichier : src/BaobaTesterBox.Core/Driver/AppiumServiceOptions.cs
namespace BaobaTesterBox.Core.Driver;

public sealed class AppiumServiceOptions
{
    public string ServerUrl { get; set; } = string.Empty;
    public string ApkPath { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string PlatformVersion { get; set; } = string.Empty;
    public string AppPackage { get; set; } = string.Empty;
    public string AppActivity { get; set; } = string.Empty;
    public int ImplicitWaitSeconds { get; set; }
    public int CommandTimeoutSeconds { get; set; }
    public bool AutoGrantPermissions { get; set; }
}