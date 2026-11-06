using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using OpenQA.Selenium;
using TALXIS.TestKit.Selectors.Browser;

namespace TALXIS.TestKit.Selectors.WebClientManagement
{
    public class WebClient : BrowserPage, IDisposable
    {
        public Guid ClientSessionId { get; }
        public string[] OnlineDomains { get; set; }
        public List<ICommandResult> CommandResults => Browser.CommandResults;

        public WebClient(BrowserOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            Browser = new InteractiveBrowser(options);
            OnlineDomains = Constants.Xrm.XrmDomains;

            ClientSessionId = Guid.NewGuid();
        }

        public void Dispose()
        {
            Browser.Dispose();
        }

        public bool TryLoseFocus()
        {
            return Execute(GetOptions("Try To Lose Focus"), driver =>
            {
                bool isElementFound = driver.TryFindElement(By.XPath("html"), out IWebElement element);

                element.Click();

                return isElementFound;
            });
        }

        internal BrowserCommandOptions GetOptions(string commandName)
        {
            return new BrowserCommandOptions(Constants.DefaultTraceSource,
                commandName,
                Constants.DefaultRetryAttempts,
                Constants.DefaultRetryDelay,
                null,
                true,
                typeof(NoSuchElementException), typeof(StaleElementReferenceException));
        }

        internal BrowserCommandOptions GetNoRetryOptions(string commandName)
        {
            return new BrowserCommandOptions(Constants.DefaultTraceSource, commandName, 1, 0, null, true, null);
        }

        internal BrowserCommandResult<bool> InitializeModes()
        {
            return Execute(GetOptions("Initialize Unified Interface Modes"), driver =>
            {
                driver.SwitchTo().DefaultContent();

                // Wait for main page to load before attempting this. If you don't do this it might still be authenticating and the URL will be wrong
                WaitForMainPage();

                string uri = driver.Url;
                if (string.IsNullOrEmpty(uri))
                    return false;

                var prevQuery = GetUrlQueryParams(uri);
                bool requireRedirect = false;
                string queryParams = "";
                if (prevQuery.Get("flags") == null)
                {
                    queryParams += "&flags=easyreproautomation=true";
                    if (Browser.Options.UCITestMode)
                        queryParams += ",testmode=true";
                    requireRedirect = true;
                }

                if (Browser.Options.UCIPerformanceMode && prevQuery.Get("perf") == null)
                {
                    queryParams += "&perf=true";
                    requireRedirect = true;
                }

                if (!requireRedirect)
                    return true;

                var testModeUri = uri + queryParams;
                driver.Navigate().GoToUrl(testModeUri);

                // Again wait for loading
                WaitForMainPage();
                return true;
            });
        }

        internal void ThinkTime(int milliseconds)
        {
            Browser.ThinkTime(milliseconds);
        }

        internal void ThinkTime(TimeSpan timespan)
        {
            ThinkTime((int)timespan.TotalMilliseconds);
        }
        #region PageWaits
        internal NameValueCollection GetUrlQueryParams(string url)
        {
            if (string.IsNullOrEmpty(url))
                return null;

            Uri uri = new Uri(url);
            var query = uri.Query.ToLower();
            NameValueCollection result = HttpUtility.ParseQueryString(query);
            return result;
        }

        internal bool WaitForMainPage(TimeSpan timeout, string errorMessage)
                    => WaitForMainPage(timeout, null, () => throw new InvalidOperationException(errorMessage));

        internal bool WaitForMainPage(TimeSpan? timeout = null, Action<IWebElement> successCallback = null, Action failureCallback = null)
        {
            IWebDriver driver = Browser.Driver;
            timeout = timeout ?? Constants.DefaultTimeout;
            successCallback = successCallback ?? (
                                  _ =>
                                  {
                                      bool isUCI = driver.HasElement(By.XPath(Elements.Xpath[Reference.Login.CrmUCIMainPage]));
                                      if (isUCI)
                                          driver.WaitForTransaction();
                                  });

            var xpathToMainPage = By.XPath(Elements.Xpath[Reference.Login.CrmMainPage]);
            var element = driver.WaitUntilAvailable(xpathToMainPage, timeout, successCallback, failureCallback);
            return element != null;
        }
        #endregion

    }
}
