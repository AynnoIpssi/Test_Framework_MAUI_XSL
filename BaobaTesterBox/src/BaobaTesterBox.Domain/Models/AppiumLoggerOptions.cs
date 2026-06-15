// Fichier : .../Models/LogModel.cs
namespace BaobaTesterBox.Core.Config.Models; // Adapte le namespace selon ton dossier Models

public sealed class AppiumLoggerOptions
{
    public string LogDirectory { get; set; } = "C:\\Temp\\BaobaLogs\\";
    public string MinimumLevel { get; set; } = "Debug";
    public bool LogToConsole { get; set; } = true; // L'enrichissement CLI

    public enum Type
    {
        Debug,
        Info,
        Warn,
        Error
    }
}