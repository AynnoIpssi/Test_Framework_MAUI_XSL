using NUnit.Framework;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using System;
using System.IO;
using TestICA.Core.Config; // AJOUT : Pour accéder à ConfigReader
using TestICA.Core.Logs;   // AJOUT : Pour accéder à LoggerService

namespace TestICA.Core
{
    /// <summary>
    /// Classe de base gérant le cycle de vie du driver Appium et les logs.
    /// </summary>
    public class AppiumSetup
    {
        protected AndroidDriver Driver;

        /// <summary>
        /// Configuration et démarrage d'une session Appium propre avant chaque test.
        /// </summary>
        [SetUp]
        public virtual void SetUp()
        {
            // Le nom du test en cours (pratique pour l'origine dans les logs)
            string testName = TestContext.CurrentContext.Test.Name;

            // 1. Utilisation de ton nouveau LoggerService static
            LoggerService.LogInfo("=== Démarrage du test ===", testName);
            LoggerService.LogInfo($"Test : {testName}", testName);
            LoggerService.LogInfo($"Date : {DateTime.Now}", testName);

            // 2. Configuration des Capabilities Appium via le "ConfigReader"
            var options = new AppiumOptions
            {
                PlatformName = "Android",
                DeviceName = ConfigReader.Appium.DeviceName, // Nettoyé : Fini les chaînes magiques Config["..."]
                PlatformVersion = ConfigReader.Appium.PlatformVersion,
                App = ConfigReader.Appium.ApkPath,
                AutomationName = "UiAutomator2"
            };
            
            // Sécurité pour cibler précisément ton vrai téléphone branché
            options.AddAdditionalAppiumOption("appium:udid", ConfigReader.Appium.DeviceName);

            // Autorisations automatiques (Photos, Caméra, etc.)
            options.AddAdditionalAppiumOption("appium:autoGrantPermissions", true);
            options.AddAdditionalAppiumOption("appium:hideKeyboard", true);
            
            // 3. Gestion du Package et de l'Activité réelle (.NET MAUI)
            string appPackage = ConfigReader.Appium.AppPackage; 
            string realActivity = "crc643d469d81e9bd1cea.MainActivity";

            options.AddAdditionalAppiumOption("appPackage", appPackage);
            options.AddAdditionalAppiumOption("appActivity", realActivity);
            options.AddAdditionalAppiumOption("appWaitActivity", realActivity);
            options.AddAdditionalAppiumOption("appium:ensureWebviewsHavePages", true);

            // Timeout chargement appli
            options.AddAdditionalAppiumOption("appium:appWaitDuration", 120000);

            // 4. Stratégie de Reset
            options.AddAdditionalAppiumOption("noReset", false);
            options.AddAdditionalAppiumOption("dontStopAppOnReset", false); 
            options.AddAdditionalAppiumOption("forceAppLaunch", true);

            // 5. Timeouts et instanciation du Driver via le ConfigReader
            //var commandTimeout = TimeSpan.FromSeconds(ConfigReader.Appium.CommandTimeoutSeconds);
            var serverUri = new Uri(ConfigReader.Appium.ServerUrl);

            // try
            // {
            //     Driver = new AndroidDriver(serverUri, options, commandTimeout);
            //     
            //     Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(
            //         ConfigReader.Appium.ImplicitWaitSeconds
            //     );
            //     
            //     LoggerService.LogInfo("Driver Appium démarré ✅", testName);
            // }
            // catch (Exception ex)
            // {
            //     LoggerService.LogError("Erreur critique lors de l'initialisation du Driver Appium", testName, ex);
            //     throw;
            // }
        }

        /// <summary>
        /// Nettoyage et fermeture de la session après chaque exécution.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            string testName = TestContext.CurrentContext.Test.Name;
            var outcome = TestContext.CurrentContext.Result.Outcome.Status;
            
            LoggerService.LogInfo($"=== Fin du test : {outcome} ===", testName);
            // PLUS BESOIN de TestLogger.WriteToFile() car le nouveau LoggerService écrit en temps réel !

            if (Driver != null)
            {
                Driver.Quit();
                Driver.Dispose();
            }
        }
    }
}