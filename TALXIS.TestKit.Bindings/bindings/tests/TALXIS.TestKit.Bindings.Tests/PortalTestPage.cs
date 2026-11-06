namespace TALXIS.TestKit.Bindings.Tests
{
    using System;
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;
    using TALXIS.TestKit.Selectors.Browser;
    using TALXIS.TestKit.Selectors.WebClientManagement;
    using WebDriverManager;
    using WebDriverManager.DriverConfigs.Impl;
    using WebDriverManager.Helpers;

    /// <summary>
    /// The headless browser of the test run and the portal test page (TestPages/portal.html), which each test opens
    /// afresh so that it starts from the page as loaded.
    /// </summary>
    [TestClass]
    public class PortalTestPage
    {
        private static IWebDriver? driver;

        /// <summary>
        /// Gets the browser of the test run.
        /// </summary>
        public static IWebDriver Driver => driver ?? throw new InvalidOperationException("The browser has not been started.");

        /// <summary>
        /// Starts a headless Chrome with a driver matching its version.
        /// </summary>
        /// <param name="context">The test context.</param>
        [AssemblyInitialize]
        public static void StartBrowser(TestContext context)
        {
            new DriverManager().SetUpDriver(new ChromeConfig(), VersionResolveStrategy.MatchingBrowser);
            var options = new ChromeOptions();
            options.AddArgument("--headless=new");
            options.AddArgument("--window-size=1920,1080");
            options.SetLoggingPreference(LogType.Browser, LogLevel.All);
            driver = new ChromeDriver(options);
        }

        /// <summary>
        /// Closes the browser.
        /// </summary>
        [AssemblyCleanup]
        public static void StopBrowser() => driver?.Quit();

        /// <summary>
        /// Opens a test page, by default the portal test page.
        /// </summary>
        /// <param name="page">The file name of the page in TestPages.</param>
        /// <returns>The browser.</returns>
        public static IWebDriver Open(string page = "portal.html")
        {
            Driver.Navigate().GoToUrl(new Uri(Path.Combine(AppContext.BaseDirectory, "TestPages", page)).AbsoluteUri);
            return Driver;
        }

        /// <summary>
        /// Gets a web client on the browser of the test run, for the apps (PortalApp, ModelDrivenApp) on the test pages.
        /// </summary>
        /// <returns>The web client.</returns>
        public static WebClient Client()
        {
            var client = new WebClient(new BrowserOptions());
            client.Browser = new InteractiveBrowser(Driver);
            return client;
        }

        /// <summary>
        /// Runs a script on the test page, e.g. one of its functions that shows a dialog.
        /// </summary>
        /// <param name="script">The script.</param>
        public static void Run(string script) => ((IJavaScriptExecutor)Driver).ExecuteScript(script);

        /// <summary>
        /// Gets what the page's buttons recorded when they were clicked, in order.
        /// </summary>
        /// <returns>The recorded clicks, comma-separated.</returns>
        public static string Clicks() => ((IJavaScriptExecutor)Driver).ExecuteScript("return clicks.join(',')")?.ToString() ?? string.Empty;
    }
}
