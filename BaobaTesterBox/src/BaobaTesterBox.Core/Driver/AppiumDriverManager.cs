// Fichier : src/BaobaTesterBox.Core/Driver/AppiumDriverManager.cs
using System;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;

namespace BaobaTesterBox.Core.Driver;

public sealed class AppiumDriverManager : IDisposable
{
    private static AppiumDriverManager? _instance;
    private static readonly object _lock = new();

    public static AppiumDriverManager Singleton
    {
        get
        {
            lock (_lock)
            {
                if (_instance == null || _instance._isDisposed)
                {
                    _instance = new AppiumDriverManager();
                }
                return _instance;
            }
        }
    }

    private AndroidDriver? _driver;
    private bool _isDisposed;

    private AppiumDriverManager() { }

    public AndroidDriver Instance
    {
        get
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(AppiumDriverManager), "[AppiumDriverManager] Le manager a été libéré (Disposed).");
            return _driver ?? throw new InvalidOperationException("[AppiumDriverManager] Driver non initialisé.");
        }
    }

    public AndroidDriver Initialize(Uri serverUri, AppiumOptions appiumOptions, int timeoutSeconds, int implicitWaitSeconds)
    {
        if (_isDisposed) throw new ObjectDisposedException(nameof(AppiumDriverManager), "Impossible d'initialiser un manager disposé.");

        if (_driver != null) return _driver;

        var commandTimeout = TimeSpan.FromSeconds(timeoutSeconds > 0 ? timeoutSeconds : 60);
        _driver = new AndroidDriver(serverUri, appiumOptions, commandTimeout);
        _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(implicitWaitSeconds > 0 ? implicitWaitSeconds : 10);

        return _driver;
    }

    public void SetActiveDriver(AndroidDriver driver)
    {
        if (_isDisposed) throw new ObjectDisposedException(nameof(AppiumDriverManager), "Manager disposé.");
        _driver = driver ?? throw new ArgumentNullException(nameof(driver));
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        
        try
        {
            _driver?.Quit();
        }
        catch { /* Ignorer les erreurs de fermeture */ }
        
        _driver = null;
        _isDisposed = true;
    }
}