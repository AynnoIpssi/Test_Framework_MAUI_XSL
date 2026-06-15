// Fichier : Tests/BaobaTesterBox.Tests/SanityChecks/AppiumElementScannerTest/ScanCurrentStageOnly.cs
using NUnit.Framework;
using BaobaTesterBox.Core.Locators;
using BaobaTesterBox.Core.Loggings;
using BaobaTesterBox.Core.Driver;
using BaobaTesterBox.Domain.Configuration;

namespace BaobaTesterBox.Tests.SanityChecks.Locators;

[TestFixture]
public class AppiumHotScannerStandaloneTest
{
    [Test]
    [Explicit("Test autonome : Lance un scan immédiat de l'écran en cours sans quitter ni relancer l'application.")]
    public void ScanCurrentScreenRightNow()
    {
        const string origineTest = "StandaloneHotScanner";

        AppiumLoggerService.LogInfo("===================================================", origineTest);
        AppiumLoggerService.LogInfo("--- DÉBUT DU SCAN À CHAUD (ZÉRO RESTART) ---", origineTest);
        AppiumLoggerService.LogInfo("===================================================", origineTest);

        // 1. Interception de la configuration globale
        var driverOptions = AppConfigProvider.GetDriverOptions();
        
        // Sauvegarde du chemin de l'APK pour les tests suivants
        string? backupApkPath = driverOptions.ApkPath; 
        
        AppiumDriverService? localService = null;

        try
        {
            // LA FEINTE : On vide l'APK. Le service va détecter ce vide, 
            // configurer les options anti-restart et les envoyer proprement au manager.
            driverOptions.ApkPath = string.Empty; 

            AppiumLoggerService.LogInfo("Connexion au service Appium en mode attachement direct...", origineTest);
            
            // 2. Initialisation du service (qui pilote désormais l'ensemble)
            localService = new AppiumDriverService();
            localService.InitializeService();

            var activeDriver = localService.Driver;
            
            // 3. Initialisation et exécution du scanner
            AppiumElementScanner.Init(activeDriver);
            
            AppiumLoggerService.LogInfo($"Connecté ! Contexte actuel : {activeDriver.Context}", origineTest);
            AppiumLoggerService.LogInfo("Scan du DOM en cours...", origineTest);

            AppiumElementScanner.AnalyzeCurrentPage();

            // 4. Affichage des résultats
            var detectedIds = AppiumElementScanner.GetLastDetectedIds();
            AppiumLoggerService.LogInfo($"--- FIN DU SCAN : {detectedIds.Count} ÉLÉMENTS TROUVÉS ---", origineTest);

            foreach (var id in detectedIds)
            {
                AppiumLoggerService.LogDebug($"  [+] ID : {id}", origineTest);
            }

            Assert.That(detectedIds, Is.Not.Null);
        }
        catch (System.Exception ex)
        {
            AppiumLoggerService.LogError($"Erreur durant le scan à chaud : {ex.Message}", origineTest);
            Assert.Fail($"Le scan à chaud a échoué : {ex.Message}");
        }
        finally
        {
            // 5. Restauration de la configuration d'origine pour ne pas polluer la suite de tests
            driverOptions.ApkPath = backupApkPath;

            if (localService != null)
            {
                AppiumLoggerService.LogInfo("Fermeture de la liaison de scan (L'application reste ouverte sur l'émulateur).", origineTest);
                localService.TerminateService();
            }
            
            AppiumLoggerService.LogInfo("===================================================", origineTest);
        }
    }
}