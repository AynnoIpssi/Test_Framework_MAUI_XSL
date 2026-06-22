using System;
using System.Threading;
using NUnit.Framework;
using OpenQA.Selenium;
using BaobaTesterBox.Core.Loggings;
using BaobaTesterBox.Tests.Infrastructure;
using BaobaTesterBox.Core.Navigation;
using BaobaTesterBox.Core.Locators; // Accès à ton AppiumElementScanner

namespace BaobaTesterBox.Tests.SanityChecks.Login;

[TestFixture]
public class FicheIcaControlesTests : BaseTest
{
    private const string Origine = "FicheIcaControlesTests";

    [Test]
    public void Test_FicheIca_Bloquante_Si_Releves_Manquants_BruteScanner()
    {
        AppiumLoggerService.LogInfo("===================================================", Origine);
        AppiumLoggerService.LogInfo("--- DÉBUT : Contrôle Relevés Manquants (Scanner) ---", Origine);
        AppiumLoggerService.LogInfo("===================================================", Origine);

        int rubriqueCible = 542;
        int moduleIdReel = 5;
        int batchIdIncomplet = 409; 

        try
        {
            // 1. Déclenchement de la navigation (génère l'apparition de la popup)
            AppiumLoggerService.LogInfo($"[Test] Navigation ModuleGo vers rubrique {rubriqueCible} pour le lot {batchIdIncomplet}...", Origine);
            new AppiumNavigationService().ModuleGo(rubriqueCible, moduleIdReel, batchIdIncomplet);

            // 2. Initialisation du scanner avec le Driver hérité de BaseTest
            Assert.That(Driver, Is.Not.Null, "Le Driver Appium n'est pas disponible pour le scanner.");
            AppiumElementScanner.Init(Driver);
 
            string messagePopup = string.Empty;
            bool popupDetectee = false;

            AppiumLoggerService.LogInfo("[Test] Analyse itérative du DOM pour intercepter la popup...", Origine);

            // 3. Boucle d'attente active du chargement / injection de la popup (Max 10s par pas de 1s)
            for (int i = 1; i <= 10; i++)
            {
                // Analyse de la page active (comme dans ton test système)
                AppiumElementScanner.AnalyzeCurrentPage();

                // Récupération des données cartographiées
                var detectedIds = AppiumElementScanner.GetLastDetectedIds();

                // On parcourt les IDs indexés à la recherche d'un élément interactif de la popup
                foreach (var id in detectedIds)
                {
                    // L'ID du bouton dans la popin ou le mot 'modal' / 'popin' détecté par le scanner
                    if (id.Contains("popin", StringComparison.OrdinalIgnoreCase) || 
                        id.Contains("modal", StringComparison.OrdinalIgnoreCase) ||
                        id.Contains("btn-save", StringComparison.OrdinalIgnoreCase))
                    {
                        var element = AppiumElementScanner.GetElement(id);
                        
                        if (element != null && element.Displayed)
                        {
                            // STRATÉGIE DOM : Le scanner a trouvé l'input/button dans la popup.
                            // Pour lire le texte d'alerte global ("Il manque..."), on remonte au conteneur <div> de la modale.
                            var conteneurPopup = element.FindElement(By.XPath("./ancestor::div[contains(@class, 'modal') or contains(@id, 'popin')][1]"));
                            
                            messagePopup = conteneurPopup.Text;
                            popupDetectee = true;
                            AppiumLoggerService.LogInfo($"[Test] Élément de popup indexé avec succès ! Clé : {id}", Origine);
                            break;
                        }
                    }
                }

                if (popupDetectee && !string.IsNullOrWhiteSpace(messagePopup))
                    break;

                Thread.Sleep(1000);
            }

            // 4. ASSERTION : Validation de la présence de la popup
            Assert.That(popupDetectee, Is.True, "Le scanner a analysé l'écran mais n'a trouvé aucun élément interactif lié à la popup bloquante.");

            AppiumLoggerService.LogInfo($"[Test] Contenu complet de la popup extrait : '{messagePopup}'", Origine);

            // 5. ASSERTIONS MÉTIER (Conformité NUnit 4)
            Assert.That(messagePopup, Does.Contain("Il manque"), 
                $"Le message de blocage de la fiche ICA est incorrect. Reçu : '{messagePopup}'");
                
            Assert.That(messagePopup, Does.Contain("mortalité"), 
                $"L'alerte ne mentionne pas le manque de saisies de mortalité. Reçu : '{messagePopup}'");

            // 6. NETTOYAGE : Fermeture de la modale pour laisser l'application propre
            try
            {
                Driver.ExecuteScript("$('#popinLevel1').modal('hide');");
                AppiumLoggerService.LogInfo("[Test] Nettoyage : Fermeture de la modale validée.", Origine);
            }
            catch { /* Ignorer si déjà masquée */ }

            AppiumLoggerService.LogInfo("--- FIN DU TEST : TOUT EST VERT ---", Origine);
        }
        catch (System.Exception ex)
        {
            AppiumLoggerService.LogError($"Erreur durant l'exécution du scénario : {ex.Message}", Origine);
            Assert.Fail($"Le scénario brut avec scanner a échoué : {ex.Message}");
        }
    }
}