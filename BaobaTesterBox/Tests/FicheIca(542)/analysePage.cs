using NUnit.Framework; // Ou Xunit selon ton framework de test
using System;
using System.Collections.Generic;
using BaobaTesterBox.Core.Locators;
using BaobaTesterBox.Core.Loggings;
using BaobaTesterBox.Core.Config;
using BaobaTesterBox.Tests.Infrastructure;

namespace BaobaTesterBox.Tests.Tools;

[TestFixture]
public class FicheIcaDiagnosticTests : BaseTest // Hérite de ta base pour récupérer le Driver initialisé
{
    private const string Origine = "OutilDiagnosticIca";

    [Test]
    [Category("Tools")]
    [Description("Outil détaché permettant de dumper l'état des variables de la Fiche ICA")]
    public void LancerRadiographieFicheIca()
    {
        AppiumLoggerService.LogInfo("🚀 [OUTIL DÉTACHÉ] Démarrage de l'inspection de l'écran...", Origine);

        // 🟢 LA CORRECTION EST ICI : On donne le Driver de la session active au scanner !
        // Remplace "Driver" par le nom de la variable de ton driver dans ta classe 'TestBase'
        if (Driver != null)
        {
            AppiumElementScanner.Init(Driver);
        }
        else
        {
            Assert.Fail("❌ Le Driver de la classe de base 'TestBase' est null. La session Appium n'est pas active.");
        }

        // 2. Maintenant le scanner a le driver, il peut faire la bascule WebView et dumper
        Dictionary<string, string> diagnosticPage = AppiumElementScanner.DumpFicheIcaCurrentState();

        // 3. Vérifications de base
        if (diagnosticPage == null || diagnosticPage.Count == 0)
        {
            Assert.Fail("❌ L'analyse a échoué. Le scanner n'a pu extraire aucune donnée (WebView introuvable ou DOM vide).");
        }

        string lotId = diagnosticPage.GetValueOrDefault("BatchId", "INCONNU");
        string estToutVenant = diagnosticPage.GetValueOrDefault("IsToutVenantVisible", "false");

        AppiumLoggerService.LogInfo("==================================================", Origine);
        AppiumLoggerService.LogInfo("📊 SYNTHÈSE DE L'OUTIL DE DIAGNOSTIC :", Origine);
        AppiumLoggerService.LogInfo($"-> Lot détecté à l'écran : {lotId}", Origine);
        AppiumLoggerService.LogInfo($"-> Mode Tout-venant actif : {estToutVenant}", Origine);
        AppiumLoggerService.LogInfo("==================================================", Origine);

        Assert.Pass("Radiographie terminée avec succès.");
    }
}