// Fichier : Tests/BaobaTesterBox.Tests/Infrastructure/BaseTest.cs
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Android;
using BaobaTesterBox.Core.Driver;
using BaobaTesterBox.Core.Waiter;
using BaobaTesterBox.Core.Interactions;
using BaobaTesterBox.Domain.Configuration;

namespace BaobaTesterBox.Tests.Infrastructure;

public abstract class BaseTest
{
    // On déclare le service principal et tous les sous-services en propriétés protégées
    // pour qu'ils soient accessibles directement dans tous tes fichiers de tests
    protected AppiumDriverService AppiumService { get; private set; } = null!;
    protected AndroidDriver Driver => AppiumService.Driver;
    protected AppiumWaiterService Waiter { get; private set; } = null!;
    protected AppiumElementActionsService Actions { get; private set; } = null!;
    protected IJavaScriptExecutor Js => (IJavaScriptExecutor)Driver;

    [SetUp]
    public void BaseSetUp()
    {
        // 1. On centralise l'initialisation du driver unique (Singleton)
        AppiumService = new AppiumDriverService();
        AppiumService.InitializeService();

        // 2. On charge TOUS nos services d'un coup pour le test qui va s'exécuter
        Waiter = new AppiumWaiterService(Driver, AppConfigProvider.GetWaiterOptions());
        Actions = new AppiumElementActionsService(Driver);
    }

    [TearDown]
    public void BaseTearDown()
    {
        // Nettoyage automatique après chaque test
        AppiumService?.TerminateService();
    }
}