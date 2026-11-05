namespace TALXIS.TestKit.Bindings
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using FluentAssertions.Extensions;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Identity.Client;
    using OpenQA.Selenium;
    using Polly;
    using TALXIS.TestKit.Bindings.Configuration;
    using TALXIS.TestKit.Bindings.Extensions;
    using TALXIS.TestKit.Selectors;
    using TALXIS.TestKit.Selectors.WebClientManagement;

    /// <summary>
    /// Base class for defining step bindings.
    /// </summary>
    public abstract class PowerAppsStepDefiner
    {
        private static TestConfiguration testConfig;

        private static IConfidentialClientApplication app;

        [ThreadStatic]
        private static string currentProfileDirectory;

        [ThreadStatic]
        private static ITestDriver testDriver;

        [ThreadStatic]
        private static ITestDataRepository testDataRepository;

        [ThreadStatic]
        private static WebClient client;

        [ThreadStatic]
        private static XrmApp xrmApp;

        [ThreadStatic]
        private static PortalApp portalApp;

        [ThreadStatic]
        private static IAppUnderTest appUnderTest;

        private static IDictionary<string, string> userProfilesDirectories;
        private static object userProfilesDirectoriesLock = new object();

        /// <summary>
        /// Gets access token used to authenticate as the application user configured for testing.
        /// </summary>
        protected static string AccessToken
        {
            get
            {
                var hostSegments = TestConfig.GetTestUrl().Host.Split('.');
                var scope = string.IsNullOrEmpty(TestConfig.DataverseUrl)
                    ? $"https://{hostSegments[0]}.api.{hostSegments[1]}.dynamics.com//.default"
                    : $"{new Uri(TestConfig.DataverseUrl).GetLeftPart(UriPartial.Authority)}/.default";

                return GetApp()
                    .AcquireTokenForClient(new string[] { scope })
                    .ExecuteAsync()
                    .Result.AccessToken;
            }
        }

        /// <summary>
        /// Gets the configuration for the test project.
        /// </summary>
        protected static TestConfiguration TestConfig
        {
            get
            {
                try
                {
                    if (testConfig == null)
                    {

                        var firstTypeFromTestsAssembly = AppDomain.CurrentDomain.GetAssemblies().Select(x => x.GetTypes()).SelectMany(x => x).First(x => x.IsDefined(Type.GetType("Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute, Microsoft.VisualStudio.TestPlatform.TestFramework"), false));
                        var testsAssembly = Assembly.GetAssembly(firstTypeFromTestsAssembly);

                        var configuration = new ConfigurationBuilder()
                           .SetBasePath(Directory.GetCurrentDirectory())
                           .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                           .AddUserSecrets(testsAssembly)
                           .AddEnvironmentVariables()
                           .Build();

                        testConfig = new TestConfiguration(configuration);
                    }

                    return testConfig;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error initializing test configuration. Please verify that the 'appsettings.json' file exists and is correctly configured, that the test assembly has a valid UserSecretsId attribute, and that all required configuration keys are provided via user secrets or environment variables. Details: " + ex.Message, ex);
                }
            }
        }

        /// <summary>
        /// Gets a repository provides access to test data.
        /// </summary>
        protected static ITestDataRepository TestDataRepository => testDataRepository ?? (testDataRepository = new FileDataRepository());

        /// <summary>
        /// Gets the EasyRepro WebClient.
        /// </summary>
        protected static WebClient Client
        {
            get
            {
                if (client == null)
                {
                    var browserOptions = (BrowserOptionsWithProfileSupport)TestConfig.BrowserOptions.Clone();

                    if (TestConfig.UseProfiles)
                    {
                        browserOptions.ProfileDirectory = CurrentProfileDirectory;
                    }

                    client = new WebClient(browserOptions);
                }

                return client;
            }
        }

        /// <summary>
        /// Gets the EasyRepro XrmApp.
        /// </summary>
        protected static XrmApp XrmApp => xrmApp ?? (xrmApp = new XrmApp(Client));

        /// <summary>
        /// Gets the Selenium WebDriver.
        /// </summary>
        protected static IWebDriver Driver => Client.Browser.Driver;

        /// <summary>
        /// Gets the TALXIS Portal, for steps that act on a portal only.
        /// </summary>
        protected static PortalApp PortalApp => portalApp ?? (portalApp = new PortalApp(Client));

        /// <summary>
        /// Gets the app the steps act on, as the configured <see cref="TestConfiguration.Target"/> says: a model-driven app
        /// or a TALXIS Portal. Steps that act on either kind of app use only this.
        /// </summary>
        protected static IAppUnderTest App => appUnderTest ?? (appUnderTest = TestConfig.Target == TestTarget.Portal
            ? PortalApp
            : new ModelDrivenApp());

        /// <summary>
        /// Gets the profile directory for the current scenario.
        /// </summary>
        protected static string CurrentProfileDirectory
        {
            get
            {
                if (!testConfig.BrowserOptions.BrowserType.SupportsProfiles())
                {
                    throw new NotSupportedException($"The {testConfig.BrowserOptions.BrowserType} does not support profiles.");
                }

                if (string.IsNullOrEmpty(currentProfileDirectory))
                {
                    var basePath = string.IsNullOrEmpty(TestConfig.ProfilesBasePath) ? Path.GetTempPath() : TestConfig.ProfilesBasePath;
                    currentProfileDirectory = Path.Combine(basePath, "profiles", "TempProfiles", Guid.NewGuid().ToString());
                }

                return currentProfileDirectory;
            }
        }

        /// <summary>
        /// Gets provides utilities for test setup/teardown.
        /// </summary>
        protected static ITestDriver TestDriver
        {
            get
            {
                if (testDriver == null)
                {
                    testDriver = new TestDriver((IJavaScriptExecutor)Driver);
                    var useApplicationUser = TestConfig.ApplicationUser != null || !string.IsNullOrEmpty(TestConfig.DataverseUrl);
                    testDriver.InjectOnPage(useApplicationUser ? AccessToken : null, TestConfig.DataverseUrl, TestConfig.SuppressFlowsForTestData);
                }

                return testDriver;
            }
        }

        /// <summary>
        /// Gets the directories for the Chrome or Firefox profiles.
        /// </summary>
        protected static IDictionary<string, string> UserProfileDirectories
        {
            get
            {
                if (!testConfig.BrowserOptions.BrowserType.SupportsProfiles())
                {
                    throw new NotSupportedException($"The {testConfig.BrowserOptions.BrowserType} does not support profiles.");
                }

                lock (userProfilesDirectoriesLock)
                {
                    if (userProfilesDirectories != null)
                    {
                        return userProfilesDirectories;
                    }

                    var basePath = string.IsNullOrEmpty(TestConfig.ProfilesBasePath) ? Path.GetTempPath() : TestConfig.ProfilesBasePath;
                    var profilesDirectory = Path.Combine(basePath, "profiles", "SaveProfiles");

                    Directory.CreateDirectory(profilesDirectory);
                    userProfilesDirectories = TestConfig.Users
                        .Where(u => !string.IsNullOrEmpty(u.Password))
                        .Select(u => u.Username)
                        .Distinct()
                        .ToDictionary(u => u, u => Path.Combine(profilesDirectory, u));

                    foreach (var dir in userProfilesDirectories.Values)
                    {
                        Directory.CreateDirectory(dir);
                    }
                }

                return userProfilesDirectories;
            }
        }

        /// <summary>
        /// Performs any cleanup necessary when quitting the WebBrowser.
        /// </summary>
        protected static void Quit()
        {
            // Try to dispose, and catch web driver errors that can occur on disposal. Retry the disposal if these occur. Trap the final exception and continue the disposal process.
            var polly = Policy
                .Handle<WebDriverException>()
                .Retry(3, (ex, i) =>
                {
                    Console.WriteLine(ex.Message);
                })
                .ExecuteAndCapture(() =>
                {
                    xrmApp?.Dispose();

                    // Ensuring that the driver gets disposed. Previously we were left with orphan processes and were unable to clean up profile folders. We cannot rely on xrmApp.Dispose to properly dispose of the web driver.
                    var driver = client?.Browser?.Driver;
                    driver?.Dispose();
                });

            xrmApp = null;
            portalApp = null;
            appUnderTest = null;
            client = null;
            testDriver = null;
            testConfig?.Flush();

            if (!string.IsNullOrEmpty(currentProfileDirectory) && Directory.Exists(currentProfileDirectory))
            {
                var directoryToDelete = currentProfileDirectory;
                currentProfileDirectory = null;

                // CrashpadMetrics-active.pma file can continue to be locked even after quitting Chrome. Requires retries.
                Policy
                    .Handle<UnauthorizedAccessException>()
                    .WaitAndRetry(3, retry => (retry * 5).Seconds())
                    .ExecuteAndCapture(() =>
                    {
                        Directory.Delete(directoryToDelete, true);
                    });
            }
        }

        /// <summary>
        /// Gets the <see cref="IConfidentialClientApplication"/> used to authenticate as the configured application user.
        /// </summary>
        private static IConfidentialClientApplication GetApp()
        {
            if (TestConfig.ApplicationUser == null)
            {
                throw new ConfigurationErrorsException("An application user has not been configured.");
            }

            if (string.IsNullOrEmpty(TestConfig.ApplicationUser.TenantId)
                || string.IsNullOrEmpty(TestConfig.ApplicationUser.ClientId)
                || string.IsNullOrEmpty(TestConfig.ApplicationUser.ClientSecret))
            {
                throw new ConfigurationErrorsException("The application user needs a tenantId, clientId and clientSecret.");
            }

            if (app == null)
            {
                app = ConfidentialClientApplicationBuilder.Create(TestConfig.ApplicationUser.ClientId)
                    .WithTenantId(TestConfig.ApplicationUser.TenantId)
                    .WithClientSecret(TestConfig.ApplicationUser.ClientSecret)
                    .Build();
            }

            return app;
        }
    }
}
