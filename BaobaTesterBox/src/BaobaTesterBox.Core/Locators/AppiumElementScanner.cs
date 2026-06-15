// Fichier : src/BaobaTesterBox.Core/Locators/AppiumElementScanner.cs
using System;
using System.Collections.Generic;
using System.Threading;
using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Android;
using BaobaTesterBox.Core.Config.Models;
using BaobaTesterBox.Domain.Configuration;
using BaobaTesterBox.Core.Loggings;
using BaobaTesterBox.Core.ScriptExecutor;

namespace BaobaTesterBox.Core.Locators;

public static class AppiumElementScanner
{
    private static AndroidDriver? _driver;
    private static Dictionary<string, IWebElement> _cache = new();
    private const string OrigineLog = "AppiumElementScanner";
    private static readonly AppiumJsExecutor _jsExecutor = new();

    public static void Init(AndroidDriver driver)
    {
        _driver = driver;
        _cache = new Dictionary<string, IWebElement>();
        AppiumLoggerService.LogInfo("Initialisé avec succès.", OrigineLog);
    }

    /// <summary>
    /// Rappel : Bascule automatiquement le Driver du contexte NATIVE_APP vers la WEBVIEW active.
    /// </summary>
    private static void SwitchToWebViewContext()
    {
        if (_driver == null) return;

        // Si on est déjà dans la WebView, pas besoin de refaire tout le traitement
        if (_driver.Context.StartsWith("WEBVIEW")) return;

        AppiumLoggerService.LogInfo("Recherche des contextes disponibles...", OrigineLog);
        
        // Récupère la liste (ex: ["NATIVE_APP", "WEBVIEW_com.company.baoba"])
        var contexts = _driver.Contexts; 

        foreach (var context in contexts)
        {
            AppiumLoggerService.LogDebug($"Contexte détecté : {context}", OrigineLog);
            
            if (context.StartsWith("WEBVIEW", StringComparison.OrdinalIgnoreCase))
            {
                _driver.Context = context; // Le switch magique !
                AppiumLoggerService.LogInfo($"Bascule réussie sur le contexte WebView : {context}", OrigineLog);
                return;
            }
        }

        AppiumLoggerService.LogWarn("Aucun contexte WEBVIEW détecté. Le scanner va tenter de s'exécuter sur le contexte actuel.", OrigineLog);
    }

    private static string GetFreshPageSource()
    {
        if (_driver == null) return string.Empty;
        var js = (IJavaScriptExecutor)_driver;
        return js.ExecuteScript("return document.documentElement.outerHTML;") as string ?? string.Empty;
    }

    public static void AnalyzeCurrentPage()
    {
        _cache.Clear();

        if (_driver == null)
        {
            AppiumLoggerService.LogError("Impossible d'analyser la page : Le Driver n'est pas initialisé.", OrigineLog);
            return;
        }

        // AJOUT : Étape cruciale pour ne pas scanner dans le vide !
        try
        {
            SwitchToWebViewContext();
        }
        catch (Exception ex)
        {
            AppiumLoggerService.LogWarn($"Échec lors de la tentative de basculement vers la WebView : {ex.Message}", OrigineLog);
        }

        AppiumLoggerService.LogInfo("=== DÉBUT DU SCAN DE PAGE ===", OrigineLog);

        AppiumElementScannerOptions config = AppConfigProvider.GetScannerOptions();
        string rootTagName = config.RootContainerTagName ?? "body";

        HtmlDocument? doc = null;
        HtmlNode? rootNode = null;

        int maxAttentes = 10;
        for (int i = 1; i <= maxAttentes; i++)
        {
            try
            {
                var html = GetFreshPageSource();
                if (string.IsNullOrEmpty(html))
                    throw new Exception("PageSource vide");

                doc = new HtmlDocument();
                doc.LoadHtml(html);

                rootNode = doc.DocumentNode.SelectSingleNode($"//{rootTagName.ToLower()}")
                           ?? doc.DocumentNode.SelectSingleNode($"//{rootTagName.ToUpper()}");

                if (rootNode != null)
                {
                    AppiumLoggerService.LogInfo($"Conteneur racine '<{rootNode.Name}>' détecté ! Fin de l'attente.", OrigineLog);
                    break;
                }
            }
            catch (Exception)
            {
                // Ignoré pendant le chargement du DOM
            }

            AppiumLoggerService.LogWarn($"En attente de l'écran principal... Tentative {i}/{maxAttentes}", OrigineLog);
            Thread.Sleep(1000);
        }

        if (rootNode == null)
        {
            AppiumLoggerService.LogError($"Le conteneur '{rootTagName}' n'est jamais apparu.", OrigineLog);
            return;
        }

        try
        {
            var nodes = rootNode.SelectNodes(".//button | .//input | .//a");

            if (nodes == null || nodes.Count == 0)
            {
                AppiumLoggerService.LogWarn("Aucun élément interactif détecté dans le conteneur racine.", OrigineLog);
                return;
            }

            var typeCounters = new Dictionary<string, int>();
            var js = (IJavaScriptExecutor)_driver;

            foreach (var node in nodes)
            {
                var tagName = node.Name;

                if (!typeCounters.ContainsKey(tagName))
                    typeCounters[tagName] = 1;
                else
                    typeCounters[tagName]++;

                var technicalAttribute = node.GetAttributeValue("name",
                                         node.GetAttributeValue("type", "element"));

                var cleanAttribute = technicalAttribute.Replace(" ", "_").ToLower();
                var smartId = $"{tagName}_{cleanAttribute}_{typeCounters[tagName]}";
                var elementXPath = $"//{rootNode.Name}//{tagName}[{typeCounters[tagName]}]";

                try
                {
                    IWebElement webElement;

                    try
                    {
                        webElement = _driver.FindElement(By.XPath(elementXPath));
                    }
                    catch (WebDriverException)
                    {
                        AppiumLoggerService.LogWarn($"W3C masqué, tentative JS pour : {smartId}", "Fingerprint");

                        var jsElement = js.ExecuteScript(
                            $"return document.evaluate(\"{elementXPath}\", document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue;"
                        ) as IWebElement;

                        if (jsElement == null)
                        {
                            AppiumLoggerService.LogWarn($"Introuvable même via JS : {smartId}", "Fingerprint");
                            continue;
                        }

                        webElement = jsElement;
                        AppiumLoggerService.LogDebug($"Récupéré via JS -> Clé : \"{smartId}\"", "Fingerprint");
                    }

                    _cache.Add(smartId, webElement);
                    AppiumLoggerService.LogDebug($"Indexé -> Clé : \"{smartId}\" | XPath: {elementXPath}", "Fingerprint");
                }
                catch (Exception ex)
                {
                    AppiumLoggerService.LogWarn($"Échec total sur {smartId} : {ex.Message}", "Fingerprint");
                }
            }

            AppiumLoggerService.LogInfo($"=== FIN DU SCAN : {_cache.Count} éléments cartographiés avec succès ! ===", OrigineLog);
        }
        catch (Exception ex)
        {
            AppiumLoggerService.LogError($"Erreur critique pendant l'indexation : {ex.Message}", OrigineLog);
        }
    }

    public static List<string> GetLastDetectedIds()
    {
        return new List<string>(_cache.Keys);
    }

    public static IWebElement? GetElement(string smartId)
    {
        return _cache.TryGetValue(smartId, out var element) ? element : null;
    }
    
    /// <summary>
    /// Analyse le DOM en arrière-plan via le script externe et retourne une liste réutilisable contenant UNIQUEMENT les inputs visibles à l'écran.
    /// </summary>
    public static List<IWebElement> GetVisibleInputsOnly()
    {
        
        if (_driver == null)
        {
            AppiumLoggerService.LogError("Impossible d'extraire les inputs : Le Driver Appium n'est pas initialisé.", OrigineLog);
            return new List<IWebElement>();
        }

        try
        {
            // 1. On s'assure d'être dans le bon contexte pour exécuter le JS
            SwitchToWebViewContext();

            AppiumLoggerService.LogInfo("=== EXTRACTION DES INPUTS VISIBLES ===", OrigineLog);

            // 2. Appel de ton exécuteur qui charge et applique le fichier 'AppiumInputFetchVisible.js'
            // Au lieu de écrire juste "Enum.JsScriptType...", on préfixe avec ton namespace :
            var result = _jsExecutor.Execute(BaobaTesterBox.Core.ScriptExecutor.Enum.JsScriptType.AppiumInputFetchVisible) as IReadOnlyCollection<IWebElement>;

            // 3. Appium remonte les éléments sous forme de ReadOnlyCollection<IWebElement>
            if (result is IReadOnlyCollection<IWebElement> elementsCollection)
            {
                AppiumLoggerService.LogInfo($"📊 Extraction réussie : {elementsCollection.Count} input(s) visible(s) détecté(s).", OrigineLog);
            
                // On retourne une liste C# standard, dynamique et réutilisable
                return new List<IWebElement>(elementsCollection);
            }
        
            AppiumLoggerService.LogWarn("Le script d'extraction n'a renvoyé aucun élément valide.", OrigineLog);
        }
        catch (Exception ex)
        {
            AppiumLoggerService.LogError($"Erreur critique lors de la récupération des inputs visibles : {ex.Message}", OrigineLog);
        }

        // Sécurité : on retourne une liste vide plutôt qu'un null pour éviter les NullReferenceException dans tes tests
        return new List<IWebElement>();
    }
}