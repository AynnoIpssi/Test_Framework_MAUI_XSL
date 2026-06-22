using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using BaobaTesterBox.Core.Interactions;
using BaobaTesterBox.Core.Loggings;
using BaobaTesterBox.Core.Locators;
using BaobaTesterBox.Core.Navigation;
using BaobaTesterBox.Tests.Infrastructure;
using OpenQA.Selenium.Appium;

namespace BaobaTesterBox.Tests.FicheIca
{
    [TestFixture]
    public class FicheIcaCreationTests : BaseTest
    {
        private const string Origine = "FicheIcaCreationTests";

        [Test]
        public void Test_Creer_Fiche_Ica_Nominale()
        {
            var alertes = new AppiumAlertHandler();
            var signService = new AppiumSignService((AppiumDriver)Driver);

            // =====================================================
            // ÉTAPE 1 : NAVIGATION
            // =====================================================
            AppiumLoggerService.LogInfo("=== NAV ICA ===", Origine);

            AppiumService.BasculeVersWebView();
            new AppiumNavigationService().ModuleGo(542, 5, 171);

            Thread.Sleep(2000);

            var msgMortalite = alertes.GetMissingReadingMessage();
            if (!string.IsNullOrEmpty(msgMortalite))
                Assert.Inconclusive(msgMortalite);

            AppiumElementScanner.Init(Driver);

            var scan1 = AppiumElementScanner.GetVisibleFormFields();

            if (!scan1.EstModeCreation)
                Assert.Inconclusive("Mode édition détecté");

            var actionField = scan1.Fields.FirstOrDefault(f => f.Id == "actionid");
            Assert.That(actionField, Is.Not.Null);

            var actionOption = actionField.Options.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Value));
            Assert.That(actionOption, Is.Not.Null);

            // =====================================================
            // ÉTAPE 2 : ACTION
            // =====================================================
            AppiumLoggerService.LogInfo("=== ACTION ===", Origine);

            Actions.FillFieldsById(new Dictionary<string, string>
            {
                ["actionid"] = actionOption.Value
            });

            Thread.Sleep(1000);

            var scan2 = AppiumElementScanner.GetVisibleFormFields();

            string today = DateTime.Now.ToString("yyyy-MM-dd");

            var values = new Dictionary<string, string>
            {
                ["dif_enr14"] = today,
                ["dif_enr15"] = today,
                ["dif_enr5"] = "ICA AUTO TEST"
            };

            var abattoir = scan2.Fields.FirstOrDefault(f => f.Id == "dif_enr16");
            if (abattoir != null)
            {
                var opt = abattoir.Options.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Value));
                if (opt != null)
                    values["dif_enr16"] = opt.Value;
            }

            foreach (var id in new[] { "dif_enr17", "dif_enr18", "dif_enr19", "dif_enr20", "dif_enr21", "dif_enr22" })
            {
                var field = scan2.Fields.FirstOrDefault(f => f.Id == id);
                if (field != null && string.IsNullOrWhiteSpace(field.Value))
                    values[id] = "1";
            }

            AppiumLoggerService.LogInfo("=== FILL ===", Origine);

            Actions.FillFieldsById(values);

            Thread.Sleep(800);

            // =====================================================
            // ÉTAPE 3 : SIGNATURE (TON SERVICE)
            // =====================================================
            AppiumLoggerService.LogInfo("=== SIGNATURE ===", Origine);

            string canvasSelector = "[id^='jq-signature-canvas-']";

            bool alreadySigned = signService.IsCanvasSigned(canvasSelector);

            if (alreadySigned)
            {
                AppiumLoggerService.LogInfo("Signature déjà présente → skip", Origine);
            }
            else
            {
                bool signed = signService.SignCanvas(canvasSelector);

                AppiumLoggerService.LogInfo(
                    signed ? "Signature effectuée" : "Signature échouée",
                    Origine
                );
            }

            Thread.Sleep(500);

            // =====================================================
            // ÉTAPE 4 : SAVE
            // =====================================================
            AppiumLoggerService.LogInfo("=== SAVE ===", Origine);

            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));

            var btn = wait.Until(d =>
            {
                var el = d.FindElement(By.Id("btn-save-popin"));
                return (el.Displayed && el.Enabled) ? el : null;
            });

            try
            {
                btn.Click();
            }
            catch
            {
                ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].click();", btn);
            }

            // =====================================================
            // RESULT
            // =====================================================
            var (statut, detail) = AttendreResultat(alertes, TimeSpan.FromSeconds(8));

            if (statut == "alerte")
            {
                alertes.AcceptAlert();
                Assert.Inconclusive(detail);
            }

            Assert.That(statut, Is.EqualTo("succes"));
        }

        private (string, string) AttendreResultat(AppiumAlertHandler alertes, TimeSpan timeout)
        {
            var end = DateTime.Now + timeout;

            while (DateTime.Now < end)
            {
                var msg = alertes.GetAlertText();

                if (!string.IsNullOrWhiteSpace(msg))
                    return ("alerte", msg);

                if (Driver.FindElements(By.Id("poultryModalContent")).Count == 0)
                    return ("succes", "");

                Thread.Sleep(400);
            }

            return ("timeout", "");
        }
    }
}