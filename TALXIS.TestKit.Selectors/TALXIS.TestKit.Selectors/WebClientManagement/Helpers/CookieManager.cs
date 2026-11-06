using System;
using System.IO;
using Newtonsoft.Json;
using OpenQA.Selenium;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;

namespace TALXIS.TestKit.Selectors.WebClientManagement.Helpers
{
    internal static class CookieManager
    {
        internal static readonly ConcurrentDictionary<string, IEnumerable<Cookie>> UserLoginCookies;

        private const string CookieFilePath = "cookies";
        private const string LoginUrl = "https://login.microsoftonline.com/";
        private const string FakeLoginUrl = "https://login.microsoftonline.com/fake/login";

        static CookieManager()
        {
            UserLoginCookies = new ConcurrentDictionary<string, IEnumerable<Cookie>>();
        }

        internal static void LoginViaCookies(IWebDriver driver, Uri orgUrl, string username)
        {
            driver.Navigate().GoToUrl(FakeLoginUrl);

            foreach (var cookie in UserLoginCookies[username])
            {
                driver.Manage().Cookies.AddCookie(cookie);
            }

            driver.Navigate().GoToUrl(orgUrl);
        }

        internal static void SaveCookies(IWebDriver driver, Uri returnUrl, string userName)
        {
            driver.Navigate().GoToUrl(LoginUrl);
            var cookies = driver.Manage().Cookies.AllCookies;
            driver.Navigate().GoToUrl(returnUrl);

            var requiredCookieNames = new HashSet<string>
            {
                "ESTSAUTH",
                "ESTSAUTHPERSISTENT",
                "buid",
                "esctx" 
            };

            var filteredCookies = cookies
                .Where(cookie => requiredCookieNames.Contains(cookie.Name) || cookie.Name.StartsWith("esctx-"))
                .ToList();

            UserLoginCookies.TryAdd(userName, filteredCookies);

            var userCookieFilePath = $"{CookieFilePath}_{userName}.json";

            File.WriteAllText(
                userCookieFilePath,
                JsonConvert.SerializeObject(filteredCookies.Select(cookie => new SerializableCookie(cookie))));
        }

        internal static bool TryLoadCookies(IWebDriver driver, string userName)
        {
            var userCookieFilePath = $"{CookieFilePath}_{userName}.json";

            if (!File.Exists(userCookieFilePath))
            {
                return false;
            }

            try
            {
                var currentDateTime = DateTime.Now;

                var cookies = JsonConvert.DeserializeObject<List<SerializableCookie>>(File.ReadAllText(userCookieFilePath));

                var seleniumCookieList = new List<Cookie>();

                foreach (var cookie in cookies)
                {
                    var seleniumCookie = new Cookie(cookie.Name, cookie.Value, cookie.Domain, cookie.Path, cookie.Expiry);

                    if (seleniumCookie.Expiry is not null && seleniumCookie.Expiry < currentDateTime)
                    {
                        UserLoginCookies.TryRemove(userName, out _);

                        File.Delete(userCookieFilePath);

                        return false;
                    }

                    seleniumCookieList.Add(seleniumCookie);
                }

                UserLoginCookies.TryAdd(userName, seleniumCookieList);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
