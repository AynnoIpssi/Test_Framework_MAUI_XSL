using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Android;
using TestICA.Core.SmartLocator;

namespace TestICA.Core.SmartWaiter;

public class SmartWaiter
{
    private readonly AndroidDriver _driver;
    private readonly int _timeoutSeconds;

    public SmartWaiter(AndroidDriver driver, int timeoutSeconds = 30)
    {
        _driver = driver;
        _timeoutSeconds = timeoutSeconds;
    }

    /// <summary>
    /// Attend qu'une variable JS globale soit non-null et non-undefined
    /// Ex: WaitForJsVariable("globalPoultryBatchId")
    /// </summary>
    public bool WaitForJsVariable(string variableName)
    {
        SmartConsole.Log("I", "SmartWaiter", $"Attente de la variable JS '{variableName}'...");

        var js = (IJavaScriptExecutor)_driver;
        var deadline = DateTime.Now.AddSeconds(_timeoutSeconds);

        while (DateTime.Now < deadline)
        {
            try
            {
                var result = js.ExecuteScript($@"
                    var v = typeof {variableName} !== 'undefined' ? {variableName} : null;
                    return v !== null && v !== undefined && v !== '' ? String(v) : null;
                ");

                if (result != null)
                {
                    SmartConsole.Log("I", "SmartWaiter", $"✅ '{variableName}' détecté : {result}");
                    return true;
                }
            }
            catch (Exception)
            {
                // DOM pas encore prêt, on continue
            }

            System.Threading.Thread.Sleep(500);
        }

        SmartConsole.Log("E", "SmartWaiter", $"❌ Timeout — '{variableName}' jamais apparu après {_timeoutSeconds}s.");
        return false;
    }

    /// <summary>
    /// Attend qu'on soit sur une page spécifique via oldrubriqueid
    /// Ex: WaitForPage("542") attend la Fiche ICA
    /// </summary>
    public bool WaitForPage(string rubriqueId)
    {
        SmartConsole.Log("I", "SmartWaiter", $"Attente de la page rubrique '{rubriqueId}'...");

        var js = (IJavaScriptExecutor)_driver;
        var deadline = DateTime.Now.AddSeconds(_timeoutSeconds);

        while (DateTime.Now < deadline)
        {
            try
            {
                var result = js.ExecuteScript(@"
                    var el = document.querySelector('input[name=""oldrubriqueid""]');
                    return el ? el.value : null;
                ") as string;

                if (result == rubriqueId)
                {
                    SmartConsole.Log("I", "SmartWaiter", $"✅ Page {rubriqueId} chargée.");
                    return true;
                }
            }
            catch (Exception)
            {
                // DOM pas encore prêt
            }

            System.Threading.Thread.Sleep(500);
        }

        SmartConsole.Log("E", "SmartWaiter", $"❌ Timeout — page '{rubriqueId}' jamais atteinte après {_timeoutSeconds}s.");
        return false;
    }

    /// <summary>
    /// Attend qu'un élément DOM existe et soit visible
    /// Ex: WaitForElement("#btnValidateSynchro")
    /// </summary>
    public bool WaitForElement(string cssSelector)
    {
        SmartConsole.Log("I", "SmartWaiter", $"Attente de l'élément '{cssSelector}'...");

        var js = (IJavaScriptExecutor)_driver;
        var deadline = DateTime.Now.AddSeconds(_timeoutSeconds);

        while (DateTime.Now < deadline)
        {
            try
            {
                var result = js.ExecuteScript($@"
                    var el = document.querySelector('{cssSelector}');
                    if (!el) return false;
                    var style = window.getComputedStyle(el);
                    return style.display !== 'none' && style.visibility !== 'hidden' && el.offsetWidth > 0;
                ");

                if (result is true)
                {
                    SmartConsole.Log("I", "SmartWaiter", $"✅ Élément '{cssSelector}' visible.");
                    return true;
                }
            }
            catch (Exception)
            {
                // DOM pas encore prêt
            }

            System.Threading.Thread.Sleep(500);
        }

        SmartConsole.Log("E", "SmartWaiter", $"❌ Timeout — élément '{cssSelector}' jamais visible après {_timeoutSeconds}s.");
        return false;
    }
}