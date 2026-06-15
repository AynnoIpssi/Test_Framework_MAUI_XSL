using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using BaobaTesterBox.Core.Driver;
using BaobaTesterBox.Core.Waiter;
using BaobaTesterBox.Core.Config.Models;
using BaobaTesterBox.Domain.Models; // Assure la liaison avec AppiumNavigationOptions

namespace BaobaTesterBox.Domain.Configuration;

public static class AppConfigProvider
{
    private static readonly IConfiguration _configuration;

    static AppConfigProvider()
    {
        var baseDir = AppContext.BaseDirectory;
        var builder = new ConfigurationBuilder();

        var pathInFolder = Path.Combine(baseDir, "Tests", "BaobaTesterBox.Tests");
        
        if (File.Exists(Path.Combine(pathInFolder, "appsettings.json")))
        {
            builder.SetBasePath(pathInFolder);
        }
        else
        {
            builder.SetBasePath(baseDir);
        }

        builder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        _configuration = builder.Build();
    }

    public static AppiumDriverManagerOptions GetDriverOptions()
    {
        var section = _configuration.GetSection("AppiumConfig");

        if (!section.Exists())
        {
            throw new InvalidOperationException("[AppConfigProvider] La section 'AppiumConfig' est introuvable.");
        }

        return new AppiumDriverManagerOptions
        {
            ServerUrl = section["ServerUrl"] ?? string.Empty,
            ApkPath = section["ApkPath"] ?? string.Empty,
            DeviceName = section["DeviceName"] ?? string.Empty,
            PlatformVersion = section["PlatformVersion"] ?? string.Empty,
            AppPackage = section["AppPackage"] ?? string.Empty,
            AppActivity = section["AppActivity"] ?? string.Empty,
            ImplicitWaitSeconds = int.TryParse(section["ImplicitWaitSeconds"], out var iw) ? iw : 0,
            CommandTimeoutSeconds = int.TryParse(section["CommandTimeoutSeconds"], out var ct) ? ct : 0,
            AutoGrantPermissions = bool.TryParse(section["AutoGrantPermissions"], out var ag) && ag,
            Chromedriver_autodownload = bool.TryParse(section["chromedriver_autodownload"], out var ca) && ca
        };
    }

    public static AppiumLoggerOptions GetLogOptions()
    {
        var section = _configuration.GetSection("LoggerConfig");

        if (!section.Exists())
        {
            return new AppiumLoggerOptions
            {
                LogDirectory = "C:\\Temp\\BaobaLogs\\",
                MinimumLevel = "Debug",
                LogToConsole = true
            };
        }

        return new AppiumLoggerOptions
        {
            LogDirectory = section["LogDirectory"] ?? "C:\\Temp\\BaobaLogs\\",
            MinimumLevel = section["MinimumLevel"] ?? "Debug",
            LogToConsole = !bool.TryParse(section["LogToConsole"], out var ltc) || ltc 
        };
    }
    
    public static AppiumElementScannerOptions GetScannerOptions()
    {
        var section = _configuration.GetSection("ScannerConfig");

        if (!section.Exists())
        {
            return new AppiumElementScannerOptions { RootContainerTagName = "body" };
        }

        return new AppiumElementScannerOptions
        {
            RootContainerTagName = section["RootContainerTagName"] ?? "body"
        };
    }
    
    public static AppiumWaiterOptions GetWaiterOptions()
    {
        var section = _configuration.GetSection("AppiumWaiterConfig");

        if (!section.Exists())
        {
            return new AppiumWaiterOptions();
        }

        return new AppiumWaiterOptions
        {
            TimeoutSeconds = int.TryParse(section["TimeoutSeconds"], out var ts) ? ts : 30,
            PollingIntervalMs = int.TryParse(section["PollingIntervalMs"], out var pi) ? pi : 500
        };
    }

    /// <summary>
    /// 🟢 CORRECTION : Utilise directement ton modèle officiel du Domain sans créer de classe doublon
    /// </summary>
    public static AppiumNavigationOptions GetNavigationOptions()
    {
        var section = _configuration.GetSection("NavigationConfig");

        if (!section.Exists())
        {
            return new AppiumNavigationOptions();
        }

        return new AppiumNavigationOptions
        {
            ActionId = int.TryParse(section["DefaultActionId"], out var da) ? da : 1,
            Param1 = section["Param1"] ?? string.Empty,
            Param2 = section["Param2"] ?? string.Empty,
            Param3 = section["Param3"] ?? string.Empty,
            Param4 = section["Param4"] ?? string.Empty
        };
    }
}