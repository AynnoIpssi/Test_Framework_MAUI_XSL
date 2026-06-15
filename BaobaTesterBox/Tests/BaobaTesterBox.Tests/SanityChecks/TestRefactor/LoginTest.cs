using NUnit.Framework;
using BaobaTesterBox.Core.Loggings;
using BaobaTesterBox.Core.Locators;
using BaobaTesterBox.Tests.Infrastructure;
using OpenQA.Selenium;

namespace BaobaTesterBox.Tests.SanityChecks.Login;

[TestFixture]
public class LoginTests : BaseTest
{
    private const string Origine = "LoginTests";

    [Test]
    public void Test_Connexion_En_Dur()
    {
        AppiumLoggerService.LogInfo("=== DÉBUT : Test de connexion (Sans Redémarrage) ===", Origine);

        // 1. RAZ du contexte Appium pour nettoyer le canal de communication
        AppiumLoggerService.LogInfo("Réinitialisation du contexte Appium...", Origine);
        Driver.Context = "NATIVE_APP"; 

        // 2. Re-bascule propre vers la WebView active
        AppiumService.BasculeVersWebView();

        // 3. Ré-initialisation du Scanner avec le flux tout neuf
        AppiumInputScanner.Init(Driver);

        // 4. Ouverture de la modale d'authentification
        AppiumLoggerService.LogInfo("Ouverture de la modale #authenticateModal...", Origine);
        Js.ExecuteScript("$('#authenticateModal').modal('show');");

        // 5. Attente dynamique que le champ password soit interactif
        bool estPret = Waiter.WaitForInputVisible("#password");
        Assert.That(estPret, Is.True, "Erreur : Le formulaire de connexion n'est pas devenu interactif.");

        // 6. Remplissage direct des champs validés par le diagnostic DOM
        AppiumLoggerService.LogInfo("Saisie des identifiants de test...", Origine);
        Driver.FindElement(By.CssSelector("#login")).SendKeys("baoba.uat.tessa1@yopmail.com");
        Driver.FindElement(By.CssSelector("#password")).SendKeys("@123456789Abc");

        // 7. Clic sur le bouton de validation
        AppiumLoggerService.LogInfo("Clic sur #btnValidateSynchro...", Origine);
        Driver.FindElement(By.CssSelector("#btnValidateSynchro")).Click();
        
        AppiumLoggerService.LogInfo("=== FIN : Test terminé avec succès ===", Origine);
    }
}