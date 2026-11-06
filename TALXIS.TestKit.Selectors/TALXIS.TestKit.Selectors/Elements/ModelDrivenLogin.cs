using System;
using System.Security;
using OpenQA.Selenium;
using TALXIS.TestKit.Selectors.Browser;
using TALXIS.TestKit.Selectors.WebClientManagement.Helpers;

namespace TALXIS.TestKit.Selectors
{
    /// <summary>
    /// Signs in to a model-driven app through Entra ID, reusing the sign-in cookies of an earlier scenario of the same user:
    /// those kept in memory, or those saved in a <c>cookies_{user}.json</c> file by an earlier test run.
    /// </summary>
    public class ModelDrivenLogin : Element, ILogin
    {
        private static readonly TimeSpan MainPageTimeout = TimeSpan.FromSeconds(15);

        private readonly XrmApp _xrmApp;

        public ModelDrivenLogin(XrmApp xrmApp) : base()
        {
            _xrmApp = xrmApp ?? throw new ArgumentNullException(nameof(xrmApp));
        }

        /// <inheritdoc/>
        /// <remarks>Opens the app that the URL names by its appid.</remarks>
        public void LogIn(Uri url, SecureString username, SecureString password, SecureString otpToken = null)
        {
            if (!url.Query.Contains("appid", StringComparison.Ordinal))
                throw new InvalidOperationException("Logging in without naming the app needs the app's appid in the test URL.");

            LogIn(url, username, password, otpToken, appName: null);
        }

        /// <summary>
        /// Opens a model-driven app at a URL, signs in and waits until the app is ready to be used.
        /// </summary>
        /// <param name="url">The URL of the app.</param>
        /// <param name="username">The user name.</param>
        /// <param name="password">The password.</param>
        /// <param name="otpToken">The secret key of the one-time codes, when the sign-in asks for one.</param>
        /// <param name="appName">The app to open when the URL names none by its appid.</param>
        public void LogIn(Uri url, SecureString username, SecureString password, SecureString otpToken, string appName)
        {
            var driver = _xrmApp._client.Browser.Driver;
            var user = username.ToUnsecureString();

            if (CookieManager.UserLoginCookies.ContainsKey(user) || CookieManager.TryLoadCookies(driver, user))
            {
                CookieManager.LoginViaCookies(driver, url, user);
            }
            else
            {
                _xrmApp.OnlineLogin.Login(url, username, password ?? throw new ArgumentNullException(nameof(password)), otpToken);

                CookieManager.SaveCookies(driver, url, user);
            }

            if (!url.Query.Contains("appid", StringComparison.Ordinal))
                _xrmApp.Navigation.OpenApp(appName);

            driver.WaitForTransaction();

            foreach (var closeButton in driver.FindElements(By.XPath(AppElements.Xpath[AppReference.Application.TeachingBubbleCloseButton])))
                closeButton.Click();

            _xrmApp._client.WaitForMainPage(MainPageTimeout);
        }
    }
}
