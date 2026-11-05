namespace TALXIS.TestKit.Bindings.Steps
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using FluentAssertions;
    using OpenQA.Selenium;
    using Reqnroll;
    using TALXIS.TestKit.Bindings.Extensions;
    using TALXIS.TestKit.Selectors;
    using TALXIS.TestKit.Selectors.Browser;

    /// <summary>
    /// Step bindings related to subgrids on forms.
    /// </summary>
    [Binding]
    public class EntitySubGridSteps : PowerAppsStepDefiner
    {
        /// <summary>
        /// Clicks a command on a subgrid.
        /// </summary>
        /// <param name="commandName">The name of the command.</param>
        /// <param name="subGridName">The name of the subgrid.</param>
        [When(@"I click the '(.*)' command on the '(.*)' subgrid")]
        public static void WhenISelectTheCommandOnTheSubgrid(string commandName, string subGridName)
        {
            App.SubGrid.ClickCommand(subGridName, commandName);
        }

        /// <summary>
        /// Clicks an inline command on the row of a previously created test record in a subgrid, as a user does, without
        /// selecting the record first (a grid can draw the highlight of a selected row over its commands). Only a TALXIS
        /// Portal renders commands on rows.
        /// </summary>
        /// <param name="commandName">The label, id or icon file name of the command.</param>
        /// <param name="alias">The alias of the test record.</param>
        /// <param name="subGridName">The name of the subgrid.</param>
        [When(@"I click the '(.*)' command on '(.*)' in the '(.*)' subgrid")]
        public static void WhenIClickTheCommandOnTheRecordInTheSubgrid(string commandName, string alias, string subGridName)
        {
            App.SubGrid.ClickRowCommand(subGridName, TestDriver.GetTestRecordReference(alias).Id, commandName);
        }

        /// <summary>
        /// Clicks an inline command on the row of the record that shows a value in a column of a subgrid, without selecting
        /// the record first. Only a TALXIS Portal renders commands on rows. Use it for records that are not test records and in the legacy view, whose
        /// rows do not carry the record id.
        /// </summary>
        /// <param name="commandName">The label, id or icon file name of the command.</param>
        /// <param name="fieldValue">The displayed value.</param>
        /// <param name="fieldName">The column header label or the column's logical name.</param>
        /// <param name="subGridName">The name of the subgrid.</param>
        [When(@"I click the '(.*)' command on the record with '(.*)' in the '(.*)' field in the '(.*)' subgrid")]
        public static void WhenIClickTheCommandOnTheRecordWithInTheFieldInTheSubgrid(string commandName, string fieldValue, string fieldName, string subGridName)
        {
            App.SubGrid.ClickRowCommand(subGridName, fieldName, fieldValue.ReplaceTemplatedText(TestConfig.Parameters), commandName);
        }

        /// <summary>
        /// Selects a previously created test record in a subgrid.
        /// </summary>
        /// <param name="alias">The alias of the test record.</param>
        /// <param name="subGridName">The name of the subgrid.</param>
        [When(@"I select '(.*)' from the '(.*)' subgrid")]
        public static void WhenISelectFromTheSubgrid(string alias, string subGridName)
        {
            App.SubGrid.SelectRecord(subGridName, TestDriver.GetTestRecordReference(alias).Id);
        }

        /// <summary>
        /// Selects all records in a subgrid.
        /// </summary>
        /// <param name="subGridName">The name of the subgrid.</param>
        [When(@"I select all in the '(.*)' subgrid")]
        public static void WhenISelectAllInTheSubgrid(string subGridName)
        {
            XrmApp.Entity.SubGrid.ClickSubgridSelectAll(subGridName);
        }

        /// <summary>
        /// Selects a records in a subgrid by index.
        /// </summary>
        /// <param name="index">The position of the record.</param>
        /// <param name="subGridName">The name of the subgrid.</param>
        [When(@"I open the record at position '(\d+)' in the '(.*)' subgrid")]
        [When(@"I open the (\d+(?:(?:st)|(?:nd)|(?:rd)|(?:th))) record in the '(.*)' subgrid")]
        public static void WhenIOpenTheRecordAtPositionInTheSubgrid(int index, string subGridName)
        {
            XrmApp.Entity.SubGrid.OpenSubGridRecord(subGridName, index);
        }

        /// <summary>
        /// Searches in a subgrid.
        /// </summary>
        /// <param name="text">The text to search for.</param>
        /// <param name="subGridName">The name of the subgrid.</param>
        [When(@"I search for '(.*)' in the '(.*)' subgrid")]
        public static void WhenISearchForInTheSubgrid(string text, string subGridName)
        {
            XrmApp.Entity.SubGrid.Search(subGridName, text);
        }

        /// <summary>
        /// Switches view in a subgrid.
        /// </summary>
        /// <param name="viewName">The name of the view.</param>
        /// <param name="subGridName">The name of the subgrid.</param>
        [When(@"I switch to the '(.*)' view in the '(.*)' subgrid")]
        public static void WhenISwitchToTheViewInTheSubgrid(string viewName, string subGridName)
        {
            XrmApp.Entity.SubGrid.SwitchView(subGridName, viewName);
        }

        /// <summary>
        /// Asserts that a record is not in a subgrid.
        /// </summary>
        /// <param name="alias">The alias of the test record.</param>
        /// <param name="subGridName">The name of the subgrid.</param>
        [Then(@"I should not be able to see '(.*)' in the '(.*)' subgrid")]
        public static void ThenIShouldNotBeAbleToSeeInTheSubgrid(string alias, string subGridName)
        {
            App.SubGrid.ContainsRecord(subGridName, TestDriver.GetTestRecordReference(alias).Id, contained: false)
                .Should().BeTrue(because: $"'{alias}' should not be shown in the '{subGridName}' subgrid");
        }

        /// <summary>
        /// Asserts that a given number of records are present in the subgrid.
        /// </summary>
        /// <param name="compare">The type of comparison.</param>
        /// <param name="count">The count to compare.</param>
        /// <param name="subGridName">The name of the subgrid.</param>
        [Then(@"I should be able to see (exactly|more than|less than) (\d+) record in the '(.*)' subgrid")]
        [Then(@"I should be able to see (exactly|more than|less than) (\d+) records in the '(.*)' subgrid")]
        public static void ThenIShouldBeAbleToSeeRecordsInTheSubgrid(string compare, int count, string subGridName)
        {
            var actualCount = App.SubGrid.CountRecords(subGridName);

            switch (compare)
            {
                case "exactly":
                    actualCount.Should().Be(count);
                    break;
                case "more than":
                    actualCount.Should().BeGreaterThan(count);
                    break;
                case "less than":
                    actualCount.Should().BeLessThan(count);
                    break;
            }
        }

        /// <summary>
        /// Asserts that the specified subgrid contains the specified test record.
        /// </summary>
        /// <param name="subGridName">The name of the subgrid.</param>
        /// <param name="recordAlias">The alias of the test record.</param>
        [Then(@"the '(.*)' subgrid should contain '(.*)'")]
        public static void ThenTheGridShouldContain(string subGridName, string recordAlias)
        {
            App.SubGrid.ContainsRecord(subGridName, TestDriver.GetTestRecordReference(recordAlias).Id, contained: true)
                .Should().BeTrue(because: $"'{recordAlias}' should be shown in the '{subGridName}' subgrid");
        }

        /// <summary>
        /// Asserts that the specified subgrid contains the specified test records.
        /// </summary>
        /// <param name="subGridName">The name of the subgrid.</param>
        /// <param name="table">The record aliases.</param>
        [Then(@"the '(.*)' subgrid should contain the following records")]
        public static void ThenTheSubgridShouldContainTheFollowingRecords(string subGridName, Table table)
        {
            if (table is null)
            {
                throw new ArgumentNullException(nameof(table));
            }

            foreach (var row in table.Rows)
            {
                ThenTheGridShouldContain(subGridName, row[0]);
            }
        }

        /// <summary>
        /// Asserts that the subgrid contains a record that has a reference to the given test record in a given column.
        /// </summary>
        /// <param name="subGridName">The name of the subgrid.</param>
        /// <param name="alias">The alias of the test record.</param>
        /// <param name="lookup">The logical name of the lookup column.</param>
        [Then(@"the '(.*)' subgrid should contain a record with a reference to '(.*)' in the '(.*)' lookup field")]
        public static void ThenTheSubgridShouldContainARecordWithInTheLookup(string subGridName, string alias, string lookup)
        {
            var reference = TestDriver.GetTestRecordReference(alias);

            // Temporary until XrmApp.Entity.SubGrid.GetSubGridItems(subGridName) returns the correct ID for each item.
            var index = (long)Driver.ExecuteScript(
                $"return Xrm.Page.getControl(\"{subGridName}\").getGrid().getRows().get().findIndex(row => row.data.entity.attributes.get().findIndex(a => a.getName() === \"{lookup}\" && a.getValue() && a.getValue()[0].id === \"{reference.Id.ToString("B").ToUpper(CultureInfo.CurrentCulture)}\") > -1)");

            index.Should().BeGreaterOrEqualTo(0, because: "a matching record should be present in the subgrid");
        }

        /// <summary>
        /// Asserts that the subgrid contains records that have references to the given test records in a given column.
        /// </summary>
        /// <param name="subGridName">The name of the subgrid.</param>
        /// <param name="lookup">The logical name of the lookup column.</param>
        /// <param name="table">The record aliases.</param>
        [Then(@"the '(.*)' subgrid should contain records with the following in the '(.*)' lookup")]
        public static void ThenTheSubgridShouldContainRecordsWithTheFollowingInTheLookup(string subGridName, string lookup, Table table)
        {
            if (table is null)
            {
                throw new ArgumentNullException(nameof(table));
            }

            foreach (var row in table.Rows)
            {
                ThenTheSubgridShouldContainARecordWithInTheLookup(subGridName, row[0], lookup);
            }
        }

        /// <summary>
        /// Asserts that the subgrid contains a record with a field matching the criteria.
        /// </summary>
        /// <param name="subGridName">The name of the subgrid.</param>
        /// <param name="fieldValue">The expected value of the field.</param>
        /// <param name="fieldName">The name of the field.</param>
        [Then(@"the '(.*)' subgrid should contain a record with '(.*)' in the '(.*)' (?:text|numeric|currency|lookup|datetime|optionset|multioptionset|boolean) field")]
        public static void ThenTheSubgridShouldContainRecordsWithInTheField(string subGridName, string fieldValue, string fieldName)
        {
            fieldValue = fieldValue.ReplaceTemplatedText(TestConfig.Parameters);
            App.SubGrid.ContainsRecord(subGridName, fieldName, fieldValue)
                .Should().BeTrue(because: $"a record with '{fieldValue}' in '{fieldName}' should be shown in the '{subGridName}' subgrid");
        }

        /// <summary>
        /// Asserts the command is visible on the subgrid.
        /// </summary>
        /// <param name="commandName">The name of the command.</param>
        /// <param name="subGridName">The name of the subgrid.</param>
        [Then(@"I should be able to see the '(.*)' command on the '(.*)' subgrid")]
        public static void ThenIShouldBeAbleToSeeTheCommandOnTheSubgrid(string commandName, string subGridName)
        {
            App.SubGrid.IsCommandShown(subGridName, commandName, shown: true)
                .Should().BeTrue(because: $"the '{commandName}' command should be shown on the '{subGridName}' subgrid");
        }

        /// <summary>
        /// Clicks a flyout on a subgrid.
        /// </summary>
        /// <param name="flyoutName">The name of the flyout.</param>
        /// <param name="subGridName">The name of the subgrid.</param>
        [When(@"I click the '([^']+)' flyout on the '([^']+)' subgrid")]
        public static void WhenIClickTheFlyoutOnTheSubgrid(string flyoutName, string subGridName)
        {
            XrmApp.Entity.SubGrid.ClickCommand(subGridName, flyoutName);
        }

        /// <summary>
        /// Asserts the command is not visible on the subgrid.
        /// </summary>
        /// <param name="commandName">The name of the command.</param>
        /// <param name="subGridName">The name of the subgrid.</param>
        [Then(@"I should not be able to see the '(.*)' command on the '(.*)' subgrid")]
        public static void ThenIShouldNotBeAbleToSeeTheCommandOnTheSubgrid(string commandName, string subGridName)
        {
            App.SubGrid.IsCommandShown(subGridName, commandName, shown: false)
                .Should().BeTrue(because: $"the '{commandName}' command should not be shown on the '{subGridName}' subgrid");
        }

        /// <summary>
        /// Asserts that a subgrid command is present and enabled. Distinct from visibility: some
        /// commands stay in the DOM (just greyed out) when their EnableRule evaluates to false,
        /// rather than being removed by a DisplayRule.
        /// </summary>
        /// <param name="commandName">The name of the command.</param>
        /// <param name="subGridName">The name of the subgrid.</param>
        [Then(@"the '(.*)' command on the '(.*)' subgrid should be enabled")]
        public static void ThenTheCommandOnTheSubgridShouldBeEnabled(string commandName, string subGridName)
        {
            App.SubGrid.IsCommandEnabled(subGridName, commandName, enabled: true)
                .Should().BeTrue(because: $"the '{commandName}' command should be enabled");
        }

        /// <summary>
        /// Asserts that a subgrid command is present but disabled.
        /// </summary>
        /// <param name="commandName">The name of the command.</param>
        /// <param name="subGridName">The name of the subgrid.</param>
        [Then(@"the '(.*)' command on the '(.*)' subgrid should be disabled")]
        public static void ThenTheCommandOnTheSubgridShouldBeDisabled(string commandName, string subGridName)
        {
            App.SubGrid.IsCommandEnabled(subGridName, commandName, enabled: false)
                .Should().BeTrue(because: $"the '{commandName}' command should be disabled");
        }

        /// <summary>
        /// Asserts the command is visible on the open flyout of the subgrid.
        /// </summary>
        /// <param name="commandName">The name of the command.</param>
        [Then(@"I should be able to see the '(.*)' command on the flyout of the subgrid")]
        public static void ThenIShouldBeAbleToSeeTheCommandOnTheFlyoutOfTheSubgrid(string commandName)
        {
            XrmApp.Entity.SubGrid.WaitUntilFlyoutCommandVisible(commandName, TimeSpan.FromSeconds(10));
        }

        /// <summary>
        /// Asserts the command is not visible on the open flyout of the subgrid.
        /// </summary>
        /// <param name="commandName">The name of the command.</param>
        [Then(@"I should not be able to see the '(.*)' command on the flyout of the subgrid")]
        public static void ThenIShouldNotBeAbleToSeeTheCommandOnTheFlyoutOfTheSubgrid(string commandName)
        {
            Action act = () => XrmApp.Entity.SubGrid.WaitUntilFlyoutCommandVisible(commandName, TimeSpan.FromSeconds(1));
            act.Should().Throw<Exception>($"The command '{commandName}' should not be visible on the flyout of the subgrid.");
        }

        /// <summary>
        /// Clicks a command under a flyout on a subgrid.
        /// </summary>
        /// <param name="commandName">The name of the command.</param>
        /// <param name="flyoutName">The name of the flyout.</param>
        /// <param name="subGridName">The name of the subgrid.</param>
        [When(@"I click the '([^']+)' command under the '([^']+)' flyout on the '([^']+)' subgrid")]
        public static void WhenIClickTheCommandUnderTheFlyoutOnTheSubgrid(string commandName, string flyoutName, string subGridName)
        {
            XrmApp.Entity.SubGrid.ClickCommand(subGridName, flyoutName, commandName);
        }

        /// <summary>
        /// Selects a record in the subgrid given a value is in a particular field.
        /// </summary>
        /// <param name="fieldValue">The field value.</param>
        /// <param name="fieldName">The field name.</param>
        /// <param name="subGridName">The subgrid name.</param>
        [When(@"I select a record with '(.*)' in the '(.*)' field in the '(.*)' subgrid")]
        public static void WhenISelectARecordWithInTheFieldInTheSubgrid(string fieldValue, string fieldName, string subGridName)
        {
            App.SubGrid.SelectRecord(subGridName, fieldName, fieldValue.ReplaceTemplatedText(TestConfig.Parameters));
        }
    }
}
