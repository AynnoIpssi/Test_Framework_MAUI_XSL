using System;
using BaobaTesterBox.Core.ScriptExecutor;
using BaobaTesterBox.Core.ScriptExecutor.Enum;
using BaobaTesterBox.Core.Loggings;

namespace BaobaTesterBox.Core.Tools;

public class AppiumScrollService
{
    private readonly AppiumJsExecutor _jsExecutor;
    private const string Origine = "AppiumScrollService";

    public AppiumScrollService()
    {
        _jsExecutor = new AppiumJsExecutor();
    }

    /// <summary>
    /// Fait défiler la WebView jusqu'à ce que l'élément cible soit parfaitement au centre de l'écran.
    /// </summary>
    /// <param name="cssSelector">Le sélecteur CSS de l'élément (ex: "#btn-save-popin")</param>
    public void ScrollToElement(string cssSelector)
    {
        AppiumLoggerService.LogDebug($"[Scroll] Demande de défilement vers : {cssSelector}", Origine);

        try
        {
            // Appel via l'architecture officielle V2
            var result = _jsExecutor.Execute(JsScriptType.AppiumScrollToElement, cssSelector);

            if (result is bool success && !success)
            {
                AppiumLoggerService.LogWarn($"Impossible de scroller : l'élément '{cssSelector}' n'a pas été trouvé dans le DOM.", Origine);
            }
            else
            {
                // Petite pause pour laisser le temps au défilement fluide de se terminer sur l'écran
                System.Threading.Thread.Sleep(500);
            }
        }
        catch (Exception ex)
        {
            AppiumLoggerService.LogError($"Erreur lors de l'exécution du scroll vers {cssSelector} : {ex.Message}", Origine);
        }
    }
}