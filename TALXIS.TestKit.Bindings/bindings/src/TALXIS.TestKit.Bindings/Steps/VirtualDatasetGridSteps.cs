namespace TALXIS.TestKit.Bindings.Steps
{
    using FluentAssertions;
    using Reqnroll;

    /// <summary>
    /// Step bindings for a field-bound dataset grid (e.g. the talxis_TALXIS.PCF.VirtualDataset control), whose inline row
    /// buttons are notification expressions of the dataset client API (record.expressions.ui.setNotificationsExpression)
    /// rather than ribbon commands.
    ///
    /// A test record is found by its id wherever the grid is on the page. A row button is named by its title, aria-label or
    /// text; an icon-only button that shows its label only in a tooltip by its Fluent icon name (e.g. <c>ClipboardList</c>).
    /// Only a TALXIS Portal is supported.
    /// </summary>
    [Binding]
    public class VirtualDatasetGridSteps : PowerAppsStepDefiner
    {
        /// <summary>
        /// Asserts that a previously created test record is present in the virtual dataset grid.
        /// </summary>
        /// <param name="recordAlias">The alias of the test record.</param>
        [Then(@"the virtual dataset grid should contain '(.*)'")]
        public static void ThenTheVirtualDatasetGridShouldContain(string recordAlias)
        {
            App.DatasetGrid.ContainsRecord(TestDriver.GetTestRecordReference(recordAlias).Id)
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
            App.DatasetGrid.ClickRowButton(TestDriver.GetTestRecordReference(recordAlias).Id, buttonLabel);
        }

        /// <summary>
        /// Asserts that an inline notification-expression button is visible on a record's row.
        /// </summary>
        /// <param name="buttonLabel">The button's visible title, aria-label, or text.</param>
        /// <param name="recordAlias">The alias of the test record.</param>
        [Then(@"I should be able to see the '(.*)' button on '(.*)' in the virtual dataset grid")]
        public static void ThenIShouldBeAbleToSeeTheButtonOnInTheVirtualDatasetGrid(string buttonLabel, string recordAlias)
        {
            App.DatasetGrid.IsRowButtonShown(TestDriver.GetTestRecordReference(recordAlias).Id, buttonLabel, shown: true)
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
            App.DatasetGrid.IsRowButtonShown(TestDriver.GetTestRecordReference(recordAlias).Id, buttonLabel, shown: false)
                .Should().BeTrue(because: $"'{recordAlias}' should be shown without the '{buttonLabel}' button");
        }
    }
}
