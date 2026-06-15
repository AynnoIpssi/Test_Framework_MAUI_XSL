// Fichier : src/BaobaTesterBox.Core/Interactions/AppiumElementActionsService.cs
using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Support.UI;
using BaobaTesterBox.Core.Loggings;
using BaobaTesterBox.Core.Locators;
using BaobaTesterBox.Core.ScriptExecutor;

namespace BaobaTesterBox.Core.Interactions;

/// <summary>
/// Boîte à outils d'actions sur la WebView : clic sécurisé et saisie de champ,
/// avec secours JavaScript en cas de comportement capricieux du moteur XSL/Bootstrap.
/// </summary>
public class AppiumElementActionsService
{
    private const string Origine = "AppiumElementActionsService";

    private readonly AndroidDriver _driver;
    private readonly WebDriverWait _wait;
    private readonly AppiumClickerService _clicker; // TODO: ex-SmartClicker, à porter
    private static readonly AppiumJsExecutor _jsExecutor = new();

    public AppiumElementActionsService(AndroidDriver driver)
    {
        _driver = driver;
        _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(15));
        _clicker = new AppiumClickerService(_driver);
    }

    /// <summary>
    /// Récupère l'élément depuis le cache du AppiumElementScanner (rempli par AnalyzeCurrentPage()).
    /// </summary>
    protected IWebElement FindById(string id)
    {
        return AppiumElementScanner.GetElement(id);
    }

    /// <summary>
    /// Clique sur un élément (avec secours JavaScript si Appium est bloqué).
    /// </summary>
    public void Click(string id)
    {    
        var element = FindById(id);
        _clicker.Click(element, id);
    }

    /// <summary>
    /// Remplit un champ de texte (avec secours JS pour contrer les caprices du moteur XSL).
    /// </summary>
    public void FillField(string id, string text)
    {
        var element = FindById(id);

        try
        {
            _wait.Until(d => element.Displayed && element.Enabled);
            element.Clear();
            element.SendKeys(text);
            AppiumLoggerService.LogInfo($"Texte saisi de manière standard dans '{id}'.", Origine);
        }
        catch (Exception)
        {
            AppiumLoggerService.LogWarn($"Saisie standard impossible sur '{id}' (effet de style Bootstrap/XSL). Passage en force JS...", Origine);

            var js = (IJavaScriptExecutor)_driver;
            const string jsScript = @"
                arguments[0].focus(); 
                arguments[0].value = arguments[1]; 
                arguments[0].dispatchEvent(new Event('input', { bubbles: true })); 
                arguments[0].dispatchEvent(new Event('change', { bubbles: true }));";

            js.ExecuteScript(jsScript, element, text);
            AppiumLoggerService.LogInfo($"Valeur injectée avec succès via JS dans '{id}'.", Origine);
        }
    }
    
    /// <summary>
    /// Remplit tous les inputs visibles de la page dans l'ordre exact du DOM via script externe.
    /// </summary>
    /// <param name="saisies">Tableau ordonné des valeurs à saisir (ex: "monEmail", "monPassword")</param>
    public void FillFieldsInOrder(params string[] saisies)
    {
        if (saisies == null || saisies.Length == 0)
        {
            AppiumLoggerService.LogWarn("Aucune valeur fournie pour la saisie ordonnée.", Origine);
            return;
        }

        try
        {
            AppiumLoggerService.LogInfo($"Lancement de la saisie automatique pour {saisies.Length} champs...", Origine);

            // Appel de l'exécuteur en lui passant le tableau de strings dans les arguments du script
            var resultat = _jsExecutor.Execute(BaobaTesterBox.Core.ScriptExecutor.Enum.JsScriptType.AppiumInputFillSequentially, saisies);

            AppiumLoggerService.LogInfo($"Saisie ordonnée terminée. Statut : {resultat}", Origine);
        }
        catch (Exception ex)
        {
            AppiumLoggerService.LogError($"Échec de la saisie globale en ordre. Erreur : {ex.Message}", Origine);
            throw;
        }
    }
    
    /// <summary>
    /// Force le focus et le clic sur un élément du DOM via son sélecteur CSS à l'aide d'un script externe.
    /// </summary>
    /// <param name="cssSelector">Le sélecteur CSS du bouton ou de l'élément à cliquer (ex: "#btnValidateSynchro")</param>
    public void ClickOnElement(string cssSelector)
    {
        if (string.IsNullOrEmpty(cssSelector))
        {
            AppiumLoggerService.LogWarn("Sélecteur CSS invalide ou vide fourni pour le clic.", Origine);
            return;
        }

        try
        {
            AppiumLoggerService.LogInfo($"Déclenchement du clic JS sur l'élément : '{cssSelector}'", Origine);
            
            // Exécution du script via l'instance statique _jsExecutor déjà présente dans ta classe
            var resultat = _jsExecutor.Execute(BaobaTesterBox.Core.ScriptExecutor.Enum.JsScriptType.AppiumClickElement, cssSelector) as string;

            AppiumLoggerService.LogInfo($"Résultat du clic : {resultat}", Origine);

            if (resultat == "ERR_ELEMENT_NOT_FOUND")
            {
                throw new Exception($"L'élément cible '{cssSelector}' n'existe pas dans le DOM actuel.");
            }
        }
        catch (Exception ex)
        {
            AppiumLoggerService.LogError($"Échec critique lors du clic sur '{cssSelector}' : {ex.Message}", Origine);
            throw;
        }
    }
}