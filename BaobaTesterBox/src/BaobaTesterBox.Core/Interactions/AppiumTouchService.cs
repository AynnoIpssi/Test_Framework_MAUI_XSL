using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using BaobaTesterBox.Core.Driver;
using BaobaTesterBox.Core.Loggings;

namespace BaobaTesterBox.Core.Tools;

public class AppiumTouchService
{
    private const string Origine = "AppiumTouchService";

    public void SignerDansCanvas(string cssSelector)
    {
        var driver = new AppiumDriverService().Driver;

        if (driver == null)
        {
            throw new Exception("[AppiumTouchService] Driver Appium non initialisé.");
        }

        AppiumLoggerService.LogDebug(
            $"[Signature] Début signature sur : {cssSelector}",
            Origine
        );

        try
        {
            var canvas = driver.FindElement(By.CssSelector(cssSelector));

            if (canvas == null)
                throw new Exception("Canvas introuvable");

            // 🔥 1. S'assurer que l'élément est visible à l'écran
            ((IJavaScriptExecutor)driver).ExecuteScript(
                "arguments[0].scrollIntoView({block:'center', inline:'center'});",
                canvas
            );

            System.Threading.Thread.Sleep(300);

            // 🔥 2. Actions Appium fiables (PAS de coordinates brutes)
            var actions = new Actions(driver);

            actions
                .MoveToElement(canvas, 20, 20)
                .ClickAndHold()
                .MoveByOffset(30, -10)
                .MoveByOffset(60, 15)
                .MoveByOffset(90, -5)
                .MoveByOffset(120, 10)
                .Release()
                .Perform();

            System.Threading.Thread.Sleep(300);

            AppiumLoggerService.LogInfo(
                "Signature effectuée avec succès (Appium Actions).",
                Origine
            );
        }
        catch (Exception ex)
        {
            AppiumLoggerService.LogError(
                $"Erreur signature canvas : {ex.Message}",
                Origine
            );

            throw;
        }
    }
}