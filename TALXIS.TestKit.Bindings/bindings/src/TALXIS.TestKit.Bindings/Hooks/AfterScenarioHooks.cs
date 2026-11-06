namespace TALXIS.TestKit.Bindings.Hooks
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using OpenQA.Selenium;
    using Reqnroll;
    using TALXIS.TestKit.Bindings.Extensions;

    /// <summary>
    /// After scenario hooks.
    /// </summary>
    [Binding]
    public class AfterScenarioHooks : PowerAppsStepDefiner
    {
        private readonly ScenarioContext scenarioContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="AfterScenarioHooks"/> class.
        /// </summary>
        /// <param name="scenarioContext">The scenario context.</param>
        public AfterScenarioHooks(ScenarioContext scenarioContext)
        {
            this.scenarioContext = scenarioContext;
        }

        /// <summary>
        /// Deletes the test data created during the test and disposes of the browser.
        /// </summary>
        [AfterScenario(Order = 1)]
        public static void TestCleanup()
        {
            try
            {
                if (TestConfig.DeleteTestData)
                {
                    TestDriver.DeleteTestData();
                }
            }
            catch (WebDriverException)
            {
                // Ignore - tests might have failed before driver was initialised.
            }
            finally
            {
                Quit();
            }
        }

        /// <summary>
        /// Takes a screenshot of the browser and saves the page's HTML and the browser's console when a test fails.
        /// </summary>
        [AfterScenario(Order = 0)]
        public void ScreenshotFailedScenario()
        {
            if (this.scenarioContext.ScenarioExecutionStatus != ScenarioExecutionStatus.TestError)
            {
                return;
            }

            var rootFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var screenshotsFolder = Path.Combine(rootFolder, "screenshots");

            if (!Directory.Exists(screenshotsFolder))
            {
                Directory.CreateDirectory(screenshotsFolder);
            }

            var title = this.scenarioContext.ScenarioInfo.Title;
            var arguments = this.scenarioContext.ScenarioInfo.Arguments;
            if (arguments?.Count > 0)
            {
                // Scenario Outline examples share the title; keep a screenshot per example.
                title += $" ({string.Join(", ", arguments.Values.Cast<object>())})";
            }

            var fileName = string.Concat(title.Split(Path.GetInvalidFileNameChars()));

            try
            {
                Client.Browser.TakeWindowScreenShot(Path.Combine(screenshotsFolder, $"{fileName}.png"));

                // The page's DOM shows what the steps were looking for when the screenshot does not.
                File.WriteAllText(Path.Combine(screenshotsFolder, $"{fileName}.html"), Driver.PageSource);
            }
            catch (Exception ex) when (ex is WebDriverException || ex is IOException || ex is UnauthorizedAccessException)
            {
                // Don't throw an unhandled exception if the screenshot can't be captured as this will prevent the WebDriver instance from being cleaned up.
                Console.WriteLine($"Failed to capture screenshot: {ex.Message}.");
            }

            try
            {
                // An error of a page script (e.g. a form script that failed) shows only in the browser's console.
                File.WriteAllLines(Path.Combine(screenshotsFolder, $"{fileName}.console.log"), Driver.ReadBrowserConsole());
            }
            catch (Exception ex) when (ex is WebDriverException || ex is NotImplementedException)
            {
                // Browsers other than Chromium ones do not offer their console.
                Console.WriteLine($"Failed to read the browser's console: {ex.Message}.");
            }
        }

        [AfterScenario]
        public static void DisposeTempProfiles()
        {
            var basePath = string.IsNullOrEmpty(TestConfig.ProfilesBasePath) ? Path.GetTempPath() : TestConfig.ProfilesBasePath;

            var tempProfilesDirectory = Path.Combine(basePath, "profiles", "TempProfiles");

            if (Directory.Exists(tempProfilesDirectory))
            {
                Directory.Delete(tempProfilesDirectory, true);
            }
        }
    }
}
