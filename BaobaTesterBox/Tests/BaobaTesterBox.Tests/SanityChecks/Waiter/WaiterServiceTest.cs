// Fichier : Tests/BaobaTesterBox.Tests/SanityChecks/Waiter/AppiumWaiterServiceIsolatedTest.cs
using NUnit.Framework;
using BaobaTesterBox.Core.Waiter;
using BaobaTesterBox.Core.Loggings;
using BaobaTesterBox.Domain.Configuration;
using BaobaTesterBox.Tests.Infrastructure;

namespace BaobaTesterBox.Tests.SanityChecks.Waiter;

[TestFixture]
public class AppiumWaiterServiceIsolatedTest : BaseTest
{
    private const string Origine = "AppiumWaiterServiceIsolatedTest";

    [Test]
    public void Should_Detect_LoginScreen_After_AppLaunch()
    {
        AppiumLoggerService.LogInfo("=== Test isolé : détection de l'écran de connexion ===", Origine);

        var driver = AppiumService.Driver;
        var waiterOptions = AppConfigProvider.GetWaiterOptions();
        var waiter = new AppiumWaiterService(driver, waiterOptions);

        // TODO: remplacer par le vrai sélecteur du champ identifiant sur l'écran de login
        bool loginScreenReached = waiter.WaitForElement("#login-username");

        Assert.That(loginScreenReached, Is.True, "L'écran de connexion n'a jamais été détecté après le lancement de l'app.");

        AppiumLoggerService.LogInfo("=== Écran de connexion détecté avec succès ===", Origine);
    }
}