using System;
using NUnit.Framework;
using OpenQA.Selenium;
using BaobaTesterBox.Core.Loggings;
using BaobaTesterBox.Tests.Infrastructure;

namespace BaobaTesterBox.Tests.SanityChecks.Login;

[TestFixture]
public class NavigationDirecteTests : BaseTest
{
    private const string Origine = "NavigationDirecteTests";

    [Test]
    public void Test_Navigation_Ica_Valeurs_Exactes()
    {
        AppiumLoggerService.LogInfo("=== DÉBUT : Navigation Fiche ICA avec valeurs réelles ===", Origine);

        // 1. On s'assure d'être sur la WebView
        AppiumService.BasculeVersWebView();
        var jsExecutor = (IJavaScriptExecutor)Driver;

        // 2. Préparation des paramètres validés par l'espion
        int rubriqueCible = 542;
        int moduleIdReel = 5;
        int batchIdReel = 409;

        AppiumLoggerService.LogInfo($"Envoi de openPoultryModal avec Module: {moduleIdReel}, Rubrique: {rubriqueCible}, Batch: {batchIdReel}...", Origine);
    
        try
        {
            // 3. Appel direct avec les arguments numériques typés
            string scriptRepare = "openPoultryModal(arguments[0], arguments[1], null, arguments[2]);";
            
            jsExecutor.ExecuteScript(scriptRepare, moduleIdReel, rubriqueCible, batchIdReel);
            AppiumLoggerService.LogInfo("validation : Commande de navigation transmise !", Origine);
        }
        catch (Exception ex)
        {
            Assert.Fail($"[Erreur Navigation] L'appel a échoué avec les valeurs réelles : {ex.Message}");
        }

        // 4. Pause pour voir l'écran changer sur le téléphone
        AppiumLoggerService.LogInfo("Attente de 3 secondes pour observer la transition...", Origine);
        System.Threading.Thread.Sleep(3000);

        AppiumLoggerService.LogInfo("=== FIN DU TEST ===", Origine);
    }
}