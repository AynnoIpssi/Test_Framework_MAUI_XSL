using NUnit.Framework;
using TestICA.Core;
using TestICA.Core.Logs;

namespace TestICA.Tests
{
    [TestFixture]
    [Description("Validation technique du démarrage de l'application via Appium")]
    public class AppLaunchTest : AppiumSetup
    {
        // 💡 REMARQUE : Aucun bloc [SetUp] ici ! 
        // Le starter AppiumSetup s'occupe de tout lancer automatiquement en tâche de fond.

        [Test]
        public void TC00_Verification_Demarrage_Application()
        {
            // 1. Récupération du nom du test pour le logger
            string testName = TestContext.CurrentContext.Test.Name;
            LoggerService.LogInfo("--- Début de la vérification du Driver ---", testName);

            // 2. Étape 1 : On vérifie que l'objet Driver existe bien en mémoire
            Assert.That(Driver, Is.Not.Null, "ÉCHEC : Le Driver Appium est 'null'. L'initialisation a raté.");
            LoggerService.LogInfo("Le Driver Appium est bien instancié en mémoire.", testName);

            // 3. Étape 2 : On demande au téléphone quelle est l'activité actuellement affichée à l'écran
            // Si l'application a démarré, le téléphone va répondre sans erreur.
            string activiteActuelle = Driver.CurrentActivity;
            
            LoggerService.LogInfo($"L'application est lancée avec succès ! Activité détectée : {activiteActuelle}", testName);

            // 4. Étape 3 : Validation finale (Le test réussit si on récupère une activité)
            Assert.That(activiteActuelle, Is.Not.Null.Or.Empty, "L'application ne semble pas avoir d'activité active.");
            
            LoggerService.LogInfo("--- Fin du test de démarrage : TOUT EST OK ✅ ---", testName);
        }
    }
}