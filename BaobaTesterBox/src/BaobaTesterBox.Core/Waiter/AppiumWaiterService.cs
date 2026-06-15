// Fichier : src/BaobaTesterBox.Core/Waiter/AppiumWaiterService.cs
using System;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Android;
using BaobaTesterBox.Core.Loggings;
using BaobaTesterBox.Domain.Models;

namespace BaobaTesterBox.Core.Waiter;

/// <summary>
/// Attend des conditions JS/DOM sur la WebView de l'application (variables globales,
/// page chargée via rubrique, présence/visibilité d'un élément).
/// </summary>
public class AppiumWaiterService
{
    private const string Origine = "AppiumWaiterService";

    private readonly AndroidDriver _driver;
    private readonly AppiumWaiterOptions _options;

    public AppiumWaiterService(AndroidDriver driver, AppiumWaiterOptions? options = null)
    {
        _driver = driver;
        _options = options ?? new AppiumWaiterOptions();
    }

    /// <summary>
    /// Attend qu'une variable JS globale soit non-null, non-undefined et non vide.
    /// Ex: WaitForJsVariable("globalPoultryBatchId")
    /// </summary>
    public bool WaitForJsVariable(string variableName)
    {
        AppiumLoggerService.LogInfo($"Attente de la variable JS '{variableName}'...", Origine);

        return PollUntil(() =>
        {
            var result = ExecuteScript($@"
                var v = typeof {variableName} !== 'undefined' ? {variableName} : null;
                return v !== null && v !== undefined && v !== '' ? String(v) : null;
            ");

            return result != null;
        },
        onSuccess: () => AppiumLoggerService.LogInfo($"Variable '{variableName}' détectée.", Origine),
        onTimeout: () => AppiumLoggerService.LogError($"Timeout — '{variableName}' jamais apparue après {_options.TimeoutSeconds}s.", Origine));
    }

    /// <summary>
    /// Attend qu'on soit sur une page spécifique via l'attribut oldrubriqueid.
    /// Ex: WaitForPage("542") attend la Fiche ICA
    /// </summary>
    public bool WaitForPage(string rubriqueId)
    {
        AppiumLoggerService.LogInfo($"Attente de la page rubrique '{rubriqueId}'...", Origine);

        return PollUntil(() =>
        {
            var result = ExecuteScript(@"
                var el = document.querySelector('input[name=""oldrubriqueid""]');
                return el ? el.value : null;
            ") as string;

            return result == rubriqueId;
        },
        onSuccess: () => AppiumLoggerService.LogInfo($"Page '{rubriqueId}' chargée.", Origine),
        onTimeout: () => AppiumLoggerService.LogError($"Timeout — page '{rubriqueId}' jamais atteinte après {_options.TimeoutSeconds}s.", Origine));
    }

    /// <summary>
    /// Attend qu'un élément DOM existe et soit visible.
    /// Ex: WaitForElement("#btnValidateSynchro")
    /// </summary>
    public bool WaitForElement(string cssSelector)
    {
        AppiumLoggerService.LogInfo($"Attente de l'élément '{cssSelector}'...", Origine);

        return PollUntil(() =>
        {
            var result = ExecuteScript($@"
                var el = document.querySelector('{cssSelector}');
                if (!el) return false;
                var style = window.getComputedStyle(el);
                return style.display !== 'none' && style.visibility !== 'hidden' && el.offsetWidth > 0;
            ");

            return result is true;
        },
        onSuccess: () => AppiumLoggerService.LogInfo($"Élément '{cssSelector}' visible.", Origine),
        onTimeout: () => AppiumLoggerService.LogError($"Timeout — élément '{cssSelector}' jamais visible après {_options.TimeoutSeconds}s.", Origine));
    }

    /// <summary>
    /// Boucle de polling commune : exécute <paramref name="condition"/> jusqu'à succès ou timeout.
    /// </summary>
    private bool PollUntil(Func<bool> condition, Action onSuccess, Action onTimeout)
    {
        var deadline = DateTime.Now.AddSeconds(_options.TimeoutSeconds);

        while (DateTime.Now < deadline)
        {
            try
            {
                if (condition())
                {
                    onSuccess();
                    return true;
                }
            }
            catch (Exception)
            {
                // DOM pas encore prêt, on continue
            }

            Thread.Sleep(_options.PollingIntervalMs);
        }

        onTimeout();
        return false;
    }
    
    /// <summary>
    /// Attend qu'un champ de saisie ou un élément spécifique soit prêt et visible dans le DOM.
    /// </summary>
    /// <param name="cssSelector">Le sélecteur CSS de l'input attendu (ex: "#login", "input[type='email']")</param>
    public bool WaitForInputVisible(string cssSelector)
    {
        if (string.IsNullOrEmpty(cssSelector))
        {
            AppiumLoggerService.LogWarn("Sélecteur CSS invalide fourni pour l'attente de l'input.", Origine);
            return false;
        }

        AppiumLoggerService.LogInfo($"Attente de la visibilité de l'input cible '{cssSelector}'...", Origine);
    
        // On réutilise ta logique de polling native
        return WaitForElement(cssSelector);
    }

    private object ExecuteScript(string script)
    {
        return ((IJavaScriptExecutor)_driver).ExecuteScript(script);
    }
}