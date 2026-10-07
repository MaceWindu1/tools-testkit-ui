namespace TALXIS.TestKit.Bindings.Steps
{
    using System.Linq;
    using FluentAssertions;
    using OpenQA.Selenium;
    using Reqnroll;
    using TALXIS.TestKit.Selectors.Browser;

    /// <summary>
    /// Step bindings for a talxis_TALXIS.PCF.VirtualDataset field-bound grid. This is a custom PCF
    /// (built on ag-grid) rather than the standard Dynamics grid/subgrid that GridSteps/EntitySubGridSteps
    /// target, and its inline row buttons (drawn via the dataset client API's
    /// record.expressions.ui.setNotificationsExpression) are not ribbon commands either.
    ///
    /// The PCF root element's CSS class is assumed to equal its manifest namespace+constructor
    /// ("TALXIS.PCF.VirtualDataset"). Rows are matched by ag-grid's conventional `row-id` DOM
    /// attribute, assumed to be set to the record's GUID. Only one VirtualDataset grid is assumed to
    /// be visible/relevant at a time; if multiple appear on the same page, these steps will need a
    /// control-name/alias disambiguator added.
    /// </summary>
    [Binding]
    public class VirtualDatasetGridSteps : PowerAppsStepDefiner
    {
        private const string PcfRootClassName = "TALXIS.PCF.VirtualDataset";

        /// <summary>
        /// Asserts that a previously created test record is present in the virtual dataset grid.
        /// </summary>
        /// <param name="recordAlias">The alias of the test record.</param>
        [Then(@"the virtual dataset grid should contain '(.*)'")]
        public static void ThenTheVirtualDatasetGridShouldContain(string recordAlias)
        {
            TryFindRow(recordAlias).Should().NotBeNull(because: $"'{recordAlias}' should be present in the virtual dataset grid");
        }

        /// <summary>
        /// Clicks an inline notification-expression button on a record's row.
        /// </summary>
        /// <param name="buttonLabel">The button's visible title, aria-label, or text.</param>
        /// <param name="recordAlias">The alias of the test record.</param>
        [When(@"I click the '(.*)' button on '(.*)' in the virtual dataset grid")]
        public static void WhenIClickTheButtonOnInTheVirtualDatasetGrid(string buttonLabel, string recordAlias)
        {
            var button = FindRowButton(recordAlias, buttonLabel);
            button.Click();
            Driver.WaitForTransaction();
        }

        /// <summary>
        /// Asserts that an inline notification-expression button is visible on a record's row.
        /// </summary>
        /// <param name="buttonLabel">The button's visible title, aria-label, or text.</param>
        /// <param name="recordAlias">The alias of the test record.</param>
        [Then(@"I should be able to see the '(.*)' button on '(.*)' in the virtual dataset grid")]
        public static void ThenIShouldBeAbleToSeeTheButtonOnInTheVirtualDatasetGrid(string buttonLabel, string recordAlias)
        {
            TryFindRowButton(recordAlias, buttonLabel).Should().NotBeNull(because: $"the '{buttonLabel}' button should be visible on '{recordAlias}'");
        }

        /// <summary>
        /// Asserts that an inline notification-expression button is not visible on a record's row.
        /// </summary>
        /// <param name="buttonLabel">The button's visible title, aria-label, or text.</param>
        /// <param name="recordAlias">The alias of the test record.</param>
        [Then(@"I should not be able to see the '(.*)' button on '(.*)' in the virtual dataset grid")]
        public static void ThenIShouldNotBeAbleToSeeTheButtonOnInTheVirtualDatasetGrid(string buttonLabel, string recordAlias)
        {
            TryFindRowButton(recordAlias, buttonLabel).Should().BeNull(because: $"the '{buttonLabel}' button should not be visible on '{recordAlias}'");
        }

        private static IWebElement FindRowButton(string recordAlias, string buttonLabel)
        {
            var button = TryFindRowButton(recordAlias, buttonLabel);
            button.Should().NotBeNull(because: $"the '{buttonLabel}' button should be present on '{recordAlias}' to click it");
            return button!;
        }

        private static IWebElement? TryFindRowButton(string recordAlias, string buttonLabel)
        {
            var row = TryFindRow(recordAlias);
            if (row is null)
            {
                return null;
            }

            return row
                .FindElements(By.XPath($".//button[contains(@title, '{buttonLabel}') or contains(@aria-label, '{buttonLabel}') or contains(., '{buttonLabel}')]"))
                .FirstOrDefault();
        }

        private static IWebElement? TryFindRow(string recordAlias)
        {
            var reference = TestDriver.GetTestRecordReference(recordAlias);
            var recordId = reference.Id.ToString("D");

            var grid = Driver.FindElements(By.ClassName(PcfRootClassName)).FirstOrDefault();
            if (grid is null)
            {
                return null;
            }

            return grid
                .FindElements(By.CssSelector($"[row-id='{recordId}']"))
                .FirstOrDefault();
        }
    }
}
