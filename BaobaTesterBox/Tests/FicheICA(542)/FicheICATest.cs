// using NUnit.Framework;
// using BaobaTesterBox.Core;
// using BaobaTesterBox.Pages.FicheICA;
// using Newtonsoft.Json;
// using BaobaTesterBox.Data;
// using System;
// using System.IO;
// using System.Threading.Tasks;
// using BaobaTesterBox.Core.Config; // AJOUT : Pour accéder à ConfigReader
// using BaobaTesterBox.Core.Logs;   // AJOUT : Pour accéder à LoggerService
//
// namespace BaobaTesterBox.Tests.FicheICA
// {
//     /// <summary>
//     /// Tests end-to-end de la Fiche ICA (page 542).
//     /// Hérite de AppiumSetup qui gère le démarrage/arrêt du driver.
//     /// </summary>
//     [TestFixture]
//     public class FicheICATests : AppiumSetup
//     {
//         private FicheICAPage _page;
//
//         [SetUp]
//         public override void SetUp()
//         {
//             base.SetUp(); // Démarre Appium proprement
//             _page = new FicheICAPage(Driver);
//             
//             var nav = new NavigationHelper(Driver);
//             nav.NavigateToFicheIca();
//             LoginHelper.ConnexionAutomatique(Driver);
//         }
//
//         // ═══════════════════════════════════════
//         // MÉTHODE UTILITAIRE
//         // ═══════════════════════════════════════
//
//         private FicheICAScenario ChargerScenario(string fichier)
//         {
//             var chemin = Path.Combine(
//                 AppDomain.CurrentDomain.BaseDirectory,
//                 "Data", "Scenarios", "FicheICA", fichier
//             );
//             var json = File.ReadAllText(chemin);
//             return JsonConvert.DeserializeObject<FicheICAScenario>(json) 
//                 ?? throw new InvalidOperationException($"Impossible de désérialiser {fichier}");
//         }
//         
//         [Test]
//         public void TC00_Navigation_VersFicheICA()
//         {
//             var nav = new NavigationHelper(Driver);
//             nav.NavigateToFicheIca();
//             Assert.That(nav.TitleIsHere(), Is.True, "La page Fiche ICA n'a pas été atteinte");
//         }
//
//         // ═══════════════════════════════════════
//         // TC01 — LOT PLANNED, DONNÉES OK
//         // ═══════════════════════════════════════
//         
//         [Test]
//         [Description("Lot planifié avec toutes les données valides → sauvegarde OK")]
//         public async Task TC01_LotPlanned_ToutesDonneesOK_SauvegardeReussie()
//         {
//             string testName = TestContext.CurrentContext.Test.Name;
//             var scenario = ChargerScenario("TC01_PlannedLot.json");
//             LoggerService.LogInfo("Scénario chargé : TC01", testName);
//
//             _page.RemplirFormulaire(scenario);
//             LoggerService.LogInfo("Formulaire rempli", testName);
//             
//             _page.Sauvegarder();
//             LoggerService.LogInfo("Bouton sauvegarder cliqué", testName);
//
//             Assert.That(_page.SauvegardeReussie(), Is.True, "La page aurait dû revenir à la liste après sauvegarde");
//             LoggerService.LogInfo("Retour page liste : OK", testName);
//
//             // 💡 NOTE : Les vérifications de la base de données (_db) ont été retirées 
//             // car le fichier DatabaseHelper.cs a été supprimé du projet.
//         }
//
//         // ═══════════════════════════════════════
//         // TC04 — DATE DANS LE FUTUR
//         // ═══════════════════════════════════════
//
//         [Test]
//         [Description("Date de la fiche dans le futur → alerte + pas de sauvegarde")]
//         public void TC04_DateFutur_AlerteAffichee_PasDeSauvegarde()
//         {
//             string testName = TestContext.CurrentContext.Test.Name;
//             var scenario = ChargerScenario("TC04_FutureDate.json");
//             _page.RemplirFormulaire(scenario);
//             _page.Sauvegarder();
//
//             Assert.That(_page.AlerteEstAffichee(FicheICAPage.AlerteDateFuture), Is.True, "L'alerte 'date future' aurait dû s'afficher");
//             LoggerService.LogInfo("Alerte date future validée", testName);
//         }
//
//         // ═══════════════════════════════════════
//         // TC05 — ÉCART DATE > 5 JOURS
//         // ═══════════════════════════════════════
//
//         [Test]
//         [Description("Écart entre date fiche et date réception > 5 jours → alerte")]
//         public void TC05_EcartDateSuperieur5Jours_AlerteValidite()
//         {
//             var scenario = ChargerScenario("TC05_Over5Days.json");
//             _page.RemplirFormulaire(scenario);
//             _page.Sauvegarder();
//
//             Assert.That(_page.AlerteEstAffichee(FicheICAPage.AlerteValidite5j), Is.True);
//         }
//
//         // ═══════════════════════════════════════
//         // TC12 — PAS DE SIGNATURE
//         // ═══════════════════════════════════════
//
//         [Test]
//         [Description("Formulaire complet sans signature → alerte signature manquante")]
//         public void TC12_SansSignature_AlerteSignatureManquante()
//         {
//             var scenario = ChargerScenario("TC12_NoSignature.json");
//             _page.RemplirFormulaire(scenario);
//             _page.Sauvegarder();
//
//             Assert.That(_page.AlerteEstAffichee(FicheICAPage.AlerteSignature), Is.True, "L'alerte signature manquante aurait dû s'afficher");
//         }
//
//         // ═══════════════════════════════════════
//         // TC14 — MODE LECTURE
//         // ═══════════════════════════════════════
//
//         [Test]
//         [Description("Utilisateur sans droits d'écriture → mode lecture uniquement")]
//         public void TC14_ModeLecture_PasDeBoutonSauvegarde()
//         {
//             Assert.That(_page.EstEnModeLecture(), Is.True, "Le bouton de sauvegarde ne devrait pas être visible en mode lecture");
//         }
//
//         [TearDown]
//         public new void TearDown()
//         {
//             string testName = TestContext.CurrentContext.Test.Name;
//             if (TestContext.CurrentContext.Result.Outcome.Status == NUnit.Framework.Interfaces.TestStatus.Failed)
//             {
//                 // Utilisation du dossier de log configuré pour stocker la capture d'écran
//                 string outputPath = ConfigReader.Logger.LogDirectory ?? Path.GetTempPath();
//                 
//                 _page.TakeScreenshot(testName, outputPath);
//                 LoggerService.LogWarn("📸 Capture d'écran sauvegardée en cas d'échec", testName);
//             }
//
//             // NUnit exécutera automatiquement le TearDown d'AppiumSetup juste après.
//         }
//     }
// }