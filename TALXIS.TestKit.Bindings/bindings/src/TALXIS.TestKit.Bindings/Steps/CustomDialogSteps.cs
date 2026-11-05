namespace TALXIS.TestKit.Bindings.Steps
{
    using System;
    using FluentAssertions;
    using Reqnroll;
    using TALXIS.TestKit.Bindings.Extensions;

    /// <summary>
    /// Step bindings related to dialogs.
    /// </summary>
    [Binding]
    public class CustomDialogSteps : PowerAppsStepDefiner
    {
        // A safety cap against an infinite loop if a button never disappears.
        private const int MaxClicks = 50;

        /// <summary>
        /// Click a button on a custom dialog.
        /// </summary>
        /// <param name="buttonLabel">The option to click.</param>
        [When(@"I select the '(.*)' command on the displayed dialog")]
        [When(@"I click the '(.*)' button on the displayed dialog")]
        public static void WhenIClickButtonOnCustomDialog(string buttonLabel)
        {
            App.Dialogs.ClickButton(buttonLabel);
        }

        /// <summary>
        /// Selects an option of a choice group (e.g. option set tiles) on the displayed dialog.
        /// </summary>
        /// <param name="option">The option label.</param>
        [When(@"I choose the '(.*)' option on the dialog")]
        public static void WhenIChooseTheOptionOnTheDialog(string option)
        {
            App.Dialogs.ChooseOption(option);
        }

        /// <summary>
        /// Asserts that a button is present on the displayed dialog.
        /// </summary>
        /// <param name="buttonLabel">The button's visible label, title or aria-label.</param>
        [Then(@"I should be able to see the '(.*)' button on the displayed dialog")]
        public static void ThenIShouldBeAbleToSeeTheButtonOnTheDisplayedDialog(string buttonLabel)
        {
            App.Dialogs.IsButtonShown(buttonLabel, shown: true)
                .Should().BeTrue(because: $"the '{buttonLabel}' button should be present on the displayed dialog");
        }

        /// <summary>
        /// Asserts that a button is not present on the displayed dialog.
        /// </summary>
        /// <param name="buttonLabel">The button's visible title or aria-label.</param>
        [Then(@"I should not be able to see the '(.*)' button on the displayed dialog")]
        public static void ThenIShouldNotBeAbleToSeeTheButtonOnTheDisplayedDialog(string buttonLabel)
        {
            App.Dialogs.IsButtonShown(buttonLabel, shown: false)
                .Should().BeTrue(because: $"the '{buttonLabel}' button should not be present on the displayed dialog");
        }

        /// <summary>
        /// Closes the displayed dialog via its own close ('x') control, without saving.
        /// </summary>
        [When(@"I close the displayed dialog without saving")]
        public static void WhenICloseTheDisplayedDialogWithoutSaving()
        {
            App.Dialogs.CloseWithoutSaving();
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
            var clicks = 0;
            while (clicks < MaxClicks && App.Dialogs.TryClickButton(buttonLabel))
            {
                clicks++;
            }

            clicks.Should().BeGreaterThan(0, because: $"the '{buttonLabel}' button should be on the displayed dialog");
            clicks.Should().BeLessThan(MaxClicks, because: $"the '{buttonLabel}' button should stop being offered");
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
            for (int i = 0; i < MaxClicks && !App.Dialogs.IsTextShown(text); i++)
            {
                if (!App.Dialogs.TryClickButton(buttonLabel))
                {
                    break;
                }
            }

            App.Dialogs.IsTextShown(text).Should().BeTrue(because: $"'{text}' should become visible after repeatedly clicking '{buttonLabel}'");
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
            App.Dialogs.SetFieldValue(fieldLabel, fieldValue.ReplaceTemplatedText(TestConfig.Parameters));
        }

        /// <summary>
        /// Asserts that a custom dialog is displayed.
        /// </summary>
        /// <param name="shouldOrShouldNot">Whether the tab should be visible.</param>
        [Then(@"^a dialog (should|should not) be displayed$")]
        public static void ThenADialogShouldBeDisplayed(string shouldOrShouldNot)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(shouldOrShouldNot, nameof(shouldOrShouldNot));

            bool shouldBeVisible = shouldOrShouldNot.Equals("should", StringComparison.InvariantCultureIgnoreCase);

            App.Dialogs.IsDialogShown(shouldBeVisible)
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
            App.Dialogs.HasTitle(dialogTitle, out var actual);
            actual.Should().Be(dialogTitle);
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
        /// Asserts that a field on the displayed dialog shows a validation error (e.g. set by a script with setIsValid).
        /// </summary>
        /// <param name="fieldLabel">The field label.</param>
        /// <param name="errorMessage">The expected error message.</param>
        [Then("the '(.*)' field on the dialog should show the error '(.*)'")]
        public static void ThenTheFieldOnTheDialogShouldShowTheError(string fieldLabel, string errorMessage)
        {
            App.Dialogs.HasFieldError(fieldLabel, errorMessage)
                .Should()
                .BeTrue(because: $"the '{fieldLabel}' field should show the error '{errorMessage}'");
        }

        /// <summary>
        /// Asserts that a value is shown in a field.
        /// </summary>
        /// <param name="expectedValue">The expected value.</param>
        /// <param name="fieldLabel">The field label.</param>
        [Then("I should be able to see a value of '(.*)' in the '(.*)' field on the dialog")]
        public static void ThenIShouldBeAbleToSeeAValueInTheFieldOnTheDialog(string expectedValue, string fieldLabel)
        {
            App.Dialogs.HasFieldValue(fieldLabel, expectedValue, out var actual)
                .Should().BeTrue(because: $"the '{fieldLabel}' field on the dialog should show '{expectedValue}', not '{actual}'");
        }
    }
}
