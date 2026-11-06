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
            ModelDrivenApp.Login.LogIn(TestConfig.GetTestUrl(), user.Username.ToSecureString(), user.Password?.ToSecureString(), user.OtpToken?.ToSecureString(), appName);
        }

        /// <summary>
        /// Logs in to the configured target as a given user: the model-driven app that the test URL opens (by its appid),
        /// or the TALXIS Portal at the test URL.
        /// </summary>
        /// <param name="userAlias">The alias of the user.</param>
        [Given(@"I am logged in as '(.*)'")]
        public static void GivenIAmLoggedInAs(string userAlias)
        {
            var user = TestConfig.GetUser(userAlias, useCurrentUser: false);
            if (string.IsNullOrEmpty(user.Username))
            {
                throw new InvalidOperationException($"User '{userAlias}' has no username configured. Set it in user secrets or environment variables.");
            }

            App.Login.LogIn(TestConfig.GetTestUrl(), user.Username.ToSecureString(), user.Password?.ToSecureString(), user.OtpToken?.ToSecureString());
        }
    }
}
