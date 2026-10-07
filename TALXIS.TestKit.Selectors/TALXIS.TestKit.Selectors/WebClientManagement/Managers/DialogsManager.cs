using System;
using System.Collections.Generic;
using System.Linq;
using OpenQA.Selenium;
using TALXIS.TestKit.Selectors.DTO.Locators;
using TALXIS.TestKit.Selectors.Browser;

namespace TALXIS.TestKit.Selectors.WebClientManagement.Helpers
{
    public class DialogsManager
    {
        public WebClient Client { get; }

        public DialogsManager(WebClient client)
        {
            Client = client ?? throw new ArgumentNullException(nameof(client));
        }

        internal static ICollection<IWebElement> GetListItems(IWebElement container, LookupItem control)
        {
            var name = control.Name;
            var xpathToItems = EntityElementsLocators.LookupFieldResultListItem(name);

            //wait for complete the search
            container.WaitUntil(d => d.FindVisible(xpathToItems)?.Text?.Contains(control.Value, StringComparison.OrdinalIgnoreCase) == true);

            ICollection<IWebElement> result = container.WaitUntil(
                d => d.FindElements(xpathToItems),
                failureCallback: () => throw new InvalidOperationException($"No Results Matching {control.Value} Were Found.")
                );
            return result;
        }

        internal BrowserCommandResult<bool> AssignDialog(Dialogs.AssignTo to, string userOrTeamName = null)
            => DialogHelper.AssignDialog(to, Client, userOrTeamName);

        internal BrowserCommandResult<bool> ClickOk()
        {
            //Passing true clicks the confirm button.  Passing false clicks the Cancel button.
            return Client.Execute(Client.GetOptions($"Dialog Click OK"), driver =>
            {
                var inlineDialog = this.SwitchToDialog();
                if (inlineDialog)
                {
                    //Wait until the buttons are available to click
                    var dialogFooter = driver.WaitUntilAvailable(DialogsElementsLocators.OkButton);

                    if (
                        !(dialogFooter?.FindElements(DialogsElementsLocators.OkButton).Count >
                          0)) return true;

                    //Click the Confirm or Cancel button
                    IWebElement buttonToClick;

                    buttonToClick = dialogFooter.FindElement(DialogsElementsLocators.OkButton);
                    buttonToClick.Click();
                }

                return true;
            });
        }

        internal BrowserCommandResult<bool> CloseWarningDialog()
        {
            return Client.Execute(Client.GetOptions($"Close Warning Dialog"), driver =>
            {
                var inlineDialog = this.SwitchToDialog();
                if (inlineDialog)
                {
                    var dialogFooter = driver.WaitUntilAvailable(By.XPath(Elements.Xpath[Reference.Dialogs.WarningFooter]));

                    if (
                        !(dialogFooter?.FindElements(By.XPath(Elements.Xpath[Reference.Dialogs.WarningCloseButton])).Count >
                          0)) return true;
                    var closeBtn = dialogFooter.FindElement(By.XPath(Elements.Xpath[Reference.Dialogs.WarningCloseButton]));
                    closeBtn.Click();
                }

                return true;
            });
        }

        internal BrowserCommandResult<bool> ConfirmationDialog(bool ClickConfirmButton)
            => DialogHelper.ConfirmationDialog(ClickConfirmButton, Client);

        internal BrowserCommandResult<bool> DuplicateDetection(bool clickSaveOrCancel)
        {
            string operationType;

            if (clickSaveOrCancel)
            {
                operationType = "Ignore and Save";
            }
            else
                operationType = "Cancel";

            //Passing true clicks the Ignore and Save button.  Passing false clicks the Cancel button.
            return Client.Execute(Client.GetOptions($"{operationType} Duplicate Detection Dialog"), driver =>
            {
                var inlineDialog = this.SwitchToDialog();
                if (inlineDialog)
                {
                    //Wait until the buttons are available to click
                    var dialogFooter = driver.WaitUntilAvailable(DialogsElementsLocators.DuplicateDetectionIgnoreSaveButton);

                    if (
                        !(dialogFooter?.FindElements(DialogsElementsLocators.DuplicateDetectionIgnoreSaveButton).Count >
                          0)) return true;

                    //Click the Confirm or Cancel button
                    IWebElement buttonToClick;
                    if (clickSaveOrCancel)
                        buttonToClick = dialogFooter.FindElement(DialogsElementsLocators.DuplicateDetectionIgnoreSaveButton);
                    else
                        buttonToClick = dialogFooter.FindElement(DialogsElementsLocators.DuplicateDetectionCancelButton);

                    buttonToClick.Click();
                }

                if (clickSaveOrCancel)
                {
                    // Wait for Save before proceeding
                    driver.WaitForTransaction();
                }

                return true;
            });
        }

        internal BrowserCommandResult<string> GetBusinessProcessErrorText(int waitTimeInSeconds)
         => GetDataHelper.GetBusinessProcessErrorText(waitTimeInSeconds, Client);

        internal BrowserCommandResult<bool> PublishDialog(bool ClickConfirmButton)
        {
            //Passing true clicks the confirm button.  Passing false clicks the Cancel button.
            return Client.Execute(Client.GetOptions($"Confirm or Cancel Publish Dialog"), driver =>
            {
                var inlineDialog = this.SwitchToDialog();
                if (inlineDialog)
                {
                    //Wait until the buttons are available to click
                    var dialogFooter = driver.WaitUntilAvailable(DialogsElementsLocators.PublishConfirmButton);

                    if (
                        !(dialogFooter?.FindElements(DialogsElementsLocators.PublishConfirmButton).Count >
                          0)) return true;

                    //Click the Confirm or Cancel button
                    IWebElement buttonToClick;
                    if (ClickConfirmButton)
                        buttonToClick = dialogFooter.FindElement(DialogsElementsLocators.PublishConfirmButton);
                    else
                        buttonToClick = dialogFooter.FindElement(DialogsElementsLocators.PublishCancelButton);

                    buttonToClick.Click();
                }

                return true;
            });
        }

        internal BrowserCommandResult<bool> SetStateDialog(bool clickOkButton)
        {
            //Passing true clicks the Activate/Deactivate button.  Passing false clicks the Cancel button.
            return Client.Execute(Client.GetOptions($"Interact with Set State Dialog"), driver =>
            {
                var inlineDialog = this.SwitchToDialog();
                if (inlineDialog)
                {
                    //Wait until the buttons are available to click
                    var dialog = driver.WaitUntilAvailable(DialogsElementsLocators.SetStateDialog);

                    if (
                        !(dialog?.FindElements(By.TagName("button")).Count >
                          0)) return true;

                    //Click the Activate/Deactivate or Cancel button
                    IWebElement buttonToClick;
                    if (clickOkButton)
                        buttonToClick = dialog.FindElement(DialogsElementsLocators.SetStateActionButton);
                    else
                        buttonToClick = dialog.FindElement(DialogsElementsLocators.SetStateCancelButton);

                    buttonToClick.Click();
                }

                return true;
            });
        }

        internal BrowserCommandResult<bool> SwitchProcessDialog(string processToSwitchTo)
        {
            return Client.Execute(Client.GetOptions($"Switch Process Dialog"), driver =>
            {
                //Wait for the Grid to load
                driver.WaitUntilVisible(DialogsElementsLocators.ActiveProcessGridControlContainer);

                //Select the Process
                var popup = driver.FindElement(DialogsElementsLocators.SwitchProcessContainer);
                var labels = popup.FindElements(By.TagName("label"));
                foreach (var label in labels)
                {
                    if (label.Text.Equals(processToSwitchTo, StringComparison.OrdinalIgnoreCase))
                    {
                        label.Click();
                        break;
                    }
                }

                //Click the OK button
                var okBtn = driver.FindElement(DialogsElementsLocators.SwitchProcessDialogOK);
                okBtn.Click();

                return true;
            });
        }

        internal bool SwitchToDialog(int frameIndex = 0) => DialogHelper.SwitchToDialog(Client, frameIndex);

        internal bool IsDialogVisible()
        {
            return Client.Execute<bool>(Client.GetOptions("Check If Dialog Is Visible"), driver =>
            {
                // Blocking dialogs (e.g. TALXIS Portal form dialogs, Fluent UI isBlocking) use the alertdialog role.
                return driver.IsVisible(By.CssSelector("div[role='dialog'], div[role='alertdialog']"));
            });
        }

        internal string GetCurrentDialogLable()
        {
            return Client.Execute<string>(Client.GetOptions("Get Current Dialog Lable"), driver =>
            {
                var dialog = driver.FindElement(By.XPath("//div[@role='dialog']"));

                if(dialog is null)
                {
                    return string.Empty;
                }

                return dialog.GetAttribute("aria-label");
            });
        }

        internal BrowserCommandResult<IWebElement> GetAlertDialog()
        {
            return Client.Execute<IWebElement>(Client.GetOptions("Get Alert Dialog"), driver =>
            {
                return driver.FindElement(By.XPath("//div[@data-id='alertdialog']"));
            });
        }

        internal void SelectButtonInDialogWindow(string buttonLabel)
        {
            Client.Execute<object>(Client.GetOptions("Select Button In Dialog Window"), driver =>
            {

                var dialog = driver.FindElement(DialogsElementsLocators.DialogContext);

                try
                {
                    var dialogButton = dialog.FindElement(By.XPath($".//button[@title='{buttonLabel}']"));
                    dialogButton.Click();
                }
                catch
                {
                    var dialogButton = dialog.FindElement(By.XPath($".//button[@aria-label='{buttonLabel}']"));
                    dialogButton.Click();
                }

                driver.WaitForPageToLoad();
                driver.WaitForTransaction();

                return null;
            });
        }

        internal string GetAlertDialogMessageText()
        {
            return Client.Execute<string>(Client.GetOptions("Get Alert Dialog Message Text"), driver =>
            {
                var dialog = driver.FindElement(By.XPath("//div[@data-id='alertdialog']"));

                if (dialog is null)
                {
                    return string.Empty;
                }

                return driver.FindElement(By.XPath("//div[@data-id='dialogMessageText']")).Text;
            });
        }

    }
}
