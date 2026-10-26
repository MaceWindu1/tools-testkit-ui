namespace TALXIS.TestKit.Bindings.Tests
{
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TALXIS.TestKit.Bindings.Configuration;
    using TALXIS.TestKit.Bindings.Extensions;

    /// <summary>
    /// The browser console, where the errors of page scripts end up (saved next to the screenshot of a failed scenario).
    /// </summary>
    [TestClass]
    public class BrowserConsoleTests
    {
        [TestMethod]
        public void StartsChromeWithItsConsoleKept()
        {
            var capabilities = new BrowserOptionsWithProfileSupport().ToChrome().ToCapabilities();

            Assert.IsNotNull(capabilities.GetCapability("goog:loggingPrefs"));
        }

        [TestMethod]
        public void ReadsTheErrorsThatPageScriptsLogged()
        {
            var driver = PortalTestPage.Open();

            PortalTestPage.Run("console.error('the form script failed')");

            Assert.IsTrue(driver.ReadBrowserConsole().Any(message => message.Contains("the form script failed")));
        }
    }
}
