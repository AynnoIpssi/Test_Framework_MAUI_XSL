// Fichier : Tests/BaobaTesterBox.Tests/SanityChecks/Login/DiagnosticPageTests.cs
using System;
using System.Collections.Generic;
using NUnit.Framework;
using OpenQA.Selenium;
using BaobaTesterBox.Core.Loggings;
using BaobaTesterBox.Core.Locators;
using BaobaTesterBox.Domain.Configuration;
using BaobaTesterBox.Tests.Infrastructure;

namespace BaobaTesterBox.Tests.SanityChecks.Login;

[TestFixture]
public class DiagnosticPageTests : BaseTest
{
    private const string Origine = "DiagnosticDOM";

    [Test]
    public void Tester_Et_Analyser_Tous_Les_Inputs_De_La_Page()
    {
        AppiumLoggerService.LogInfo("=== DÉBUT DU DIAGNOSTIC DOM ===", Origine);

        var driver = AppiumService.Driver;
        
        // 1. Connexion à la WebView et Init du Scanner avec le bon Driver
        AppiumService.BasculeVersWebView();
        AppiumElementScanner.Init(driver);

        // 2. Récupération des inputs via notre infrastructure (uniquement les visibles détectés)
        AppiumLoggerService.LogInfo("Lancement de la détection de notre script (Filtre Visibles)...", Origine);
        List<IWebElement> inputsDetectesParScript = AppiumElementScanner.GetVisibleInputsOnly();
        
        AppiumLoggerService.LogInfo($"[RESULTAT SCRIPT] : {inputsDetectesParScript.Count} input(s) détecté(s) comme visible(s).", Origine);

        // 3. Extraction brute de TOUS les inputs de la page (sans aucun filtre) via WebDriver standard
        AppiumLoggerService.LogInfo("Extraction brute de TOUS les éléments <input> de la page...", Origine);
        var tousLesInputsDuDom = driver.FindElements(By.TagName("input"));

        AppiumLoggerService.LogInfo($"=== RAPPORT DÉTAILLÉ DES {tousLesInputsDuDom.Count} INPUTS TROUVÉS ===", Origine);
        
        int index = 0;
        foreach (var input in tousLesInputsDuDom)
        {
            string id = input.GetAttribute("id") ?? "N/A";
            string name = input.GetAttribute("name") ?? "N/A";
            string type = input.GetAttribute("type") ?? "N/A";
            string placeholder = input.GetAttribute("placeholder") ?? "N/A";
            
            // Propriétés de visibilité selon Appium
            bool estAfficheAppium = input.Displayed;
            bool estActiveAppium = input.Enabled;

            // Analyse de sa géométrie basique via le driver
            var size = input.Size;
            bool aUneTailleValide = size.Width > 0 && size.Height > 0;

            AppiumLoggerService.LogInfo(
                $"Input #{index} | ID: {id} | Name: {name} | Type: {type} | Placeholder: {placeholder}\n" +
                $"   -> [Appium Displayed]: {estAfficheAppium} | [Enabled]: {estActiveAppium}\n" +
                $"   -> [Taille HTML]: {size.Width}x{size.Height} (Valide: {aUneTailleValide})", 
                Origine
            );
            
            index++;
        }

        AppiumLoggerService.LogInfo("=== FIN DU DIAGNOSTIC DOM ===", Origine);
        
        // On ne met pas d'Assert.Fail exprès pour que tu puisses voir les logs même si le compte est à 0 !
        Assert.Pass("Diagnostic terminé. Regarde les logs de la console pour analyser la structure.");
    }
}