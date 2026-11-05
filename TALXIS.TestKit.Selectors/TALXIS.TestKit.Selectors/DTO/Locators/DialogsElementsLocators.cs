using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using TALXIS.TestKit.Selectors.Browser;

namespace TALXIS.TestKit.Selectors.DTO.Locators
{
    internal class DialogsElementsLocators
    {
        internal static class CloseOpportunity
        {
            internal static By Ok => By.XPath(AppElements.Xpath[AppReference.Dialogs.CloseOpportunity.Ok]);
            internal static By Cancel => By.XPath(AppElements.Xpath[AppReference.Dialogs.CloseOpportunity.Cancel]);
            internal static By ActualRevenueId => By.XPath(AppElements.Xpath[AppReference.Dialogs.CloseOpportunity.ActualRevenueId]);
            internal static By CloseDateId => By.XPath(AppElements.Xpath[AppReference.Dialogs.CloseOpportunity.CloseDateId]);
            internal static By DescriptionId => By.XPath(AppElements.Xpath[AppReference.Dialogs.CloseOpportunity.DescriptionId]);
        }
        internal static class CloseActivity
        {
            internal static By Close => By.XPath(AppElements.Xpath[AppReference.Dialogs.CloseActivity.Close]);
            internal static By Cancel => By.XPath(AppElements.Xpath[AppReference.Dialogs.CloseActivity.Cancel]);
        }
        internal static By AssignDialogUserTeamLookupResults => By.XPath(AppElements.Xpath[AppReference.Dialogs.AssignDialogUserTeamLookupResults]);
        internal static By AssignDialogOKButton => By.XPath(AppElements.Xpath[AppReference.Dialogs.AssignDialogOKButton]);
        internal static By AssignDialogToggle => By.XPath(AppElements.Xpath[AppReference.Dialogs.AssignDialogToggle]);
        internal static By ConfirmButton => By.XPath(AppElements.Xpath[AppReference.Dialogs.ConfirmButton]);
        internal static By Displayed => By.XPath(AppElements.Xpath[AppReference.Dialogs.Displayed]);
        internal static By ButtonByLabel(string label) => By.XPath(AppElements.Xpath[AppReference.Dialogs.ButtonByLabel].Replace("[LABEL]", label.ToXPathLiteral()));
        internal static By CloseButton => By.XPath(AppElements.Xpath[AppReference.Dialogs.CloseButton]);
        internal static By TextOnPage(string text) => By.XPath(AppElements.Xpath[AppReference.Dialogs.TextOnPage].Replace("[LABEL]", text.ToXPathLiteral()));
        internal static By CancelButton => By.XPath(AppElements.Xpath[AppReference.Dialogs.CancelButton]);
        internal static By OkButton => By.XPath(AppElements.Xpath[AppReference.Dialogs.OkButton]);
        internal static By DuplicateDetectionIgnoreSaveButton => By.XPath(AppElements.Xpath[AppReference.Dialogs.DuplicateDetectionIgnoreSaveButton]);
        internal static By DuplicateDetectionCancelButton => By.XPath(AppElements.Xpath[AppReference.Dialogs.DuplicateDetectionCancelButton]);
        internal static By PublishConfirmButton => By.XPath(AppElements.Xpath[AppReference.Dialogs.PublishConfirmButton]);
        internal static By PublishCancelButton => By.XPath(AppElements.Xpath[AppReference.Dialogs.PublishCancelButton]);
        internal static By SetStateDialog => By.XPath(AppElements.Xpath[AppReference.Dialogs.SetStateDialog]);
        internal static By SetStateActionButton => By.XPath(AppElements.Xpath[AppReference.Dialogs.SetStateActionButton]);
        internal static By SetStateCancelButton => By.XPath(AppElements.Xpath[AppReference.Dialogs.SetStateCancelButton]);
        internal static By SwitchProcessDialog => By.XPath(AppElements.Xpath[AppReference.Dialogs.SwitchProcessDialog]);
        internal static By SwitchProcessDialogOK => By.XPath(AppElements.Xpath[AppReference.Dialogs.SwitchProcessDialogOK]);
        internal static By ActiveProcessGridControlContainer => By.XPath(AppElements.Xpath[AppReference.Dialogs.ActiveProcessGridControlContainer]);
        internal static By DialogContext => By.XPath(AppElements.Xpath[AppReference.Dialogs.DialogContext]);
        internal static By SwitchProcessContainer => By.XPath(AppElements.Xpath[AppReference.Dialogs.SwitchProcessContainer]);
    }
}
