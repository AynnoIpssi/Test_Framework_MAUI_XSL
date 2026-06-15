// Fichier : src/BaobaTesterBox.Core/Interactions/AppiumClickerService.cs
using System;
using System.Collections.Generic;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Interactions;
using BaobaTesterBox.Core.Loggings;

namespace BaobaTesterBox.Core.Interactions;

/// <summary>
/// Clic robuste sur la WebView : tente plusieurs stratégies en cascade
/// (JS direct, dispatch d'événement, tap par coordonnées, fallback Selenium).
/// </summary>
public class AppiumClickerService
{
    private const string Origine = "AppiumClickerService";

    private readonly AndroidDriver _driver;
    private readonly IJavaScriptExecutor _js;

    public AppiumClickerService(AndroidDriver driver)
    {
        _driver = driver;
        _js = (IJavaScriptExecutor)driver;
    }

    public void Click(IWebElement element, string debugId = "?")
    {
        if (TryStrategy("JS click()", debugId, () =>
                _js.ExecuteScript("arguments[0].click();", element)))
            return;

        if (TryStrategy("JS dispatchEvent(MouseEvent)", debugId, () =>
                _js.ExecuteScript("arguments[0].dispatchEvent(new MouseEvent('click', {bubbles: true, cancelable: true}));", element)))
            return;

        if (TryStrategy("Tap par coordonnées", debugId, () => TapByCoordinates(element)))
            return;

        AppiumLoggerService.LogWarn($"Toutes les stratégies JS/Tap ont échoué sur '{debugId}'. Fallback Selenium.", Origine);
        element.Click();
    }

    /// <summary>
    /// Exécute une stratégie de clic, logue le résultat, et indique si elle a réussi.
    /// </summary>
    private bool TryStrategy(string strategyName, string debugId, Action action)
    {
        try
        {
            action();
            AppiumLoggerService.LogInfo($"Stratégie '{strategyName}' réussie sur '{debugId}'.", Origine);
            return true;
        }
        catch (Exception ex)
        {
            AppiumLoggerService.LogWarn($"Stratégie '{strategyName}' échouée sur '{debugId}' : {ex.Message}", Origine);
            return false;
        }
    }

    private void TapByCoordinates(IWebElement element)
    {
        var location = element.Location;
        var size = element.Size;
        int centerX = location.X + (size.Width / 2);
        int centerY = location.Y + (size.Height / 2);

        var input = new PointerInputDevice(PointerKind.Touch, "finger");
        var sequence = new ActionSequence(input, 0);
        sequence.AddAction(input.CreatePointerMove(CoordinateOrigin.Viewport, centerX, centerY, TimeSpan.Zero));
        sequence.AddAction(input.CreatePointerDown(MouseButton.Left));
        sequence.AddAction(input.CreatePointerUp(MouseButton.Left));

        ((IActionExecutor)_driver).PerformActions(new List<ActionSequence> { sequence });
    }
}