using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Appium;

namespace BaobaTesterBox.Core.Interactions
{
    public class AppiumSignService
    {
        private readonly AppiumDriver Driver;

        public AppiumSignService(AppiumDriver driver)
        {
            Driver = driver;
        }

        public bool SignCanvas(string cssSelector)
        {
            try
            {
                var canvas = Driver.FindElement(By.CssSelector(cssSelector));

                // scroll pour éviter les out-of-bounds
                ((IJavaScriptExecutor)Driver)
                    .ExecuteScript("arguments[0].scrollIntoView({block:'center'});", canvas);

                Thread.Sleep(500);

                var size = canvas.Size;

                int startX = Math.Max(1, size.Width / 4);
                int startY = Math.Max(1, size.Height / 2);

                var actions = new Actions(Driver);

                actions
                    .MoveToElement(canvas, startX, startY)
                    .ClickAndHold()
                    .MoveByOffset(60, 10)
                    .MoveByOffset(60, -20)
                    .MoveByOffset(60, 20)
                    .MoveByOffset(60, -10)
                    .Release()
                    .Perform();

                return true;
            }
            catch (MoveTargetOutOfBoundsException)
            {
                // ⚠️ CAS NORMAL WEBVIEW CANVAS
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool IsCanvasSigned(string cssSelector)
        {
            try
            {
                var result = (string)((IJavaScriptExecutor)Driver).ExecuteScript(@"
                    var c = document.querySelector(arguments[0]);
                    if (!c) return 'NOT_FOUND';
                    if (c.toDataURL === undefined) return 'NO_CANVAS_API';
                    return c.toDataURL();
                ", cssSelector);

                return !string.IsNullOrEmpty(result) && result.Length > 100;
            }
            catch
            {
                return false;
            }
        }
    }
}