using NUnit.Framework;
using OpenQA.Selenium;
using BaobaTesterBox.Core.Interactions;
using BaobaTesterBox.Core.Loggings;
using BaobaTesterBox.Tests.Infrastructure;

namespace BaobaTesterBox.Tests.FicheIca
{
    [TestFixture]
    public class TestSignatureAppiumOnly : BaseTest
    {
        private const string Origine = "TestSignatureAppiumOnly";

        [Test]
        public void Test_Signature_Canvas_Appium_Only()
        {
            AppiumLoggerService.LogInfo("=== TEST SIGNATURE APPIUM ONLY ===", Origine);
            AppiumService.BasculeVersWebView();

            var signService = new AppiumSignService(Driver);

            IWebElement canvas;

            try
            {
                canvas = Driver.FindElement(By.CssSelector("[id^='jq-signature-canvas-']"));
            }
            catch (OpenQA.Selenium.UnknownErrorException)
            {
                Assert.Fail("❌ UiAutomator2 crash avant même accès canvas (driver mort)");
                return;
            }

            Assert.That(canvas, Is.Not.Null, "Canvas introuvable");

            bool signOk = signService.SignCanvas("[id^='jq-signature-canvas-']");
            Assert.That(signOk, Is.True, "Erreur technique signature");

            bool isSigned = signService.IsCanvasSigned("[id^='jq-signature-canvas-']");

            Assert.That(isSigned, Is.True, "Signature non détectée");

            AppiumLoggerService.LogInfo("✅ Signature OK", Origine);
        }
    }
}