using BaobaTesterBox.Tests.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;
using BaobaTesterBox.Core.Interactions;
using BaobaTesterBox.Core.Loggings;
using BaobaTesterBox.Core.Locators;
using BaobaTesterBox.Core.Navigation;
using BaobaTesterBox.Core.Tools;
using BaobaTesterBox.Domain.Models;
using BaobaTesterBox.Tests.Infrastructure;

namespace TestICA.Tests.FicheIca_542_;

public class testBtn : BaseTest
{
    [Test]
    public void Test_Click_Save_Popin_With_Method_Detection()
    {
        AppiumLoggerService.LogInfo("=== TEST CLICK SAVE POPIN (METHOD DETECTION) ===", "SaveButtonTest");

        AppiumService.BasculeVersWebView();
        Thread.Sleep(2000);

        var btn = Driver.FindElement(By.Id("btn-save-popin"));

        ((IJavaScriptExecutor)Driver)
            .ExecuteScript("arguments[0].scrollIntoView({block:'center'});", btn);

        Thread.Sleep(500);

        var methods = new List<(string Name, Action Action)>
        {
            ("Selenium.Click", () => btn.Click()),

            ("JS.Click", () =>
                ((IJavaScriptExecutor)Driver)
                .ExecuteScript("arguments[0].click();", btn)),

            ("JS.DispatchEvent", () =>
                ((IJavaScriptExecutor)Driver)
                .ExecuteScript(@"
                    arguments[0].dispatchEvent(
                        new MouseEvent('click', {
                            bubbles: true,
                            cancelable: true,
                            view: window
                        })
                    );
                ", btn))
        };

        string usedMethod = null;

        foreach (var method in methods)
        {
            try
            {
                method.Action();

                Thread.Sleep(800);

                AppiumLoggerService.LogInfo(
                    $"Méthode testée: {method.Name} → OK",
                    "SaveButtonTest");

                usedMethod = method.Name;
                break;
            }
            catch (Exception ex)
            {
                AppiumLoggerService.LogWarn(
                    $"Méthode testée: {method.Name} → FAIL ({ex.Message})",
                    "SaveButtonTest");
            }
        }

        Assert.That(usedMethod, Is.Not.Null, "Aucune méthode de clic n'a fonctionné");

        AppiumLoggerService.LogInfo(
            $"🎯 Méthode retenue pour click save: {usedMethod}",
            "SaveButtonTest");
    }
}