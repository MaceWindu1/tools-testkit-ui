namespace TALXIS.TestKit.Bindings.Extensions
{
    using System;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Support.UI;

    /// <summary>
    /// Extensions for pages of a TALXIS Portal.
    /// </summary>
    public static class PortalExtensions
    {
        private const string ErrorStatePrefix = "error:";

        // The Xrm API is available before the portal finishes loading; its loading screen (MainLoading) is the only
        // element showing the TALXIS logo by this absolute path (the navbar logo uses a relative one). A dialog shown
        // during loading (e.g. a customer selection) waits for input, so the portal is ready to be interacted with.
        private const string PortalStateScript = @"
            const errorIcon = document.querySelector('.TALXIS-Portal-errorDialog-title-icon');
            if (errorIcon) {
                const dialog = errorIcon.closest('[role=""dialog""], [role=""alertdialog""]') ?? document.body;
                const details = [...dialog.querySelectorAll('textarea')].map(t => t.value).join(' ');
                return '" + ErrorStatePrefix + @"' + (dialog.innerText + ' ' + details).replace(/\s+/g, ' ').trim();
            }
            const xrmReady = typeof Xrm !== 'undefined' && !!Xrm.WebApi && !!Xrm.Navigation;
            const loading = !!document.querySelector(""img[src='/images/talxis_logo_black.png']"");
            const waitingForInput = !!document.querySelector('[role=""dialog""], [role=""alertdialog""]');
            return xrmReady && (!loading || waitingForInput) ? 'ready' : null;";

        /// <summary>
        /// Waits until the portal has loaded (or waits for input in a dialog), failing fast when it shows an error.
        /// </summary>
        /// <param name="driver">The web driver.</param>
        /// <param name="timeout">How long to wait.</param>
        /// <param name="message">The message of the timeout exception.</param>
        public static void WaitForPortalReady(this IWebDriver driver, TimeSpan timeout, string message)
        {
            var wait = new WebDriverWait(driver, timeout) { Message = message };
            var state = wait.Until(d => (string)((IJavaScriptExecutor)d).ExecuteScript(PortalStateScript));

            if (state.StartsWith(ErrorStatePrefix, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"The portal shows an error: {state.Substring(ErrorStatePrefix.Length)}");
            }
        }
    }
}
