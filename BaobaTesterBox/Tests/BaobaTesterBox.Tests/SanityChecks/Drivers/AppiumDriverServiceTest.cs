// Fichier : Tests/BaobaTesterBox.Tests/Infrastructure/AppiumDriverServiceTests.cs
using NUnit.Framework;
using BaobaTesterBox.Core.Driver;

namespace BaobaTesterBox.Tests.Infrastructure;

[TestFixture]
public class AppiumDriverServiceTests
{
    private AppiumDriverService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _service = new AppiumDriverService();
    }

    [Test]
    public void Should_Initialize_And_Run_Via_Driver_Service()
    {
        // 1. Démarrage du service (qui gère l'init du manager en interne)
        _service.InitializeService();

        // 2. Vérification que le driver exposé par le service est actif
        Assert.That(_service.Driver, Is.Not.Null, "Le service n'a pas réussi à instancier ou exposer le Driver.");
        
        //3. Bascule sur la WebView
        _service.BasculeVersWebView();
    }

    [TearDown]
    public void TearDown()
    {
        // Arrêt propre du service
        _service.TerminateService();
    }
}