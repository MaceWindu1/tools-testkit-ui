namespace TALXIS.TestKit.Bindings.Steps
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using FluentAssertions;
    using OpenQA.Selenium.Support.UI;
    using Reqnroll;
    using TALXIS.TestKit.Bindings.Extensions;
    using TALXIS.TestKit.Selectors;
    using TALXIS.TestKit.Selectors.Browser;
    using static System.Net.Mime.MediaTypeNames;
    using static TALXIS.TestKit.Selectors.Browser.Constants;
    using System;
    using System.IO;

    /// <summary>
    /// Step bindings related to quick creates.
    /// </summary>
    [Binding]
    public class QuickCreateSteps : PowerAppsStepDefiner
    {
        /// <summary>
        /// Sets the value for the field on the quick create.
        /// </summary>
        /// <param name="fieldValue">The field value.</param>
        /// <param name="fieldLabel">The field name.</param>
        [When(@"I enter '(.*)' into the '(.*)' field on the quick create")]
        public static void WhenIEnterInTheFieldOnTheQuickCreate(string fieldValue, string fieldLabel)
        {
            Driver.WaitForPageToLoad();

            string fieldLogicalName = XrmApp.Entity.GetFieldLogicalNameFromLabel(fieldLabel);
            string fieldType = MetadataHelper.GetFieldTypeFromDomByLogicalName(fieldLogicalName);

            SetFieldValue(fieldLogicalName, fieldValue.ReplaceTemplatedText(TestConfig.Parameters), fieldType);

            // Click to lose focus - So that business rules and other form events can occur
            Client.TryLoseFocus();

            Driver.WaitForTransaction();
        }

        /// <summary>
        /// Sets the values for the fields in the table for the quick create without requiring Type column.
        /// </summary>
        /// <param name="fields">Fields to be set.</param>
        [When(@"I enter the following into the quick create")]
        public static void WhenIEnterTheFollowingIntoTheQuickCreate(Table fields)
        {
            Driver.WaitForTransaction();
            Driver.WaitForPageToLoad();

            fields = fields ?? throw new ArgumentNullException(nameof(fields));

            Driver.WaitForPageToLoad();

            foreach (DataTableRow row in fields.Rows)
            {
                string fieldName = row["Field"];
                string value = row["Value"];

                WhenIEnterInTheFieldOnTheQuickCreate(value, fieldName);
            }
        }

        /// <summary>+
        /// Cancels a quick create.
        /// </summary>
        [When(@"I cancel the quick create")]
        public static void WhenICancelTheQuickCreate()
        {
            XrmApp.QuickCreate.Cancel();
        }

        /// <summary>
        /// Clears the value for the field.
        /// </summary>
        /// <param name="fieldLabel">The field name.</param>
        [When(@"I clear the '(.*)' (?:currency|numeric|text|datetime|boolean) field on the quick create")]
        public static void WhenIClearTheFieldOnTheQuickCreate(string fieldLabel)
        {
            string fieldLogicalName = XrmApp.Entity.GetFieldLogicalNameFromLabel(fieldLabel);
            XrmApp.QuickCreate.ClearValue(fieldLogicalName);
        }

        /// <summary>
        /// Clears the value for the optionset field.
        /// </summary>
        /// <param name="field">The field.</param>
        [When(@"I clear the '(.*)' optionset field on the quick create")]
        public static void WhenIClearTheOptionSetFieldOnTheQuickCreate(OptionSet field)
        {
            XrmApp.QuickCreate.ClearValue(field);
        }

        /// <summary>
        /// Clears the value for the lookup field.
        /// </summary>
        /// <param name="field">The field.</param>
        [When(@"I clear the '(.*)' lookup field on the quick create")]
        public static void WhenIClearTheLookupFieldOnTheQuickCreate(LookupItem field)
        {
            XrmApp.QuickCreate.ClearValue(field);
        }

        /// <summary>
        /// Saves a quick create.
        /// </summary>
        [When(@"I save the quick create")]
        public static void WhenISaveTheQuickCreate()
        {
            XrmApp.QuickCreate.Save();
        }

        /// <summary>
        /// Asserts that a value is shown in a text, currency or numeric field.
        /// </summary>
        /// <param name="expectedValue">The expected value.</param>
        /// <param name="field">The field name.</param>
        [Then("I should be able to see a value of '(.*)' in the '(.*)' (?:currency|numeric|text) field on the quick create")]
        public static void ThenIShouldBeAbleToSeeAValueOfInTheFieldOnTheQuickCreate(string expectedValue, string field)
        {
            XrmApp.QuickCreate.GetValue(field).Should().Be(expectedValue);
        }

        /// <summary>
        /// Asserts that a value is shown in a text, currency or numeric field.
        /// </summary>
        /// <param name="expectedValue">The expected value.</param>
        /// <param name="field">The field name.</param>
        [Then("I should be able to see a value of '(.*)' in the '(.*)' multioptionset field on the quick create")]
        public static void ThenIShouldBeAbleToSeeAValueOfInTheMultiOptionsetFieldOnTheQuickCreate(string[] expectedValue, MultiValueOptionSet field)
        {
            XrmApp.QuickCreate.GetValue(field).Values.Should().Equal(expectedValue);
        }

        /// <summary>
        /// Asserts that a value is shown in an option set field.
        /// </summary>
        /// <param name="expectedValue">The expected value.</param>
        /// <param name="field">The field name.</param>
        [Then("I should be able to see a value of '(.*)' in the '(.*)' optionset field on the quick create")]
        public static void ThenIShouldBeAbleToSeeAValueOfInTheOptionSetFieldOnTheQuickCreate(string expectedValue, OptionSet field)
        {
            XrmApp.QuickCreate.GetValue(field).Should().Be(expectedValue);
        }

        /// <summary>
        /// Asserts that a value is shown in a lookup field.
        /// </summary>
        /// <param name="expectedValue">The expected value.</param>
        /// <param name="field">The field name.</param>
        [Then("I should be able to see a value of '(.*)' in the '(.*)' lookup field on the quick create")]
        public static void ThenIShouldBeAbleToSeeAValueOfInTheOptionSetFieldOnTheQuickCreate(string expectedValue, LookupItem field)
        {
            XrmApp.QuickCreate.GetValue(field).Should().Be(expectedValue);
        }

        /// <summary>
        /// Asserts that a value is shown in a lookup field.
        /// </summary>
        /// <param name="expectedValue">The expected value.</param>
        /// <param name="field">The field name.</param>
        [Then("I should be able to see a value of '(true|false)' in the '(.*)' boolean field on the quick create")]
        public static void ThenIShouldBeAbleToSeeAValueOfInTheBooleanFieldOnTheQuickCreate(bool expectedValue, BooleanItem field)
        {
            XrmApp.QuickCreate.GetValue(field).Should().Be(expectedValue);
        }

        /// <summary>
        /// Asserts that a value is shown in a datetime field.
        /// </summary>
        /// <param name="expectedValue">The expected value.</param>
        /// <param name="field">The field name.</param>
        [Then(@"I should be able to see a value of '(.*)' in the '(.*)' datetime field on the quick create")]
        public static void ThenIShouldBeAbleToSeeAValueOfInTheDateTimeField(DateTime? expectedValue, DateTimeControl field)
        {
            XrmApp.QuickCreate.GetValue(field).Should().Be(expectedValue);
        }

        private static void SetFieldValue(string fieldName, string fieldValue, string fieldType)
        {
            switch (fieldType)
            {
                case "multioptionset":
                    XrmApp.QuickCreate.SetMultiSelectOptionSetValue(
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
                    XrmApp.QuickCreate.SetValue(new OptionSet()
                    {
                        Name = fieldName,
                        Value = fieldValue,
                    });
                    break;
                case "boolean":
                    XrmApp.QuickCreate.SetValue(new BooleanItem()
                    {
                        Name = fieldName,
                        Value = bool.Parse(fieldValue),
                    });
                    break;
                case "datetime":
                    XrmApp.QuickCreate.SetValue(fieldName, DateTime.Parse(fieldValue, CultureInfo.CurrentCulture));
                    break;
                case "lookup":
                    XrmApp.QuickCreate.SetValue(new LookupItem()
                    {
                        Name = fieldName,
                        Value = fieldValue,
                    });
                    break;
                case "currency":
                case "numeric":
                case "text":
                default:
                    XrmApp.QuickCreate.SetValue(fieldName, fieldValue);
                    break;
            }
        }
    }
}
