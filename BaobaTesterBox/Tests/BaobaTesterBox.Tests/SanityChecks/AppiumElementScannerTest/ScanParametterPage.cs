using System;
using System.Collections.Generic;
using NUnit.Framework;
using OpenQA.Selenium;
using BaobaTesterBox.Core.Loggings;
using BaobaTesterBox.Tests.Infrastructure;

namespace BaobaTesterBox.Tests.SanityChecks.Login;

[TestFixture]
public class NavigationDirecteTestsS : BaseTest
{
    private const string Origine = "NavigationDirecteTests";

    public class AppContextModel
    {
        public string FarmId { get; set; } = "N/A";
        public string BatchId { get; set; } = "N/A";
        public string OldRubriqueId { get; set; } = "N/A";
        public string ModuleId { get; set; } = "N/A";
        public string CurrentRubriqueId { get; set; } = "N/A";
    }

    [Test]
    public void Test_Analyse_Variables_Seules()
    {
        AppiumLoggerService.LogInfo("=== DÉBUT : Scan unique des variables de la page ===", Origine);

        // 1. Connexion au canal WebView
        AppiumService.BasculeVersWebView();
        var jsExecutor = (IJavaScriptExecutor)Driver;

        // 2. Script JS d'extraction brute dans window
        string scriptExtract = @"
            return {
                farmId: (window.farmId || window.globalFarmId || 'N/A').toString(),
                batchId: (window.globalPoultryBatchId || window.batchId || 'N/A').toString(),
                oldRubriqueId: (window.oldrubriqueid || window.oldRubriqueId || 'N/A').toString(),
                moduleId: (window.globalPoultryModuleid || window.moduleId || 'N/A').toString(),
                currentRubrique: (window.rubriqueid || window.currentRubriqueId || 'N/A').toString()
            };
        ";

        var result = (Dictionary<string, object>)jsExecutor.ExecuteScript(scriptExtract);
        
        var contexte = new AppContextModel
        {
            FarmId = result["farmId"].ToString(),
            BatchId = result["batchId"].ToString(),
            OldRubriqueId = result["oldRubriqueId"].ToString(),
            ModuleId = result["moduleId"].ToString(),
            CurrentRubriqueId = result["currentRubrique"].ToString()
        };

        // 3. Affichage ultra-lisible avec les séparations nettes réclamées
        AppiumLoggerService.LogInfo("==================================================", "ESPION CONTEXTE");
        AppiumLoggerService.LogInfo($" 🏠 FARM ID             : {contexte.FarmId}", "ESPION CONTEXTE");
        AppiumLoggerService.LogInfo($" 📦 BATCH ID            : {contexte.BatchId}", "ESPION CONTEXTE");
        AppiumLoggerService.LogInfo($" 📄 ANCIENNE RUBRIQUE   : {contexte.OldRubriqueId}", "ESPION CONTEXTE");
        AppiumLoggerService.LogInfo($" 🧩 MODULE ID           : {contexte.ModuleId}", "ESPION CONTEXTE");
        AppiumLoggerService.LogInfo($" 📍 RUBRIQUE ACTUELLE   : {contexte.CurrentRubriqueId}", "ESPION CONTEXTE");
        AppiumLoggerService.LogInfo("==================================================", "ESPION CONTEXTE");

        AppiumLoggerService.LogInfo("=== FIN : Fin de l'aspiration ===", Origine);
        Assert.Pass("Données récupérées.");
    }
}