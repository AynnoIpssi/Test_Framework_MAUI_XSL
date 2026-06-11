using System;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome; // Ou votre driver habituel (Firefox, etc.)
using OpenQA.Selenium.Support.UI;

namespace TestICA.Tests
{
    [TestFixture]
    public class CaptureUrlTests
    {
        private IWebDriver driver;

        [SetUp]
        public void Setup()
        {
            // Initialisation de votre driver (à adapter selon votre configuration existante)
            ChromeOptions options = new ChromeOptions();
            // options.AddArgument("--headless"); // Optionnel
            driver = new ChromeDriver(options);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
        }

        [Test]
        public void Test_Capturer_Vraie_Url_Moteur()
        {
            try
            {
                // 1. Ouvrir votre application ou la page de test
                // driver.Navigate().GoToUrl("URL_DE_VOTRE_APPLICATION");

                // =========================================================================
                // JOUEZ ICI VOS ÉTAPES (Clics, sélection de la ferme/exploitation, etc.)
                // Exemple :
                // var nomExploitation = "EARL DE LISQUILLY";
                // =========================================================================

                // 2. Attendre un court instant que l'application génère l'URL en arrière-plan
                System.Threading.Thread.Sleep(2000);

                // 3. OPTION 1 : Exécution du script JavaScript pour intercepter et RETOURNER la valeur à C#
                // Note : "window.go" est un exemple, remplacez par la variable ou fonction exacte de votre app
                string vraieUrlInterceptee = (string)((IJavaScriptExecutor)driver).ExecuteScript(
                    "return window.go || window.vraieUrl || document.querySelector('iframe')?.src || 'URL introuvable dans le contexte JS actuel';"
                );

                // 4. Affichage TRÈS VISIBLE dans la console de Rider (Onglet Output du test)
                TestContext.WriteLine(" ");
                TestContext.WriteLine("========================================================================");
                TestContext.WriteLine("🎯 [RIDER OUTPUT] - URL INTERCEPTÉE PAR L'ESPION JAVASCRIPT :");
                TestContext.WriteLine(vraieUrlInterceptee);
                TestContext.WriteLine("========================================================================");
                TestContext.WriteLine(" ");

                // 5. Sauvegarde de secours dans un fichier texte local si Rider fait des caprices
                try
                {
                    string dossierBackup = @"C:\Dev\test3";
                    if (System.IO.Directory.Exists(dossierBackup))
                    {
                        string cheminFichier = System.IO.Path.Combine(dossierBackup, "url_capture_secours.txt");
                        System.IO.File.WriteAllText(cheminFichier, vraieUrlInterceptee);
                        TestContext.WriteLine($"💾 Sauvegarde de secours réussie dans : {cheminFichier}");
                    }
                }
                catch (Exception exFichier)
                {
                    TestContext.WriteLine($"⚠️ Impossible d'écrire le fichier de secours : {exFichier.Message}");
                }

                // 6. Forcer l'échec pour marquer la fin de la phase de diagnostic et analyser l'output
                Assert.Fail("[Capture] ❌ Phase de capture terminée. Regarde le bloc ci-dessus pour copier la vraie structure d'URL.");
            }
            catch (AssertionException)
            {
                // On laisse passer l'Assert.Fail pour qu'il soit détecté par le framework de test
                throw;
            }
            catch (Exception ex)
            {
                // Capture les vrais plantages (éléments DOM introuvables, crash du driver...)
                TestContext.WriteLine($"💥 Erreur système durant le test : {ex.Message}");
                Assert.Fail($"[Capture] ❌ Échec critique de la capture : {ex.Message}");
            }
        }

        [TearDown]
        public void Teardown()
        {
            // Fermeture propre du navigateur après le test
            if (driver != null)
            {
                driver.Quit();
                driver.Dispose();
            }
        }
    }
}