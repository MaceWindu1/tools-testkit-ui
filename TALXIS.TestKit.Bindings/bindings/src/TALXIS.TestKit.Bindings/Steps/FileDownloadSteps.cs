namespace TALXIS.TestKit.Bindings.Steps
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using FluentAssertions;
    using Reqnroll;

    /// <summary>
    /// Asserts that a browser file download actually happened. Selenium has no first-class "was a
    /// file downloaded" API; this uses the common workaround of watching the OS Downloads folder for
    /// a new file created since the scenario started.
    ///
    /// Watches the browser's download directory: browserOptions.downloadsPath when configured, otherwise the user's
    /// Downloads folder (the browser's default "just download, don't prompt" behaviour is assumed).
    /// </summary>
    [Binding]
    public class FileDownloadSteps : PowerAppsStepDefiner
    {
        private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(250);

        [ThreadStatic]
        private static DateTime scenarioStartedAtUtc;

        private static string DownloadsDirectory =>
            string.IsNullOrEmpty(TestConfig.BrowserOptions?.DownloadsPath)
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads")
                : TestConfig.BrowserOptions.DownloadsPath;

        [BeforeScenario]
        public static void RecordScenarioStartTime()
        {
            scenarioStartedAtUtc = DateTime.UtcNow;
        }

        /// <summary>
        /// Asserts that a file has been downloaded to the Downloads folder since the scenario started.
        /// </summary>
        [Then(@"a file should have been downloaded")]
        public static void ThenAFileShouldHaveBeenDownloaded()
        {
            var deadline = DateTime.UtcNow.AddSeconds(10);
            bool downloaded;

            while (true)
            {
                downloaded = Directory.Exists(DownloadsDirectory)
                    && Directory.GetFiles(DownloadsDirectory)
                        .Any(f => File.GetLastWriteTimeUtc(f) >= scenarioStartedAtUtc && !f.EndsWith(".crdownload", StringComparison.OrdinalIgnoreCase));
                if (downloaded || DateTime.UtcNow >= deadline)
                {
                    break;
                }

                Thread.Sleep(PollInterval);
            }

            downloaded.Should().BeTrue(because: "a file should have been downloaded to the Downloads folder during this scenario");
        }
    }
}
