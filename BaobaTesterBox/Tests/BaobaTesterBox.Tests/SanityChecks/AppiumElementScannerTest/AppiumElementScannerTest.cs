// Fichier : Tests/BaobaTesterBox.Tests/SanityChecks/Locators/AppiumElementScannerTests.cs
using NUnit.Framework;
using BaobaTesterBox.Tests.Infrastructure;
using BaobaTesterBox.Core.Locators;
using BaobaTesterBox.Core.Loggings;

namespace BaobaTesterBox.Tests.SanityChecks.Locators;

[TestFixture]
public class AppiumElementScannerTests : BaseTest
{
    [Test]
    public void Should_Initialize_Scanner_And_Execute_Full_Page_Scan_Successfully()
    {
        const string origineTest = "TestSystemScanner";

        AppiumLoggerService.LogInfo("--- DÉBUT DU TEST SYSTÈME : SCANNER ---", origineTest);

        // 1. Test de l'initialisation du scanner avec le Driver hérité de BaseTest
        Assert.That(Driver, Is.Not.Null, "Le Driver Appium n'est pas disponible pour le scanner.");
        
        AppiumElementScanner.Init(Driver);
        AppiumLoggerService.LogInfo("Étape 1/3 : Initialisation du scanner validée.", origineTest);

        // 2. Test du scan de la page active (l'application doit être ouverte sur l'émulateur)
        // Cette méthode va chercher la configuration via AppConfigProvider et analyser le DOM
        AppiumElementScanner.AnalyzeCurrentPage();
        AppiumLoggerService.LogInfo("Étape 2/3 : Analyse de la page exécutée.", origineTest);

        // 3. Récupération et validation des données cartographiées
        var detectedIds = AppiumElementScanner.GetLastDetectedIds();
        
        AppiumLoggerService.LogInfo($"Étape 3/3 : Extraction des résultats terminée. Éléments détectés : {detectedIds.Count}", origineTest);

        // Affichage de la liste des IDs trouvés dans la console CLI pour contrôle visuel
        if (detectedIds.Count > 0)
        {
            AppiumLoggerService.LogInfo("--- LISTE DES ÉLÉMENTS INDEXÉS ---", origineTest);
            foreach (var id in detectedIds)
            {
                AppiumLoggerService.LogDebug($"  -> ID Cartographié : {id}", origineTest);
                
                // Vérification unitaire que l'élément extrait du cache n'est pas null
                var element = AppiumElementScanner.GetElement(id);
                Assert.That(element, Is.Not.Null, $"L'élément avec l'ID '{id}' est indexé mais introuvable dans le cache.");
            }
        }
        else
        {
            AppiumLoggerService.LogWarn("Aucun élément interactif (button, input, a) n'a été détecté sur cet écran. Vérifie que tu es bien sur une WebView active.", origineTest);
        }

        // Assert final pour valider que le système n'a pas planté et a renvoyé une liste valide
        Assert.That(detectedIds, Is.Not.Null, "Le scanner a renvoyé une liste d'IDs nulle.");
        
        AppiumLoggerService.LogInfo("--- FIN DU TEST SYSTÈME : TOUT EST VERT ---", origineTest);
    }
}