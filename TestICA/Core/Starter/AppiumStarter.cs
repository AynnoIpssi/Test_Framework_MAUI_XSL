using NUnit.Framework;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Support.UI;
using TestICA.Core.Config;
using System;
using System.Linq;

namespace TestICA.Core.Starter;

public class AppiumStarter
{
    public static AndroidDriver? Driver { get; set; }

    public void InitDriver()
    {
        var options = new AppiumOptions();
        options.PlatformName = "Android";
        options.AutomationName = "UiAutomator2";
        options.DeviceName = ConfigReader.Appium.DeviceName;
        options.PlatformVersion = ConfigReader.Appium.PlatformVersion;
        options.App = ConfigReader.Appium.ApkPath;
        options.AddAdditionalAppiumOption("appPackage", ConfigReader.Appium.AppPackage);
        options.AddAdditionalAppiumOption("appActivity", "crc643d469d81e9bd1cea.MainActivity");
        options.AddAdditionalAppiumOption("noReset", true);
        options.AddAdditionalAppiumOption("dontStopAppOnReset", true);
        options.AddAdditionalAppiumOption("appium:chromedriverArgs", new List<string> { "--whitelisted-ips=127.0.0.1" });

        var commandTimeout = TimeSpan.FromSeconds(180);
        string serverUrl = ConfigReader.Appium.ServerUrl ?? "http://127.0.0.1:4723";
        if (!serverUrl.EndsWith("/")) serverUrl += "/";

        Driver = new AndroidDriver(new Uri(serverUrl), options, commandTimeout);
        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(120);
    }

    public void BasculeVersWebView()
    {
        if (Driver == null)
            throw new InvalidOperationException("[AppiumStarter] Driver non initialisé avant BasculeVersWebView().");

        // Attend qu'un contexte WEBVIEW apparaisse (max 30 secondes)
        var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(30));
        wait.Until(_ =>
        {
            var contexts = Driver.Contexts;
            return contexts.Any(c => c.Contains("WEBVIEW"));
        });

        // Bascule vers le premier WEBVIEW trouvé
        var webviewContext = Driver.Contexts.First(c => c.Contains("WEBVIEW"));
        Driver.Context = webviewContext;

        Serilog.Log.Information($"[AppiumStarter] Contexte basculé vers : {webviewContext}");
    }

    [TearDown]
    public void QuitDriver()
    {
        if (Driver != null)
        {
            Driver.Quit();
            Driver.Dispose();
            Driver = null;
        }
    }
}