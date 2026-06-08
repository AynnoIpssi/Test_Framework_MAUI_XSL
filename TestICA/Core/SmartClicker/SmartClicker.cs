using System;
using System.Collections.Generic;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Interactions;
using Serilog;
using TestICA.Core.ScriptExecutor;

namespace TestICA.Core.SmartClickerService;

public class SmartClicker
{
    private readonly AndroidDriver _driver;
    private readonly JsExecutor _js;

    public SmartClicker(AndroidDriver driver)
    {
        _driver = driver;
        _js = new JsExecutor { configDriver = driver };
    }

    public void Click(IWebElement element, string debugId = "?")
    {
        // Stratégie 1 — JS element.click() (le plus fiable en WebView)
        if (TryJsClick(element, debugId)) return;

        // Stratégie 2 — JS MouseEvent dispatch
        if (TryJsDispatchClick(element, debugId)) return;

        // Stratégie 3 — Tap par coordonnées
        if (TryTapByCoordinates(element, debugId)) return;

        // Stratégie 4 — Selenium fallback
        Log.Warning($"[SmartClicker] Fallback Selenium sur '{debugId}'.");
        element.Click();
    }

    private bool TryJsClick(IWebElement element, string debugId)
    {
        try
        {
            _js.ExecuteRaw("arguments[0].click();", element);
            Log.Information($"[SmartClicker] ✅ JS click() réussi sur '{debugId}'.");
            return true;
        }
        catch (Exception ex)
        {
            Log.Warning($"[SmartClicker] JS click() échoué sur '{debugId}' : {ex.Message}");
            return false;
        }
    }

    private bool TryJsDispatchClick(IWebElement element, string debugId)
    {
        try
        {
            _js.ExecuteRaw(
                "arguments[0].dispatchEvent(new MouseEvent('click', {bubbles: true, cancelable: true}));",
                element);
            Log.Information($"[SmartClicker] ✅ JS dispatchEvent réussi sur '{debugId}'.");
            return true;
        }
        catch (Exception ex)
        {
            Log.Warning($"[SmartClicker] JS dispatchEvent échoué sur '{debugId}' : {ex.Message}");
            return false;
        }
    }

    private bool TryTapByCoordinates(IWebElement element, string debugId)
    {
        try
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
            Log.Information($"[SmartClicker] ✅ Tap coordonnées réussi sur '{debugId}' [{centerX},{centerY}].");
            return true;
        }
        catch (Exception ex)
        {
            Log.Warning($"[SmartClicker] Tap coordonnées échoué sur '{debugId}' : {ex.Message}");
            return false;
        }
    }
}