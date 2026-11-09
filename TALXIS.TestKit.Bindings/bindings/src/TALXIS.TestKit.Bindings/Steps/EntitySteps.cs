namespace TALXIS.TestKit.Bindings.Steps
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;
    using FluentAssertions;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Interactions;
    using Reqnroll;
    using TALXIS.TestKit.Bindings.Extensions;
    using TALXIS.TestKit.Selectors;
    using TALXIS.TestKit.Selectors.DTO;
    using TALXIS.TestKit.Selectors.Browser;
    using static System.Net.Mime.MediaTypeNames;

    /// <summary>
    /// Step bindings related to forms.
    /// </summary>
    [Binding]
    public class EntitySteps : PowerAppsStepDefiner
    {
        /// <summary>
        /// Selects a tab on the form.
        /// </summary>
        /// <param name="tabName">The name of the tab.</param>
        [When(@"I select the '(.*)' tab")]
        public static void ISelectTab(string tabName)
        {
            // Tab labels can contain record values (e.g. a number), referenced as parameters.
            App.Entity.SelectTab(tabName.ReplaceTemplatedText(TestConfig.Parameters));
        }

        /// <summary>
        /// Asserts whether a tab is currently visible.
        /// </summary>
        /// <param name="shouldOrShouldNot">Whether the tab should be visible.</param>
        /// <param name="tabName">The name of the tab.</param>
        [Then(@"I (should|should not) be able to see the '(.*)' tab")]
        public static void IShouldBeAbleToSeeTab(string shouldOrShouldNot, string tabName)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(shouldOrShouldNot, nameof(shouldOrShouldNot));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(tabName, nameof(tabName));

            bool shouldBeVisible = shouldOrShouldNot.Equals("should", StringComparison.InvariantCultureIgnoreCase);
            tabName = tabName.ReplaceTemplatedText(TestConfig.Parameters);

            App.Entity.IsTabShown(tabName, shouldBeVisible)
                .Should()
                .BeTrue(because: $"The tab '{tabName}' {(shouldBeVisible ? "should" : "should not")} be visible.");
        }

        /// <summary>
        /// Sets the value for the field.
        /// </summary>
        /// <param name="fieldValue">The field value.</param>
        /// <param name="fieldLabel">The field name.</param>
        [When(@"I enter '(.*)' into the '(.*)' field on the form")]
        public static void WhenIEnterInTheField(string fieldValue, string fieldLabel)
        {
            App.Entity.SetValue(fieldLabel, fieldValue.ReplaceTemplatedText(TestConfig.Parameters));
        }

        /// <summary>
        /// Sets the values of the fields in the table on the form.
        /// </summary>
        /// <param name="fields">The fields to set.</param>
        [When(@"I enter the following into the form")]
        public static void WhenIEnterTheFollowingIntoTheForm(Table fields)
        {
            fields = fields ?? throw new ArgumentNullException(nameof(fields));

            foreach (DataTableRow row in fields.Rows)
            {
                WhenIEnterInTheField(row["Value"], row["Field"]);
            }
        }

        /// <summary>
        /// Clears the value for the field.
        /// </summary>
        /// <param name="field">The field name.</param>
        [When(@"I clear the '(.*)' (?:currency|numeric|text|boolean) field")]
        public static void WhenIClearTheField(string field)
        {
            XrmApp.Entity.ClearValue(field);
        }

        /// <summary>
        /// Clears the value for the option set field.
        /// </summary>
        /// <param name="field">The field.</param>
        [When(@"I clear the '(.*)' optionset field")]
        public static void WhenIClearTheOptionSetField(OptionSet field)
        {
            XrmApp.Entity.ClearValue(field);
        }

        /// <summary>
        /// Clears the value for the multi-select option set field.
        /// </summary>
        /// <param name="field">The field.</param>
        [When(@"I clear the '(.*)' multioptionset field")]
        public static void WhenIClearTheOptionSetField(MultiValueOptionSet field)
        {
            XrmApp.Entity.ClearValue(field);
        }

        /// <summary>
        /// Clears the value for the boolean field.
        /// </summary>
        /// <param name="field">The field.</param>
        [When(@"I clear the '(.*)' datetime field")]
        public static void WhenIClearTheDateTimeField(DateTimeControl field)
        {
            XrmApp.Entity.ClearValue(field);
        }

        /// <summary>
        /// Clears the value for the lookup field.
        /// </summary>
        /// <param name="field">The field.</param>
        [When(@"I clear the '(.*)' lookup field")]
        public static void WhenIClearTheLookupField(LookupItem field)
        {
            XrmApp.Entity.ClearValue(field);
        }

        /// <summary>
        /// Deletes the record.
        /// </summary>
        [When(@"I delete the record")]
        public static void WhenIDeleteTheRecord()
        {
            XrmApp.Entity.Delete();
        }

        /// <summary>
        /// Opens the record set navigator.
        /// </summary>
        [When(@"I open the record set navigator")]
        public static void WhenIOpenTheRecordSetNavigator()
        {
            XrmApp.Entity.OpenRecordSetNavigator();
        }

        /// <summary>
        /// Closes the record set navigator.
        /// </summary>
        [When(@"I close the record set navigator")]
        public static void WhenICloseTheRecordSetNavigator()
        {
            XrmApp.Entity.CloseRecordSetNavigator();
        }

        /// <summary>
        /// Select a Form.
        /// </summary>
        /// <param name="formName">The name of the form.</param>
        [When(@"I select '(.*)' form")]
        public static void WhenISelectForm(string formName)
        {
            XrmApp.Entity.SelectForm(formName);
        }

        /// <summary>
        /// Select a lookup on the form.
        /// </summary>
        /// <param name="fieldName">The name of the lookup.</param>
        [When(@"I select '(.*)' lookup")]
        public static void WhenISelectLookup(string fieldName)
        {
            XrmApp.Entity.SelectLookup(new LookupItem { Name = fieldName });
        }

        /// <summary>
        /// Saves the record.
        /// </summary>
        [When(@"I save the record")]
        public static void WhenISaveTheRecord()
        {
            XrmApp.Entity.SaveTheRecord();
        }

        /// <summary>
        /// Assigns the record to a user or team.
        /// </summary>
        /// <param name="userOrTeamName">The name of the user or team.</param>
        [When(@"I assign the record to a (?:user|team) named '(.*)'")]
        public static void WhenIAssignTheRecordToANamed(string userOrTeamName)
        {
            XrmApp.Entity.Assign(userOrTeamName);
        }

        /// <summary>
        /// Switches business process flow.
        /// </summary>
        /// <param name="process">The name of the process.</param>
        [When(@"I switch process to the '(.*)' process")]
        public static void WhenISwitchProcessToTheProcess(string process)
        {
            XrmApp.Entity.SwitchProcess(process);
        }

        /// <summary>
        /// Asserts that values are shown in fields.
        /// </summary>
        /// <param name="fields">The fields to set.</param>
        [Then(@"the values of the fields should be as the following")]
        public static void ThenTheValuesOfTheFieldsShouldBeAsTheFollowing(Table fields)
        {
            fields = fields ?? throw new ArgumentNullException(nameof(fields));

            foreach (DataTableRow row in fields.Rows)
            {
                ThenIShouldBeAbleToSeeAValueInTheField(row["Value"], row["Field"]);
            }
        }

        /// <summary>
        /// Asserts that a value is shown in a field.
        /// </summary>
        /// <param name="expectedValue">The expected value.</param>
        /// <param name="fieldLabel">The field label.</param>
        [Then("I should be able to see a value of '(.*)' in the '(.*)' field")]
        public static void ThenIShouldBeAbleToSeeAValueInTheField(string expectedValue, string fieldLabel)
        {
            expectedValue = expectedValue.ReplaceTemplatedText(TestConfig.Parameters);

            App.Entity.HasValue(fieldLabel, expectedValue, out var actualValue)
                .Should().BeTrue(because: $"the '{fieldLabel}' field should show '{expectedValue}', not '{actualValue}'");
        }

        /// <summary>
        /// Asserts that a field is mandatory or optional.
        /// </summary>
        /// <param name="fieldLabel">The name of the field.</param>
        /// <param name="requirementLevel">Whether the field should be mandatory or optional.</param>
        [Then(@"the '(.*)' field should be (mandatory|optional)")]
        public static void ThenTheFieldShouldBeMandatory(string fieldLabel, string requirementLevel)
        {
            App.Entity.IsFieldRequired(fieldLabel, requirementLevel == "mandatory")
                .Should().BeTrue(because: $"the field should be {requirementLevel}");
        }

        /// <summary>
        /// Asserts that a value is shown in a boolean field.
        /// </summary>
        /// <param name="expectedValue">The expected value.</param>
        /// <param name="field">The field name.</param>
        /// <param name="fieldLocation">Where the field is located.</param>
        [Then("I should be able to see a value of '(true|false)' in the '(.*)' boolean (field|header field)")]
        public static void ThenIShouldBeAbleToSeeAValueOfInTheBooleanField(bool expectedValue, BooleanItem field, string fieldLocation)
        {
            var actualValue = fieldLocation == "field" ? XrmApp.Entity.GetValue(field) : XrmApp.Entity.GetHeaderValue(field);
            actualValue.Should().Be(expectedValue);
        }

        /// <summary>
        /// Asserts that a value is shown in a datetime field.
        /// </summary>
        /// <param name="expectedValue">The expected value.</param>
        /// <param name="field">The field label.</param>
        /// <param name="fieldLocation">Where the field is located.</param>
        [Then(@"I should be able to see a value of '(.*)' in the '(.*)' datetime (field|header field)")]
        public static void ThenIShouldBeAbleToSeeAValueOfInTheDateTimeField(DateTime? expectedValue, DateTimeControl field, string fieldLocation)
        {
            var actualValue = fieldLocation == "field" ? XrmApp.Entity.GetValue(field) : XrmApp.Entity.GetHeaderValue(field);
            actualValue.Should().Be(expectedValue);
        }

        /// <summary>
        /// Asserts that a business process error is shown with a given message.
        /// </summary>
        /// <param name="expectedMessage">The expected message.</param>
        [Then(@"I should be able to see a business process error stating '(.*)'")]
        public static void ThenIShouldBeAbleToSeeABusinessProcessErrorStating(string expectedMessage)
        {
            XrmApp.Entity.GetBusinessProcessError().Should().Be(expectedMessage);
        }

        /// <summary>
        /// Asserts that the provided form for the provided entity is shown.
        /// </summary>
        /// <param name="formName">The name of the form.</param>
        /// <param name="entityName">The name of the entity.</param>
        [Then(@"I should be presented with a '(.*)' form for the '(.*)' entity")]
        public static void ThenIShouldBePresentedWithANewFormForTheEntity(string formName, string entityName)
        {
            XrmApp.Entity.GetFormName().Should().Be(formName);
            XrmApp.Entity.GetEntityName().Should().Be(entityName);
        }

        /// <summary>
        /// Asserts that an info form notification can be seen with the given message.
        /// </summary>
        /// <param name="message">The message of the notification.</param>
        [Then(@"I should be able to see an info form notification stating '(.*)'")]
        public static void ThenIShouldBeAbleToSeeAnInfoFormNotificationStating(string message)
        {
            XrmApp.Entity.GetFormNotifications().Should().Contain(formNotification => formNotification.Type == FormNotificationType.Information && formNotification.Message == message);
        }

        /// <summary>
        /// Asserts that a warning form notification can be seen with the given message.
        /// </summary>
        /// <param name="message">The message of the notification.</param>
        [Then(@"I should be able to see a warning form notification stating '(.*)'")]
        public static void ThenIShouldBeAbleToSeeAWarningFormNotificationStating(string message)
        {
            XrmApp.Entity.GetFormNotifications().Should().Contain(formNotification => formNotification.Type == FormNotificationType.Warning && formNotification.Message == message);
        }

        /// <summary>
        /// Asserts that an error form notification can be seen with the given message.
        /// </summary>
        /// <param name="message">The message of the notification.</param>
        [Then(@"I should be able to see an error form notification stating '(.*)'")]
        public static void ThenIShouldBeAbleToSeeAnErrorFormNotificationStating(string message)
        {
            XrmApp.Entity.GetFormNotifications().Should().Contain(formNotification => formNotification.Type == FormNotificationType.Error && formNotification.Message == message);
        }

        /// <summary>
        /// Asserts that the given value is in the header title.
        /// </summary>
        /// <param name="message">The header title.</param>
        [Then(@"I should be able to see a value of '(.*)' as the header title")]
        public static void ThenIShouldBeAbleToSeeAsTheHeaderTitle(string message)
        {
            XrmApp.Entity.GetHeaderTitle().Should().Contain(message);
        }

        /// <summary>
        /// Asserts that a field is editable on the form.
        /// </summary>
        /// <param name="fieldName">The label or logical name of the field.</param>
        [Then(@"I should be able to edit the '(.*)' field")]
        public static void ThenIShouldBeAbleToEditTheField(string fieldName)
        {
            App.Entity.IsFieldEditable(fieldName, editable: true).Should().BeTrue(because: "the field should be visible and editable");
        }

        /// <summary>
        /// Asserts that the given values are available in an option set.
        /// </summary>
        /// <param name="fieldName">The name of the option set field.</param>
        /// <param name="expectedOptionsTable">The options.</param>
        [Then(@"I should be able to see the following options in the '(.*)' option set field")]
        public static void ThenIShouldBeAbleToSeeTheFollowingOptionsInTheOptionSetField(string fieldName, Table expectedOptionsTable)
        {
            ArgumentNullException.ThrowIfNull(expectedOptionsTable, nameof(expectedOptionsTable));

            var expectedOptions = expectedOptionsTable.Rows.Select(r => r[0]);

            foreach (var option in XrmApp.Entity.GetAllOptionSetValue(fieldName))
            {
                expectedOptions.Should().Contain(option.Text, because: "the options be in the list of expected options");
            }
        }

        /// <summary>
        /// Asserts that the provided fields are editable.
        /// </summary>
        /// <param name="table">A table containing the fields to assert against.</param>
        [Then(@"I should be able to edit the following fields")]
        public static void ThenIShouldBeAbleToEditTheFollowingFields(Table table)
        {
            ArgumentNullException.ThrowIfNull(table, nameof(table));

            var fields = table.Rows.Select((row) => row.Values.First());

            foreach (var field in fields)
            {
                ThenIShouldBeAbleToEditTheField(field);
            }
        }

        /// <summary>
        /// Asserts that a field is read-only.
        /// </summary>
        /// <param name="fieldName">The label or logical name of the field.</param>
        [Then(@"I should not be able to edit the '(.*)' field")]
        public static void ThenIShouldNotBeAbleToEditTheField(string fieldName)
        {
            App.Entity.IsFieldEditable(fieldName, editable: false).Should().BeTrue(because: "the field should not be editable");
        }

        /// <summary>
        /// Asserts that the provided fields are not editable.
        /// </summary>
        /// <param name="table">A table containing the fields to assert against.</param>
        [Then(@"I should not be able to edit the following fields")]
        public static void ThenIShouldNotBeAbleToEditTheFollowingFields(Table table)
        {
            ArgumentNullException.ThrowIfNull(table, nameof(table));

            foreach (var field in table.Rows.Select((row) => row.Values.First()))
            {
                ThenIShouldNotBeAbleToEditTheField(field);
            }
        }

        /// <summary>
        /// Asserts that a field is visible.
        /// </summary>
        /// <param name="fieldName">The label or logical name of the field.</param>
        [Then(@"I should be able to see the '(.*)' field")]
        public static void ThenIShouldBeAbleToSeeTheField(string fieldName)
        {
            App.Entity.IsFieldShown(fieldName, shown: true).Should().BeTrue(because: $"the '{fieldName}' field should be visible");
        }

        /// <summary>
        /// Asserts that a field is not visible.
        /// </summary>
        /// <param name="fieldName">The label or logical name of the field.</param>
        [Then(@"I should not be able to see the '(.*)' field")]
        public static void ThenIShouldNotBeAbleToSeeTheField(string fieldName)
        {
            App.Entity.IsFieldShown(fieldName, shown: false).Should().BeTrue(because: $"the '{fieldName}' field should not be visible");
        }

        /// <summary>
        /// Asserts that a field has no value. In a model-driven app it reads the form's value, so the field may be hidden.
        /// </summary>
        /// <param name="field">The label or logical name of the field.</param>
        [Then(@"the '(.*)' field should be empty")]
        public static void ThenTheFieldShouldBeEmpty(string field)
        {
            App.Entity.IsFieldEmpty(field).Should().BeTrue(because: $"the '{field}' field should be empty");
        }

        /// <summary>
        /// Asserts that a record is active or inactive.
        /// </summary>
        /// <param name="status">The status.</param>
        [Then(@"^the status of the record should be (active|inactive)$")]
        public static void ThenTheStatusOfTheRecordShouldBe(string status)
        {
            GetFormStatus().Should().BeEquivalentTo(status);
        }

        /// <summary>
        /// Asserts that a section is visible.
        /// </summary>
        /// <param name="sectionName">The name of the section.</param>
        [Then(@"I should be able to see the '(.*)' section")]
        public static void ThenIShouldBeAbleToSeeTheSection(string sectionName)
        {
            IsSectionVisible(sectionName).Should().BeTrue(because: "the section should be visible");
        }

        /// <summary>
        /// Asserts that a section is not visible.
        /// </summary>
        /// <param name="sectionName">The name of the section.</param>
        [Then(@"I should not be able to see the '(.*)' section")]
        public static void ThenIShouldNotBeAbleToSeeTheSection(string sectionName)
        {
            IsSectionVisible(sectionName).Should().BeFalse(because: "the section should be visible");
        }

        /// <summary>
        /// Asserts that the active tab is the provided one.
        /// </summary>
        /// <param name="expectedTabLabel">The label of the expected tab.</param>
        [Then(@"the active tab should be '(.*)'")]
        public static void ThenTheActiveTabShouldBe(string expectedTabLabel)
        {
            expectedTabLabel = expectedTabLabel.ReplaceTemplatedText(TestConfig.Parameters);
            App.Entity.IsTabSelected(expectedTabLabel).Should().BeTrue(because: $"the '{expectedTabLabel}' tab should be the active one");
        }

        private static bool IsSectionVisible(string sectionName)
        {
            return XrmApp.Entity.IsSectionVisible(sectionName);
        }

        private static string GetFormStatus()
        {
            // 'Active' without a read-only notification, the status a read-only notification names, or nothing when the
            // record is read-only for another reason; then only the header can show the status.
            var result = XrmApp.Entity.GetFormStatus();

            if (!string.IsNullOrEmpty(result))
            {
                return result;
            }

            try
            {
                return XrmApp.Entity.GetHeaderValue(new OptionSet { Name = "statecode" });
            }
            catch (Exception ex)
            {
                throw new NotFoundException("Unable to determine the status from the form. This can happen if you do not have access to edit the record and the state is not in the header.", ex);
            }
        }
    }
}
