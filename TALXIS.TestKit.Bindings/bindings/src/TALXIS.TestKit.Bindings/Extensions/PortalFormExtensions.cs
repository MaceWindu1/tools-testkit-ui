namespace TALXIS.TestKit.Bindings.Extensions
{
    using System;
    using System.Linq;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Support.UI;

    /// <summary>
    /// Works with fields, dialogs and buttons of TALXIS Portal forms, which are rendered differently from Dynamics 365 forms.
    /// </summary>
    public static class PortalFormExtensions
    {
        private const string ModalXPath = "//div[@role='dialog' or @role='alertdialog']";
        private const string FormDialogXPath = ModalXPath + "[.//section[contains(@class, 'TALXIS__form')]]";
        private const string SuggestionXPath = "//div[contains(@class, 'TALXIS__tag-picker__suggestions')]//button[@role='option']";
        private const string CloseButtons = ".ms-Dialog-button--close, .ms-Panel-closeButton";
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Gets a value indicating whether the current page is a TALXIS Portal.
        /// </summary>
        /// <param name="driver">The web driver.</param>
        /// <returns>Whether the page is a TALXIS Portal.</returns>
        public static bool IsTalxisPortal(this IWebDriver driver)
        {
            return (bool)((IJavaScriptExecutor)driver).ExecuteScript("return !!(window.TALXIS && window.TALXIS.Portal);");
        }

        /// <summary>
        /// Finds a field of the topmost form dialog (or of the page when no form dialog is open) by its label or logical name.
        /// </summary>
        /// <param name="driver">The web driver.</param>
        /// <param name="labelOrName">The field label, or the logical name of its control.</param>
        /// <returns>The form cell of the field.</returns>
        public static IWebElement FindPortalField(this IWebDriver driver, string labelOrName)
        {
            var wait = new WebDriverWait(driver, DefaultTimeout) { Message = $"The field '{labelOrName}' was not found on the form." };
            wait.IgnoreExceptionTypes(typeof(NoSuchElementException), typeof(StaleElementReferenceException));
            return wait.Until(d => GetFormScope(d).FindElement(FieldCell(labelOrName)));
        }

        /// <summary>
        /// Finds a field like <see cref="FindPortalField"/> without waiting for it.
        /// </summary>
        /// <param name="driver">The web driver.</param>
        /// <param name="labelOrName">The field label, or the logical name of its control.</param>
        /// <returns>The form cell of the field, or null when it is not rendered.</returns>
        public static IWebElement TryFindPortalField(this IWebDriver driver, string labelOrName)
        {
            return GetFormScope(driver).FindElements(FieldCell(labelOrName)).FirstOrDefault();
        }

        /// <summary>
        /// Gets a value indicating whether a field is marked as required.
        /// </summary>
        /// <param name="cell">The form cell of the field.</param>
        /// <returns>Whether the field is required.</returns>
        public static bool IsPortalFieldRequired(this IWebElement cell)
        {
            return cell.FindElements(By.CssSelector(".TALXIS__form__cell__required-indicator")).Count > 0;
        }

        /// <summary>
        /// Gets a value indicating whether a field is read-only (shown with a lock icon).
        /// </summary>
        /// <param name="cell">The form cell of the field.</param>
        /// <returns>Whether the field is read-only.</returns>
        public static bool IsPortalFieldReadOnly(this IWebElement cell)
        {
            return cell.FindElements(By.XPath(".//i[contains(@class, 'TALXIS__form__cell__lockIcon') and not(contains(@class, 'TALXIS__form__cell__lockIcon--hidden'))]")).Count > 0;
        }

        /// <summary>
        /// Gets the displayed value of a field.
        /// </summary>
        /// <param name="cell">The form cell of the field.</param>
        /// <returns>The value, or an empty string.</returns>
        public static string GetPortalFieldValue(this IWebElement cell)
        {
            var lookupValue = cell.FindElements(By.CssSelector(".talxis__lookupControl button[data-entity]"));
            if (lookupValue.Count > 0)
            {
                return lookupValue[0].GetAttribute("title") ?? lookupValue[0].Text;
            }

            var input = cell.FindElements(By.CssSelector("textarea, input.ms-TextField-field, input.ms-ComboBox-Input"));
            if (input.Count > 0)
            {
                return input[0].GetAttribute("value") ?? string.Empty;
            }

            // Read-only controls may show their value as text.
            var control = cell.FindElements(By.CssSelector("[data-id$='fieldControl_container'], [data-id*='.fieldControl']")).FirstOrDefault();
            return control != null ? PortalPageExtensions.NormalizeText(control.Text) : string.Empty;
        }

        /// <summary>
        /// Finds a button of the topmost dialog by its label: a footer button, or any button labelled by its title or
        /// aria-label (e.g. an icon button in a grid on the dialog).
        /// </summary>
        /// <param name="driver">The web driver.</param>
        /// <param name="label">The button label.</param>
        /// <returns>The enabled button, or null.</returns>
        public static IWebElement FindPortalDialogButton(this IWebDriver driver, string label)
        {
            var modal = driver.GetTopmostPortalModal();
            if (modal == null)
            {
                return null;
            }

            var literal = XPathLiteral(label);
            return modal.FindElements(By.XPath(
                    $".//div[contains(@class, 'ms-Dialog-actions') or contains(@class, 'ms-Panel-footer')]//button[.//span[contains(@class, 'ms-Button-label') and normalize-space()={literal}]]" +
                    $" | .//button[@title={literal} or @aria-label={literal}]"))
                .FirstOrDefault(button => button.Displayed && button.Enabled && !button.IsPortalElementDisabled());
        }

        /// <summary>
        /// Closes the topmost dialog with its close ('x') button, without saving, waiting for a dialog that the previous
        /// action opened and that is still loading.
        /// </summary>
        /// <param name="driver">The web driver.</param>
        /// <returns>The closed dialog.</returns>
        public static IWebElement ClosePortalDialog(this IWebDriver driver)
        {
            var wait = new WebDriverWait(driver, DefaultTimeout) { Message = "No dialog with a close button is displayed." };
            wait.IgnoreExceptionTypes(typeof(NoSuchElementException), typeof(StaleElementReferenceException));
            var modal = wait.Until(d => d.GetTopmostPortalModal() is { } topmost
                && topmost.FindElements(By.CssSelector(CloseButtons)).Count > 0 ? topmost : null);
            modal.FindElements(By.CssSelector(CloseButtons)).First().Click();
            return modal;
        }

        /// <summary>
        /// Clicks the confirming (primary) or the cancelling (secondary) button of the topmost alert or confirmation dialog.
        /// </summary>
        /// <param name="driver">The web driver.</param>
        /// <param name="confirm">Whether to confirm.</param>
        public static void AnswerPortalMessageDialog(this IWebDriver driver, bool confirm)
        {
            var wait = new WebDriverWait(driver, DefaultTimeout) { Message = "No alert or confirmation dialog is displayed." };
            wait.IgnoreExceptionTypes(typeof(NoSuchElementException), typeof(StaleElementReferenceException));
            var dialog = wait.Until(d => d.FindElements(By.XPath(ModalXPath + "[not(.//section[contains(@class, 'TALXIS__form')])]")).LastOrDefault());

            wait.Message = $"The displayed dialog has no {(confirm ? "confirming" : "cancelling")} button.";
            wait.Until(_ => dialog.FindElement(By.CssSelector(confirm ? ".ms-Dialog-actions .ms-Button--primary" : ".ms-Dialog-actions .ms-Button--default"))).Click();
        }

        /// <summary>
        /// Sets the value of a text, multi-line text, lookup or option set field.
        /// </summary>
        /// <param name="driver">The web driver.</param>
        /// <param name="cell">The form cell of the field.</param>
        /// <param name="value">The value; for lookups the search text (e.g. the record name).</param>
        public static void SetPortalFieldValue(this IWebDriver driver, IWebElement cell, string value)
        {
            if (cell.FindElements(By.CssSelector(".talxis__lookupControl")).Count > 0)
            {
                SetLookupValue(driver, cell, value);
                return;
            }

            if (cell.FindElements(By.CssSelector("input.ms-ComboBox-Input")).Count > 0)
            {
                SetOptionSetValue(driver, cell, value);
                return;
            }

            var input = cell.FindElement(By.CssSelector("textarea, input.ms-TextField-field"));
            input.Clear();
            input.SendKeys(value);

            // Text fields save their value on blur.
            input.SendKeys(Keys.Tab);
        }

        /// <summary>
        /// Clicks a footer button of the topmost dialog.
        /// </summary>
        /// <param name="driver">The web driver.</param>
        /// <param name="label">The button label.</param>
        public static void ClickPortalDialogButton(this IWebDriver driver, string label)
        {
            var button = By.XPath($".//div[contains(@class, 'ms-Dialog-actions')]//button[.//span[contains(@class, 'ms-Button-label') and normalize-space()={XPathLiteral(label)}]]");

            var wait = new WebDriverWait(driver, DefaultTimeout) { Message = $"The '{label}' button was not found on the displayed dialog." };
            wait.IgnoreExceptionTypes(typeof(NoSuchElementException), typeof(StaleElementReferenceException));
            wait.Until(d => d.FindElements(By.XPath(ModalXPath)).LastOrDefault()?.FindElement(button) is { Enabled: true } element ? element : null).Click();
        }

        /// <summary>
        /// Selects an option of a choice group (e.g. option set tiles) on the topmost dialog by its label.
        /// </summary>
        /// <param name="driver">The web driver.</param>
        /// <param name="option">The option label.</param>
        public static void ChoosePortalDialogOption(this IWebDriver driver, string option)
        {
            var label = By.XPath($".//label[contains(@class, 'ms-ChoiceField-field')][.//span[contains(@class, 'ms-ChoiceFieldLabel') and normalize-space()={XPathLiteral(option)}]]");

            var wait = new WebDriverWait(driver, DefaultTimeout) { Message = $"The '{option}' option was not found on the displayed dialog." };
            wait.IgnoreExceptionTypes(typeof(NoSuchElementException), typeof(StaleElementReferenceException));
            wait.Until(d => d.FindElements(By.XPath(ModalXPath)).LastOrDefault()?.FindElement(label)).Click();
        }

        /// <summary>
        /// Gets the title of the topmost alert or confirmation dialog (a modal without a form, e.g. opened by
        /// Xrm.Navigation.openAlertDialog or openConfirmDialog).
        /// </summary>
        /// <param name="driver">The web driver.</param>
        /// <returns>The title, or null when no alert dialog is displayed.</returns>
        public static string GetPortalAlertDialogTitle(this IWebDriver driver)
        {
            var alert = driver.FindElements(By.XPath(ModalXPath + "[not(.//section[contains(@class, 'TALXIS__form')])]")).LastOrDefault();
            return alert?.FindElements(By.CssSelector(".ms-Dialog-title")).FirstOrDefault()?.Text.Trim();
        }

        /// <summary>
        /// Waits until a condition is met, returning whether it was met within the timeout.
        /// </summary>
        /// <param name="driver">The web driver.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="timeout">How long to wait.</param>
        /// <returns>Whether the condition was met.</returns>
        public static bool WaitForPortalCondition(this IWebDriver driver, Func<bool> condition, TimeSpan timeout)
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

        // The innermost form cell with the label or control: a cell can host a nested form (e.g. the detail of the record
        // selected in a subgrid), whose cells it then contains.
        private static By FieldCell(string labelOrName)
        {
            const string Cell = "div[contains(concat(' ', normalize-space(@class), ' '), ' TALXIS__form__cell ')]";
            var match = $"[.//label[contains(@class, 'TALXIS__form__cell__label') and @title={XPathLiteral(labelOrName)}]" +
                $" or .//div[starts-with(@data-id, {XPathLiteral(labelOrName + ".")})]]";
            return By.XPath($".//{Cell}{match}[not(.//{Cell}{match})]");
        }

        private static ISearchContext GetFormScope(IWebDriver driver)
        {
            return (ISearchContext)driver.FindElements(By.XPath(FormDialogXPath)).LastOrDefault() ?? driver;
        }

        private static void SetLookupValue(IWebDriver driver, IWebElement cell, string value)
        {
            // A single-value lookup only shows its search input when it is empty.
            foreach (var removeButton in cell.FindElements(By.XPath(".//button[.//i[@data-icon-name='Cancel']]")))
            {
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", removeButton);
            }

            var wait = new WebDriverWait(driver, DefaultTimeout) { Message = $"No record matching '{value}' was suggested in the lookup." };
            wait.IgnoreExceptionTypes(typeof(NoSuchElementException), typeof(StaleElementReferenceException));

            wait.Until(_ => cell.FindElement(By.CssSelector("input.ms-BasePicker-input"))).SendKeys(value);

            // Lookups may display another column than the searched name (e.g. a record number), so prefer a suggestion
            // showing the value and otherwise take the only suggestion. Several suggestions without the value may still
            // be those of a shorter search, so wait for the search to narrow them down.
            wait.Until(d =>
            {
                var options = d.FindElements(By.XPath(SuggestionXPath)).Where(o => o.Displayed && !string.IsNullOrWhiteSpace(o.Text)).ToList();
                return options.FirstOrDefault(o => o.Text.Contains(value, StringComparison.Ordinal)) ?? (options.Count == 1 ? options[0] : null);
            }).Click();

            wait.Message = $"The lookup did not take the record matching '{value}'.";
            wait.Until(_ => cell.FindElements(By.CssSelector(".talxis__lookupControl button[data-entity]")).Count > 0);
        }

        private static void SetOptionSetValue(IWebDriver driver, IWebElement cell, string value)
        {
            cell.FindElement(By.CssSelector("input.ms-ComboBox-Input")).Click();

            var option = By.XPath($"//button[contains(@class, 'ms-ComboBox-option') and @title={XPathLiteral(value)}]");
            var wait = new WebDriverWait(driver, DefaultTimeout) { Message = $"The option '{value}' was not offered." };
            wait.IgnoreExceptionTypes(typeof(NoSuchElementException), typeof(StaleElementReferenceException));
            wait.Until(d => d.FindElement(option)).Click();
        }

        internal static string XPathLiteral(string value)
        {
            if (!value.Contains('\''))
            {
                return $"'{value}'";
            }

            return "concat('" + value.Replace("'", "', \"'\", '") + "')";
        }
    }
}
