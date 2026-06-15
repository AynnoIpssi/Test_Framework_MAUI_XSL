using System;
using NUnit.Framework;
using BaobaTesterBox.Core.Loggings;
using BaobaTesterBox.Tests.Infrastructure;
using BaobaTesterBox.Core.Navigation;

namespace BaobaTesterBox.Tests.SanityChecks.Login;

[TestFixture]
public class NavigationDirecteTests2 : BaseTest
{
    private const string Origine = "NavigationDirecteTests";

    [Test]
    public void Test_Navigation_Ica_Valeurs_Exactes()
    {
        AppiumLoggerService.LogInfo("=== DÉBUT : Navigation Fiche ICA ===", Origine);

        AppiumService.BasculeVersWebView();

        try
        {
            // 🎯 Tout sur une seule ligne, sans aucune variable intermédiaire
            new AppiumNavigationService().ModuleGo(542, 5, 409);
            
            AppiumLoggerService.LogInfo("validation : Commande de navigation transmise !", Origine);
        }
        catch (Exception ex)
        {
            Assert.Fail($"[Erreur Navigation] L'appel a échoué : {ex.Message}");
        }

        System.Threading.Thread.Sleep(3000);
        AppiumLoggerService.LogInfo("=== FIN DU TEST ===", Origine);
    }
}