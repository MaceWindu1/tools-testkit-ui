using System;
using System.Linq;
using OpenQA.Selenium;
using TALXIS.TestKit.Selectors.Browser;
using TALXIS.TestKit.Selectors.DTO.Locators;

namespace TALXIS.TestKit.Selectors.WebClientManagement
{
    /// <summary>
    /// The tabs and fields of a TALXIS Portal form: of the topmost form dialog when one is open, otherwise of the page.
    /// </summary>
    /// <remarks>
    /// Form tabs are command bar buttons with <c>data-id="tablist-{tab name}"</c> showing the tab label without its
    /// <c>#Icon#</c> prefix; hidden tabs are not rendered, the current one has <c>data-selected-top-level</c>. A field is a
    /// form cell (<c>.TALXIS__form__cell</c>) with the label as its label's title and the control's data-id starting with
    /// the logical name.
    /// </remarks>
    public class PortalFormManager
    {
        // Form scripts set states (e.g. the requirement level) after the form loads.
        internal static readonly TimeSpan StateTimeout = TimeSpan.FromSeconds(10);

        // Tabs and fields of a page (or of the record selected in its subgrid) can render well after the page loads.
        internal static readonly TimeSpan RenderTimeout = TimeSpan.FromSeconds(30);

        // How long a click on a tab is given to select it before the tab is clicked again.
        private static readonly TimeSpan TabClickSettleTime = TimeSpan.FromSeconds(3);

        // How long a tab is watched before it is taken as not selected (it can be replaced while it is being read).
        private static readonly TimeSpan TabStateCheckTime = TimeSpan.FromSeconds(1);

        public PortalFormManager(WebClient client)
        {
            Client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public WebClient Client { get; }

        internal BrowserCommandResult<bool> SelectTab(string tab)
        {
            return Client.Execute(Client.GetNoRetryOptions($"Select Portal Tab: {tab}"), driver =>
            {
                PortalPageManager.WaitFor(driver, () => FindTab(driver, tab) != null, RenderTimeout);
                var selected = SelectTab(driver, tab, RenderTimeout);
                PortalPageManager.CloseMenus(driver);
                if (!selected)
                    throw new InvalidOperationException($"The '{tab}' tab did not become the selected one after clicking it.");

                return true;
            });
        }

        internal BrowserCommandResult<bool> IsTabShown(string tab, bool shown)
        {
            return Client.Execute(Client.GetNoRetryOptions($"Is Portal Tab Shown: {tab}"), driver =>
            {
                // Tabs are shown or hidden by scripts after the page loads.
                var reached = shown
                    ? PortalPageManager.WaitFor(driver, () => FindTab(driver, tab) != null, RenderTimeout)
                    : PortalPageManager.BecomesAbsent(driver, () => FindTab(driver, tab) != null, StateTimeout);
                PortalPageManager.CloseMenus(driver);
                return reached;
            });
        }

        internal BrowserCommandResult<bool> IsTabSelected(string tab)
        {
            return Client.Execute(Client.GetNoRetryOptions($"Is Portal Tab Selected: {tab}"), driver =>
            {
                var selected = PortalPageManager.WaitFor(driver, () => IsTabSelected(FindTab(driver, tab)), StateTimeout);
                PortalPageManager.CloseMenus(driver);
                return selected;
            });
        }

        internal BrowserCommandResult<bool> SetValue(string field, string value)
        {
            return Client.Execute(Client.GetNoRetryOptions($"Set Portal Field Value: {field}"), driver =>
            {
                SetValue(driver, FindField(driver, field), value);
                return true;
            });
        }

        internal BrowserCommandResult<string> GetValue(string field, string expected, TimeSpan timeout)
        {
            return Client.Execute(Client.GetNoRetryOptions($"Get Portal Field Value: {field}"), driver =>
            {
                // Values may be filled in asynchronously (e.g. a nested form showing the selected record, onchange handlers).
                PortalPageManager.WaitFor(driver, () => GetValue(FindField(driver, field)) == expected, timeout);
                return GetValue(FindField(driver, field));
            });
        }

        internal BrowserCommandResult<bool> IsFieldShown(string field, bool shown)
        {
            return Client.Execute(Client.GetNoRetryOptions($"Is Portal Field Shown: {field}"), driver => shown
                ? PortalPageManager.WaitFor(driver, () => TryFindField(driver, field)?.Displayed == true, RenderTimeout)
                : PortalPageManager.BecomesAbsent(driver, () => TryFindField(driver, field)?.Displayed == true, StateTimeout));
        }

        internal BrowserCommandResult<bool> IsFieldRequired(string field, bool required)
        {
            return Client.Execute(Client.GetNoRetryOptions($"Is Portal Field Required: {field}"), driver =>
                PortalPageManager.WaitFor(driver, () => IsRequired(FindField(driver, field)) == required, StateTimeout));
        }

        internal BrowserCommandResult<bool> IsFieldEditable(string field, bool editable)
        {
            return Client.Execute(Client.GetNoRetryOptions($"Is Portal Field Editable: {field}"), driver =>
                PortalPageManager.WaitFor(driver, () => FindField(driver, field) is { } cell && (cell.Displayed && !IsReadOnly(cell)) == editable, StateTimeout));
        }

        internal BrowserCommandResult<bool> IsFieldEmpty(string field)
        {
            return Client.Execute(Client.GetNoRetryOptions($"Is Portal Field Empty: {field}"), driver =>
                PortalPageManager.WaitFor(driver, () => GetValue(FindField(driver, field)).Length == 0, StateTimeout));
        }

        internal BrowserCommandResult<bool> HasFieldError(string field, string message)
        {
            // The control renders the error inside the field's form cell; scripts validate asynchronously.
            return Client.Execute(Client.GetNoRetryOptions($"Has Portal Field Error: {field}"), driver =>
                PortalPageManager.WaitFor(driver, () => FindField(driver, field).Text.Contains(message, StringComparison.Ordinal), StateTimeout));
        }

        /// <summary>
        /// Finds a tab by its label or name, in the topmost dialog when one with tabs is open, otherwise on the page. Tabs
        /// moved into the tab bar's overflow menu are found by opening it.
        /// </summary>
        /// <returns>The tab button or overflow menu item, or null when the tab is not displayed.</returns>
        internal static IWebElement FindTab(IWebDriver driver, string labelOrName)
        {
            var scope = GetTopmostModalWithTabs(driver) ?? (ISearchContext)driver;
            var bars = scope.FindElements(PortalTabElementsLocators.Bars).Where(bar => bar.Displayed).ToList();

            var tab = bars.SelectMany(bar => bar.FindElements(PortalTabElementsLocators.Tab)).FirstOrDefault(button => IsTab(button, labelOrName));
            if (tab != null)
                return tab;

            foreach (var overflow in bars.SelectMany(bar => bar.FindElements(PortalPageElementsLocators.OverflowButton)).Where(button => button.Displayed))
            {
                overflow.Click();
                var menuItem = driver.FindElements(PortalTabElementsLocators.MenuTab).FirstOrDefault(item => IsTab(item, labelOrName));
                if (menuItem != null)
                    return menuItem;

                PortalPageManager.CloseMenus(driver);
            }

            return null;
        }

        /// <summary>
        /// Gets a value indicating whether a tab is the current one.
        /// </summary>
        internal static bool IsTabSelected(IWebElement tab)
        {
            return tab != null && (tab.GetAttribute("data-selected-top-level") == "true" || tab.GetAttribute("data-selected") == "true");
        }

        /// <summary>
        /// Selects a tab, clicking it again while it is not selected: the tabs re-render while the page loads, and a page can
        /// drop a click on a tab while it is still loading the current one. The tab is looked up again for every check and click.
        /// </summary>
        /// <returns>Whether the tab became the selected one.</returns>
        internal static bool SelectTab(IWebDriver driver, string labelOrName, TimeSpan timeout)
        {
            // Checked through WaitFor, which looks a tab up again when it was replaced while being read.
            bool IsSelectedWithin(TimeSpan wait) => PortalPageManager.WaitFor(driver, () => IsTabSelected(FindTab(driver, labelOrName)), wait);

            var deadline = DateTime.UtcNow + timeout;
            while (!IsSelectedWithin(TabStateCheckTime))
            {
                var remaining = deadline - DateTime.UtcNow;
                if (remaining <= TimeSpan.Zero)
                    return false;

                PortalPageManager.ClickWhenUnblocked(driver, () => FindTab(driver, labelOrName), remaining, $"The '{labelOrName}' tab");
                IsSelectedWithin(remaining < TabClickSettleTime ? remaining : TabClickSettleTime);
            }

            return true;
        }

        /// <summary>
        /// Finds a field of the topmost form dialog (or of the page when no form dialog is open) by its label or logical name.
        /// </summary>
        /// <returns>The form cell of the field.</returns>
        internal static IWebElement FindField(IWebDriver driver, string labelOrName)
        {
            return PortalPageManager.WaitUntil(
                driver,
                d => GetFormScope(d).FindElement(PortalFormElementsLocators.FieldCell(labelOrName)),
                RenderTimeout,
                $"The field '{labelOrName}' was not found on the form.");
        }

        /// <summary>
        /// Finds a field like <see cref="FindField"/> without waiting for it.
        /// </summary>
        /// <returns>The form cell of the field, or null when it is not rendered.</returns>
        internal static IWebElement TryFindField(IWebDriver driver, string labelOrName)
        {
            return GetFormScope(driver).FindElements(PortalFormElementsLocators.FieldCell(labelOrName)).FirstOrDefault();
        }

        /// <summary>
        /// Gets a value indicating whether a field is marked as required.
        /// </summary>
        internal static bool IsRequired(IWebElement cell)
        {
            return cell.FindElements(PortalFormElementsLocators.RequiredIndicator).Count > 0;
        }

        /// <summary>
        /// Gets a value indicating whether a field is read-only (shown with a lock icon).
        /// </summary>
        internal static bool IsReadOnly(IWebElement cell)
        {
            return cell.FindElements(PortalFormElementsLocators.LockIcon).Count > 0;
        }

        /// <summary>
        /// Gets the displayed value of a field.
        /// </summary>
        /// <returns>The value, or an empty string.</returns>
        internal static string GetValue(IWebElement cell)
        {
            var lookupValue = cell.FindElements(PortalFormElementsLocators.LookupValue);
            if (lookupValue.Count > 0)
                return lookupValue[0].GetAttribute("title") ?? lookupValue[0].Text;

            var input = cell.FindElements(PortalFormElementsLocators.ValueInput);
            if (input.Count > 0)
                return input[0].GetAttribute("value") ?? string.Empty;

            // Read-only controls may show their value as text.
            var control = cell.FindElements(PortalFormElementsLocators.ReadOnlyValue).FirstOrDefault();
            return control != null ? PortalPageManager.NormalizeText(control.Text) : string.Empty;
        }

        /// <summary>
        /// Sets the value of a text, multi-line text, lookup or option set field.
        /// </summary>
        /// <param name="driver">The web driver.</param>
        /// <param name="cell">The form cell of the field.</param>
        /// <param name="value">The value; for lookups the search text (e.g. the record name).</param>
        internal static void SetValue(IWebDriver driver, IWebElement cell, string value)
        {
            if (cell.FindElements(PortalFormElementsLocators.LookupControl).Count > 0)
            {
                SetLookupValue(driver, cell, value);
                return;
            }

            if (cell.FindElements(PortalFormElementsLocators.OptionSetInput).Count > 0)
            {
                SetOptionSetValue(driver, cell, value);
                return;
            }

            var input = cell.FindElement(PortalFormElementsLocators.TextInput);
            input.Clear();
            input.SendKeys(value);

            // Text fields save their value on blur.
            input.SendKeys(Keys.Tab);
        }

        private static ISearchContext GetFormScope(IWebDriver driver)
        {
            return (ISearchContext)driver.FindElements(PortalFormElementsLocators.FormDialog).LastOrDefault() ?? driver;
        }

        private static IWebElement GetTopmostModalWithTabs(IWebDriver driver)
        {
            var modal = PortalPageManager.GetTopmostModal(driver);
            return modal != null && modal.FindElements(PortalTabElementsLocators.Bars).Count > 0 ? modal : null;
        }

        private static bool IsTab(IWebElement element, string labelOrName)
        {
            return element.GetAttribute("data-id") == PortalTabElementsLocators.TabIdPrefix + labelOrName
                || PortalPageManager.NormalizeText(element.Text) == labelOrName;
        }

        private static void SetLookupValue(IWebDriver driver, IWebElement cell, string value)
        {
            // A single-value lookup only shows its search input when it is empty.
            foreach (var removeButton in cell.FindElements(PortalFormElementsLocators.LookupRemove))
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", removeButton);

            PortalPageManager.WaitUntil(driver, _ => cell.FindElement(PortalFormElementsLocators.LookupInput), RenderTimeout, $"The lookup offered no search for '{value}'.")
                .SendKeys(value);

            // Lookups may display another column than the searched name (e.g. a record number), so prefer a suggestion
            // showing the value and otherwise take the only suggestion. Several suggestions without the value may still
            // be those of a shorter search, so wait for the search to narrow them down.
            PortalPageManager.WaitUntil(
                driver,
                d =>
                {
                    var options = d.FindElements(PortalFormElementsLocators.LookupSuggestions).Where(o => o.Displayed && !string.IsNullOrWhiteSpace(o.Text)).ToList();
                    return options.FirstOrDefault(o => o.Text.Contains(value, StringComparison.Ordinal)) ?? (options.Count == 1 ? options[0] : null);
                },
                RenderTimeout,
                $"No record matching '{value}' was suggested in the lookup.").Click();

            PortalPageManager.WaitUntil(driver, _ => cell.FindElements(PortalFormElementsLocators.LookupValue).Count > 0, RenderTimeout, $"The lookup did not take the record matching '{value}'.");
        }

        private static void SetOptionSetValue(IWebDriver driver, IWebElement cell, string value)
        {
            cell.FindElement(PortalFormElementsLocators.OptionSetInput).Click();
            PortalPageManager.WaitUntil(driver, d => d.FindElement(PortalFormElementsLocators.OptionSetOption(value)), RenderTimeout, $"The option '{value}' was not offered.")
                .Click();
        }
    }
}
