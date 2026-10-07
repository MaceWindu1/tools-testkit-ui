namespace TALXIS.TestKit.Bindings.Steps
{
    using System;
    using System.IO;
    using System.Linq;
    using FluentAssertions;
    using Reqnroll;

    /// <summary>
    /// Asserts that a browser file download actually happened. Selenium has no first-class "was a
    /// file downloaded" API; this uses the common workaround of watching the OS Downloads folder for
    /// a new file created since the scenario started.
    ///
    /// Assumes the browser is using its default download directory and default "just download,
    /// don't prompt" behaviour. If a project's BrowserOptions configures a different download
    /// directory, adjust <see cref="DownloadsDirectory"/> accordingly.
    /// </summary>
    [Binding]
    public class FileDownloadSteps : PowerAppsStepDefiner
    {
        private static DateTime scenarioStartedAtUtc;

        private static string DownloadsDirectory =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

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

            do
            {
                downloaded = Directory.Exists(DownloadsDirectory)
                    && Directory.GetFiles(DownloadsDirectory)
                        .Any(f => File.GetLastWriteTimeUtc(f) >= scenarioStartedAtUtc && !f.EndsWith(".crdownload", StringComparison.OrdinalIgnoreCase));
            }
            while (!downloaded && DateTime.UtcNow < deadline);

            downloaded.Should().BeTrue(because: "a file should have been downloaded to the Downloads folder during this scenario");
        }
    }
}
