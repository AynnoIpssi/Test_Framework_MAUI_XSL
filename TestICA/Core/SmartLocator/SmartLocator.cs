using System;
using System.Collections.Generic;
using System.Threading;
using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Android;
using TestICA.Core.Config;

namespace TestICA.Core.SmartLocator;

public static class SmartLocator
{
    private static AndroidDriver _driver;
    private static Dictionary<string, IWebElement> _cache;

    public static void Init(AndroidDriver driver)
    {
        _driver = driver;
        _cache = new Dictionary<string, IWebElement>();
        SmartConsole.Initialize();
        SmartConsole.Log("I", "SmartLocator", "Initialisé avec succès.");
    }

    private static string GetFreshPageSource()
    {
        var js = (IJavaScriptExecutor)_driver;
        return js.ExecuteScript("return document.documentElement.outerHTML;") as string ?? string.Empty;
    }

    public static void AnalyzeCurrentPage()
    {
        if (_cache == null)
            _cache = new Dictionary<string, IWebElement>();
        _cache.Clear();

        if (_driver == null)
        {
            SmartConsole.Log("E", "SmartLocator", "Impossible d'analyser la page : Le Driver n'est pas initialisé.");
            return;
        }

        SmartConsole.Log("I", "SmartLocator", "=== DÉBUT DU SCAN DE PAGE ===");

        string rootTagName = "body";
        if (ConfigReader.SmartLocator != null && !string.IsNullOrEmpty(ConfigReader.SmartLocator.RootContainerTagName))
        {
            rootTagName = ConfigReader.SmartLocator.RootContainerTagName;
        }
        else
        {
            SmartConsole.Log("W", "SmartLocator", "ConfigReader.SmartLocator est null ou incomplet. Utilisation du fallback par défaut : 'body'.");
        }

        HtmlDocument doc = null;
        HtmlNode rootNode = null;

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
                    SmartConsole.Log("I", "SmartLocator", $"Conteneur racine '<{rootNode.Name}>' détecté ! Fin de l'attente.");
                    break;
                }
            }
            catch (Exception)
            {
                // Ignoré pendant le chargement de la page
            }

            SmartConsole.Log("W", "SmartLocator", $"En attente de l'écran principal... Tentative {i}/{maxAttentes}");
            Thread.Sleep(1000);
        }

        if (rootNode == null)
        {
            SmartConsole.Log("E", "SmartLocator", $"Le conteneur '{rootTagName}' (ou '{rootTagName.ToUpper()}') n'est jamais apparu.");
            return;
        }

        try
        {
            var nodes = rootNode.SelectNodes(".//button | .//input | .//a");

            if (nodes == null || nodes.Count == 0)
            {
                SmartConsole.Log("W", "SmartLocator", "Aucun élément interactif détecté dans le conteneur racine.");
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
                        // Tentative standard W3C
                        webElement = _driver.FindElement(By.XPath(elementXPath));
                    }
                    catch (WebDriverException)
                    {
                        // Fallback : récupération via JS direct sur le DOM frais
                        SmartConsole.Log("W", "Fingerprint", $"W3C masqué, tentative JS pour : {smartId}");

                        var jsElement = js.ExecuteScript(
                            $"return document.evaluate(\"{elementXPath}\", document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue;"
                        ) as IWebElement;

                        if (jsElement == null)
                        {
                            SmartConsole.Log("W", "Fingerprint", $"Introuvable même via JS : {smartId}");
                            continue;
                        }

                        webElement = jsElement;
                        SmartConsole.Log("D", "Fingerprint", $"Récupéré via JS -> Clé : \"{smartId}\"");
                    }

                    _cache.Add(smartId, webElement);
                    SmartConsole.Log("D", "Fingerprint", $"Indexé -> Clé : \"{smartId}\" | XPath: {elementXPath}");
                }
                catch (Exception ex)
                {
                    SmartConsole.Log("W", "Fingerprint", $"Échec total sur {smartId} : {ex.Message}");
                    continue;
                }
            }

            SmartConsole.Log("I", "SmartLocator", $"=== FIN DU SCAN : {_cache.Count} éléments cartographiés avec succès ! ===");
        }
        catch (Exception ex)
        {
            SmartConsole.Log("E", "SmartLocator", $"Erreur critique pendant l'indexation : {ex.Message}");
        }
    }

    public static List<string> GetLastDetectedIds()
    {
        if (_cache == null)
            return new List<string>();

        return new List<string>(_cache.Keys);
    }

    public static IWebElement GetElement(string smartId)
    {
        if (_cache != null && _cache.TryGetValue(smartId, out var element))
            return element;

        return null;
    }
}