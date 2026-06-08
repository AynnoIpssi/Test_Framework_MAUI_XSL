using NUnit.Framework;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using System;
using System.Collections.Generic;
using TestICA.Core.Tools;
using TestICA.Core.SmartLocator;

namespace TestICA.Tests;

[TestFixture]
public class GenerationTests
{
    private AndroidDriver _driver;
    
    [Test]
    public void GenererMaPageConnexion()
    {
        // 1. IMPORTANT : On passe le driver au SmartLocator pour qu'il s'initialise
        SmartLocator.Init(_driver); // <-- Ajoute cette ligne ! (Ajuste le nom de la variable de ton driver)

        // 2. Maintenant tu peux scanner sans plantage
        SmartLocator.AnalyzeCurrentPage();

        List<string> idsScannes = SmartLocator.GetLastDetectedIds(); 
        string dossierDestination = @"C:\Dev\test3\FicheTest\TestICA\Pages";
        PageFactoryTools.CreatePageFile("Connexion", idsScannes, dossierDestination);

        Assert.Pass("Le fichier ConnexionPage.cs a été généré avec succès !");
    }
    
    [TearDown]
    public void TearDown()
    {
        // Fermeture propre de l'application après le scan
        if (_driver != null)
        {
            _driver.Quit();
            _driver.Dispose();
        }
    }
}