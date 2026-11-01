namespace TALXIS.TestKit.Bindings.Steps
{
    using System;
    using System.IO;
    using Newtonsoft.Json;
    using OpenQA.Selenium;
    using Reqnroll;
    using TALXIS.TestKit.Selectors;
    using TALXIS.TestKit.Selectors.Browser;
    using System.Collections.Generic;
    using TALXIS.TestKit.Bindings.Configuration;
    using TALXIS.TestKit.Bindings.Extensions;
    using AngleSharp.Dom;
    using System.Threading;
    using OpenQA.Selenium.DevTools;
    using TALXIS.TestKit.Bindings.Extensions.CookiesManagement;

    /// <summary>
    /// Step bindings related to logging in.
    /// </summary>
    [Binding]
    public class LoginSteps : PowerAppsStepDefiner
    {
        /// <summary>
        /// Logs in to a given app as a given user.
        /// </summary>
        /// <param name="appName">The name of the app.</param>
        /// <param name="userAlias">The alias of the user.</param>
        [Given("I am logged in to the '(.*)' app as '(.*)'")]
        public static void GivenIAmLoggedInToTheAppAs(string appName, string userAlias)
        {
            var user = TestConfig.GetUser(userAlias, useCurrentUser: false);
            var url = TestConfig.GetTestUrl();
            var driver = Driver;

            Login(driver, url, user);

            if (!url.Query.Contains("appid"))
            {
                XrmApp.Navigation.OpenApp(appName);
            }

            Driver.WaitForTransaction();

            CloseTeachingBubbles();

            WaitForMainPage(driver);
        }

        /// <summary>
        /// Logs in to the configured target as a given user: the model-driven app that the test URL opens (by its appid),
        /// or the TALXIS Portal at the test URL.
        /// </summary>
        /// <param name="userAlias">The alias of the user.</param>
        [Given(@"I am logged in as '(.*)'")]
        public static void GivenIAmLoggedInAs(string userAlias)
        {
            switch (TestConfig.Target)
            {
                case TestTarget.Portal:
                    PortalLoginSteps.GivenIAmLoggedInToThePortalAs(userAlias);
                    break;
                case TestTarget.ModelDrivenApp:
                    if (!TestConfig.GetTestUrl().Query.Contains("appid"))
                    {
                        throw new InvalidOperationException("Logging in without naming the app needs the app's appid in the test URL.");
                    }

                    GivenIAmLoggedInToTheAppAs(appName: null, userAlias);
                    break;
                default:
                    throw new InvalidOperationException("Set the Target (ModelDrivenApp or Portal) in the configuration to log in without naming the app.");
            }
        }

        private static void Login(IWebDriver driver, Uri url, UserConfiguration user)
        {
            if (!CookieManager.UserLoginCookies.ContainsKey(user.Username))
            {
                if (!CookieManager.TryLoadCookies(driver, user.Username))
                {
                    LoginViaUi(url, user);

                    CookieManager.SaveCookies(driver, url, user.Username);
                }
                else
                {
                    CookieManager.LoginViaCookies(driver, url, user.Username);
                }
            }
            else
            {
                CookieManager.LoginViaCookies(driver, url, user.Username);
            }
        }

        private static void LoginViaUi(Uri url, UserConfiguration user)
        {
            if (!string.IsNullOrEmpty(user.OtpToken))
            {
                XrmApp.OnlineLogin.Login(url, user.Username.ToSecureString(), user.Password.ToSecureString(), user.OtpToken.ToSecureString());
            }
            else
            {
                XrmApp.OnlineLogin.Login(url, user.Username.ToSecureString(), user.Password.ToSecureString());
            }
        }

        private static void CloseTeachingBubbles()
        {
            foreach (var closeButton in Driver.FindElements(By.ClassName("ms-TeachingBubble-closebutton")))
            {
                closeButton.Click();
            }
        }

        private static bool WaitForMainPage(IWebDriver driver, TimeSpan? timeout = null)
        {
            timeout = timeout ?? TimeSpan.FromSeconds(15);
            var isUCI = driver.HasElement(By.XPath(Elements.Xpath[Reference.Login.CrmUCIMainPage]));

            if (isUCI) driver.WaitForTransaction();

            var element = driver.WaitUntilAvailable(By.XPath(Elements.Xpath[Reference.Login.CrmMainPage]), timeout);

            return element != null;
        }
    }
}
