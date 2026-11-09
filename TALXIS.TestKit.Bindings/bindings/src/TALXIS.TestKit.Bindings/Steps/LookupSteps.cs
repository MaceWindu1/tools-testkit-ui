namespace TALXIS.TestKit.Bindings.Steps
{
    using System;
    using System.Linq;
    using FluentAssertions;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Support.UI;
    using Reqnroll;
    using TALXIS.TestKit.Selectors;
    using TALXIS.TestKit.Selectors.Browser;
    using static TALXIS.TestKit.Selectors.AppReference;

    /// <summary>
    /// Steps relating to lookups.
    /// </summary>
    [Binding]
    public class LookupSteps : PowerAppsStepDefiner
    {
        /// <summary>
        /// Clicks the new button in a lookup.
        /// </summary>
        [When("I click the new button in the lookup")]
        public static void WhenIClickTheNewButtonInTheLookup()
        {
            XrmApp.Lookup.New();
        }

        /// <summary>
        /// Selects a records in a lookup by index.
        /// </summary>
        /// <param name="index">The position of the record.</param>
        [When(@"I open the record at position '(\d+)' in the lookup")]
        [When(@"I open the (\d+(?:(?:st)|(?:nd)|(?:rd)|(?:th))) record in the lookup")]
        public static void WhenIOpenTheRecordAtPositionInTheLookup(int index)
        {
            XrmApp.Lookup.OpenRecord(index);
        }

        /// <summary>
        /// Searches records in a lookup.
        /// </summary>
        /// <param name="searchCriteria">The search criteria.</param>
        /// <param name="control">The lookup.</param>
        [When(@"I search for '(.*)' in the '(.*)' lookup")]
        public static void WhenISearchForInTheLookup(string searchCriteria, LookupItem control)
        {
            if (control is null)
            {
                throw new ArgumentNullException(nameof(control));
            }

            XrmApp.Lookup.SearchInLookup(searchCriteria, control);
        }

        /// <summary>
        /// Selects a related entity in a lookup.
        /// </summary>
        /// <param name="entity">The entity label.</param>
        [When(@"I select the related '(.*)' entity in the lookup")]
        public static void WhenISelectTheRelatedEntityInTheLookup(string entity)
        {
            XrmApp.Lookup.SelectRelatedEntity(entity);
        }

        /// <summary>
        /// Switches to a given view in a lookup.
        /// </summary>
        /// <param name="viewName">The name of the view.</param>
        [When(@"I switch to the '(.*)' view in the lookup")]
        public static void WhenISwitchToTheViewInTheLookup(string viewName)
        {
            XrmApp.Lookup.SwitchView(viewName);
        }

        /// <summary>
        /// Asserts that the given record names are visible in a lookup flyout.
        /// </summary>
        /// <param name="lookupName">The name of the lookup.</param>
        /// <param name="recordNames">The names of the records that should be visible.</param>
        [Then(@"I should see only the following records in the '(.*)' lookup")]
        public static void ThenIShouldSeeOnlyTheFollowingRecordsInTheLookup(string lookupName, Table recordNames)
        {
            if (recordNames is null)
            {
                throw new ArgumentNullException(nameof(recordNames));
            }

            // Use the LookupFlyoutBrowser to retrieve the records displayed in the flyout.
            var items = XrmApp.Lookup.GetLookupRecordNames();

            // Assert the count of records is as expected.
            items.Count().Should().Be(recordNames.Rows.Count, because: "the flyout should only contain the given records");

            // Assert that each record from the flyout is present in the expected list.
            foreach (var item in items)
            {
                recordNames.Rows.Should().Contain(r => r[0] == item, because: "every given record should be present in the flyout");
            }
        }

        /// <summary>
        /// Opens the related record by clicking on the link.
        /// </summary>
        /// <param name="lookupLabel">The label of the lookup.</param>
        [When(@"I select a related '(.*)' lookup field")]
        public static void WhenISelectARelatedLookupInTheForm(string lookupLabel)
        {
            string lookupLogicalName = XrmApp.Entity.GetFieldLogicalNameFromLabel(lookupLabel);
            XrmApp.Lookup.SelectRelatedLookupRecord(lookupLogicalName);
        }

        /// <summary>
        /// Open the advanced lookup dialog for the given lookup field.
        /// </summary>
        /// <param name="lookupField">The name of the lookup.</param>
        [When(@"I click to perform an advanced lookup on '(.*)' lookup")]
        public void WhenIClickToPerformAnAdvancedLookup(string lookupField)
        {
            XrmApp.Lookup.OpenAdvancedLookupDialog(lookupField);
        }
    }
}
