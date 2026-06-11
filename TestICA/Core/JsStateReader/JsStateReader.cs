using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Android;
using TestICA.Core.SmartLocator;

namespace TestICA.Core.JsStateReader;

public class AppState
{
    public string? RubriqueId { get; set; }
    public string? OldRubriqueId { get; set; }
    public string? FarmId { get; set; }
    public string? PoultryPlannedBatchId { get; set; }
    public string? PoultryAreaId { get; set; }
    public string? GlobalPoultryBatchId { get; set; }
    public string? GlobalPoultryBatchType { get; set; }
    public string? GlobalModuleId { get; set; }

    // Enrichi depuis AppMap.json
    public string? PageNom { get; set; }
    public string? PageCategorie { get; set; }
}

public class JsStateReader
{
    private readonly AndroidDriver _driver;
    private static JsonNode? _appMap;

    public JsStateReader(AndroidDriver driver)
    {
        _driver = driver;
        LoadAppMap();
    }

    // =========================================================================
    // APPMAP — chargement unique au démarrage
    // =========================================================================
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
                SmartConsole.Log("W", "JsStateReader", $"AppMap.json introuvable à : {path}");
                return;
            }

            string json = File.ReadAllText(path);
            _appMap = JsonNode.Parse(json);
            SmartConsole.Log("I", "JsStateReader", "AppMap.json chargé.");
        }
        catch (Exception ex)
        {
            SmartConsole.Log("E", "JsStateReader", $"Erreur chargement AppMap : {ex.Message}");
        }
    }

    // =========================================================================
    // ENRICHISSEMENT — résout le nom et la catégorie depuis l'AppMap
    // =========================================================================
    private static (string? nom, string? categorie) ResolveFromAppMap(string? rubriqueId)
    {
        if (_appMap == null || string.IsNullOrEmpty(rubriqueId))
            return (null, null);

        try
        {
            var page = _appMap["pages"]?[rubriqueId];
            if (page == null) return (null, null);

            return (
                page["nom"]?.GetValue<string>(),
                page["categorie"]?.GetValue<string>()
            );
        }
        catch
        {
            return (null, null);
        }
    }

    // =========================================================================
    // READ — lecture complète de l'état courant
    // =========================================================================
    public AppState ReadCurrentState()
    {
        SmartConsole.Log("I", "JsStateReader", "Lecture de l'état courant de la page...");

        var js = (IJavaScriptExecutor)_driver;

        try
        {
            string script = @"
                function readField(name) {
                    var el = document.querySelector('input[name=""' + name + '""]');
                    return el ? el.value : null;
                }
                function readGlobal(name) {
                    try {
                        var v = eval(name);
                        return (v !== null && v !== undefined) ? String(v) : null;
                    } catch(e) { return null; }
                }
                return JSON.stringify({
                    RubriqueId:             readField('rubriqueid'),
                    OldRubriqueId:          readField('oldrubriqueid'),
                    FarmId:                 readField('farmId'),
                    PoultryPlannedBatchId:  readField('poultryPlannedBatchId'),
                    PoultryAreaId:          readField('poultryAreaId'),
                    GlobalPoultryBatchId:   readGlobal('globalPoultryBatchId'),
                    GlobalPoultryBatchType: readGlobal('globalPoultryBatchType'),
                    GlobalModuleId:         readGlobal('globalModuleId')
                });
            ";

            string json = (string)js.ExecuteScript(script);
            var state = JsonSerializer.Deserialize<AppState>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new AppState();

            // Enrichissement depuis AppMap
            var (nom, categorie) = ResolveFromAppMap(state.OldRubriqueId);
            state.PageNom = nom;
            state.PageCategorie = categorie;

            SmartConsole.Log("I", "JsStateReader",
                $"Page           : [{state.OldRubriqueId}] {state.PageNom ?? "inconnue"} ({state.PageCategorie ?? "?"})");
            SmartConsole.Log("I", "JsStateReader", $"FarmId         : {state.FarmId ?? "null"}");
            SmartConsole.Log("I", "JsStateReader", $"BatchId        : {state.PoultryPlannedBatchId ?? "null"}");
            SmartConsole.Log("I", "JsStateReader", $"GlobalBatchId  : {state.GlobalPoultryBatchId ?? "null"}");
            SmartConsole.Log("I", "JsStateReader", $"GlobalBatchType: {state.GlobalPoultryBatchType ?? "null"}");
            SmartConsole.Log("I", "JsStateReader", $"GlobalModuleId : {state.GlobalModuleId ?? "null"}");

            return state;
        }
        catch (Exception ex)
        {
            SmartConsole.Log("E", "JsStateReader", $"Erreur lecture état : {ex.Message}");
            return new AppState();
        }
    }

    // =========================================================================
    // IsOnPage — vérifie qu'on est sur la bonne page
    // =========================================================================
    public bool IsOnPage(string rubriqueId)
    {
        var state = ReadCurrentState();
        bool ok = state.OldRubriqueId == rubriqueId;

        if (ok)
            SmartConsole.Log("I", "JsStateReader",
                $"✅ Page confirmée : [{rubriqueId}] {state.PageNom ?? ""}");
        else
            SmartConsole.Log("W", "JsStateReader",
                $"❌ Page attendue [{rubriqueId}] mais on est sur [{state.OldRubriqueId}] {state.PageNom ?? ""}");

        return ok;
    }

    // =========================================================================
    // GetPageName — retourne le nom lisible de la page courante
    // =========================================================================
    public string GetCurrentPageName()
    {
        var state = ReadCurrentState();
        return state.PageNom ?? state.OldRubriqueId ?? "inconnue";
    }
}