using System;
using System.IO;
using System.Text.Json.Nodes;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Android;
using TestICA.Core.SmartLocator;
using TestICA.Core.SmartWaiter;

namespace TestICA.Core.JsNavigator;

public class JsNavigator
{
    private readonly AndroidDriver _driver;
    private readonly SmartWaiter.SmartWaiter _waiter;
    private static JsonNode? _appMap;

    public JsNavigator(AndroidDriver driver)
    {
        _driver = driver;
        _waiter = new SmartWaiter.SmartWaiter(driver);
        LoadAppMap();
    }

    private static void LoadAppMap()
    {
        if (_appMap != null) return;

        try
        {
            string path = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Data", "AppMap.json"
            );

            if (!File.Exists(path))
            {
                SmartConsole.Log("E", "JsNavigator", $"AppMap.json introuvable : {path}");
                return;
            }

            _appMap = JsonNode.Parse(File.ReadAllText(path));
            SmartConsole.Log("I", "JsNavigator", "AppMap.json chargé.");
        }
        catch (Exception ex)
        {
            SmartConsole.Log("E", "JsNavigator", $"Erreur chargement AppMap : {ex.Message}");
        }
    }

    // =========================================================================
    // GoTo — navigue vers une rubrique par son ID
    // Ex: navigator.GoTo("580")
    // =========================================================================
    public bool GoTo(string rubriqueId)
    {
        if (_appMap == null)
        {
            SmartConsole.Log("E", "JsNavigator", "AppMap non chargé.");
            return false;
        }

        var page = _appMap["pages"]?[rubriqueId];
        if (page == null)
        {
            SmartConsole.Log("E", "JsNavigator", $"Rubrique {rubriqueId} inconnue dans AppMap.");
            return false;
        }

        bool navigable = page["navigable"]?.GetValue<bool>() ?? false;
        if (!navigable)
        {
            SmartConsole.Log("W", "JsNavigator",
                $"Rubrique {rubriqueId} marquée non navigable dans AppMap.");
            return false;
        }

        string nom = page["nom"]?.GetValue<string>() ?? rubriqueId;
        string jsNav = page["jsNavigation"]?.GetValue<string>()
                       ?? $"go(1, {rubriqueId}, '', '', '', '')";

        SmartConsole.Log("I", "JsNavigator", $"Navigation vers [{rubriqueId}] {nom}...");
        SmartConsole.Log("I", "JsNavigator", $"JS : {jsNav}");

        try
        {
            var js = (IJavaScriptExecutor)_driver;
            js.ExecuteScript(jsNav);

            // Attend que la page soit chargée
            bool arrived = _waiter.WaitForPage(rubriqueId);

            if (arrived)
                SmartConsole.Log("I", "JsNavigator", $"✅ Arrivé sur [{rubriqueId}] {nom}");
            else
                SmartConsole.Log("E", "JsNavigator", $"❌ Timeout — page [{rubriqueId}] jamais chargée");

            return arrived;
        }
        catch (Exception ex)
        {
            SmartConsole.Log("E", "JsNavigator", $"Erreur navigation : {ex.Message}");
            return false;
        }
    }

    // =========================================================================
    // GoToWithParams — pour les pages qui nécessitent des paramètres
    // Ex: navigator.GoToWithParams("goPoultryBatch({0}, {1})", batchId, farmId)
    // =========================================================================
    public bool GoToWithParams(string rubriqueId, string jsTemplate, params string[] args)
    {
        string jsCall = string.Format(jsTemplate, args);
        string nom = _appMap?["pages"]?[rubriqueId]?["nom"]?.GetValue<string>() ?? rubriqueId;

        SmartConsole.Log("I", "JsNavigator", $"Navigation vers [{rubriqueId}] {nom}...");
        SmartConsole.Log("I", "JsNavigator", $"JS : {jsCall}");

        try
        {
            var js = (IJavaScriptExecutor)_driver;
            js.ExecuteScript(jsCall);

            bool arrived = _waiter.WaitForPage(rubriqueId);

            if (arrived)
                SmartConsole.Log("I", "JsNavigator", $"✅ Arrivé sur [{rubriqueId}] {nom}");
            else
                SmartConsole.Log("E", "JsNavigator", $"❌ Timeout — page [{rubriqueId}] jamais chargée");

            return arrived;
        }
        catch (Exception ex)
        {
            SmartConsole.Log("E", "JsNavigator", $"Erreur navigation : {ex.Message}");
            return false;
        }
    }
}