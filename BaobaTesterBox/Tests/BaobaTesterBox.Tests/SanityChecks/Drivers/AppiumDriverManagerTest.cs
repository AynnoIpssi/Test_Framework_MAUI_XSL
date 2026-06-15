// Fichier : Tests/BaobaTesterBox.Tests/SanityChecks/Driver/AppiumDriverManagerTests.cs
using System;
using NUnit.Framework;
using OpenQA.Selenium.Appium;
using BaobaTesterBox.Core.Driver;
using BaobaTesterBox.Domain.Configuration;

namespace BaobaTesterBox.Tests.SanityChecks.Driver;

[TestFixture]
public class AppiumDriverManagerTests
{
    private AppiumDriverManager _manager;

    [SetUp]
    public void SetUp()
    {
        // 🟢 On cible directement l'instance Singleton unique de l'application
        _manager = AppiumDriverManager.Singleton;
    }

    [TearDown]
    public void TearDown()
    {
        // On libère le driver physique s'il a été initialisé pendant le test
        _manager?.Dispose();
    }

    [Test]
    public void Should_Initialize_AndroidDriver_With_Provided_AppiumOptions()
    {
        // Arrange
        var globalOptions = AppConfigProvider.GetDriverOptions();
        var serverUri = new Uri(globalOptions.ServerUrl);

        var appiumOptions = new AppiumOptions
        {
            PlatformName = "Android",
            AutomationName = "UiAutomator2",
            DeviceName = globalOptions.DeviceName
        };

        if (!string.IsNullOrWhiteSpace(globalOptions.ApkPath))
        {
            appiumOptions.App = globalOptions.ApkPath;
        }

        appiumOptions.AddAdditionalAppiumOption("appium:appPackage", globalOptions.AppPackage);
        appiumOptions.AddAdditionalAppiumOption("appium:ensureWebviewsHavePages", true);

        // Act
        var driver = _manager.Initialize(
            serverUri, 
            appiumOptions, 
            globalOptions.CommandTimeoutSeconds, 
            globalOptions.ImplicitWaitSeconds
        );

        // Assert
        Assert.That(driver, Is.Not.Null, "Le driver n'a pas été instancié par le manager.");
        Assert.That(_manager.Instance, Is.Not.Null, "La propriété Instance du manager est vide.");
        Assert.That(driver, Is.SameAs(_manager.Instance), "L'instance retournée n'est pas celle stockée dans le manager.");
    }

    [Test]
    public void Should_Throw_Exception_When_Accessing_Instance_Before_Initialization()
    {
        // Act & Assert
        // Si un test précédent a initialisé le driver, on force un Dispose avant pour tester l'état à blanc
        _manager.Dispose(); 

        var ex = Assert.Throws<InvalidOperationException>(() => { var _ = _manager.Instance; });
        Assert.That(ex.Message, Does.Contain("Driver non initialisé"));
    }

    [Test]
    public void Should_Throw_Exception_When_Accessing_Instance_After_Dispose()
    {
        // Arrange
        var globalOptions = AppConfigProvider.GetDriverOptions();
        var serverUri = new Uri(globalOptions.ServerUrl);
        var appiumOptions = new AppiumOptions { PlatformName = "Android", AutomationName = "UiAutomator2" };

        _manager.Initialize(serverUri, appiumOptions, 10, 2);

        // Act
        _manager.Dispose();

        // Assert
        Assert.Throws<ObjectDisposedException>(() => { var _ = _manager.Instance; });
    }
}