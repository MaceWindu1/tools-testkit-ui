using System;
using System.Linq;
using OpenQA.Selenium;
using TALXIS.TestKit.Selectors.Browser;
using TALXIS.TestKit.Selectors.DTO.Locators;

namespace TALXIS.TestKit.Selectors.WebClientManagement
{
    /// <summary>
    /// The dialogs of a TALXIS Portal: Fluent dialogs and panels (form dialogs, wizards), and the alert and confirmation
    /// dialogs (modals without a form) opened with Xrm.Navigation.openAlertDialog and openConfirmDialog.
    /// </summary>
    public class PortalDialogsManager
    {
        // Dialogs open, close and re-render (e.g. a wizard's next step) asynchronously.
        private static readonly TimeSpan StateTimeout = TimeSpan.FromSeconds(10);
        private static readonly TimeSpan LoadTimeout = TimeSpan.FromSeconds(30);

        public PortalDialogsManager(WebClient client)
        {
            Client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public WebClient Client { get; }

        internal BrowserCommandResult<bool> IsDialogShown(bool shown)
        {
            return Client.Execute(Client.GetNoRetryOptions("Is Portal Dialog Shown"), driver =>
                PortalPageManager.WaitFor(driver, () => (PortalPageManager.GetTopmostModal(driver) != null) == shown, StateTimeout));
        }

        internal BrowserCommandResult<string> GetTitle(string expected)
        {
            return Client.Execute(Client.GetNoRetryOptions("Get Portal Dialog Title"), driver =>
            {
                PortalPageManager.WaitFor(driver, () => GetTitle(driver) == expected, StateTimeout);
                return GetTitle(driver);
            });
        }

        internal BrowserCommandResult<bool> ClickButton(string label)
        {
            return Client.Execute(Client.GetNoRetryOptions($"Click Portal Dialog Button: {label}"), driver =>
            {
                PortalPageManager.WaitUntil(
                    driver,
                    d => d.FindElements(PortalDialogElementsLocators.Modal).LastOrDefault()?.FindElement(PortalDialogElementsLocators.FooterButton(label)) is { Enabled: true } button ? button : null,
                    LoadTimeout,
                    $"The '{label}' button was not found on the displayed dialog.").Click();
                return true;
            });
        }

        internal BrowserCommandResult<bool> TryClickButton(string label)
        {
            return Client.Execute(Client.GetNoRetryOptions($"Try Click Portal Dialog Button: {label}"), driver =>
            {
                // Right after the dialog opens (still loading) or after a wizard moved to its next step.
                if (!PortalPageManager.WaitFor(driver, () => FindButton(driver, label) != null, StateTimeout))
                    return false;

                // The step and the button are looked up again when they are met while the wizard replaces them.
                IWebElement step = null;
                IWebElement button = null;
                string stepText = null;
                var evaluated = PortalPageManager.WaitFor(driver, () =>
                {
                    step = PortalPageManager.GetTopmostModal(driver)?.FindElements(PortalDialogElementsLocators.StepContent).FirstOrDefault();
                    button = FindButton(driver, label);
                    if (button == null)
                        return true;

                    stepText = step?.Text;
                    button.Click();
                    return true;
                }, StateTimeout);
                if (!evaluated || button == null)
                    return false;

                // A wizard re-renders its footer for the next step; wait for that before looking for the button again.
                PortalPageManager.WaitFor(driver, () => PortalPageManager.IsStale(button) || PortalPageManager.IsStale(step) || step.Text != stepText, StateTimeout);
                return true;
            });
        }

        internal BrowserCommandResult<bool> IsButtonShown(string label, bool shown)
        {
            return Client.Execute(Client.GetNoRetryOptions($"Is Portal Dialog Button Shown: {label}"), driver => shown
                ? PortalPageManager.WaitFor(driver, () => FindButton(driver, label) != null, StateTimeout)
                : PortalPageManager.StaysAbsent(() => FindButton(driver, label) != null));
        }

        internal BrowserCommandResult<bool> IsTextShown(string text)
        {
            return Client.Execute(Client.GetNoRetryOptions("Is Text Shown On Portal Dialog"), driver =>
            {
                // A wizard re-renders its step after a click; a text met while it is replaced is looked up again.
                var shown = false;
                PortalPageManager.WaitFor(driver, () =>
                {
                    var modal = PortalPageManager.GetTopmostModal(driver);
                    shown = modal != null && modal.FindElements(PortalDialogElementsLocators.Text(text)).Any(e => e.Displayed);
                    return true;
                }, StateTimeout);
                return shown;
            });
        }

        internal BrowserCommandResult<bool> CloseWithoutSaving()
        {
            return Client.Execute(Client.GetNoRetryOptions("Close Portal Dialog"), driver =>
            {
                var modal = CloseDialog(driver);
                return PortalPageManager.WaitFor(driver, () => PortalPageManager.IsStale(modal), StateTimeout);
            });
        }

        internal BrowserCommandResult<bool> ChooseOption(string option)
        {
            return Client.Execute(Client.GetNoRetryOptions($"Choose Portal Dialog Option: {option}"), driver =>
            {
                PortalPageManager.WaitUntil(
                    driver,
                    d => d.FindElements(PortalDialogElementsLocators.Modal).LastOrDefault()?.FindElement(PortalDialogElementsLocators.ChoiceOption(option)),
                    LoadTimeout,
                    $"The '{option}' option was not found on the displayed dialog.").Click();
                return true;
            });
        }

        internal BrowserCommandResult<bool> AnswerMessageDialog(bool confirm)
        {
            return Client.Execute(Client.GetNoRetryOptions("Answer Portal Message Dialog"), driver =>
            {
                AnswerMessageDialog(driver, confirm);
                return true;
            });
        }

        internal BrowserCommandResult<string> GetMessageTitle(string expected)
        {
            return Client.Execute(Client.GetNoRetryOptions("Get Portal Message Dialog Title"), driver =>
            {
                PortalPageManager.WaitFor(driver, () => expected == null ? GetMessageTitle(driver) != null : GetMessageTitle(driver) == expected, StateTimeout);
                return GetMessageTitle(driver);
            });
        }

        /// <summary>
        /// Closes the topmost dialog with its close ('x') button, waiting for a dialog that the previous action opened and
        /// that is still loading.
        /// </summary>
        /// <returns>The closed dialog.</returns>
        internal static IWebElement CloseDialog(IWebDriver driver)
        {
            var modal = PortalPageManager.WaitUntil(
                driver,
                d => PortalPageManager.GetTopmostModal(d) is { } topmost && topmost.FindElements(PortalDialogElementsLocators.CloseButton).Count > 0 ? topmost : null,
                LoadTimeout,
                "No dialog with a close button is displayed.");
            modal.FindElements(PortalDialogElementsLocators.CloseButton).First().Click();
            return modal;
        }

        /// <summary>
        /// Clicks the confirming (primary) or the cancelling (secondary) button of the topmost alert or confirmation dialog.
        /// </summary>
        internal static void AnswerMessageDialog(IWebDriver driver, bool confirm)
        {
            var dialog = PortalPageManager.WaitUntil(driver, d => d.FindElements(PortalDialogElementsLocators.MessageDialog).LastOrDefault(), LoadTimeout, "No alert or confirmation dialog is displayed.");
            PortalPageManager.WaitUntil(
                driver,
                _ => dialog.FindElement(confirm ? PortalDialogElementsLocators.ConfirmButton : PortalDialogElementsLocators.CancelButton),
                LoadTimeout,
                $"The displayed dialog has no {(confirm ? "confirming" : "cancelling")} button.").Click();
        }

        /// <summary>
        /// Finds a button of the topmost dialog by its label: a footer button, or any button labelled by its title or
        /// aria-label (e.g. an icon button in a grid on the dialog).
        /// </summary>
        /// <returns>The enabled button, or null.</returns>
        internal static IWebElement FindButton(IWebDriver driver, string label)
        {
            return PortalPageManager.GetTopmostModal(driver)?.FindElements(PortalDialogElementsLocators.Button(label))
                .FirstOrDefault(button => button.Displayed && button.Enabled && !PortalPageManager.IsDisabled(button));
        }

        /// <summary>
        /// Gets the title of the topmost alert or confirmation dialog.
        /// </summary>
        /// <returns>The title, or null when no alert dialog is displayed.</returns>
        internal static string GetMessageTitle(IWebDriver driver)
        {
            var alert = driver.FindElements(PortalDialogElementsLocators.MessageDialog).LastOrDefault();
            return alert?.FindElements(PortalDialogElementsLocators.MessageTitle).FirstOrDefault()?.Text.Trim();
        }

        private static string GetTitle(IWebDriver driver)
        {
            var title = PortalPageManager.GetTopmostModal(driver)?.FindElements(PortalDialogElementsLocators.Title).FirstOrDefault();
            return title == null ? null : PortalPageManager.NormalizeText(title.Text);
        }
    }
}
