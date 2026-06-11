using System;
using System.IO;
using System.Text.Json;
using OpenQA.Selenium;
using TestICA.Core.Config.Models;

namespace TestICA.Core.ScriptExecutor;

public class JsExecutor
{
    public string pathScript = "../ScriptJS";
    public IWebDriver? configDriver { get; set; }
    
    public object? ExecuteRaw(string scriptContent, IWebElement? element = null)
    {
        checkConf();

        if (configDriver is not IJavaScriptExecutor jsDriver)
            throw new Exception("Le driver ne supporte pas JavaScript.");

        if (element != null)
            return jsDriver.ExecuteScript(scriptContent, element);

        return jsDriver.ExecuteScript(scriptContent);
    }

    public void checkConf()
    {
        if (pathScript == null)
        {
            throw new Exception("Vérifier le chemin Vers le Script!");
        }

        if (configDriver == null)
        {
            throw new Exception("Configuration de driver invalide (le IWebDriver est null).");
        }
    }

    public object? Execute(Enum.JsScriptType scriptType)
    {
        // 1. Vérification de la configuration
        checkConf();

        // 2. Construction dynamique du chemin du fichier
        string fileName = scriptType.ToString() + ".js";
        string fullpath = Path.Combine(pathScript, fileName);

        if (!File.Exists(fullpath))
        {
            throw new FileNotFoundException($"Le fichier script JS est introuvable : {fullpath}");
        }

        // 3. Lecture du script
        string scriptContent = File.ReadAllText(fullpath);

        // 4. Vérification de la compatibilité JavaScript du Driver
        if (configDriver is not IJavaScriptExecutor jsDriver)
        {
            throw new Exception("Le driver fourni ne supporte pas l'exécution de scripts JavaScript.");
        }

        // =========================================================================
        // 5. ⚡ LE FIX GÉNÉRAL PROPRE : Aiguillage automatique et sécurisé du Return
        // =========================================================================
        string scriptExecuteconforme = scriptContent;

        if (scriptContent.Contains("window.jsResult"))
        {
            // Si le script utilise notre nouvelle norme propre, on append le return à la fin
            scriptExecuteconforme = scriptContent + "\nreturn window.jsResult;";
        }
        else if (!scriptContent.Trim().StartsWith("return") && !scriptContent.Contains("return "))
        {
            // Fallback pour tes anciens scripts très courts d'une seule ligne sans return
            scriptExecuteconforme = "return " + scriptContent;
        }
        
        

        // Execution finale garantie sans NullRefSyntax
        string? jsonResult = jsDriver.ExecuteScript(scriptExecuteconforme) as string;

        if (string.IsNullOrEmpty(jsonResult))
        {
            return null;
        }

        // 6. Désérialisation automatique vers le modèle
        return scriptType switch
        {
            Enum.JsScriptType.PageAnalyser => JsonSerializer.Deserialize<PageAnalyserModel>(jsonResult, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            }),

            Enum.JsScriptType.InspecteurButton => jsonResult, 
            Enum.JsScriptType.InspecteurPopup  => jsonResult,
            Enum.JsScriptType.InspecteurForm   => jsonResult,
            Enum.JsScriptType.CartoMapAnalyser => jsonResult, 

            _ => throw new ArgumentException("Type de script non géré.")
        };
    }
}