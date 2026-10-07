namespace TALXIS.TestKit.Bindings.Steps
{
    using System;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Support.UI;
    using Reqnroll;
    using TALXIS.TestKit.Bindings.Extensions;

    /// <summary>
    /// Logs in to a TALXIS Portal that authenticates through an Azure AD B2C local-account sign-in page.
    ///
    /// LoginSteps targets Dynamics 365: it treats any non-*.dynamics.com host as on-premises pass-through,
    /// caches Entra ID (login.microsoftonline.com) cookies and opens the app through the app picker. None of
    /// that applies to a portal, so this fills in the B2C form directly, waits for the redirect back to the
    /// portal and then for the portal to finish loading (the data steps' driver.js depends on its Xrm API).
    /// </summary>
    [Binding]
    public class PortalLoginSteps : PowerAppsStepDefiner
    {
        private const string SignInNameInputId = "signInName";
        private const string PasswordInputId = "password";
        private const string SignInButtonId = "next";
        private static readonly TimeSpan LoginTimeout = TimeSpan.FromSeconds(60);

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

            var portalUrl = TestConfig.GetTestUrl();
            var wait = new WebDriverWait(Driver, LoginTimeout);

            Driver.Navigate().GoToUrl(portalUrl);

            // The portal redirects to the B2C sign-in page once its MSAL client has loaded.
            wait.Message = "The Azure AD B2C sign-in page was not displayed. Is the browser already signed in?";
            wait.Until(driver => driver.FindElements(By.Id(SignInNameInputId)).Count > 0);

            Driver.FindElement(By.Id(SignInNameInputId)).SendKeys(user.Username);
            Driver.FindElement(By.Id(PasswordInputId)).SendKeys(user.Password);
            Driver.FindElement(By.Id(SignInButtonId)).Click();

            wait.Message = $"Signing in as '{userAlias}' did not redirect back to {portalUrl.Host}. Check the username and password.";
            wait.Until(driver => new Uri(driver.Url).Host == portalUrl.Host);

            Driver.WaitForPortalReady(LoginTimeout, "The portal did not finish loading after signing in.");
        }
    }
}
