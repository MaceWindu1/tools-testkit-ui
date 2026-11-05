namespace TALXIS.TestKit.Bindings.Configuration
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Test configuration for PowerApps UI testing.
    /// </summary>
    public class TestConfiguration
    {
        private readonly IConfiguration configuration;
        private static Dictionary<string, UserConfiguration> currentUsers = new Dictionary<string, UserConfiguration>();

        public TestConfiguration(IConfiguration configuration)
        {
            this.configuration = configuration;
            this.BrowserOptions = configuration.GetSection("BrowserOptions").Get<BrowserOptionsWithProfileSupport>();
            this.Users = configuration.GetSection("Users").Get<List<UserConfiguration>>();
            this.Url = configuration["Url"];
            this.Target = configuration.GetValue("Target", TestTarget.ModelDrivenApp);
            this.DataverseUrl = configuration["DataverseUrl"];
            this.SuppressFlowsForTestData = configuration.GetValue<bool>("SuppressFlowsForTestData");
            this.UseProfiles = configuration.GetValue<bool>("UseProfiles");
            this.DeleteTestData = configuration.GetValue<bool>("DeleteTestData");
            this.ProfilesBasePath = configuration["ProfilesBasePath"];
            this.ApplicationUser = configuration.GetSection("applicationUser").Get<ClientCredentials>();
            this.Parameters = new Dictionary<string, string>(
                configuration.GetSection("Parameters").Get<Dictionary<string, string>>() ?? new Dictionary<string, string>(),
                StringComparer.OrdinalIgnoreCase);

            // TODO: Make this overrideable from config
            //this.BrowserOptions.DriversPath
        }

        /// <summary>
        /// Gets or sets the URL of the target PowerApps instance.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the kind of app at <see cref="Url"/>, which the steps that act on either kind of app act on: a
        /// model-driven app unless a TALXIS Portal is configured, as projects written before the portal support expect.
        /// </summary>
        public TestTarget Target { get; set; }

        /// <summary>
        /// Gets or sets the URL of the Dataverse environment holding the test data, when it is not the app under test
        /// (e.g. a TALXIS Portal). When set, test data is created and deleted by the application user through its Web API.
        /// </summary>
        public string DataverseUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether creating and deleting test data should not trigger Power Automate flows
        /// (e.g. integrations with systems where the test data does not exist). Applies to data handled by the application user.
        /// </summary>
        public bool SuppressFlowsForTestData { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use profiles.
        /// </summary>
        public bool UseProfiles { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to delete test data.
        /// </summary>
        public bool DeleteTestData { get; set; } = true;

        /// <summary>
        /// Gets or sets the base path where the user profiles are stored.
        /// </summary>
        public string ProfilesBasePath { get; set; }
        public BrowserOptionsWithProfileSupport BrowserOptions { get; set; }
        public List<UserConfiguration> Users { get; set; }

        public ClientCredentials ApplicationUser { get; set; }

        /// <summary>
        /// Gets or sets named values that data files and step arguments reference as <c>{{Parameter:Name}}</c>, e.g. the
        /// identifiers of the test records a runner uses, so that runners can use different records with the same tests.
        /// </summary>
        public Dictionary<string, string> Parameters { get; set; }


        /// <summary>
        /// Gets the target URL.
        /// </summary>
        /// <returns>The URL of the test environment.</returns>
        public Uri GetTestUrl()
        {
            // An unset placeholder (e.g. 'REQNROLL_POWERAPPS_URL') would otherwise fail as an invalid URI format.
            if (!Uri.TryCreate(this.Url, UriKind.Absolute, out Uri url))
            {
                throw new InvalidOperationException($"The test URL '{this.Url}' is not an absolute URL. Set 'Url' in appsettings.json, in the user secrets or in the 'Url' environment variable.");
            }

            return url;
        }

        public UserConfiguration GetUser(string userAlias, bool useCurrentUser = true)
        {
            if (useCurrentUser && currentUsers.ContainsKey(userAlias))
            {
                return currentUsers[userAlias];
            }

            var user = this.Users.Find(u => u.Alias == userAlias);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            currentUsers[userAlias] = user;
            return user;
        }

        /// <summary>
        /// Called internally between scenarios to reset thread state.
        /// </summary>
        internal void Flush()
        {
            currentUsers.Clear();
        }
    }
}
