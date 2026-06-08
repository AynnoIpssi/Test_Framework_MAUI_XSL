using System;
using System.Collections.Generic;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Support.UI;

namespace TestICA.Core.SmartDriver;

public class SmartDriver
{
    private readonly AndroidDriver _driver;
    private readonly WebDriverWait _wait;

    public SmartDriver(AndroidDriver driver, int timeoutInSeconds = 15)
    {
        _driver = driver;
        _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeoutInSeconds));
    }
    
    public IWebElement FindSmartElement(By selector)
    {
        _driver.SwitchTo().DefaultContent();

        try
        {
            return _wait.Until(d => d.FindElement(selector));
        }
        catch (WebDriverTimeoutException)
        {
            var iframes = _driver.FindElements(By.TagName("iframe"));
            foreach (var iframe in iframes)
            {
                try
                {
                    _driver.SwitchTo().Frame(iframe);
                    var element = _driver.FindElement(selector);
                    if (element != null) return element;
                }
                catch
                {
                    _driver.SwitchTo().ParentFrame();
                }
            }
            
            throw new NoSuchElementException($"[SmartDriver] Impossible de trouver l'élément avec le sélecteur : {selector} (Même après scan des iFrames et attente de 15s).");
        }
    }
    
    public void SmartClick(By selector)
    {
        var element = FindSmartElement(selector);
        
        _wait.Until(d => element.Displayed && element.Enabled);
        element.Click();
    }
    
    public void SmartType(By selector, string text)
    {
        var element = FindSmartElement(selector);
        _wait.Until(d => element.Displayed && element.Enabled);
        element.Clear();
        element.SendKeys(text);
    }
    
    public AndroidDriver RawDriver => _driver;
}