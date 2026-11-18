using System;
using System.Linq;
using OpenQA.Selenium;
using TALXIS.TestKit.Selectors.DTO;
using TALXIS.TestKit.Selectors.DTO.Locators;
using TALXIS.TestKit.Selectors.Browser;

namespace TALXIS.TestKit.Selectors.WebClientManagement.Helpers
{
    internal class DialogHelper
    {
        // How long a dialog that a click has just opened may take to render in the page.
        private static readonly TimeSpan InPageDialogTimeout = TimeSpan.FromSeconds(5);

        internal static BrowserCommandResult<bool> AssignDialog(Dialogs.AssignTo to, WebClient client, string userOrTeamName = null)
        {
            return client.Execute(client.GetOptions($"Assign to User or Team Dialog"), driver =>
            {
                var inlineDialog = SwitchToDialog(client);
                if (!inlineDialog)
                    return false;

                if (to == Dialogs.AssignTo.Me)
                {
                    SetValueHelper.SetOptionSetValue(client,new OptionSet { Name = Elements.ElementId[Reference.Dialogs.Assign.AssignToId], Value = "Me" }, FormContextType.Dialog);
                }
                else
                {
                    SetValueHelper.SetOptionSetValue(client, new OptionSet { Name = Elements.ElementId[Reference.Dialogs.Assign.AssignToId], Value = "User or team" }, FormContextType.Dialog);

                    //Set the User Or Team
                    var userOrTeamField = driver.WaitUntilAvailable(EntityElementsLocators.TextFieldLookup, "User field unavailable");
                    var input = userOrTeamField.ClickWhenAvailable(By.TagName("input"), "User field unavailable");
                    input.SendKeys(userOrTeamName, true);

                    client.ThinkTime(2000);

                    //Pick the User from the list
                    var container = driver.WaitUntilVisible(DialogsElementsLocators.AssignDialogUserTeamLookupResults);
                    container.WaitUntil(
                        c => c.FindElements(By.TagName("li")).FirstOrDefault(r => r.Text.StartsWith(userOrTeamName, StringComparison.OrdinalIgnoreCase)),
                        successCallback: e => e.Click(true),
                        failureCallback: () => throw new InvalidOperationException($"None {to} found which match with '{userOrTeamName}'"));
                }

                //Click Assign
                driver.ClickWhenAvailable(DialogsElementsLocators.AssignDialogOKButton, TimeSpan.FromSeconds(5),
                    "Unable to click the OK button in the assign dialog");

                return true;
            });
        }

        internal static BrowserCommandResult<bool> CloseOpportunityDialog(bool clickOK, WebClient client)
        {
            return client.Execute(client.GetOptions($"Close Opportunity Dialog"), driver =>
            {
                var inlineDialog = SwitchToDialog(client);

                if (inlineDialog)
                {
                    //Close Opportunity
                    var xPath = AppElements.Xpath[AppReference.Dialogs.CloseOpportunity.Ok];

                    //Cancel
                    if (!clickOK)
                        xPath = AppElements.Xpath[AppReference.Dialogs.CloseOpportunity.Ok];

                    driver.ClickWhenAvailable(By.XPath(xPath), TimeSpan.FromSeconds(5), "The Close Opportunity dialog is not available.");
                }

                return true;
            });
        }

        internal static BrowserCommandResult<bool> ConfirmationDialog(bool ClickConfirmButton, WebClient client)
        {
            //Passing true clicks the confirm button.  Passing false clicks the Cancel button.
            return client.Execute(client.GetOptions($"Confirm or Cancel Confirmation Dialog"), driver =>
            {
                if (!SwitchToDialog(client))
                    throw new InvalidOperationException("No confirmation dialog is displayed.");

                driver.ClickWhenAvailable(
                    ClickConfirmButton ? DialogsElementsLocators.ConfirmButton : DialogsElementsLocators.CancelButton,
                    TimeSpan.FromSeconds(5),
                    $"The confirmation dialog has no {(ClickConfirmButton ? "confirm" : "cancel")} button.");

                return true;
            });
        }

        internal static bool SwitchToDialog(WebClient client, int frameIndex = 0)
        {
            var index = "";
            if (frameIndex > 0)
                index = frameIndex.ToString();

            client.Browser.Driver.SwitchTo().DefaultContent();

            // Check to see if dialog is InlineDialog or popup
            var inlineDialog = client.Browser.Driver.HasElement(By.XPath(Elements.Xpath[Reference.Frames.DialogFrame].Replace("[INDEX]", index)));
            if (inlineDialog)
            {
                //wait for the content panel to render
                client.Browser.Driver.WaitUntilAvailable(By.XPath(Elements.Xpath[Reference.Frames.DialogFrame].Replace("[INDEX]", index)),
                    TimeSpan.FromSeconds(2),
                    d => { client.Browser.Driver.SwitchTo().Frame(Elements.ElementId[Reference.Frames.DialogFrameId].Replace("[INDEX]", index)); });
                return true;
            }

            // The current UI renders its dialogs in the page itself: nothing to switch to.
            if (client.Browser.Driver.WaitUntilAvailable(DialogsElementsLocators.InPageDialog, InPageDialogTimeout) != null)
                return true;

            return SwitchToPopup(client);
        }
        private static bool SwitchToPopup(WebClient client)
        {
            var driver = client.Browser.Driver;
            var mainWindow = driver.CurrentWindowHandle;
            var windowHandles = driver.WindowHandles;

            if (windowHandles.Count <= 1)
                return false;

            foreach (var handle in windowHandles)
            {
                if (handle != mainWindow)
                {
                    driver.SwitchTo().Window(handle);

                    if (driver.WaitUntilAvailable(By.TagName("body"), TimeSpan.FromSeconds(2)) != null)
                        return true;

                    driver.SwitchTo().Window(mainWindow);
                }
            }

            return false;
        }
    }
}
