using System;
using System.Linq;
using OpenQA.Selenium;
using TALXIS.TestKit.Selectors.Browser;
using TALXIS.TestKit.Selectors.DTO.Locators;

namespace TALXIS.TestKit.Selectors.WebClientManagement
{
    /// <summary>
    /// The navigation (site map) of a TALXIS Portal.
    /// </summary>
    /// <remarks>
    /// The portal renders the sitemap either as a horizontal command bar (items carry <c>data-id="sitemap-entity-{subarea id}"</c>,
    /// a disabled item has <c>aria-disabled</c> and its disabled message as <c>title</c>; hidden items are not rendered) or as a
    /// vertical Fluent navigation.
    /// </remarks>
    public class PortalNavigationManager
    {
        private static readonly TimeSpan NavigationTimeout = TimeSpan.FromSeconds(10);
        private static readonly TimeSpan PageLoadTimeout = TimeSpan.FromSeconds(60);

        public PortalNavigationManager(WebClient client)
        {
            Client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public WebClient Client { get; }

        internal BrowserCommandResult<bool> OpenPage(string label)
        {
            return Client.Execute(Client.GetNoRetryOptions($"Open Portal Page: {label}"), driver =>
            {
                if (!PortalPageManager.WaitFor(driver, () => FindSiteMapItem(driver, label) is { } item && !PortalPageManager.IsDisabled(item), NavigationTimeout))
                    throw new NoSuchElementException($"The '{label}' subarea is not shown or not enabled.");

                PortalPageManager.ClickWhenUnblocked(driver, () => FindSiteMapItem(driver, label), NavigationTimeout, $"The '{label}' subarea");
                return PortalPageManager.WaitForReady(driver, PageLoadTimeout, $"The '{label}' page did not finish loading.");
            });
        }

        internal BrowserCommandResult<bool> IsSubAreaShown(string label, bool shown)
        {
            return Client.Execute(Client.GetNoRetryOptions($"Is Portal Subarea Shown: {label}"), driver => shown
                ? PortalPageManager.WaitFor(driver, () => FindSiteMapItem(driver, label) != null, NavigationTimeout)
                : PortalPageManager.BecomesAbsent(driver, () => FindSiteMapItem(driver, label) != null, NavigationTimeout));
        }

        internal BrowserCommandResult<bool> IsSubAreaDisabled(string label, string message)
        {
            return Client.Execute(Client.GetNoRetryOptions($"Is Portal Subarea Disabled: {label}"), driver =>
                PortalPageManager.WaitFor(
                    driver,
                    () => FindSiteMapItem(driver, label) is { } item && PortalPageManager.IsDisabled(item) && item.GetAttribute("title") == message,
                    NavigationTimeout));
        }

        /// <summary>
        /// Finds a navigation item by its label or by its sitemap subarea id.
        /// </summary>
        /// <returns>The item, or null when it is not displayed.</returns>
        internal static IWebElement FindSiteMapItem(IWebDriver driver, string labelOrId)
        {
            var horizontal = driver.FindElements(PortalNavigationElementsLocators.HorizontalItems)
                .FirstOrDefault(item => item.GetAttribute("data-id") == PortalNavigationElementsLocators.ItemIdPrefix + labelOrId
                    || PortalPageManager.NormalizeText(item.Text) == labelOrId);
            if (horizontal != null)
                return horizontal;

            return driver.FindElements(PortalNavigationElementsLocators.VerticalItems)
                .FirstOrDefault(item => item.Displayed && PortalPageManager.NormalizeText(item.Text) == labelOrId);
        }
    }
}
