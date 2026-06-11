using System;
using NUnit.Framework;
using OpenQA.Selenium;
using Serilog;
using TestICA.Core.Starter;
using TestICA.Core.SmartLocator;
using TestICA.Core.Shell;

namespace TestICA.Tests;

[TestFixture]
public class NavigationDiagnosticTest : AppiumStarter
{
    [Test]
    public void DiagnostiquerFonctionsJsNavigation()
    {
        SmartShell.Launch();
        Log.Information("=== [DEBUT] Diagnostic des fonctions JS de navigation ===");

        InitDriver();

        try
        {
            SmartConsole.Initialize();
            System.Threading.Thread.Sleep(3000);

            string? webviewContext = null;
            foreach (var context in Driver!.Contexts)
            {
                SmartConsole.Log("I", "ContextManager", $"Mode détecté : {context}");
                if (context.Contains("WEBVIEW")) webviewContext = context;
            }

            if (webviewContext != null)
            {
                Driver.Context = webviewContext;
                SmartConsole.Log("I", "ContextManager", $"Bascule réussie : {webviewContext}");
            }
        }
        catch (Exception ex)
        {
            SmartConsole.Log("E", "ContextManager", $"Erreur bascule : {ex.Message}");
        }

        // Pause pour laisser l'appli être connectée manuellement
        SmartConsole.Log("I", "Diagnostic", "Attente de 10 secondes — connecte toi manuellement sur l'appli...");
        System.Threading.Thread.Sleep(10000);

        var js = (IJavaScriptExecutor)Driver!;

        // Diagnostic des fonctions JS disponibles
        SmartConsole.Log("I", "Diagnostic", "=== FONCTIONS JS DISPONIBLES ===");
        string scriptFonctions = @"
            return JSON.stringify({
                goExploitation: typeof goExploitation !== 'undefined',
                goPoultryBatch: typeof goPoultryBatch !== 'undefined',
                openPoultryModal: typeof openPoultryModal !== 'undefined',
                go: typeof go !== 'undefined',
                rubriqueid: document.querySelector('input[name=""rubriqueid""]')?.value ?? 'null',
                oldrubriqueid: document.querySelector('input[name=""oldrubriqueid""]')?.value ?? 'null',
                farmId: document.querySelector('input[name=""farmId""]')?.value ?? 'null',
                poultryPlannedBatchId: document.querySelector('input[name=""poultryPlannedBatchId""]')?.value ?? 'null'
            });
        ";

        string resultat = (string)js.ExecuteScript(scriptFonctions);
        SmartConsole.Log("I", "Diagnostic", $"Résultat : {resultat}");

        // Si goPoultryBatch est dispo, on tente la navigation
        SmartConsole.Log("I", "Diagnostic", "=== TENTATIVE NAVIGATION VERS LOT ===");
        SmartConsole.Log("I", "Diagnostic", "Navigue manuellement sur un lot puis attends 5 secondes...");
        System.Threading.Thread.Sleep(20000);
        
        string scriptGlobaux = @"
            return JSON.stringify({
                globalPoultryBatchId: typeof globalPoultryBatchId !== 'undefined' ? globalPoultryBatchId : 'undefined',
                globalFarmId: typeof globalFarmId !== 'undefined' ? globalFarmId : 'undefined',
                globalModuleId: typeof globalModuleId !== 'undefined' ? globalModuleId : 'undefined',
                globalPoultryBatchType: typeof globalPoultryBatchType !== 'undefined' ? globalPoultryBatchType : 'undefined'
            });
        ";

        string resultatGlobaux = (string)js.ExecuteScript(scriptGlobaux);
        SmartConsole.Log("I", "Diagnostic", $"Variables globales JS : {resultatGlobaux}");

        string scriptApresNav = @"
            return JSON.stringify({
                rubriqueid: document.querySelector('input[name=""rubriqueid""]')?.value ?? 'null',
                oldrubriqueid: document.querySelector('input[name=""oldrubriqueid""]')?.value ?? 'null',
                farmId: document.querySelector('input[name=""farmId""]')?.value ?? 'null',
                poultryPlannedBatchId: document.querySelector('input[name=""poultryPlannedBatchId""]')?.value ?? 'null',
                poultryAreaId: document.querySelector('input[name=""poultryAreaId""]')?.value ?? 'null'
            });
        ";

        string resultatApresNav = (string)js.ExecuteScript(scriptApresNav);
        SmartConsole.Log("I", "Diagnostic", $"Valeurs après navigation manuelle : {resultatApresNav}");

        Assert.Pass("Diagnostic terminé — consulte les logs.");
    }
}