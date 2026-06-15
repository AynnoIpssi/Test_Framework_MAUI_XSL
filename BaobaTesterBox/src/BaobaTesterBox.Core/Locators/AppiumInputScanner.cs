// Fichier : src/BaobaTesterBox.Core/Locators/AppiumInputScanner.cs
using System;
using System.Collections.Generic;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Android;
using BaobaTesterBox.Core.Loggings;
using BaobaTesterBox.Core.ScriptExecutor;

namespace BaobaTesterBox.Core.Locators;

public static class AppiumInputScanner
{
    private const string OrigineLog = "AppiumInputScanner";
    
    private static AndroidDriver? _driver;
    private static AppiumJsExecutor? _jsExecutor;

    public static void Init(AndroidDriver driver)
    {
        _driver = driver ?? throw new ArgumentNullException(nameof(driver), "Le driver fourni au scanner est null.");
        // Ton exécuteur JS garde son constructeur à 0 paramètre d'origine !
        _jsExecutor = new AppiumJsExecutor(); 
        AppiumLoggerService.LogInfo("Scanner dédié aux inputs initialisé avec succès.", OrigineLog);
    }

    public static List<IWebElement> GetVisibleInputsOnly()
    {
        if (_driver == null || _jsExecutor == null)
        {
            AppiumLoggerService.LogError("Impossible d'extraire les inputs : Le Driver Appium n'est pas initialisé ou Init() n'a pas été appelé.", OrigineLog);
            return new List<IWebElement>();
        }

        try
        {
            SwitchToWebViewContext();

            AppiumLoggerService.LogInfo("=== EXTRACTION DES INPUTS VISIBLES ===", OrigineLog);

            // Appel de ton exécuteur de scripts inchangé
            var result = _jsExecutor.Execute(BaobaTesterBox.Core.ScriptExecutor.Enum.JsScriptType.AppiumInputFetchVisible);

            if (result is IReadOnlyCollection<IWebElement> elementsCollection)
            {
                AppiumLoggerService.LogInfo($"📊 Extraction réussie : {elementsCollection.Count} input(s) visible(s) détecté(s).", OrigineLog);
                return new List<IWebElement>(elementsCollection);
            }
        
            AppiumLoggerService.LogWarn("Le script d'extraction n'a renvoyé aucun élément valide.", OrigineLog);
        }
        catch (Exception ex)
        {
            AppiumLoggerService.LogError($"Erreur critique lors de la récupération des inputs visibles : {ex.Message}", OrigineLog);
        }

        return new List<IWebElement>();
    }

    private static void SwitchToWebViewContext()
    {
        if (_driver == null) return;

        try
        {
            if (!_driver.Context.Contains("WEBVIEW"))
            {
                var webViewContext = _driver.Contexts.FirstOrDefault(c => c.Contains("WEBVIEW"));
                if (!string.IsNullOrEmpty(webViewContext))
                {
                    _driver.Context = webViewContext;
                    AppiumLoggerService.LogInfo($"Bascule de sécurité vers le contexte : {webViewContext}", OrigineLog);
                }
            }
        }
        catch (Exception ex)
        {
            AppiumLoggerService.LogWarn($"Impossible de vérifier ou basculer le contexte WebView : {ex.Message}", OrigineLog);
        }
    }
}