using System;
using System.Collections.ObjectModel;
using System.Linq;
using OpenQA.Selenium;
using TALXIS.TestKit.Selectors.Browser;
using TALXIS.TestKit.Selectors.DTO.Locators;

namespace TALXIS.TestKit.Selectors.WebClientManagement.Helpers
{
    /// <summary>
    /// The Fluent dropdown that model-driven forms render for choice and two-option columns: a
    /// button[data-id='[NAME].fieldControl-option-set-select'] whose value is the selected label, and a listbox (named by
    /// the button's aria-controls once it is open) of role=option elements. A choice column lists its no-value option
    /// first ('---' or '--Select--'); a two-option column lists its false option first and its true option second.
    /// </summary>
    internal static class FluentDropdownHelper
    {
        // The label of a choice column's no-value option starts with this ('---', '--Select--').
        private const string NoValuePrefix = "--";

        private static readonly TimeSpan ListboxTimeout = TimeSpan.FromSeconds(5);

        /// <summary>The column's dropdown button, or null when the column is not rendered as a Fluent dropdown.</summary>
        internal static IWebElement FindButton(ISearchContext container, string name) =>
            container.FindElements(EntityElementsLocators.EntityNewLookOptionsetStatusComboButton(name)).FirstOrDefault();

        /// <summary>The selected label, or an empty string when nothing is selected.</summary>
        internal static string GetSelectedLabel(IWebElement button) => button.GetAttribute("value") ?? string.Empty;

        /// <summary>Whether a two-option column's true option is selected.</summary>
        internal static bool GetTwoOptionValue(IWebDriver driver, IWebElement button)
        {
            var options = Open(driver, button);
            try
            {
                var selected = options.ToList().FindIndex(o => o.GetAttribute("aria-selected") == "true");
                if (selected < 0)
                    throw new InvalidOperationException($"No option of '{button.GetAttribute("aria-label")}' is selected.");

                return selected == 1;
            }
            finally
            {
                Close(button);
            }
        }

        /// <summary>Selects a two-option column's true or false option.</summary>
        internal static void SetTwoOptionValue(IWebDriver driver, IWebElement button, bool value)
        {
            var options = Open(driver, button);
            if (options.Count != 2)
            {
                Close(button);
                throw new InvalidOperationException($"'{button.GetAttribute("aria-label")}' has {options.Count} options, not the two of a two-option column.");
            }

            var option = options[value ? 1 : 0];
            if (option.GetAttribute("aria-selected") == "true")
            {
                Close(button);
                return;
            }

            option.Click();
        }

        /// <summary>Selects the option with a label, or the no-value option (clears a choice column) when label is null.</summary>
        internal static void SelectOption(IWebDriver driver, IWebElement button, string label)
        {
            var options = Open(driver, button);
            var option = label == null
                ? options.FirstOrDefault(o => Label(o).StartsWith(NoValuePrefix, StringComparison.Ordinal))
                : options.FirstOrDefault(o => Label(o) == label);
            if (option == null)
            {
                Close(button);
                var labels = string.Join("', '", options.Select(Label));
                throw new InvalidOperationException($"'{button.GetAttribute("aria-label")}' has no {(label == null ? "no-value option" : $"option '{label}'")}, only '{labels}'.");
            }

            option.Click();
        }

        private static string Label(IWebElement option) => (option.GetAttribute("textContent") ?? string.Empty).Trim();

        private static ReadOnlyCollection<IWebElement> Open(IWebDriver driver, IWebElement button)
        {
            if (button.GetAttribute("aria-expanded") != "true")
                button.Click();

            var listbox = driver.WaitUntilAvailable(By.Id(button.GetAttribute("aria-controls")), ListboxTimeout,
                $"The options of '{button.GetAttribute("aria-label")}' did not open.");

            return listbox.FindElements(By.CssSelector("[role='option']"));
        }

        private static void Close(IWebElement button)
        {
            if (button.GetAttribute("aria-expanded") == "true")
                button.Click();
        }
    }
}
