using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using TALXIS.TestKit.Selectors.Browser;
using TALXIS.TestKit.Selectors.DTO.Locators;

namespace TALXIS.TestKit.Selectors.WebClientManagement
{
    /// <summary>
    /// What all pages of a TALXIS Portal share: whether the portal is ready, waiting for a page that renders asynchronously,
    /// clicking through the progress overlay, menus and the topmost modal.
    /// </summary>
    public class PortalPageManager
    {
        /// <summary>How long a condition is watched to confirm that something does not appear.</summary>
        internal static readonly TimeSpan AbsenceSettleTime = TimeSpan.FromSeconds(3);

        private const string ErrorStatePrefix = "error:";

        // The portal hides its loading screen (MainLoading) once its navigation bar has loaded (window.TALXIS.Portal.Context
        // .isNavbarLoaded) and shows a page once it has loaded (isLoading). It publishes no ready signal of its own, and
        // window.Xrm is assigned at boot, before the portal has loaded. A dialog shown during loading (e.g. a customer
        // selection) waits for input, so the portal is ready to be interacted with.
        private const string PortalStateScript = @"
            const [errorIconSelector, modalSelector] = arguments;
            const errorIcon = document.querySelector(errorIconSelector);
            if (errorIcon) {
                const dialog = errorIcon.closest('[role=""dialog""], [role=""alertdialog""]') ?? document.body;
                const details = [...dialog.querySelectorAll('textarea')].map(t => t.value).join(' ');
                return '" + ErrorStatePrefix + @"' + (dialog.innerText + ' ' + details).replace(/\s+/g, ' ').trim();
            }
            const context = window.TALXIS && window.TALXIS.Portal && window.TALXIS.Portal.Context;
            const loaded = !!context && context.isNavbarLoaded === true && context.isLoading === false;
            const waitingForInput = !!document.querySelector(modalSelector);
            return loaded || (!!context && waitingForInput) ? 'ready' : null;";

        private static readonly Regex Whitespace = new Regex(@"\s+");
        // The Private Use Area, where icon fonts put their glyphs.
        private static readonly Regex IconGlyphs = new Regex("[\uE000-\uF8FF]");

        public PortalPageManager(WebClient client)
        {
            Client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public WebClient Client { get; }

        /// <summary>
        /// Waits until the portal has loaded (or waits for input in a dialog), failing fast when it shows an error.
        /// </summary>
        internal BrowserCommandResult<bool> WaitForReady(TimeSpan timeout, string message)
        {
            return Client.Execute(Client.GetNoRetryOptions("Wait For Portal Ready"), driver => WaitForReady(driver, timeout, message));
        }

        internal static bool WaitForReady(IWebDriver driver, TimeSpan timeout, string message)
        {
            var wait = new WebDriverWait(driver, timeout) { Message = message };
            var state = wait.Until(d => (string)((IJavaScriptExecutor)d).ExecuteScript(
                PortalStateScript,
                PortalPageElementsLocators.ErrorIcon,
                PortalPageElementsLocators.ModalSelector));

            if (state.StartsWith(ErrorStatePrefix, StringComparison.Ordinal))
                throw new InvalidOperationException($"The portal shows an error: {state.Substring(ErrorStatePrefix.Length)}");

            return true;
        }

        /// <summary>
        /// Waits until a condition is met, returning whether it was met within the timeout.
        /// </summary>
        internal static bool WaitFor(IWebDriver driver, Func<bool> condition, TimeSpan timeout)
        {
            var wait = new WebDriverWait(driver, timeout);
            wait.IgnoreExceptionTypes(typeof(NoSuchElementException), typeof(StaleElementReferenceException));
            try
            {
                return wait.Until(_ => condition());
            }
            catch (WebDriverTimeoutException)
            {
                return false;
            }
        }

        /// <summary>
        /// Waits until something is found and returns it, failing with a message.
        /// </summary>
        internal static T WaitUntil<T>(IWebDriver driver, Func<IWebDriver, T> find, TimeSpan timeout, string message)
        {
            var wait = new WebDriverWait(driver, timeout) { Message = message };
            wait.IgnoreExceptionTypes(typeof(NoSuchElementException), typeof(StaleElementReferenceException));
            return wait.Until(find);
        }

        /// <summary>
        /// Waits until something is absent and then checks that it stays absent for <see cref="AbsenceSettleTime"/>. Use it
        /// for things that scripts or refreshes remove after they were rendered (navigation items and tabs hidden once the
        /// page has loaded, rows and commands after a grid refresh).
        /// </summary>
        internal static bool BecomesAbsent(IWebDriver driver, Func<bool> isPresent, TimeSpan timeout)
        {
            return WaitFor(driver, () => !isPresent(), timeout) && StaysAbsent(isPresent);
        }

        /// <summary>
        /// Watches a condition for <see cref="AbsenceSettleTime"/> and returns whether it never became true. Use it to assert
        /// that something does not appear on a page that renders asynchronously.
        /// </summary>
        internal static bool StaysAbsent(Func<bool> isPresent)
        {
            var deadline = DateTime.UtcNow + AbsenceSettleTime;
            do
            {
                if (SafeEvaluate(isPresent))
                    return false;

                Thread.Sleep(250);
            }
            while (DateTime.UtcNow < deadline);

            return !SafeEvaluate(isPresent);
        }

        /// <summary>
        /// Clicks an element, retrying while another element covers it: page scripts show the portal's progress overlay
        /// (<c>.ms-Overlay</c>) while they load data after the page has rendered.
        /// </summary>
        internal static void ClickWhenUnblocked(IWebDriver driver, Func<IWebElement> findElement, TimeSpan timeout, string description)
        {
            ElementClickInterceptedException lastInterception = null;
            var clicked = WaitFor(
                driver,
                () =>
                {
                    try
                    {
                        (findElement() ?? throw new NoSuchElementException($"{description} is not displayed.")).Click();
                        return true;
                    }
                    catch (ElementClickInterceptedException e)
                    {
                        lastInterception = e;
                        return false;
                    }
                },
                timeout);

            if (!clicked)
            {
                throw lastInterception != null
                    ? new WebDriverTimeoutException($"{description} stayed covered by another element for {timeout.TotalSeconds} seconds.", lastInterception)
                    : new NoSuchElementException($"{description} is not displayed.");
            }
        }

        /// <summary>
        /// Closes open menus (e.g. an overflow menu opened to look for an item).
        /// </summary>
        internal static void CloseMenus(IWebDriver driver)
        {
            if (driver.FindElements(PortalPageElementsLocators.ContextualMenu).Count > 0)
                driver.FindElement(By.TagName("body")).SendKeys(Keys.Escape);
        }

        /// <summary>
        /// Gets the topmost modal dialog (form dialog, alert or confirmation), if any.
        /// </summary>
        internal static IWebElement GetTopmostModal(IWebDriver driver)
        {
            return driver.FindElements(PortalPageElementsLocators.Modal).LastOrDefault(modal => modal.Displayed);
        }

        /// <summary>
        /// Gets a value indicating whether a navigation item or command is disabled.
        /// </summary>
        internal static bool IsDisabled(IWebElement item)
        {
            return item.GetAttribute("aria-disabled") == "true"
                || item.GetAttribute("disabled") != null
                || (item.GetAttribute("class") ?? string.Empty).Split(' ').Contains("is-disabled");
        }

        /// <summary>
        /// Gets a value indicating whether an element was removed or hidden.
        /// </summary>
        internal static bool IsStale(IWebElement element)
        {
            try
            {
                return element == null || !element.Displayed;
            }
            catch (StaleElementReferenceException)
            {
                return true;
            }
        }

        /// <summary>
        /// Removes icon glyphs, collapses whitespace and trims the text of an element as it is displayed. Fluent UI font
        /// icons (e.g. of tabs and commands) render their glyph as a Private Use Area character in the element's text.
        /// </summary>
        internal static string NormalizeText(string text)
        {
            return Whitespace.Replace(IconGlyphs.Replace(text ?? string.Empty, string.Empty), " ").Trim();
        }

        private static bool SafeEvaluate(Func<bool> condition)
        {
            try
            {
                return condition();
            }
            catch (WebDriverException)
            {
                return false;
            }
        }
    }
}
