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
        private static readonly TimeSpan PortalStateTimeout = TimeSpan.FromSeconds(10);
        // Tabs and fields of a portal page (or of the record selected in its subgrid) can render well after the page loads.
        private static readonly TimeSpan PortalRenderTimeout = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Selects a tab on the form.
        /// </summary>
        /// <param name="tabName">The name of the tab.</param>
        [When(@"I select the '(.*)' tab")]
        public static void ISelectTab(string tabName)
        {
            if (Driver.IsTalxisPortal())
            {
                // Tab labels can contain record values (e.g. a number), referenced as parameters.
                tabName = tabName.ReplaceTemplatedText(TestConfig.Parameters);
                Driver.WaitForPortalCondition(() => Driver.FindPortalTab(tabName) != null, PortalRenderTimeout);
                Driver.SelectPortalTab(tabName, PortalRenderTimeout)
                    .Should().BeTrue(because: $"the '{tabName}' tab should be the selected one after clicking it");
                Driver.CloseMenus();
                return;
            }

            XrmApp.Entity.SelectTab(tabName);
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

            if (Driver.IsTalxisPortal())
            {
                tabName = tabName.ReplaceTemplatedText(TestConfig.Parameters);

                // Tabs are shown or hidden by scripts after the page loads.
                var visible = shouldBeVisible
                    ? Driver.WaitForPortalCondition(() => Driver.FindPortalTab(tabName) != null, PortalRenderTimeout)
                    : !Driver.BecomesAbsent(() => Driver.FindPortalTab(tabName) != null, PortalStateTimeout);
                Driver.CloseMenus();
                visible.Should().Be(shouldBeVisible, because: $"The tab '{tabName}' {(shouldBeVisible ? "should" : "should not")} be visible.");
                return;
            }

            XrmApp.Entity.IsTabVisible(tabName)
                .Should()
                .Be(
                    shouldBeVisible,
                    because: $"The tab '{tabName}' {(shouldBeVisible ? "should" : "should not")} be visible.");
        }

        // !TO-DO: WI:35121 - Remove fieldLocation

        /// <summary>
        /// Sets the value for the field.
        /// </summary>
        /// <param name="fieldValue">The field value.</param>
        /// <param name="fieldLabel">The field name.</param>
        [When(@"I enter '(.*)' into the '(.*)' field on the form")]
        public static void WhenIEnterInTheField(string fieldValue, string fieldLabel)
        {
            Driver.WaitForPageToLoad();

            string fieldLogicalName = XrmApp.Entity.GetFieldLogicalNameFromLabel(Driver, fieldLabel);

            string fieldType = MetadataHelper.GetFieldTypeFromDomByLogicalName(fieldLogicalName);
            string fieldLocation = MetadataHelper.GetFieldLocationFromDomByLogicalName(fieldLogicalName);

            if (fieldLocation == "field")
            {
                SetFieldValue(fieldLogicalName, fieldValue.ReplaceTemplatedText(TestConfig.Parameters), fieldType);
            }
            else
            {
                SetHeaderFieldValue(fieldLabel, fieldValue.ReplaceTemplatedText(TestConfig.Parameters), fieldType);
            }

            Client.TryLoseFocus();

            Driver.WaitForTransaction();
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
            if (Driver.IsTalxisPortal())
            {
                expectedValue = expectedValue.ReplaceTemplatedText(TestConfig.Parameters);

                // Values may be filled in asynchronously (e.g. a nested form showing the selected record).
                Driver.WaitForPortalCondition(() => Driver.FindPortalField(fieldLabel).GetPortalFieldValue() == expectedValue, PortalRenderTimeout);
                Driver.FindPortalField(fieldLabel).GetPortalFieldValue().Should().Be(expectedValue);
                return;
            }

            string fieldLogicalName;
            string actualValue;

            fieldLogicalName = XrmApp.Entity.GetFieldLogicalNameFromLabel(Driver, fieldLabel);

            string fieldLocation = MetadataHelper.GetFieldLocationFromDomByLogicalName(fieldLogicalName);
            string fieldType = MetadataHelper.GetFieldTypeFromDomByLogicalName(fieldLogicalName);

            switch (fieldType)
            {
                case "currency":
                case "numeric":
                case "text":
                    actualValue = fieldLocation == "field" ? XrmApp.Entity.GetValue(fieldLogicalName) : XrmApp.Entity.GetHeaderValue(fieldLogicalName);
                    actualValue.Should().Be(expectedValue);
                    break;
                case "optionset":

                    OptionSet optionsetField = new()
                    {
                        Name = fieldLogicalName
                    };

                    actualValue = fieldLocation == "field"
                        ? XrmApp.Entity.GetValue(optionsetField)
                        : XrmApp.Entity.GetHeaderValue(optionsetField);
                    actualValue.Should().Be(expectedValue);
                    break;
                case "multioptionset":
                    MultiValueOptionSet multioptionsetField = new()
                    {
                        Name = fieldLogicalName,
                    };
                    XrmApp.Entity.GetValue(multioptionsetField).Values.Should().Equal(expectedValue);
                    break;
                case "lookup":
                    LookupItem lookupField = new()
                    {
                        Name = fieldLogicalName,
                    };
                    actualValue = fieldLocation == "field" ? XrmApp.Entity.GetValue(lookupField) : XrmApp.Entity.GetHeaderValue(lookupField);
                    actualValue.Should().Be(expectedValue);
                    break;
                case "datetime":
                    DateTimeControl datetimeField = new DateTimeControl(fieldLogicalName);
                    System.DateTime? datetimeActualValue = fieldLocation == "field" ? XrmApp.Entity.GetValue(datetimeField) : XrmApp.Entity.GetHeaderValue(datetimeField);

                    var expectedDateTime = DateTime.Parse(expectedValue, CultureInfo.CurrentCulture);

                    if (expectedValue.Contains(":"))
                    {
                        datetimeActualValue.Should().Be(expectedDateTime);
                    }
                    else
                    {
                        datetimeActualValue.Value.Date.Should().Be(expectedDateTime.Date);
                    }

                    break;
                case "boolean":
                    BooleanItem booleanField = new()
                    {
                        Name = fieldLogicalName,
                    };
                    bool booleanActualValue = fieldLocation == "field" ? XrmApp.Entity.GetValue(booleanField) : XrmApp.Entity.GetHeaderValue(booleanField);
                    booleanActualValue.Should().Be(bool.Parse(expectedValue));
                    break;
            }
        }

        /// <summary>
        /// Asserts that a field is mandatory or optional.
        /// </summary>
        /// <param name="fieldLabel">The name of the field.</param>
        /// <param name="requirementLevel">Whether the field should be mandatory or optional.</param>
        [Then(@"the '(.*)' field should be (mandatory|optional)")]
        public static void ThenTheFieldShouldBeMandatory(string fieldLabel, string requirementLevel)
        {
            if (Driver.IsTalxisPortal())
            {
                var required = requirementLevel == "mandatory";

                // Form scripts may set the requirement level after the form loads.
                Driver.WaitForPortalCondition(() => Driver.FindPortalField(fieldLabel).IsPortalFieldRequired() == required, PortalStateTimeout);
                Driver.FindPortalField(fieldLabel).IsPortalFieldRequired().Should().Be(required, because: $"the field should be {requirementLevel}");
                return;
            }

            string fieldName = XrmApp.Entity.GetFieldLogicalNameFromLabel(Driver, fieldLabel);

            XrmApp.Entity.GetFieldSectionItemContainertAttribute<int>(fieldLabel, "data-fieldrequirement")
                .Should().Be(requirementLevel == "mandatory" ? 2 : 0, because: $"the field should be {requirementLevel}");
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
        /// <param name="fieldName">The name of the field.</param>
        [Then(@"I should be able to edit the '(.*)' field")]
        public static void ThenIShouldBeAbleToEditTheField(string fieldName)
        {
            var field = XrmApp.Entity.GetField(fieldName);

            field.IsVisible.Should().BeTrue(because: "the field must be visible to be editable");
            field.IsReadOnly(Driver).Should().BeFalse(because: "the field should be editable");
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
        /// <param name="fieldName">The name of the field.</param>
        [Then(@"I should not be able to edit the '(.*)' field")]
        public static void ThenIShouldNotBeAbleToEditTheField(string fieldName)
        {
            if (Driver.IsTalxisPortal())
            {
                Driver.WaitForPortalCondition(() => Driver.FindPortalField(fieldName).IsPortalFieldReadOnly(), PortalStateTimeout);
                Driver.FindPortalField(fieldName).IsPortalFieldReadOnly().Should().BeTrue(because: "the field should not be editable");
                return;
            }

            XrmApp.Entity.GetField(fieldName).IsReadOnly(Driver).Should().BeTrue(because: "the field should not be editable");
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
        /// Asserts that a field is not visible.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        [Then(@"I should be able to see the '(.*)' field")]
        public static void ThenIShouldBeAbleToSeeTheField(string fieldName)
        {
            if (Driver.IsTalxisPortal())
            {
                Driver.WaitForPortalCondition(() => Driver.TryFindPortalField(fieldName)?.Displayed == true, PortalRenderTimeout)
                    .Should().BeTrue(because: $"the '{fieldName}' field should be visible");
                return;
            }

            XrmApp.Entity.IsFieldVisible(Driver, fieldName).Should().BeTrue(because: "the field should be visible");
        }

        /// <summary>
        /// Asserts that a field is not visible.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        [Then(@"I should not be able to see the '(.*)' field")]
        public static void ThenIShouldNotBeAbleToSeeTheField(string fieldName)
        {
            if (Driver.IsTalxisPortal())
            {
                Driver.BecomesAbsent(() => Driver.TryFindPortalField(fieldName)?.Displayed == true, PortalStateTimeout)
                    .Should().BeTrue(because: $"the '{fieldName}' field should not be visible");
                return;
            }

            XrmApp.Entity.IsFieldVisible(Driver, fieldName).Should().BeFalse(because: "the field should not be visible");
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
            if (Driver.IsTalxisPortal())
            {
                expectedTabLabel = expectedTabLabel.ReplaceTemplatedText(TestConfig.Parameters);
                Driver.WaitForPortalCondition(() => Driver.FindPortalTab(expectedTabLabel)?.IsPortalTabSelected() == true, PortalStateTimeout)
                    .Should().BeTrue(because: $"the '{expectedTabLabel}' tab should be the active one");
                Driver.CloseMenus();
                return;
            }

            XrmApp.Entity.GetAttributeOfActiveTab("aria-label").Should().Be(expectedTabLabel);
        }

        private static bool IsSectionVisible(string sectionName)
        {
            return XrmApp.Entity.IsSectionVisible(sectionName);
        }

        private static string GetFormStatus()
        {
            var result = XrmApp.Entity.GetFormStatus();

            if (string.IsNullOrEmpty(result))
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

        private static void SetFieldValue(string fieldName, string fieldValue, string fieldType)
        {
            switch (fieldType)
            {
                case "multioptionset":
                    XrmApp.Entity.SetValue(
                        new MultiValueOptionSet()
                        {
                            Name = fieldName,
                            Values = fieldValue
                                        .Split(',')
                                        .Select(v => v.Trim())
                                        .ToArray(),
                        },
                        true);
                    break;
                case "optionset":
                    bool isNewLookEnabled = AppLookExtensions.IsNewLookEnabled(Driver);

                    XrmApp.Entity.SetValue(new OptionSet()
                    {
                        Name = fieldName,
                        Value = fieldValue,
                    });

                    break;
                case "boolean":
                    XrmApp.Entity.SetValue(new BooleanItem()
                    {
                        Name = fieldName,
                        Value = bool.Parse(fieldValue),
                    });
                    break;
                case "datetime":
                    XrmApp.Entity.SetValue(new DateTimeControl(fieldName)
                    {
                        // !TO-DO: Datetime formattings
                        Value = DateTime.Parse(fieldValue, CultureInfo.CurrentCulture),
                    });
                    break;
                case "lookup":
                    XrmApp.Entity.SetValue(new LookupItem()
                    {
                        Name = fieldName,
                        Value = fieldValue,
                    });
                    break;
                case "currency":
                case "numeric":
                case "text":
                default:
                    XrmApp.Entity.SetValue(fieldName, fieldValue);
                    break;
            }
        }

        private static void SetHeaderFieldValue(string fieldName, string fieldValue, string fieldType)
        {
            switch (fieldType)
            {
                case "optionset":
                    XrmApp.Entity.SetHeaderValue(new OptionSet()
                    {
                        Name = fieldName,
                        Value = fieldValue,
                    });
                    break;
                case "boolean":
                    XrmApp.Entity.SetHeaderValue(new BooleanItem()
                    {
                        Name = fieldName,
                        Value = bool.Parse(fieldValue),
                    });
                    break;
                case "datetime":
                    XrmApp.Entity.SetHeaderValue(new DateTimeControl(fieldName)
                    {
                        Value = DateTime.Parse(fieldValue, CultureInfo.CurrentCulture),
                    });
                    break;
                case "lookup":
                    XrmApp.Entity.SetHeaderValue(new LookupItem()
                    {
                        Name = fieldName,
                        Value = fieldValue,
                    });
                    break;
                case "currency":
                case "numeric":
                case "text":
                default:
                    XrmApp.Entity.SetHeaderValue(fieldName, fieldValue);
                    break;
            }
        }
    }
}
