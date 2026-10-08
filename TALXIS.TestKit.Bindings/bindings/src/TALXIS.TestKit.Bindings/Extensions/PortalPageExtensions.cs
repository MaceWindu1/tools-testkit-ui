namespace TALXIS.TestKit.Bindings.Extensions
{
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using OpenQA.Selenium;

    /// <summary>
    /// Works with the navigation (sitemap) and the form tabs of TALXIS Portal pages.
    /// </summary>
    /// <remarks>
    /// The portal renders the sitemap either as a horizontal command bar (items carry <c>data-id="sitemap-entity-{subarea id}"</c>,
    /// a disabled item has <c>aria-disabled</c> and its disabled message as <c>title</c>; hidden items are not rendered) or as a
    /// vertical Fluent navigation. Form tabs are command bar buttons with <c>data-id="tablist-{tab name}"</c> showing the tab label
    /// without its <c>#Icon#</c> prefix; hidden tabs are not rendered, the current one has <c>data-selected-top-level</c>.
    /// </remarks>
    public static class PortalPageExtensions
    {
        /// <summary>How long a condition is watched to confirm that something does not appear.</summary>
        public static readonly TimeSpan AbsenceSettleTime = TimeSpan.FromSeconds(3);

        private const string HorizontalSiteMapItems = "[data-id^='sitemap-entity-']";
        private const string VerticalSiteMapItems = ".TALXIS__navigation--vertical .ms-Nav-link";
        private const string TabBars = ".TALXIS__form__tab";
        private const string ModalSelector = "div[role='dialog'], div[role='alertdialog']";
        private static readonly Regex Whitespace = new Regex(@"\s+");
        private static readonly Regex IconGlyphs = new Regex(@"[-]");

        /// <summary>
        /// Finds a navigation item by its label or by its sitemap subarea id.
        /// </summary>
        /// <param name="driver">The web driver.</param>
        /// <param name="labelOrId">The label shown in the navigation, or the subarea id.</param>
        /// <returns>The item, or null when it is not displayed.</returns>
        public static IWebElement FindPortalSiteMapItem(this IWebDriver driver, string labelOrId)
        {
            var horizontal = driver.FindElements(By.CssSelector(HorizontalSiteMapItems))
                .FirstOrDefault(item => item.GetAttribute("data-id") == $"sitemap-entity-{labelOrId}" || NormalizeText(item.Text) == labelOrId);
            if (horizontal != null)
            {
                return horizontal;
            }

            return driver.FindElements(By.CssSelector(VerticalSiteMapItems))
                .FirstOrDefault(item => item.Displayed && NormalizeText(item.Text) == labelOrId);
        }

        /// <summary>
        /// Gets a value indicating whether a navigation item or command is disabled.
        /// </summary>
        /// <param name="item">The navigation item or command.</param>
        /// <returns>Whether it is disabled.</returns>
        public static bool IsPortalElementDisabled(this IWebElement item)
        {
            return item.GetAttribute("aria-disabled") == "true"
                || item.GetAttribute("disabled") != null
                || (item.GetAttribute("class") ?? string.Empty).Split(' ').Contains("is-disabled");
        }

        /// <summary>
        /// Finds a tab by its label or name, in the topmost dialog when one with tabs is open, otherwise on the page.
        /// Tabs moved into the tab bar's overflow menu are found by opening it.
        /// </summary>
        /// <param name="driver">The web driver.</param>
        /// <param name="labelOrName">The tab label (without its icon) or the tab name.</param>
        /// <returns>The tab button or overflow menu item, or null when the tab is not displayed.</returns>
        public static IWebElement FindPortalTab(this IWebDriver driver, string labelOrName)
        {
            var scope = GetTopmostModalWithTabs(driver) ?? (ISearchContext)driver;
            var bars = scope.FindElements(By.CssSelector(TabBars)).Where(bar => bar.Displayed).ToList();

            var tab = bars.SelectMany(bar => bar.FindElements(By.CssSelector("[data-id^='tablist-']")))
                .FirstOrDefault(button => IsTab(button, labelOrName));
            if (tab != null)
            {
                return tab;
            }

            foreach (var overflow in bars.SelectMany(bar => bar.FindElements(By.CssSelector(".ms-CommandBar-overflowButton"))).Where(button => button.Displayed))
            {
                overflow.Click();
                var menuItem = driver.FindElements(By.CssSelector(".ms-ContextualMenu [data-id^='tablist-']"))
                    .FirstOrDefault(item => IsTab(item, labelOrName));
                if (menuItem != null)
                {
                    return menuItem;
                }

                CloseMenus(driver);
            }

            return null;
        }

        /// <summary>
        /// Gets a value indicating whether a tab is the current one.
        /// </summary>
        /// <param name="tab">The tab button.</param>
        /// <returns>Whether the tab is selected.</returns>
        public static bool IsPortalTabSelected(this IWebElement tab)
        {
            return tab.GetAttribute("data-selected-top-level") == "true" || tab.GetAttribute("data-selected") == "true";
        }

        /// <summary>
        /// Closes open menus (e.g. an overflow menu opened to look for an item).
        /// </summary>
        /// <param name="driver">The web driver.</param>
        public static void CloseMenus(this IWebDriver driver)
        {
            if (driver.FindElements(By.CssSelector(".ms-ContextualMenu")).Count > 0)
            {
                driver.FindElement(By.TagName("body")).SendKeys(Keys.Escape);
            }
        }

        /// <summary>
        /// Clicks an element, retrying while another element covers it: page scripts show the portal's progress overlay
        /// (<c>.ms-Overlay</c>) while they load data after the page has rendered.
        /// </summary>
        /// <param name="driver">The web driver.</param>
        /// <param name="findElement">Finds the element to click; it is looked up again for each attempt.</param>
        /// <param name="timeout">How long to keep trying.</param>
        /// <param name="description">The element's description for the error message.</param>
        public static void ClickWhenUnblocked(this IWebDriver driver, Func<IWebElement> findElement, TimeSpan timeout, string description)
        {
            ElementClickInterceptedException lastInterception = null;
            var clicked = driver.WaitForPortalCondition(
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
        /// Waits until something is absent and then checks that it stays absent for <see cref="AbsenceSettleTime"/>. Use it
        /// for things that scripts or refreshes remove after they were rendered (navigation items and tabs hidden once the
        /// page has loaded, rows and commands after a grid refresh).
        /// </summary>
        /// <param name="driver">The web driver.</param>
        /// <param name="isPresent">Whether the watched thing is present.</param>
        /// <param name="timeout">How long to wait for it to disappear.</param>
        /// <returns>Whether it disappeared and stayed absent.</returns>
        public static bool BecomesAbsent(this IWebDriver driver, Func<bool> isPresent, TimeSpan timeout)
        {
            return driver.WaitForPortalCondition(() => !isPresent(), timeout) && StaysAbsent(isPresent);
        }

        /// <summary>
        /// Watches a condition for <see cref="AbsenceSettleTime"/> and returns whether it never became true.
        /// Use it to assert that something does not appear on a page that renders asynchronously.
        /// </summary>
        /// <param name="isPresent">Whether the watched thing is present.</param>
        /// <returns>Whether it stayed absent.</returns>
        public static bool StaysAbsent(Func<bool> isPresent)
        {
            var deadline = DateTime.UtcNow + AbsenceSettleTime;
            do
            {
                if (SafeEvaluate(isPresent))
                {
                    return false;
                }

                Thread.Sleep(250);
            }
            while (DateTime.UtcNow < deadline);

            return !SafeEvaluate(isPresent);
        }

        /// <summary>
        /// Removes icon glyphs, collapses whitespace and trims the text of an element as it is displayed. Fluent UI font
        /// icons (e.g. of tabs and commands) render their glyph as a Private Use Area character in the element's text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The normalized text.</returns>
        public static string NormalizeText(string text)
        {
            return Whitespace.Replace(IconGlyphs.Replace(text ?? string.Empty, string.Empty), " ").Trim();
        }

        /// <summary>
        /// Gets the topmost modal dialog (form dialog, alert or confirmation), if any.
        /// </summary>
        /// <param name="driver">The web driver.</param>
        /// <returns>The modal, or null.</returns>
        public static IWebElement GetTopmostPortalModal(this IWebDriver driver)
        {
            return driver.FindElements(By.CssSelector(ModalSelector)).LastOrDefault(modal => modal.Displayed);
        }

        private static IWebElement GetTopmostModalWithTabs(IWebDriver driver)
        {
            var modal = driver.GetTopmostPortalModal();
            return modal != null && modal.FindElements(By.CssSelector(TabBars)).Count > 0 ? modal : null;
        }

        private static bool IsTab(IWebElement element, string labelOrName)
        {
            return element.GetAttribute("data-id") == $"tablist-{labelOrName}" || NormalizeText(element.Text) == labelOrName;
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
