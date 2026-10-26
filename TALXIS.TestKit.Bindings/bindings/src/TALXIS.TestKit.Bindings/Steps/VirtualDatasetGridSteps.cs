namespace TALXIS.TestKit.Bindings.Steps
{
    using System;
    using FluentAssertions;
    using OpenQA.Selenium;
    using Reqnroll;
    using TALXIS.TestKit.Bindings.Extensions;

    /// <summary>
    /// Step bindings for a field-bound dataset grid (e.g. the talxis_TALXIS.PCF.VirtualDataset control), whose inline row
    /// buttons are notification expressions of the dataset client API (record.expressions.ui.setNotificationsExpression)
    /// rather than ribbon commands.
    ///
    /// The grid is an ag-grid whose rows carry the record id as <c>row-id</c> (one element per pinned part of a row), so a
    /// test record is found by its id wherever the grid is on the page. A row button is named by its title, aria-label or
    /// text; an icon-only button that shows its label only in a tooltip by its Fluent icon name (e.g. <c>ClipboardList</c>).
    /// </summary>
    [Binding]
    public class VirtualDatasetGridSteps : PowerAppsStepDefiner
    {
        private static readonly TimeSpan GridTimeout = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Asserts that a previously created test record is present in the virtual dataset grid.
        /// </summary>
        /// <param name="recordAlias">The alias of the test record.</param>
        [Then(@"the virtual dataset grid should contain '(.*)'")]
        public static void ThenTheVirtualDatasetGridShouldContain(string recordAlias)
        {
            Driver.WaitForPortalCondition(() => FindRows(recordAlias).Count > 0, GridTimeout)
                .Should().BeTrue(because: $"'{recordAlias}' should be present in the virtual dataset grid");
        }

        /// <summary>
        /// Clicks an inline notification-expression button on a record's row.
        /// </summary>
        /// <param name="buttonLabel">The button's visible title, aria-label, or text.</param>
        /// <param name="recordAlias">The alias of the test record.</param>
        [When(@"I click the '(.*)' button on '(.*)' in the virtual dataset grid")]
        public static void WhenIClickTheButtonOnInTheVirtualDatasetGrid(string buttonLabel, string recordAlias)
        {
            Driver.WaitForPortalCondition(() => TryFindRowButton(recordAlias, buttonLabel) != null, GridTimeout)
                .Should().BeTrue(because: $"the '{buttonLabel}' button should be present on '{recordAlias}' to click it");
            Driver.ClickWhenUnblocked(() => TryFindRowButton(recordAlias, buttonLabel), GridTimeout, $"The '{buttonLabel}' button on '{recordAlias}'");
        }

        /// <summary>
        /// Asserts that an inline notification-expression button is visible on a record's row.
        /// </summary>
        /// <param name="buttonLabel">The button's visible title, aria-label, or text.</param>
        /// <param name="recordAlias">The alias of the test record.</param>
        [Then(@"I should be able to see the '(.*)' button on '(.*)' in the virtual dataset grid")]
        public static void ThenIShouldBeAbleToSeeTheButtonOnInTheVirtualDatasetGrid(string buttonLabel, string recordAlias)
        {
            Driver.WaitForPortalCondition(() => TryFindRowButton(recordAlias, buttonLabel) != null, GridTimeout)
                .Should().BeTrue(because: $"the '{buttonLabel}' button should be visible on '{recordAlias}'");
        }

        /// <summary>
        /// Asserts that an inline notification-expression button is not visible on a record's row, waiting for a button
        /// that an action removes when it refreshes the grid.
        /// </summary>
        /// <param name="buttonLabel">The button's visible title, aria-label, or text.</param>
        /// <param name="recordAlias">The alias of the test record.</param>
        [Then(@"I should not be able to see the '(.*)' button on '(.*)' in the virtual dataset grid")]
        public static void ThenIShouldNotBeAbleToSeeTheButtonOnInTheVirtualDatasetGrid(string buttonLabel, string recordAlias)
        {
            Driver.WaitForPortalCondition(() => FindRows(recordAlias).Count > 0, GridTimeout)
                .Should().BeTrue(because: $"'{recordAlias}' should be present in the virtual dataset grid");
            Driver.BecomesAbsent(() => TryFindRowButton(recordAlias, buttonLabel) != null, GridTimeout)
                .Should().BeTrue(because: $"the '{buttonLabel}' button should not be visible on '{recordAlias}'");
        }

        private static IWebElement TryFindRowButton(string recordAlias, string buttonLabel)
        {
            return FindRows(recordAlias).FindPortalGridRowButton(buttonLabel);
        }

        private static System.Collections.Generic.IReadOnlyList<IWebElement> FindRows(string recordAlias)
        {
            return Driver.FindPortalGridRows(TestDriver.GetTestRecordReference(recordAlias).Id);
        }
    }
}
