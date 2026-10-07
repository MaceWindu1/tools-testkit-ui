namespace TALXIS.TestKit.Bindings.Steps
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using FluentAssertions;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Support.UI;
    using Reqnroll;
    using TALXIS.TestKit.Bindings.Extensions;
    using TALXIS.TestKit.Selectors;
    using TALXIS.TestKit.Selectors.Browser;

    /// <summary>
    /// Step bindings related to dialogs.
    /// </summary>
    [Binding]
    public class CustomDialogSteps : PowerAppsStepDefiner
    {
        private static readonly TimeSpan DialogStateTimeout = TimeSpan.FromSeconds(10);

        /// <summary>
        /// Click a button on a custom dialog.
        /// </summary>
        /// <param name="buttonLabel">The option to click.</param>
        [When(@"I select the '(.*)' command on the displayed dialog")]
        [When(@"I click the '(.*)' button on the displayed dialog")]
        public static void WhenIClickButtonOnCustomDialog(string buttonLabel)
        {
            XrmApp.Dialogs.SelectButtonInDialogWindow(buttonLabel);
        }

        /// <summary>
        /// Asserts that a button is not present on the displayed dialog.
        /// </summary>
        /// <param name="buttonLabel">The button's visible title or aria-label.</param>
        [Then(@"I should not be able to see the '(.*)' button on the displayed dialog")]
        public static void ThenIShouldNotBeAbleToSeeTheButtonOnTheDisplayedDialog(string buttonLabel)
        {
            var dialog = Driver.FindElement(By.XPath("//div[@role='dialog']"));

            var matches = dialog.FindElements(By.XPath($".//button[@title='{buttonLabel}' or @aria-label='{buttonLabel}']"));

            matches.Should().BeEmpty(because: $"the '{buttonLabel}' button should not be present on the displayed dialog");
        }

        /// <summary>
        /// Closes the displayed dialog via its own close ('x') control, without saving.
        /// </summary>
        [When(@"I close the displayed dialog without saving")]
        public static void WhenICloseTheDisplayedDialogWithoutSaving()
        {
            var dialog = Driver.FindElement(By.XPath("//div[@role='dialog']"));

            var closeButton = dialog.FindElement(By.XPath(".//button[@aria-label='Close' or @title='Close']"));
            closeButton.Click();

            Driver.WaitForTransaction();
        }

        /// <summary>
        /// Repeatedly clicks a button on the displayed dialog until it is no longer present. Useful
        /// for a multi-step/wizard-style dialog's own "Continue"/"Next" button, to reach its last step
        /// without knowing how many steps it has.
        /// </summary>
        /// <param name="buttonLabel">The button's visible title or aria-label.</param>
        [When(@"I click the '(.*)' button on the displayed dialog repeatedly until it is no longer available")]
        public static void WhenIClickTheButtonOnTheDisplayedDialogRepeatedlyUntilItIsNoLongerAvailable(string buttonLabel)
        {
            const int maxClicks = 50; // safety cap against an infinite loop if the button never disappears
            for (int i = 0; i < maxClicks && TryClickButtonOnDisplayedDialog(buttonLabel); i++)
            {
            }
        }

        /// <summary>
        /// Repeatedly clicks a button on the displayed dialog until an element containing the given
        /// text becomes visible. Useful for navigating a multi-step/wizard-style dialog to a specific
        /// step by its label, without a known step-index mapping.
        /// </summary>
        /// <param name="buttonLabel">The button's visible title or aria-label.</param>
        /// <param name="text">Text expected to become visible once the target step is reached.</param>
        [When(@"I click the '(.*)' button on the displayed dialog repeatedly until '(.*)' is visible")]
        public static void WhenIClickTheButtonOnTheDisplayedDialogRepeatedlyUntilIsVisible(string buttonLabel, string text)
        {
            const int maxClicks = 50;
            for (int i = 0; i < maxClicks; i++)
            {
                if (IsTextVisible(text))
                {
                    return;
                }

                if (!TryClickButtonOnDisplayedDialog(buttonLabel))
                {
                    break;
                }
            }

            IsTextVisible(text).Should().BeTrue(because: $"'{text}' should become visible after repeatedly clicking '{buttonLabel}'");
        }

        private static bool IsTextVisible(string text)
        {
            return Driver.FindElements(By.XPath($"//*[contains(text(), '{text}')]")).Count > 0;
        }

        private static bool TryClickButtonOnDisplayedDialog(string buttonLabel)
        {
            var dialog = Driver.FindElement(By.XPath("//div[@role='dialog']"));
            var buttons = dialog.FindElements(By.XPath($".//button[@title='{buttonLabel}' or @aria-label='{buttonLabel}']"));

            if (buttons.Count == 0)
            {
                return false;
            }

            buttons[0].Click();
            Driver.WaitForTransaction();
            return true;
        }

        /// <summary>
        /// Sets the values of the fields in the table on the custom dialog.
        /// </summary>
        /// <param name="fields">The fields to set.</param>
        [When(@"I enter the following into the dialog")]
        public static void WhenIEnterTheFollowingIntoTheDialog(Table fields)
        {
            fields = fields ?? throw new ArgumentNullException(nameof(fields));

            foreach (DataTableRow row in fields.Rows)
            {
                WhenIEnterInTheFieldOnTheDialog(row["Value"], row["Field"]);
            }
        }

        /// <summary>
        /// Sets the value for the field on a custom dialog.
        /// </summary>
        /// <param name="fieldValue">The field value.</param>
        /// <param name="fieldLabel">The field name.</param>
        [When("^I enter '(.*)' into the '(.*)' field on the dialog$")]
        public static void WhenIEnterInTheFieldOnTheDialog(string fieldValue, string fieldLabel)
        {
            string fieldName = XrmApp.Entity.GetFieldLogicalNameFromLabel(Driver, fieldLabel);

            SetFieldValue(fieldName, fieldValue.ReplaceTemplatedText());

            Driver.WaitForTransaction();
        }

        /// <summary>
        /// Asserts that a custom dialog is displayed.
        /// </summary>
        /// <param name="shouldOrShouldNot">Whether the tab should be visible.</param>
        [Then(@"^a dialog (should|should not) be displayed$")]
        public static void ThenADialogShouldBeDisplayed(string shouldOrShouldNot)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(shouldOrShouldNot,nameof(shouldOrShouldNot));

            bool shouldBeVisible = shouldOrShouldNot.Equals("should", StringComparison.InvariantCultureIgnoreCase);

            // Dialogs open and close asynchronously (e.g. in TALXIS Portal, where WaitForTransaction does not apply).
            bool isInExpectedState;
            try
            {
                isInExpectedState = new WebDriverWait(Driver, DialogStateTimeout)
                    .Until(_ => XrmApp.Dialogs.IsDialogVisible() == shouldBeVisible);
            }
            catch (WebDriverTimeoutException)
            {
                isInExpectedState = false;
            }

            isInExpectedState
                .Should()
                .BeTrue(because: $"A dialog {(shouldBeVisible ? "should" : "should not")} be displayed.");
        }

        /// <summary>
        /// Asserts that a custom dialog with the given title is displayed.
        /// </summary>
        /// <param name="dialogTitle">The title of the displayed custom dialog.</param>
        [Then(@"a dialog with '(.*)' title should be displayed")]
        public static void ThenADialogWithTitleShouldBeDisplayed(string dialogTitle)
        {
            string dialogTitleElement = XrmApp.Dialogs.GetCurrentDialogLable();

            dialogTitleElement.Should().Be(dialogTitle);
        }

        /// <summary>
        /// Asserts that values are shown in fields.
        /// </summary>
        /// <param name="fields">The fields to set.</param>
        [Then(@"the values of the fields on the dialog should be as the following")]
        public static void ThenTheValuesOfTheFieldsOnTheDialogShouldBeAsTheFollowing(Table fields)
        {
            fields = fields ?? throw new ArgumentNullException(nameof(fields));

            foreach (DataTableRow row in fields.Rows)
            {
                ThenIShouldBeAbleToSeeAValueInTheFieldOnTheDialog(row["Value"], row["Field"]);
            }
        }

        /// <summary>
        /// Asserts that a value is shown in a field.
        /// </summary>
        /// <param name="expectedValue">The expected value.</param>
        /// <param name="fieldLabel">The field label.</param>
        [Then("I should be able to see a value of '(.*)' in the '(.*)' field on the dialog")]
        public static void ThenIShouldBeAbleToSeeAValueInTheFieldOnTheDialog(string expectedValue, string fieldLabel)
        {
            string fieldLogicalName;
            string actualValue;

            fieldLogicalName = XrmApp.Entity.GetFieldLogicalNameFromLabel(Driver, fieldLabel);
            string fieldType = MetadataHelper.GetFieldTypeFromDomByLogicalName(fieldLogicalName);

            bool isNewLookEnabled = AppLookExtensions.IsNewLookEnabled(Driver);

            switch (fieldType)
            {
                case "currency":
                case "numeric":
                case "text":
                    actualValue = XrmApp.Entity.GetValue(fieldLogicalName);
                    actualValue.Should().Be(expectedValue);
                    break;
                case "optionset":
                    OptionSet optionsetField = new()
                    {
                        Name = fieldLogicalName,
                    };
                    actualValue = XrmApp.Entity.GetValue(optionsetField);
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
                    actualValue = XrmApp.Entity.GetValue(lookupField);
                    actualValue.Should().Be(expectedValue);
                    break;
                case "datetime":
                    DateTimeControl datetimeField = new DateTimeControl(fieldLogicalName);
                    System.DateTime? datetimeActualValue = XrmApp.Entity.GetValue(datetimeField);
                    datetimeActualValue.Should().Be(DateTime.Parse(expectedValue, CultureInfo.CurrentCulture));
                    break;
                case "boolean":
                    BooleanItem booleanField = new()
                    {
                        Name = fieldLogicalName,
                    };
                    bool booleanActualValue = XrmApp.Entity.GetValue(booleanField);
                    booleanActualValue.Should().Be(bool.Parse(expectedValue));
                    break;
            }
        }

        private static void SetFieldValue(string fieldLogicalName, string fieldValue)
        {
            string fieldType = MetadataHelper.GetFieldTypeFromDomByLogicalName(fieldLogicalName);

            switch (fieldType)
            {
                case "multioptionset":
                    XrmApp.Dialogs.SetValue(
                        new MultiValueOptionSet()
                        {
                            Name = fieldLogicalName,
                            Values = fieldValue
                                        .Split(',')
                                        .Select(v => v.Trim())
                                        .ToArray(),
                        },
                        true);
                    break;
                case "optionset":

                    XrmApp.Dialogs.SetValue(new OptionSet()
                    {
                        Name = fieldLogicalName,
                        Value = fieldValue,
                    });
                    break;
                case "boolean":
                    XrmApp.Dialogs.SetValue(new BooleanItem()
                    {
                        Name = fieldLogicalName,
                        Value = bool.Parse(fieldValue),
                    });
                    break;
                case "datetime":
                    XrmApp.Dialogs.SetValue(new DateTimeControl(fieldLogicalName)
                    {
                        Value = DateTime.Parse(fieldValue, CultureInfo.CurrentCulture),
                    });
                    break;
                case "lookup":
                    XrmApp.Dialogs.SetValue(new LookupItem()
                    {
                        Name = fieldLogicalName,
                        Value = fieldValue,
                    });
                    break;
                case "currency":
                case "numeric":
                case "text":
                    XrmApp.Dialogs.SetValue(fieldLogicalName, fieldValue);
                    break;
                default: throw new Exception(fieldType);
            }
        }
    }
}
