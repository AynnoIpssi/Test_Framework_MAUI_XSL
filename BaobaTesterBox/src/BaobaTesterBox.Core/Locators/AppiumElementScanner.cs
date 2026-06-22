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
using Newtonsoft.Json;
using BaobaTesterBox.Domain.Models;

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
        if (_driver.Context.StartsWith("WEBVIEW")) return;

        AppiumLoggerService.LogInfo("Recherche des contextes disponibles...", OrigineLog);
        var contexts = _driver.Contexts; 

        foreach (var context in contexts)
        {
            AppiumLoggerService.LogDebug($"Contexte détecté : {context}", OrigineLog);
            
            if (context.StartsWith("WEBVIEW", StringComparison.OrdinalIgnoreCase))
            {
                _driver.Context = context;
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
                    }

                    _cache.Add(smartId, webElement);
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
    
    public static List<IWebElement> GetVisibleInputsOnly()
    {
        if (_driver == null)
        {
            AppiumLoggerService.LogError("Impossible d'extraire les inputs : Le Driver Appium n'est pas initialisé.", OrigineLog);
            return new List<IWebElement>();
        }

        try
        {
            SwitchToWebViewContext();
            AppiumLoggerService.LogInfo("=== EXTRACTION DES INPUTS VISIBLES ===", OrigineLog);

            var result = _jsExecutor.Execute(BaobaTesterBox.Core.ScriptExecutor.Enum.JsScriptType.AppiumInputFetchVisible);

            if (result is IReadOnlyCollection<IWebElement> elementsCollection)
            {
                AppiumLoggerService.LogInfo($"📊 Extraction réussie : {elementsCollection.Count} input(s) visible(s) détecté(s).", OrigineLog);
                return new List<IWebElement>(elementsCollection);
            }
        }
        catch (Exception ex)
        {
            AppiumLoggerService.LogError($"Erreur critique lors de la récupération des inputs visibles : {ex.Message}", OrigineLog);
        }

        return new List<IWebElement>();
    }
    
    public static AppiumFormScanResult GetVisibleFormFields()
    {
        var resultat = new AppiumFormScanResult();

        if (_driver == null)
        {
            AppiumLoggerService.LogError("Impossible de scanner les champs : Le Driver n'est pas initialisé.", OrigineLog);
            return resultat;
        }

        try
        {
            SwitchToWebViewContext();
            AppiumLoggerService.LogInfo("=== ANALYSE DES CHAMPS VISIBLES (input + select + textarea) ===", OrigineLog);

            var resultatJson = _jsExecutor.Execute(BaobaTesterBox.Core.ScriptExecutor.Enum.JsScriptType.AppiumFormFieldsFetchVisible) as string;

            if (string.IsNullOrWhiteSpace(resultatJson))
            {
                AppiumLoggerService.LogWarn("Le scan des champs n'a renvoyé aucune donnée.", OrigineLog);
                return resultat;
            }

            resultat = JsonConvert.DeserializeObject<AppiumFormScanResult>(resultatJson) ?? new AppiumFormScanResult();
            AppiumLoggerService.LogInfo($"📋 Mode : {resultat.Mode} | {resultat.Fields.Count} champ(s) visible(s) détecté(s).", OrigineLog);
        }
        catch (Exception ex)
        {
            AppiumLoggerService.LogError($"Erreur pendant le scan des champs visibles : {ex.Message}", OrigineLog);
        }

        return resultat;
    }

    /// <summary>
    /// 🟢 NOUVELLE MÉTHODE PROPRE : Extrait de façon isolée les informations du lot actif (#batch) dans le DOM
    /// </summary>
    public static Dictionary<string, string> ScanCurrentBatchInformation()
    {
        var lotInfos = new Dictionary<string, string>();
        
        if (_driver == null)
        {
            AppiumLoggerService.LogError("Impossible de scanner le lot : Le Driver n'est pas initialisé.", OrigineLog);
            return lotInfos;
        }

        try
        {
            SwitchToWebViewContext();
            var html = GetFreshPageSource();
            if (string.IsNullOrEmpty(html)) return lotInfos;

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var selectBatchNode = doc.DocumentNode.SelectSingleNode("//select[@id='batch']");
            if (selectBatchNode == null)
            {
                AppiumLoggerService.LogWarn("❌ [SCAN BATCH] Élément <select id='batch'> introuvable sur cette page.", OrigineLog);
                return lotInfos;
            }

            var options = selectBatchNode.SelectNodes(".//option");
            if (options == null || options.Count == 0) return lotInfos;

            string tousLesLots = "";
            foreach (var option in options)
            {
                string val = option.GetAttributeValue("value", "").Trim();
                string txt = option.InnerText.Trim();
                bool isSelected = option.Attributes.Contains("selected");

                if (!string.IsNullOrEmpty(val)) tousLesLots += $"[{val}:{txt}] ";

                if (isSelected || selectBatchNode.GetAttributeValue("value", "") == val)
                {
                    lotInfos["SelectedValue"] = val;
                    lotInfos["SelectedText"] = txt;
                }
            }

            if (!lotInfos.ContainsKey("SelectedValue") && options.Count > 0)
            {
                lotInfos["SelectedValue"] = options[0].GetAttributeValue("value", "");
                lotInfos["SelectedText"] = options[0].InnerText.Trim();
            }
            lotInfos["AllAvailableOptions"] = tousLesLots;

            AppiumLoggerService.LogInfo("==========================================================", OrigineLog);
            AppiumLoggerService.LogInfo($"🟢 LOT ACTUELLEMENT CHARGÉ : {lotInfos.GetValueOrDefault("SelectedValue", "INCONNU")}", OrigineLog);
            AppiumLoggerService.LogInfo($"📄 TEXTE INTEGRAL DU LOT   : {lotInfos.GetValueOrDefault("SelectedText", "INCONNU")}", OrigineLog);
            AppiumLoggerService.LogInfo($"📦 TOUTES LES OPTIONS DISPO : {lotInfos.GetValueOrDefault("AllAvailableOptions", "AUCUNE")}", OrigineLog);
            AppiumLoggerService.LogInfo("==========================================================", OrigineLog);
        }
        catch (Exception ex)
        {
            AppiumLoggerService.LogError($"Erreur pendant le scan ciblé du lot : {ex.Message}", OrigineLog);
        }

        return lotInfos;
    }
    
    /// <summary>
    /// 🕵️‍♂️ OUTIL DE DIAGNOSTIC : Radiographie complète de l'état de la page Fiche ICA
    /// </summary>
    public static Dictionary<string, string> DumpFicheIcaCurrentState()
    {
        var diagnostics = new Dictionary<string, string>();
        
        if (_driver == null)
        {
            AppiumLoggerService.LogError("Impossible de dumper la page : Le Driver n'est pas initialisé.", OrigineLog);
            return diagnostics;
        }

        try
        {
            SwitchToWebViewContext();
            var html = GetFreshPageSource();
            if (string.IsNullOrEmpty(html)) return diagnostics;

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            AppiumLoggerService.LogInfo("==================================================================", OrigineLog);
            AppiumLoggerService.LogInfo("🔍 [PAGE DUMP] INSPECTION COMPLÈTE DE L'ÉCRAN FICHE ICA", OrigineLog);
            AppiumLoggerService.LogInfo("==================================================================", OrigineLog);

            // 1. Extraction de l'Action
            var actionNode = doc.DocumentNode.SelectSingleNode("//select[@id='actionid']");
            string actionTxt = actionNode?.SelectSingleNode(".//option[@selected]")?.InnerText.Trim() 
                               ?? actionNode?.SelectSingleNode(".//option[1]")?.InnerText.Trim() ?? "Non trouvé";
            AppiumLoggerService.LogInfo($"🎬 Action sélectionnée  : {actionTxt}", OrigineLog);

            // 2. Extraction du Lot (#batch)
            var batchNode = doc.DocumentNode.SelectSingleNode("//select[@id='batch']");
            var selectedBatchOpt = batchNode?.SelectSingleNode(".//option[@selected]");
            string batchId = selectedBatchOpt?.GetAttributeValue("value", "") ?? batchNode?.GetAttributeValue("value", "") ?? "Inconnu";
            string batchTxt = selectedBatchOpt?.InnerText.Trim() ?? "Aucun texte";
            AppiumLoggerService.LogInfo($"📦 Lot Actif (#batch)   : ID = {batchId} | Label = {batchTxt}", OrigineLog);

            // 3. Extraction de l'Abattoir (#dif_enr16)
            var abattoirNode = doc.DocumentNode.SelectSingleNode("//select[@id='dif_enr16']");
            string abattoirTxt = abattoirNode?.SelectSingleNode(".//option[@selected]")?.InnerText.Trim() ?? "Non sélectionné";
            AppiumLoggerService.LogInfo($"🏢 Abattoir (#dif_enr16): {abattoirTxt}", OrigineLog);

            // 4. Lecture des dates clés insérées dans les inputs
            string dateSignature = doc.DocumentNode.SelectSingleNode("//input[@id='dif_enr2']")?.GetAttributeValue("value", "") ?? "Vide";
            string dateReception = doc.DocumentNode.SelectSingleNode("//input[@id='dif_enr15']")?.GetAttributeValue("value", "") ?? "Vide";
            string dateEnlevement = doc.DocumentNode.SelectSingleNode("//input[@id='removal_date' or @id='dif_enr14']")?.GetAttributeValue("value", "") ?? "Vide";
            AppiumLoggerService.LogInfo($"📅 Date Signature       : {dateSignature}", OrigineLog);
            AppiumLoggerService.LogInfo($"📅 Date Réception Abat. : {dateReception}", OrigineLog);
            AppiumLoggerService.LogInfo($"📅 Date Enlèvement      : {dateEnlevement}", OrigineLog);

            // 5. Analyse de l'état des sections d'animaux (Masquées ou Affichées dans le style HTML)
            var maleNode = doc.DocumentNode.SelectSingleNode("//div[@id='male']");
            var femaleNode = doc.DocumentNode.SelectSingleNode("//div[@id='female']");
            var nonSexNode = doc.DocumentNode.SelectSingleNode("//div[@id='non-sexing']");

            string stateMale = maleNode?.GetAttributeValue("style", "").Contains("display: none") == true ? "❌ MASQUÉ" : "🟢 VISIBLE";
            string stateFemale = femaleNode?.GetAttributeValue("style", "").Contains("display: none") == true ? "❌ MASQUÉ" : "🟢 VISIBLE";
            string stateNonSex = nonSexNode?.GetAttributeValue("style", "").Contains("display: none") == true ? "❌ MASQUÉ" : "🟢 VISIBLE (Tout-venant)";

            AppiumLoggerService.LogInfo($"🧬 Section Mâles        : {stateMale}", OrigineLog);
            AppiumLoggerService.LogInfo($"🧬 Section Femelles      : {stateFemale}", OrigineLog);
            AppiumLoggerService.LogInfo($"🧬 Section Tout-venant   : {stateNonSex}", OrigineLog);

            // 6. Lecture des valeurs de quantités saisies (si présentes)
            string qteMale = doc.DocumentNode.SelectSingleNode("//input[@id='dif_enr17']")?.GetAttributeValue("value", "") ?? "0";
            string qteFemale = doc.DocumentNode.SelectSingleNode("//input[@id='dif_enr19']")?.GetAttributeValue("value", "") ?? "0";
            string qteToutVenant = doc.DocumentNode.SelectSingleNode("//input[@id='dif_enr21']")?.GetAttributeValue("value", "") ?? "0";
            AppiumLoggerService.LogInfo($"📊 Quantité [M]: {qteMale} | [F]: {qteFemale} | [Tout-venant]: {qteToutVenant}", OrigineLog);

            AppiumLoggerService.LogInfo("==================================================================", OrigineLog);

            // Remplissage du dictionnaire pour exploitation par le code
            diagnostics["BatchId"] = batchId;
            diagnostics["BatchText"] = batchTxt;
            diagnostics["IsToutVenantVisible"] = (stateNonSex == "🟢 VISIBLE (Tout-venant)").ToString();
        }
        catch (Exception ex)
        {
            AppiumLoggerService.LogError($"Erreur pendant le gros dump de la page : {ex.Message}", OrigineLog);
        }

        return diagnostics;
    }
}