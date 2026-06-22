using System;
using BaobaTesterBox.Core.ScriptExecutor;
using BaobaTesterBox.Core.ScriptExecutor.Enum;
using BaobaTesterBox.Core.Loggings;

namespace BaobaTesterBox.Core.Interactions;

public class AppiumAlertHandler
{
    private readonly AppiumJsExecutor _jsExecutor;
    private const string Origine = "AppiumAlertHandler";

    public AppiumAlertHandler()
    {
        _jsExecutor = new AppiumJsExecutor();
    }

    /// <summary>
    /// Récupère le texte affiché dans la modale d'alerte active. Renvoie une chaîne vide si aucune alerte.
    /// </summary>
    public string GetAlertText()
    {
        try
        {
            var result = _jsExecutor.Execute(JsScriptType.AppiumGetAlertText);
            return result?.ToString() ?? string.Empty;
        }
        catch (Exception ex)
        {
            AppiumLoggerService.LogError($"Erreur lors de la récupération du texte de l'alerte : {ex.Message}", Origine);
            return string.Empty;
        }
    }

    /// <summary>
    /// Clique sur le bouton de fermeture/validation de la modale d'alerte.
    /// </summary>
    public bool AcceptAlert()
    {
        AppiumLoggerService.LogDebug("[Alert] Tentative d'acceptation de l'alerte pop-up", Origine);
        try
        {
            var result = _jsExecutor.Execute(JsScriptType.AppiumCloseAlert);
            if (result is bool success && success)
            {
                System.Threading.Thread.Sleep(300); // Laisse le temps à l'animation de fermeture
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            AppiumLoggerService.LogError($"Erreur lors de la fermeture de l'alerte : {ex.Message}", Origine);
            return false;
        }
    }
    
    /// <summary>
    /// Interroge directement getNoReadingDates() de la page Fiche ICA.
    /// Renvoie une chaîne vide si tous les relevés de mortalité sont complets.
    /// </summary>
    public string GetMissingReadingMessage()
    {
        try
        {
            var result = _jsExecutor.Execute(JsScriptType.AppiumCheckIcaReadingMessage);
            return result?.ToString() ?? string.Empty;
        }
        catch (Exception ex)
        {
            AppiumLoggerService.LogError($"Erreur lors de la vérification des relevés de mortalité : {ex.Message}", Origine);
            return string.Empty;
        }
    }
}