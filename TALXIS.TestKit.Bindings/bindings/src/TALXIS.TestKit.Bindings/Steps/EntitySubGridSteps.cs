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
        private static readonly TimeSpan PortalGridTimeout = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Clicks a command on a subgrid.
        /// </summary>
        /// <param name="commandName">The name of the command.</param>
        /// <param name="subGridName">The name of the subgrid.</param>
        [When(@"I click the '(.*)' command on the '(.*)' subgrid")]
        public static void WhenISelectTheCommandOnTheSubgrid(string commandName, string subGridName)
        {
            if (TargetsPortal)
            {
                var subGrid = Driver.FindPortalSubGrid(subGridName);
                Driver.WaitForPortalSubGridCommand(subGrid, commandName)
                    .Should().BeTrue(because: $"the '{commandName}' command should be available on the '{subGridName}' subgrid");
                Driver.ClickWhenUnblocked(() => Driver.FindPortalSubGridCommand(subGrid, commandName), PortalGridTimeout, $"The '{commandName}' command on the '{subGridName}' subgrid");
                return;
            }

            // The command bar is only rendered once the subgrid's ribbon has loaded.
            Driver
                .WaitUntilAvailable(
                    By.XPath(AppElements.Xpath[AppReference.Entity.SubGridContents].Replace("[NAME]", subGridName)),
                    $"Could not find the {subGridName} subgrid.")
                .WaitUntilAvailable(
                    By.XPath(AppElements.Xpath[AppReference.Entity.SubGridCommandBar]),
                    $"Could not find the command bar of the {subGridName} subgrid.");

            XrmApp.Entity.SubGrid.ClickCommand(subGridName, commandName);
        }

        /// <summary>
        /// Clicks an inline command on the row of a previously created test record in a TALXIS Portal subgrid, as a user
        /// does, without selecting the record first (a grid can draw the highlight of a selected row over its commands).
        /// </summary>
        /// <param name="commandName">The label, id or icon file name of the command.</param>
        /// <param name="alias">The alias of the test record.</param>
        /// <param name="subGridName">The name of the subgrid.</param>
        [When(@"I click the '(.*)' command on '(.*)' in the '(.*)' subgrid")]
        public static void WhenIClickTheCommandOnTheRecordInTheSubgrid(string commandName, string alias, string subGridName)
        {
            var subGrid = Driver.FindPortalSubGrid(subGridName);
            var recordId = TestDriver.GetTestRecordReference(alias).Id;
            ClickPortalRowCommand(commandName, () => subGrid.FindPortalGridRows(recordId), $"'{alias}'");
        }

        /// <summary>
        /// Clicks an inline command on the row of the record that shows a value in a column of a TALXIS Portal subgrid,
        /// without selecting the record first. Use it for records that are not test records and in the legacy view, whose
        /// rows do not carry the record id.
        /// </summary>
        /// <param name="commandName">The label, id or icon file name of the command.</param>
        /// <param name="fieldValue">The displayed value.</param>
        /// <param name="fieldName">The column header label or the column's logical name.</param>
        /// <param name="subGridName">The name of the subgrid.</param>
        [When(@"I click the '(.*)' command on the record with '(.*)' in the '(.*)' field in the '(.*)' subgrid")]
        public static void WhenIClickTheCommandOnTheRecordWithInTheFieldInTheSubgrid(string commandName, string fieldValue, string fieldName, string subGridName)
        {
            fieldValue = fieldValue.ReplaceTemplatedText(TestConfig.Parameters);
            var subGrid = Driver.FindPortalSubGrid(subGridName);
            ClickPortalRowCommand(commandName, () => subGrid.FindPortalGridRowsByValue(fieldName, fieldValue), $"the record with '{fieldValue}' in '{fieldName}'");
        }

        /// <summary>
        /// Selects a previously created test record in a subgrid.
        /// </summary>
        /// <param name="alias">The alias of the test record.</param>
        /// <param name="subGridName">The name of the subgrid.</param>
        [When(@"I select '(.*)' from the '(.*)' subgrid")]
        public static void WhenISelectFromTheSubgrid(string alias, string subGridName)
        {
            if (TargetsPortal)
            {
                var subGrid = Driver.FindPortalSubGrid(subGridName);
                var recordId = TestDriver.GetTestRecordReference(alias).Id;
                Driver.WaitForPortalCondition(() => subGrid.FindPortalGridRows(recordId).Count > 0, PortalGridTimeout);
                Driver.SelectPortalGridRecord(subGrid.FindPortalGridRows(recordId), $"'{alias}'");
                return;
            }

            var index = GetSubGridItemIndexByAlias(alias, subGridName);
            index.HasValue.Should().BeTrue(because: "the record should be found in the grid");

            XrmApp.Entity.SubGrid.HighlightRecord(subGridName, Driver, index.Value);
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
            if (TargetsPortal)
            {
                var subGrid = Driver.FindPortalSubGrid(subGridName);
                var recordId = TestDriver.GetTestRecordReference(alias).Id;
                Driver.BecomesAbsent(() => subGrid.FindPortalGridRows(recordId).Count > 0, PortalGridTimeout)
                    .Should().BeTrue(because: $"'{alias}' should not be shown in the '{subGridName}' subgrid");
                return;
            }

            GetSubGridItemIndexByAlias(alias, subGridName)
                .HasValue
                .Should().BeFalse(because: "the record should not exist in the subgrid");
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
            // var actualCount = XrmApp.Entity.SubGrid.GetSubGridItemsCount(subGridName);
            // Temporary until XrmApp.Entity.SubGrid.GetSubGridItemsCount(subGridName) works for a count of 0
            // https://github.com/microsoft/EasyRepro/issues/1318
            var actualCount = TargetsPortal
                ? Driver.FindPortalSubGrid(subGridName).CountPortalGridRecords()
                : (long)Driver.ExecuteScript($"return Xrm.Page.getControl(\"{subGridName}\").getGrid().getTotalRecordCount();");


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
            if (TargetsPortal)
            {
                var subGrid = Driver.FindPortalSubGrid(subGridName);
                var recordId = TestDriver.GetTestRecordReference(recordAlias).Id;
                Driver.WaitForPortalCondition(() => subGrid.FindPortalGridRows(recordId).Count > 0, PortalGridTimeout)
                    .Should().BeTrue(because: $"'{recordAlias}' should be shown in the '{subGridName}' subgrid");
                return;
            }

            var index = GetSubGridItemIndexByAlias(recordAlias, subGridName);

            index.HasValue.Should().BeTrue(because: "the record should be found in the grid");
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
            if (TargetsPortal)
            {
                fieldValue = fieldValue.ReplaceTemplatedText(TestConfig.Parameters);
                var subGrid = Driver.FindPortalSubGrid(subGridName);
                Driver.WaitForPortalCondition(() => subGrid.FindPortalGridRowsByValue(fieldName, fieldValue).Count > 0, PortalGridTimeout)
                    .Should().BeTrue(because: $"a record with '{fieldValue}' in '{fieldName}' should be shown in the '{subGridName}' subgrid");
                return;
            }

            XrmApp.Entity.SubGrid
                .GetSubGridItems(subGridName)
                .Should()
                .Contain(item => item.GetAttribute<string>(fieldName) == fieldValue, because: "a matching record should be present in the subgrid");
        }

        /// <summary>
        /// Asserts the command is visible on the subgrid.
        /// </summary>
        /// <param name="commandName">The name of the command.</param>
        /// <param name="subGridName">The name of the subgrid.</param>
        [Then(@"I should be able to see the '(.*)' command on the '(.*)' subgrid")]
        public static void ThenIShouldBeAbleToSeeTheCommandOnTheSubgrid(string commandName, string subGridName)
        {
            if (TargetsPortal)
            {
                Driver.WaitForPortalSubGridCommand(Driver.FindPortalSubGrid(subGridName), commandName)
                    .Should().BeTrue(because: $"the '{commandName}' command should be shown on the '{subGridName}' subgrid");
                return;
            }

            Driver
                .WaitUntilAvailable(By.XPath(AppElements.Xpath[AppReference.Entity.SubGridContents].Replace("[NAME]", subGridName)))
                .WaitUntilAvailable(By.XPath(AppElements.Xpath[AppReference.Entity.SubGridCommandBar]))
                .WaitUntilVisible(
                    By.XPath(AppElements.Xpath[AppReference.Entity.SubGridCommandLabel].Replace("[NAME]", commandName)),
                    new TimeSpan(0, 0, 5),
                    $"Could not find the {commandName} command on the {subGridName} subgrid.");
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
            if (TargetsPortal)
            {
                Driver.IsPortalSubGridCommandUnavailable(Driver.FindPortalSubGrid(subGridName), commandName)
                    .Should().BeTrue(because: $"the '{commandName}' command should not be shown on the '{subGridName}' subgrid");
                return;
            }

            XrmApp.Entity.SubGrid.TryToFindAssertSubgridCommand(commandName, subGridName).Should().BeNull();
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
            if (TargetsPortal)
            {
                ThenIShouldBeAbleToSeeTheCommandOnTheSubgrid(commandName, subGridName);
                return;
            }

            IsSubgridCommandEnabled(commandName, subGridName).Should().BeTrue(because: $"the '{commandName}' command should be enabled");
        }

        /// <summary>
        /// Asserts that a subgrid command is present but disabled.
        /// </summary>
        /// <param name="commandName">The name of the command.</param>
        /// <param name="subGridName">The name of the subgrid.</param>
        [Then(@"the '(.*)' command on the '(.*)' subgrid should be disabled")]
        public static void ThenTheCommandOnTheSubgridShouldBeDisabled(string commandName, string subGridName)
        {
            // The portal does not render commands whose enable rules fail.
            if (TargetsPortal)
            {
                ThenIShouldNotBeAbleToSeeTheCommandOnTheSubgrid(commandName, subGridName);
                return;
            }

            IsSubgridCommandEnabled(commandName, subGridName).Should().BeFalse(because: $"the '{commandName}' command should be disabled");
        }

        private static bool IsSubgridCommandEnabled(string commandName, string subGridName)
        {
            var command = XrmApp.Entity.SubGrid.TryToFindAssertSubgridCommand(commandName, subGridName);

            command.Should().NotBeNull(because: $"the '{commandName}' command should be present (visible-but-disabled, not hidden) on the '{subGridName}' subgrid");

            var ariaDisabled = command!.GetAttribute("aria-disabled");
            var disabled = command.GetAttribute("disabled");

            return ariaDisabled != "true" && string.IsNullOrEmpty(disabled);
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
            if (TargetsPortal)
            {
                fieldValue = fieldValue.ReplaceTemplatedText(TestConfig.Parameters);
                var subGrid = Driver.FindPortalSubGrid(subGridName);
                Driver.WaitForPortalCondition(() => subGrid.FindPortalGridRowsByValue(fieldName, fieldValue).Count > 0, PortalGridTimeout);
                Driver.SelectPortalGridRecord(subGrid.FindPortalGridRowsByValue(fieldName, fieldValue), $"The record with '{fieldValue}' in '{fieldName}'");
                return;
            }

            List<GridItem> subGridItems = XrmApp.Entity.SubGrid.GetSubGridItems(subGridName);

            GridItem subGridItem = subGridItems.FirstOrDefault(item => item.GetAttribute<string>(fieldName) == fieldValue);

            if (subGridItem != default(GridItem))
            {
                XrmApp.Entity.SubGrid.HighlightRecord(subGridName, Driver, subGridItems.IndexOf(subGridItem));
            }
            else
            {
                throw new Exception($"Could not find record with field '{fieldName}' with value '{fieldValue}' in the '{subGridName}' subgrid.");
            }
        }

        private static void ClickPortalRowCommand(string commandName, Func<IReadOnlyList<IWebElement>> findRows, string description)
        {
            var available = Driver.WaitForPortalCondition(() => Driver.FindPortalRowCommand(findRows(), commandName) is { } command && !command.IsPortalElementDisabled(), PortalGridTimeout);
            Driver.CloseMenus();
            available.Should().BeTrue(because: $"the '{commandName}' command should be available on the row of {description}");
            Driver.ClickWhenUnblocked(() => Driver.FindPortalRowCommand(findRows(), commandName), PortalGridTimeout, $"The '{commandName}' command on {description}");
        }

        private static int? GetSubGridItemIndexByAlias(string recordAlias, string subGridName)
        {
            var record = TestDriver.GetTestRecordReference(recordAlias);
            var index = XrmApp.Entity.SubGrid.GetRecordIndexById(subGridName, Driver, record.Id);

            return index > -1 ? index : default(int?);
        }
    }
}
