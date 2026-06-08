using System;
using System.Collections.Generic;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Support.UI;
using Serilog;
using TestICA.Core.SmartLocator;

namespace TestICA.Core;

public class PageFunction
{
    protected readonly AndroidDriver _driver;
    protected readonly WebDriverWait _wait;

    public PageFunction(AndroidDriver driver)
    {
        _driver = driver;
        _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(15));
    }

    /// <summary>
    /// Récupère l'élément directement depuis le cache du SmartLocator
    /// </summary>
    protected IWebElement FindById(string id)
    {
        // On demande l'élément au SmartLocator
        var element = SmartLocator.SmartLocator.GetElement(id);

        if (element == null)
        {
            throw new NoSuchElementException($"[PageFunction] L'ID '{id}' est introuvable dans le cache du SmartLocator. As-tu pensé à lancer AnalyzeCurrentPage() ?");
        }

        Log.Information($"[PageFunction] Élément '{id}' récupéré depuis le SmartLocator.");
        return element;
    }

    /// <summary>
    /// Clique sur un élément (avec secours JavaScript si Appium est bloqué)
    /// </summary>
    public void Click(string id)
    {
        var element = FindById(id);

        try
        {
            Log.Information($"[PageFunction] Tentative de clic physique par coordonnées (W3C Actions) sur '{id}'...");
        
            // 1. Récupérer la position exacte du bouton à l'écran
            var location = element.Location;
            var size = element.Size;
        
            // Calcul du centre exact du bouton
            int centerX = location.X + (size.Width / 2);
            int centerY = location.Y + (size.Height / 2);

            // 2. Simuler un vrai appui de doigt (Pointer Spec)
            var inputSource = new OpenQA.Selenium.Interactions.PointerInputDevice(OpenQA.Selenium.Interactions.PointerKind.Touch, "finger");
            var sequence = new OpenQA.Selenium.Interactions.ActionSequence(inputSource, 0);

            // Déplacer le doigt sur le bouton -> Presser -> Relâcher
            sequence.AddAction(inputSource.CreatePointerMove(OpenQA.Selenium.Interactions.CoordinateOrigin.Viewport, centerX, centerY, TimeSpan.Zero));
            sequence.AddAction(inputSource.CreatePointerDown(OpenQA.Selenium.Interactions.MouseButton.Left));
            sequence.AddAction(inputSource.CreatePointerUp(OpenQA.Selenium.Interactions.MouseButton.Left));

            // Exécuter l'action sur le périphérique
            ((IActionExecutor)_driver).PerformActions(new List<OpenQA.Selenium.Interactions.ActionSequence> { sequence });
        
            Log.Information($"[PageFunction] ✅ Clic par coordonnées réussi au point [{centerX}, {centerY}].");
        }
        catch (Exception ex)
        {
            Log.Error($"[PageFunction] ❌ Le clic par coordonnées a échoué : {ex.Message}");
        
            // Secours ultime si les actions plantent (on retente le clic standard)
            element.Click();
        }
    }

    /// <summary>
    /// Remplit un champ de texte (avec secours JS pour contrer les caprices du moteur XSL)
    /// </summary>
    public void FillField(string id, string text)
    {
        var element = FindById(id);
        
        try
        {
            // Attente standard que le champ soit exploitable
            _wait.Until(d => element.Displayed && element.Enabled);
            element.Clear();
            element.SendKeys(text);
            Log.Information($"[PageFunction] Texte saisi de manière standard dans '{id}'.");
        }
        catch (Exception)
        {
            Log.Warning($"[PageFunction] Saisie standard contrariée sur '{id}' (effet d'optique ou de style Bootstrap). Passage en force JS...");
            
            var js = (IJavaScriptExecutor)_driver;
            // Injection directe de la valeur suivie du déclenchement des événements d'écoute pour que le moteur XSL valide l'entrée
            string jsScript = @"
                arguments[0].focus(); 
                arguments[0].value = arguments[1]; 
                arguments[0].dispatchEvent(new Event('input', { bubbles: true })); 
                arguments[0].dispatchEvent(new Event('change', { bubbles: true }));";
            
            js.ExecuteScript(jsScript, element, text);
            Log.Information($"[PageFunction] Valeur injectée avec succès via JS dans '{id}'.");
        }
    }
}