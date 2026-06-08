using TestICA.Core.Config;
using TestICA.Core.Config.Models;

namespace TestICA.Core.Logs;

public static class LoggerService
{
    private static readonly string _logDirectory;
    private static readonly string _filePath;
    
    static LoggerService()
    {
        _logDirectory = ConfigReader.Logger.LogDirectory;
        
        string fileName = $"log_{DateTime.Now:yyyy-MM-dd}.txt";
        _filePath = Path.Combine(_logDirectory, fileName);

        if (!Directory.Exists(_logDirectory))
        {
            Directory.CreateDirectory(_logDirectory);
        }
    }

    public static void LogDebug(string message, string? origine = null) => WriteLog(LogModel.Type.Debug, message, origine);
    public static void LogInfo(string message, string? origine = null) => WriteLog(LogModel.Type.Info, message, origine);
    public static void LogWarn(string message, string? origine = null) => WriteLog(LogModel.Type.Warn, message, origine);
    public static void LogError(string message, string? origine = null, Exception? exception = null) => WriteLog(LogModel.Type.Error, message, origine, exception);

    private static void WriteLog(LogModel.Type type, string message, string? origine, Exception? exception = null)
    {
        try
        {
            string minLevelConfig = ConfigReader.Logger.MinimumLevel ?? "Debug";
            
            if (minLevelConfig == "Info" && type == LogModel.Type.Debug) return;
            
            if (minLevelConfig == "Warn" && (type == LogModel.Type.Debug || type == LogModel.Type.Info)) return;
            
            if (minLevelConfig == "Error" && type != LogModel.Type.Error) return;
            
            string dateStr = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string prefixOrigine = string.IsNullOrEmpty(origine) ? "" : $" [{origine}]";
        
            string logLine = $"[{dateStr}] [{type.ToString().ToUpper()}]{prefixOrigine} : {message}";
            
            if (exception != null)
            {
                logLine += $"\n   --> Exception: {exception.Message}\n   --> StackTrace: {exception.StackTrace}";
            }
            
            File.AppendAllText(_filePath, logLine + Environment.NewLine);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CRITICAL] Impossible d'écrire le log sur le disque : {ex.Message}");
        }
    }
}