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

    public static void AnalyzeCurrentPage()
    {
        // 1. Sécurité : Vérification du cache
        if (_cache == null)
        {
            _cache = new Dictionary<string, IWebElement>();
        }
        _cache.Clear();
        
        // 2. Sécurité : Vérification du Driver
        if (_driver == null)
        {
            SmartConsole.Log("E", "SmartLocator", "Impossible d'analyser la page : Le Driver n'est pas initialisé.");
            return;
        }

        SmartConsole.Log("I", "SmartLocator", "=== DÉBUT DU SCAN DE PAGE ===");
        
        // 3. Sécurité Ligne 26 : Gestion du ConfigReader qui peut être null
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
        
        // --- BOUCLE D'ATTENTE SYNCHRO (Gère les 2 à 7 secondes de chargement) ---
        int maxAttentes = 10;
        for (int i = 1; i <= maxAttentes; i++)
        {
            try
            {
                var html = _driver.PageSource;
                if (string.IsNullOrEmpty(html))
                {
                    throw new Exception("PageSource vide");
                }

                doc = new HtmlDocument();
                doc.LoadHtml(html);

                // Recherche insensible à la casse (Tente //body puis //BODY)
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
            Thread.Sleep(1000); // Attend 1 seconde avant la prochaine tentative
        }

        // Si après 10 secondes on n'a toujours rien trouvé
        if (rootNode == null)
        {
            SmartConsole.Log("E", "SmartLocator", $"Le conteneur '{rootTagName}' (ou '{rootTagName.ToUpper()}') n'est jamais apparu.");
            return;
        }

        // --- DEBUT DE L'INDEXATION DES ELEMENTS ---
        try
        {
            // Extrait les boutons, inputs et liens du XSLT
            var nodes = rootNode.SelectNodes(".//button | .//input | .//a");
            
            if (nodes == null || nodes.Count == 0)
            {
                SmartConsole.Log("W", "SmartLocator", "Aucun élément interactif détecté dans le conteneur racine.");
                return;
            }

            var typeCounters = new Dictionary<string, int>();

            foreach (var node in nodes)
            {
                var tagName = node.Name;
                
                if (!typeCounters.ContainsKey(tagName))
                {
                    typeCounters[tagName] = 1;
                }
                else
                {
                    typeCounters[tagName]++;
                }

                var technicalAttribute = node.GetAttributeValue("name", 
                                         node.GetAttributeValue("type", "element"));
                
                var cleanAttribute = technicalAttribute.Replace(" ", "_").ToLower();
                var smartId = $"{tagName}_{cleanAttribute}_{typeCounters[tagName]}";
                
                // Construit le XPath basé sur le vrai nom du nœud parent trouvé (BODY ou body)
                var elementXPath = $"//{rootNode.Name}//{tagName}[{typeCounters[tagName]}]";

                try
                {
                    var webElement = _driver.FindElement(By.XPath(elementXPath));
                    _cache.Add(smartId, webElement);
                    
                    // Log en bleu dans le terminal
                    SmartConsole.Log("D", "Fingerprint", $"Indexé -> Clé : \"{smartId}\" | XPath: {elementXPath}");
                }
                catch (WebDriverException)
                {
                    SmartConsole.Log("W", "Fingerprint", $"Élément HTML masqué pour Appium : {smartId}");
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
        {
            return new List<string>();
        }

        return new List<string>(_cache.Keys);
    }
    
    public static IWebElement GetElement(string smartId)
    {
        if (_cache != null && _cache.TryGetValue(smartId, out var element))
        {
            return element;
        }
        return null;
    }
}