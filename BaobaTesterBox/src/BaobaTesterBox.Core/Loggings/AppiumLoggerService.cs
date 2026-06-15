// Fichier : src/BaobaTesterBox.Core/Loggings/LoggerService.cs
using System;
using System.IO;
using BaobaTesterBox.Core.Config.Models;
using BaobaTesterBox.Domain.Configuration;

namespace BaobaTesterBox.Core.Loggings; // Corrigé ici avec Loggings !

public static class AppiumLoggerService
{
    private static readonly string _logDirectory;
    private static readonly string _filePath;
    private static readonly AppiumLoggerOptions _options;
    private static readonly object _lockObject = new();

    static AppiumLoggerService()
    {
        // Récupération via le provider
        _options = AppConfigProvider.GetLogOptions();

        _logDirectory = _options.LogDirectory;
        string fileName = $"log_{DateTime.Now:yyyy-MM-dd}.txt";
        _filePath = Path.Combine(_logDirectory, fileName);

        try
        {
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CRITICAL] Impossible de créer le dossier de log : {ex.Message}");
        }
    }

    public static void LogDebug(string message, string? origine = null) => WriteLog(AppiumLoggerOptions.Type.Debug, message, origine);
    public static void LogInfo(string message, string? origine = null) => WriteLog(AppiumLoggerOptions.Type.Info, message, origine);
    public static void LogWarn(string message, string? origine = null) => WriteLog(AppiumLoggerOptions.Type.Warn, message, origine);
    public static void LogError(string message, string? origine = null, Exception? exception = null) => WriteLog(AppiumLoggerOptions.Type.Error, message, origine, exception);

    private static void WriteLog(AppiumLoggerOptions.Type type, string message, string? origine, Exception? exception = null)
    {
        if (!ShouldLog(type)) return;

        string dateStr = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string prefixOrigine = string.IsNullOrEmpty(origine) ? "" : $" [{origine}]";
        string logLine = $"[{dateStr}] [{type.ToString().ToUpper()}]{prefixOrigine} : {message}";
        
        if (exception != null)
        {
            logLine += $"\n   --> Exception: {exception.Message}\n   --> StackTrace: {exception.StackTrace}";
        }

        lock (_lockObject)
        {
            try
            {
                File.AppendAllText(_filePath, logLine + Environment.NewLine);
                
                if (_options.LogToConsole)
                {
                    WriteToConsole(type, logLine);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CRITICAL] Échec d'écriture du log : {ex.Message}");
            }
        }
    }

    private static bool ShouldLog(AppiumLoggerOptions.Type type)
    {
        if (!Enum.TryParse<AppiumLoggerOptions.Type>(_options.MinimumLevel, true, out var minLevel))
        {
            minLevel = AppiumLoggerOptions.Type.Debug;
        }

        return type >= minLevel;
    }

    private static void WriteToConsole(AppiumLoggerOptions.Type type, string logLine)
    {
        var originalColor = Console.ForegroundColor;
        
        Console.ForegroundColor = type switch
        {
            AppiumLoggerOptions.Type.Debug => ConsoleColor.Gray,
            AppiumLoggerOptions.Type.Info => ConsoleColor.Cyan,
            AppiumLoggerOptions.Type.Warn => ConsoleColor.Yellow,
            AppiumLoggerOptions.Type.Error => ConsoleColor.Red,
            _ => originalColor
        };

        Console.WriteLine(logLine);
        Console.ForegroundColor = originalColor;
    }
}