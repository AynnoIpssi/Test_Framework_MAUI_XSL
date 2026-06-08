using System;
using OpenQA.Selenium.Appium.Android;
using TestICA.Core;

namespace TestICA.Pages;

public class ConnexionPage : PageFunction
{
    // On garde le driver brut pour ne rien casser de ton PageFunction de base
    public ConnexionPage(AndroidDriver driver) : base(driver) { }

    public void ClickContinuer()
    {
        Click("button_button_1"); 
    }
    
    public void FillEmail(string email)
    {
        FillField("input_email_23", email);
    }
    
    public void FillPassword(string password)
    {
        FillField("input_password_24", password);
    }
    
    public void ClickValider()
    {
        Click("button_button_2");
    }
}