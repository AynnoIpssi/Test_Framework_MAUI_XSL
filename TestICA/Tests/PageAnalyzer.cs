using System;
using System.Linq;
using NUnit.Framework;
using OpenQA.Selenium;
using Serilog;
using TestICA.Core.Starter;
using TestICA.Core.SmartLocator;

namespace TestICA.Tests;

[TestFixture]
public class PageAnalyser : AppiumStarter
{
    [Test]
    public void Lancer_Analyse_Profonde_WebView()
    {
        Log.Information("=======================================================");
        Log.Information("=== [DÉBUT] ANALYSE PROFONDE DE L'ÉCRAN BLOQUÉ ===");
        Log.Information("=======================================================");

        // 1. Initialisation standard du Driver Appium
        InitDriver();
        System.Threading.Thread.Sleep(2000); // Sécurité chargement app

        // 2. ÉTAPE A : Diagnostic des contextes (Multi-WebView ?)
        Log.Information("[Étape A] Analyse des contextes disponibles...");
        var contexts = Driver!.Contexts;
        foreach (var ctx in contexts)
        {
            Log.Information($"   -> Contexte détecté : {ctx}");
        }

        // Bascule standard vers la WebView
        BasculeVersWebView();
        Log.Information($"[Étape A] Contexte actuel après bascule : {Driver.Context}");

        // 3. ÉTAPE B : Diagnostic des Iframes
        Log.Information("[Étape B] Recherche d'iframes cachées dans cette WebView...");
        try
        {
            var iframes = Driver.FindElements(By.TagName("iframe"));
            Log.Information($"   -> Nombre d'iframes trouvées sur cette page : {iframes.Count}");
            
            for (int i = 0; i < iframes.Count; i++)
            {
                var idAttr = iframes[i].GetAttribute("id");
                var nameAttr = iframes[i].GetAttribute("name");
                var srcAttr = iframes[i].GetAttribute("src");
                Log.Information($"      * Iframe [{i}] -> ID: '{idAttr}' | Name: '{nameAttr}' | Src: '{srcAttr}'");
            }
        }
        catch (Exception ex)
        {
            Log.Error($"   -> Échec de la recherche d'iframes : {ex.Message}");
        }

        // 4. ÉTAPE C : URL actuelle vue par le Robot
        try
        {
            Log.Information($"[Étape C] URL actuelle du robot : {Driver.Url}");
        }
        catch (Exception ex)
        {
            Log.Error($"   -> Impossible de récupérer l'URL : {ex.Message}");
        }

        // 5. ÉTAPE D : Scan via SmartLocator
        Log.Information("[Étape D] Lancement de ton SmartLocator...");
        SmartLocator.Init(Driver!);
        SmartLocator.AnalyzeCurrentPage();

        var ids = SmartLocator.GetLastDetectedIds();
        Log.Information("=======================================================");
        Log.Information($" RESULTAT DU SMARTLOCATOR : {ids.Count} ÉLÉMENTS DANS LE CACHE");
        Log.Information("=======================================================");
        foreach (var id in ids)
        {
            Log.Information($"   -> ID enregistré : {id}");
        }

        Log.Information("=======================================================");
        Log.Information("=== [FIN] Analyse terminée. Regarde les logs ci-dessus ===");
        Log.Information("=======================================================");
    }
}