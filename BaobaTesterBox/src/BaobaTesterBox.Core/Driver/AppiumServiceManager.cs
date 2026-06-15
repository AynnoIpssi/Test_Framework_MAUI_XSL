// Fichier : src/BaobaTesterBox.Core/Driver/AppiumDriverService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading; // Ajouté pour le Thread.Sleep du redémarrage
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Support.UI;
using BaobaTesterBox.Core.Loggings; // Ajouté pour logger le redémarrage
using BaobaTesterBox.Domain.Configuration;

namespace BaobaTesterBox.Core.Driver;

public class AppiumDriverService
{
    private const string OrigineLog = "AppiumDriverService";
    private readonly AppiumDriverManager _driverManager;

    public AppiumDriverService()
    {
        // 🟢 On ne fait plus de "new", on récupère l'instance partagée universelle
        _driverManager = AppiumDriverManager.Singleton;
    }

    public AndroidDriver Driver => _driverManager.Instance;

    public void InitializeService()
    {
        var globalOptions = AppConfigProvider.GetDriverOptions();

        var appiumOptions = new AppiumOptions
        {
            PlatformName = "Android",
            AutomationName = "UiAutomator2",
            DeviceName = globalOptions.DeviceName
        };

        if (!string.IsNullOrWhiteSpace(globalOptions.PlatformVersion))
        {
            appiumOptions.PlatformVersion = globalOptions.PlatformVersion;
        }

        appiumOptions.AddAdditionalAppiumOption("appium:appPackage", globalOptions.AppPackage);
        appiumOptions.AddAdditionalAppiumOption("appium:appActivity", globalOptions.AppActivity); 

        if (string.IsNullOrWhiteSpace(globalOptions.ApkPath))
        {
            appiumOptions.AddAdditionalAppiumOption("appium:noReset", true);
            appiumOptions.AddAdditionalAppiumOption("appium:dontStopAppOnReset", true);
            appiumOptions.AddAdditionalAppiumOption("appium:enforceAppInstall", false);
            appiumOptions.AddAdditionalAppiumOption("appium:autoGrantPermissions", false);
        }
        else
        {
            appiumOptions.App = globalOptions.ApkPath;
            appiumOptions.AddAdditionalAppiumOption("appium:autoGrantPermissions", globalOptions.AutoGrantPermissions);
            appiumOptions.AddAdditionalAppiumOption("appium:noReset", true); 
            appiumOptions.AddAdditionalAppiumOption("appium:dontStopAppOnReset", true);
        }

        appiumOptions.AddAdditionalAppiumOption("appium:ensureWebviewsHavePages", true);
        appiumOptions.AddAdditionalAppiumOption("appium:chromedriverArgs", new List<string> { "--whitelisted-ips=127.0.0.1" });

        var serverUri = new Uri(globalOptions.ServerUrl);

        _driverManager.Initialize(
            serverUri, 
            appiumOptions, 
            globalOptions.CommandTimeoutSeconds, 
            globalOptions.ImplicitWaitSeconds
        );
    }

    /// <summary>
    /// Force le redémarrage à chaud de l'application (Terminate puis Activate) via l'API mobile d'Appium.
    /// Permet de nettoyer l'état de l'application sans détruire la session lourde du driver.
    /// </summary>
    /// <param name="appPackage">Le package cible à redémarrer (ex: baoba.devUITestsUAT)</param>
    public void RelaunchApp(string appPackage)
    {
        AppiumLoggerService.LogInfo($"Action de redémarrage forcé demandée pour le package : '{appPackage}'...", OrigineLog);
        try
        {
            // 1. On force l'arrêt de l'application sur le périphérique
            Driver.ExecuteScript("mobile: terminateApp", new Dictionary<string, object> { { "appId", appPackage } });
            
            // 2. Pause de sécurité pour laisser l'OS Android tuer proprement le processus actif
            Thread.Sleep(1000); 
            
            // 3. On relance l'application à froid (retour écran d'accueil de l'app)
            Driver.ExecuteScript("mobile: activateApp", new Dictionary<string, object> { { "appId", appPackage } });
            
            AppiumLoggerService.LogInfo("L'application a été redémarrée avec succès.", OrigineLog);
        }
        catch (Exception ex) 
        { 
            AppiumLoggerService.LogWarn($"Échec partiel du redémarrage, poursuite standard du flux : {ex.Message}", OrigineLog); 
        }
    }

    public void BasculeVersWebView()
    {
        var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(30));
        wait.Until(_ => Driver.Contexts.Any(c => c.Contains("WEBVIEW")));
        Driver.Context = Driver.Contexts.First(c => c.Contains("WEBVIEW"));
    }

    public void TerminateService()
    {
        _driverManager.Dispose();
    }
}