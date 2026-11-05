using System;
using System.Linq;
using System.Security;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using TALXIS.TestKit.Selectors.Browser;
using TALXIS.TestKit.Selectors.DTO.Locators;

namespace TALXIS.TestKit.Selectors.WebClientManagement
{
    /// <summary>
    /// Signs in to a TALXIS Portal that authenticates through an Azure AD B2C local-account sign-in page: fills in the B2C
    /// form, waits for the redirect back to the portal and then for the portal to finish loading. The model-driven app login
    /// (<see cref="LoginManager"/>) does not apply: it expects Entra ID and the app picker.
    /// </summary>
    public class PortalLoginManager
    {
        private const int SignInAttempts = 2;
        private static readonly TimeSpan LoginTimeout = TimeSpan.FromSeconds(60);

        public PortalLoginManager(WebClient client)
        {
            Client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public WebClient Client { get; }

        internal BrowserCommandResult<bool> LogIn(Uri portalUrl, SecureString username, SecureString password)
        {
            if (username == null || password == null)
                throw new ArgumentException("Signing in to a TALXIS Portal needs a username and a password.");

            return Client.Execute(Client.GetNoRetryOptions("Portal Login"), driver =>
            {
                for (var attempt = 1; !TrySignIn(driver, portalUrl, username, password); attempt++)
                {
                    var shown = PageText(driver);
                    if (attempt == SignInAttempts)
                        throw new WebDriverTimeoutException($"Signing in did not redirect back to {portalUrl.Host}. Check the username and password. The page shows: {shown}");

                    // Azure AD B2C sometimes fails a sign-in with its own error (e.g. "ConnectionTimeOut: An exception has occurred.").
                    Console.WriteLine($"Signing in did not redirect back to {portalUrl.Host} (the page shows: {shown}); signing in again.");
                }

                // The data steps' driver.js depends on the portal's Xrm API.
                return PortalPageManager.WaitForReady(driver, LoginTimeout, "The portal did not finish loading after signing in.");
            });
        }

        // Signs in on the B2C page the portal redirects to and returns whether the browser came back to the portal.
        private static bool TrySignIn(IWebDriver driver, Uri portalUrl, SecureString username, SecureString password)
        {
            driver.Navigate().GoToUrl(portalUrl);

            // The portal redirects to the B2C sign-in page once its MSAL client has loaded. The page renders its form before
            // its scripts are ready: typing or clicking before then is refused or does not submit the form.
            var wait = new WebDriverWait(driver, LoginTimeout) { Message = "The Azure AD B2C sign-in page was not displayed. Is the browser already signed in?" };
            wait.Until(d => IsReady(d, PortalLoginElementsLocators.SignInName) && IsReady(d, PortalLoginElementsLocators.Password) && IsReady(d, PortalLoginElementsLocators.SignIn));

            driver.FindElement(PortalLoginElementsLocators.SignInName).SendKeys(username.ToUnsecureString());
            driver.FindElement(PortalLoginElementsLocators.Password).SendKeys(password.ToUnsecureString());
            driver.FindElement(PortalLoginElementsLocators.SignIn).Click();

            try
            {
                new WebDriverWait(driver, LoginTimeout).Until(d => new Uri(d.Url).Host == portalUrl.Host);
                return true;
            }
            catch (WebDriverTimeoutException)
            {
                return false;
            }
        }

        private static string PageText(IWebDriver driver)
        {
            var text = driver.FindElements(By.TagName("body")).FirstOrDefault()?.Text ?? string.Empty;
            return text.Length > 300 ? text.Substring(0, 300) : text;
        }

        private static bool IsReady(IWebDriver driver, By element)
        {
            return driver.FindElements(element).FirstOrDefault() is { Displayed: true, Enabled: true };
        }
    }
}
