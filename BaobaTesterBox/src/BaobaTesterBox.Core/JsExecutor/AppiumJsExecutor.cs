// Fichier : src/BaobaTesterBox.Core/JsExecutor/AppiumJsExecutor.cs
using System;
using System.IO;
using OpenQA.Selenium;
using BaobaTesterBox.Core.Driver;
using BaobaTesterBox.Core.Loggings;
using System.Linq; // à ajouter en haut

namespace BaobaTesterBox.Core.ScriptExecutor;

public class AppiumJsExecutor
{
    private static readonly string LogOrigin = "AppiumJsExecutor";
    
    // 🟢 CORRECTION : On s'arrête au dossier "ScriptJS" sans mettre de fichier en dur !
    private static readonly string BaseScriptFolder = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, 
        "src", 
        "BaobaTesterBox.Core", 
        "JsExecutor", 
        "ScriptJS"
    );

    public object? Execute(Enum.JsScriptType scriptType, params object[] args)
    {
        // 1. Récupération du driver Appium actif
        var driver = new AppiumDriverService().Driver;
        if (driver == null)
        {
            throw new Exception("[AppiumJsExecutor] Le driver Appium n'est pas initialisé.");
        }

        // 2. Construction dynamique et PROPRE du chemin du fichier .js
        string fileName = scriptType.ToString() + ".js";
        string fullpath = Path.Combine(BaseScriptFolder, fileName); // 🟢 Plus de doublon ici !

        if (!File.Exists(fullpath))
        {
            throw new FileNotFoundException($"[AppiumJsExecutor] Le fichier script JS est introuvable : {fullpath}");
        }

        // 3. Lecture du fichier
        string scriptContent = File.ReadAllText(fullpath);

        // 4. Sécurité d'injection du "return" (Ton fix d'origine conservé)
        string scriptExecuteconforme = scriptContent;

        var corpsReel = string.Join("\n", scriptContent
                .Split('\n')
                .SkipWhile(l => l.TrimStart().StartsWith("//")))
            .TrimStart();

        if (scriptContent.Contains("window.jsResult"))
        {
            scriptExecuteconforme = scriptContent + "\nreturn window.jsResult;";
        }
        else if (corpsReel.StartsWith("return"))
        {
            // Déjà un vrai return en tête (scripts de navigation par ex.) — rien à faire
        }
        else if (corpsReel.StartsWith("(function") || corpsReel.StartsWith("(async"))
        {
            // IIFE : son return interne ne ressort jamais sans return explicite devant l'appel
            scriptExecuteconforme = "return " + scriptContent;
        }
        else if (!scriptContent.Contains("return "))
        {
            scriptExecuteconforme = "return " + scriptContent;
        }

        AppiumLoggerService.LogDebug($"Exécution du script JS : {fileName} avec {args.Length} argument(s)", LogOrigin);

        // 5. L'aiguillage propre : on exécute selon le besoin du script
        return scriptType switch
        {
            Enum.JsScriptType.AppiumInputFetchVisible => driver.ExecuteScript(scriptExecuteconforme, args),
            Enum.JsScriptType.AppiumInputFillSequentially => driver.ExecuteScript(scriptExecuteconforme, args),
            Enum.JsScriptType.AppiumClickElement => driver.ExecuteScript(scriptExecuteconforme, args),
            Enum.JsScriptType.AppiumNavigationGo => driver.ExecuteScript(scriptExecuteconforme, args),
            Enum.JsScriptType.AppiumNavigationModuleGo => driver.ExecuteScript(scriptExecuteconforme, args),
            Enum.JsScriptType.AppiumScrollToElement => driver.ExecuteScript(scriptExecuteconforme, args),
            Enum.JsScriptType.AppiumGetAlertText => driver.ExecuteScript(scriptExecuteconforme, args),
            Enum.JsScriptType.AppiumCloseAlert => driver.ExecuteScript(scriptExecuteconforme, args),
            Enum.JsScriptType.AppiumCheckIcaReadingMessage => driver.ExecuteScript(scriptExecuteconforme, args),
            Enum.JsScriptType.FeedFicheIca => driver.ExecuteScript(scriptExecuteconforme, args),
            Enum.JsScriptType.AppiumFormFieldsFetchVisible => driver.ExecuteScript(scriptExecuteconforme, args),
            Enum.JsScriptType.AppiumFillFormFields => driver.ExecuteScript(scriptExecuteconforme, args),
            Enum.JsScriptType.AppiumSingature => driver.ExecuteScript(scriptExecuteconforme, args),

            _ => throw new ArgumentException($"[AppiumJsExecutor] Type de script non géré : {scriptType}")
        };
    }
    
    
    
    
}