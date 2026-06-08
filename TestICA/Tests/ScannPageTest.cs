using System;
using NUnit.Framework;
using Serilog;
using TestICA.Core.Starter;
using TestICA.Core.SmartLocator;
using TestICA.Core.Shell;

namespace TestICA.Tests;

[TestFixture]
public class SmartLocatorTest : AppiumStarter
{
    [Test]
    public void VerifierDemarrageEtScanPageComplexe()
    {
        SmartShell.Launch();
        Log.Information("=== [DEBUT] Lancement du scénario de test complet ===");

        // 1. Démarre l'application (appelle InitDriver du AppiumStarter)
        InitDriver();

        // --- BASCULE DE CONTEXTE (NATIF TO WEBVIEW) ---
        try
        {
            SmartConsole.Initialize();
            SmartConsole.Log("I", "ContextManager", "Recherche des contextes disponibles...");

            // Pause pour laisser l'appli charger sa WebView
            System.Threading.Thread.Sleep(3000);

            string? webviewContext = null;
            
            foreach (var context in Driver!.Contexts)
            {
                SmartConsole.Log("I", "ContextManager", $"Mode détecté : {context}");
                if (context.Contains("WEBVIEW"))
                {
                    webviewContext = context;
                }
            }

            if (webviewContext != null)
            {
                Driver.Context = webviewContext;
                SmartConsole.Log("I", "ContextManager", $"Bascule réussie sur le mode : {webviewContext}");
            }
            else
            {
                SmartConsole.Log("W", "ContextManager", "Aucune WebView détectée. L'application est lue à 100% en Natif Android.");
            }
        }
        catch (Exception ex)
        {
            SmartConsole.Log("E", "ContextManager", $"Erreur lors de la bascule de contexte : {ex.Message}");
        }

        // 2. Connecte le driver au SmartLocator
        SmartLocator.Init(Driver);

        // 3. Lance l'analyse
        SmartLocator.AnalyzeCurrentPage();

        System.Threading.Thread.Sleep(5000); 
        Assert.Pass("Le moteur a terminé son cycle d'analyse.");
    }
}