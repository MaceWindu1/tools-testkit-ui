namespace TALXIS.TestKit.Bindings.Steps
{
    using System;
    using Reqnroll;
    using TALXIS.TestKit.Selectors.Browser;

    /// <summary>
    /// Logs in to a TALXIS Portal that authenticates through an Azure AD B2C local-account sign-in page, whatever the
    /// configured target is.
    /// </summary>
    [Binding]
    public class PortalLoginSteps : PowerAppsStepDefiner
    {
        /// <summary>
        /// Logs in to the portal configured as the test URL as a given user.
        /// </summary>
        /// <param name="userAlias">The alias of the user.</param>
        [Given(@"I am logged in to the portal as '(.*)'")]
        public static void GivenIAmLoggedInToThePortalAs(string userAlias)
        {
            var user = TestConfig.GetUser(userAlias, useCurrentUser: false);
            if (string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Password))
            {
                throw new InvalidOperationException($"User '{userAlias}' has no username or password configured. Set them in user secrets or environment variables.");
            }

            PortalApp.Login.LogIn(TestConfig.GetTestUrl(), user.Username.ToSecureString(), user.Password.ToSecureString());
        }
    }
}
