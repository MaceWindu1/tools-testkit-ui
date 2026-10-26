namespace TALXIS.TestKit.Bindings.Steps
{
    using System;
    using FluentAssertions;
    using OpenQA.Selenium;
    using Reqnroll;
    using TALXIS.TestKit.Bindings.Extensions;
    using TALXIS.TestKit.Selectors;

    /// <summary>
    /// Step bindings related to dialogs.
    /// </summary>
    [Binding]
    public class DialogSteps : PowerAppsStepDefiner
    {
        private static readonly TimeSpan PortalStateTimeout = TimeSpan.FromSeconds(10);

        /// <summary>
        /// Clicks the confirmation button on a confirm dialog.
        /// </summary>
        /// <param name="option">The option to click.</param>
        [When(@"^I (confirm|cancel) when presented with the confirmation dialog$")]
        public static void WhenIConfirmWhenPresentedWithTheConfirmationDialog(string option)
        {
            if (Driver.IsTalxisPortal())
            {
                Driver.AnswerPortalMessageDialog(option == "confirm");
                return;
            }

            XrmApp.Dialogs.ConfirmationDialog(option == "confirm");
            XrmApp.ThinkTime(2000);
        }

        /// <summary>
        /// Assigns to the current user.
        /// </summary>
        [When("I assign to me on the assign dialog")]
        public static void WhenIAssignToMeOnTheAssignDialog()
        {
            XrmApp.Dialogs.Assign(Dialogs.AssignTo.Me);
        }

        /// <summary>
        /// Assigns to a user or team with the given name.
        /// </summary>
        /// <param name="assignTo">User or team.</param>
        /// <param name="userName">The name of the user or team.</param>
        [When("I assign to a (user|team) named '(.*)' on the assign dialog")]
        public static void WhenIAssignToANamedOnTheAssignDialog(Dialogs.AssignTo assignTo, string userName)
        {
            XrmApp.Dialogs.Assign(assignTo, userName);
        }

        /// <summary>
        /// Closes an opportunity.
        /// </summary>
        /// <param name="status">Whether the opportunity was won.</param>
        [When("^I close the opportunity as (won|lost)$")]
        public static void WhenICloseTheOpportunityAs(string status)
        {
            XrmApp.Dialogs.CloseOpportunity(status == "won");
        }

        /// <summary>
        /// Closes a alert dialog.
        /// </summary>
        [When("I close the alert dialog")]
        public static void WhenICloseTheAlertDialog()
        {
            if (Driver.IsTalxisPortal())
            {
                Driver.AnswerPortalMessageDialog(confirm: true);
                return;
            }

            XrmApp.Dialogs.CloseWarningDialog();
        }

        /// <summary>
        /// Clicks an option on the publish dialog.
        /// </summary>
        /// <param name="option">The option to click.</param>
        [When("^I click (confirm|cancel) on the publish dialog$")]
        public static void WhenIClickOnThePublishDialog(string option)
        {
            XrmApp.Dialogs.PublishDialog(option == "confirm");
        }

        /// <summary>
        /// Clicks an option on the set state dialog.
        /// </summary>
        /// <param name="option">The option to click.</param>
        [When("^I click (ok|cancel) on the set state dialog$")]
        public static void WhenIClickOnTheSetStateDialog(string option)
        {
            XrmApp.Dialogs.SetStateDialog(option == "ok");
        }

        /// <summary>
        /// Check if an alert dialog is displayed.
        /// </summary>
        [Then(@"an alert dialog should be displayed")]
        public static void ThenAlertDialogIsDisplayed()
        {
            if (Driver.IsTalxisPortal())
            {
                Driver.WaitForPortalCondition(() => Driver.GetPortalAlertDialogTitle() != null, PortalStateTimeout)
                    .Should().BeTrue(because: "an alert dialog should be displayed");
                return;
            }

            var dialog = XrmApp.Dialogs.GetAlertDialog();

            dialog.Should().NotBeNull();
        }

        /// <summary>
        /// Check if an alert dialog with specified title is displayed.
        /// </summary>
        /// <param name="expectedTitle">The title of the alert dialog that is expected.</param>
        [Then(@"an alert dialog with title '(.*)' should be displayed")]
        public static void ThenAlertDialogWithTextIsDisplayed(string expectedTitle)
        {
            if (Driver.IsTalxisPortal())
            {
                // Titles can contain record values (e.g. a number), referenced as parameters.
                expectedTitle = expectedTitle.ReplaceTemplatedText(TestConfig.Parameters);
                Driver.WaitForPortalCondition(() => Driver.GetPortalAlertDialogTitle() == expectedTitle, PortalStateTimeout);
                Driver.GetPortalAlertDialogTitle().Should().Be(expectedTitle);
                return;
            }

            var dialogText = XrmApp.Dialogs.GetAlertDialogMessageText();

            dialogText.Should().Be(expectedTitle);
        }

        /// <summary>
        /// Asserts that a confirmation dialog with the given title is displayed.
        /// </summary>
        /// <param name="expectedTitle">The title of the confirmation dialog that is expected.</param>
        [Then(@"a confirmation dialog with title '(.*)' should be displayed")]
        public static void ThenAConfirmationDialogWithTitleShouldBeDisplayed(string expectedTitle)
        {
            if (Driver.IsTalxisPortal())
            {
                ThenAlertDialogWithTextIsDisplayed(expectedTitle);
                return;
            }

            XrmApp.Dialogs.IsDialogVisible().Should().BeTrue(because: "a confirmation dialog should be displayed");

            XrmApp.Dialogs.GetCurrentDialogLable().Should().Be(expectedTitle);
        }
    }
}
