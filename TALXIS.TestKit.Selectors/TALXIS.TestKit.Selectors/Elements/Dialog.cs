// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
using System;
using OpenQA.Selenium;
using TALXIS.TestKit.Selectors.DTO;
using TALXIS.TestKit.Selectors.WebClientManagement;
using TALXIS.TestKit.Selectors.WebClientManagement.Helpers;
using TALXIS.TestKit.Selectors.Browser;

namespace TALXIS.TestKit.Selectors
{
    public class Dialogs : Element
    {
        public enum AssignTo
        {
            Me,
            User,
            Team
        }

        private readonly DialogsManager _dialogsManager;
        private readonly EntityManager _entityManager;
        private readonly CommandBarManager _commandBarManager;

        public Dialogs(WebClient client) : base()
        {
            _dialogsManager = new DialogsManager(client);
            _entityManager = new EntityManager(client);
            _commandBarManager = new CommandBarManager(client);
        }

        public IWebElement GetAlertDialog()
        {
            return _dialogsManager.GetAlertDialog().Value;
        }

        public string GetAlertDialogMessageText()
        {
            return _dialogsManager.GetAlertDialogMessageText();
        }

        public bool IsDialogVisible()
        {
            return _dialogsManager.IsDialogVisible();
        }

        /// <summary>
        /// Gets a value indicating whether the displayed dialog shows a button with a title or aria-label.
        /// </summary>
        /// <param name="label">The button's title or aria-label.</param>
        public bool IsButtonVisible(string label)
        {
            return _dialogsManager.IsButtonVisible(label);
        }

        /// <summary>
        /// Clicks a button with a title or aria-label on the displayed dialog, when it shows one.
        /// </summary>
        /// <param name="label">The button's title or aria-label.</param>
        /// <returns>Whether the button was clicked.</returns>
        public bool TryClickButton(string label)
        {
            return _dialogsManager.TryClickButton(label);
        }

        /// <summary>
        /// Closes the displayed dialog with its close button, without saving.
        /// </summary>
        public void CloseDialog()
        {
            _dialogsManager.CloseDialog();
        }

        /// <summary>
        /// Gets a value indicating whether the displayed dialog shows a text.
        /// </summary>
        /// <param name="text">The text.</param>
        public bool IsTextVisible(string text)
        {
            return _dialogsManager.IsTextVisible(text);
        }

        public string GetCurrentDialogLable()
        {
            return _dialogsManager.GetCurrentDialogLable();
        }

        /// <summary>
        /// Assigns a record to a user or team
        /// </summary>
        /// <param name="to">Enum used to assign record to user or team</param>
        /// <param name="userOrTeamName">Name of the user or team to assign to</param>
        public void Assign(AssignTo to, string userOrTeamName = null)
        {
            _dialogsManager.AssignDialog(to, userOrTeamName);
        }

        /// <summary>
        /// Clicks the Close button if true, or clicks Cancel if false
        /// </summary>
        /// <param name="closeOrCancel"></param>
        public void CloseActivity(bool closeOrCancel)
        {
            ActionHelper.CloseActivity(closeOrCancel, _dialogsManager.Client);
        }

        /// <summary>
        /// Clicks Close As Won or Close As Loss on Opportunity Close dialog
        /// </summary>
        /// <param name="closeAsWon"></param>
        public void CloseOpportunity(bool closeAsWon)
        {
            CloseHelper.CloseOpportunity(closeAsWon, _dialogsManager.Client);
        }

        /// <summary>
        /// Enters the values provided and closes an opportunity
        /// </summary>
        /// <param name="revenue">Value for Revenue field</param>
        /// <param name="closeDate">Value for Close Date field</param>
        /// <param name="description">Value for Description field</param>
        public void CloseOpportunity(double revenue, DateTime closeDate, string description)
        {
            CloseHelper.CloseOpportunity(revenue, closeDate, description, _dialogsManager.Client);
        }

        /// <summary>
        /// Closes the warning dialog during login
        /// </summary>
        /// <returns></returns>
        public bool CloseWarningDialog()
        {
            return _dialogsManager.CloseWarningDialog();
        }

        /// <summary>
        /// Clicks OK or Cancel on the confirmation dialog.  true = OK, false = Cancel
        /// </summary>
        /// <param name="clickConfirmButton"></param>
        /// <returns></returns>
        public bool ConfirmationDialog(bool clickConfirmButton)
        {
            return _dialogsManager.ConfirmationDialog(clickConfirmButton);
        }

        /// <summary>
        /// Clicks 'Ignore And Save' or 'Cancel' on the Duplicate Detection dialog.  true = Ignore And Save, false = Cancel
        /// </summary>
        /// <param name="clickConfirmButton"></param>
        /// <returns></returns>
        public bool DuplicateDetection(bool clickSaveOrCancel)
        {
            return _dialogsManager.DuplicateDetection(clickSaveOrCancel);
        }
        /// <summary>
        /// Clicks OK or Cancel on the confirmation dialog.  true = OK, false = Cancel
        /// </summary>
        /// <param name="clickConfirmButton"></param>
        /// <returns></returns>
        public bool ClickOk()
        {
            return _dialogsManager.ClickOk();
        }

        public void SelectButtonInDialogWindow(string buttonLabel)
        {
            _dialogsManager.SelectButtonInDialogWindow(buttonLabel);
        }


        /// <summary>
        /// Gets the value of a Lookup.
        /// </summary>
        /// <param name="control">The lookup field name of the lookup.</param>
        public string GetValue(LookupItem control)
        {
            return _entityManager.GetValue(control);
        }


        /// <summary>
        /// Gets the value of a Lookup.
        /// </summary>
        /// <param name="control">The lookup field name of the lookup.</param>
        public DateTime? GetValue(DateTimeControl control)
        {
            return _entityManager.GetValue(control);
        }

        /// <summary>
        /// Gets the value of an ActivityParty Lookup.
        /// </summary>
        /// <param name="controls">The activityparty lookup field name, value or index of the lookup.</param>
        /// <example>xrmApp.Entity.GetValue(new LookupItem[] { new LookupItem { Name = "to" } });</example>
        public string[] GetValue(LookupItem[] controls)
        {
            return _entityManager.GetValue(controls);
        }

        /// <summary>
        /// Gets the value of a text or date field.
        /// </summary>
        /// <param name="control">The schema name of the field</param>
        /// <example>xrmApp.Entity.GetValue("emailaddress1");</example>
        public string GetValue(string field)
        {
            return _entityManager.GetValue(field);
        }

        /// <summary>
        /// Gets the value of a picklist or status field.
        /// </summary>
        /// <param name="option">The option you want to set.</param>
        public string GetValue(OptionSet optionSet)
        {
            return _entityManager.GetValue(optionSet);
        }

        /// <summary>
        /// Gets the value of a Boolean Item.
        /// </summary>
        /// <param name="option">The boolean field name.</param>
        public bool GetValue(BooleanItem option)
        {
            return _entityManager.GetValue(option);
        }

        /// <summary>
        /// Gets the value of a MultiValueOptionSet.
        /// </summary>
        /// <param name="option">The option you want to set.</param>
        public MultiValueOptionSet GetValue(MultiValueOptionSet option)
        {
            return _entityManager.GetValue(option);
        }


        /// <summary>
        /// Clicks Confirm or Cancel on the Publish dialog.  true = Confirm, false = Cancel
        /// </summary>
        /// <param name="clickOkButton"></param>
        /// <returns></returns>
        public bool PublishDialog(bool clickConfirmButton)
        {
            return _dialogsManager.PublishDialog(clickConfirmButton);
        }

        /// <summary>
        /// Sets the value of a field
        /// </summary>
        /// <param name="field">The field</param>
        /// <param name="value">The value</param>
        public void SetValue(string field, string value)
        {
            SetValueHelper.SetTextFieldValue(_entityManager.Client, field, value, FormContextType.Dialog);
        }

        /// <summary>
        /// Sets the value of a Lookup.
        /// </summary>
        /// <param name="control">The lookup field name, value or index of the lookup.</param>
        public void SetValue(LookupItem control)
        {
            SetValueHelper.SetLookUp(_entityManager.Client, control, FormContextType.Dialog);
        }

        /// <summary>
        /// Sets the value of an ActivityParty Lookup.
        /// </summary>
        /// <param name="controls">The activityparty lookup field name, value or index of the lookup.</param>
        /// <example>xrmApp.Entity.SetValue(new LookupItem[] { new LookupItem { Name = "to", Value = "A. Datum Corporation (sample)" } });</example>
        public void SetValue(LookupItem[] controls)
        {
            SetValueHelper.SetValue(_entityManager.Client, controls, FormContextType.Dialog);
        }

        /// <summary>
        /// Sets the value of a picklist or status field.
        /// </summary>
        /// <param name="option">The option you want to set.</param>
        public void SetValue(OptionSet optionSet)
        {
            SetValueHelper.SetOptionSetValue(_entityManager.Client,optionSet, FormContextType.Dialog);
        }

        /// <summary>
        /// Sets the value of a Boolean Item.
        /// </summary>
        /// <param name="option">The boolean field name.</param>
        public void SetValue(BooleanItem option)
        {
            SetValueHelper.SetBooleanValue(_entityManager.Client, option, FormContextType.Dialog);
        }

        /// <summary>
        /// Sets the value of a Date Field.
        /// </summary>
        /// <param name="field">Date field name.</param>
        /// <param name="date">DateTime value.</param>
        /// <param name="formatDate">Datetime format matching Short Date formatting personal options.</param>
        /// <param name="formatTime">Datetime format matching Short Time formatting personal options.</param>
        /// <example>xrmApp.Dialog.SetValue("birthdate", DateTime.Parse("11/1/1980"));</example>
        /// <example>xrmApp.Dialog.SetValue("new_actualclosedatetime", DateTime.Now, "MM/dd/yyyy", "hh:mm tt");</example>
        /// <example>xrmApp.Dialog.SetValue("estimatedclosedate", DateTime.Now);</example>
        public void SetValue(string field, DateTime date, string formatDate = null, string formatTime = null)
        {
            SetValueHelper.SetDateTimeValue(_entityManager.Client, field, date, FormContextType.Dialog, formatDate, formatTime);
        }

        /// <summary>
        /// Sets the value of a Date Field.
        /// </summary>
        /// <param name="control">Date field control.</param>
        public void SetValue(DateTimeControl control)
        {
            SetValueHelper.SetValue(_entityManager.Client, control, FormContextType.Dialog);
        }

        /// <summary>
        /// Sets/Removes the value from the multselect type control
        /// </summary>
        /// <param name="option">Object of type MultiValueOptionSet containing name of the Field and the values to be set/removed</param>
        /// <param name="removeExistingValues">False - Values will be set. True - Values will be removed</param>
        public void SetValue(MultiValueOptionSet option, bool removeExistingValues = false)
        {
            SetValueHelper.SetMultiSelectOptionSetValue(_entityManager.Client, option, FormContextType.Dialog, removeExistingValues);
        }

        /// <summary>
        /// Clicks OK or Cancel on the Set State (Activate / Deactivate) dialog.  true = OK, false = Cancel
        /// </summary>
        /// <param name="clickOkButton"></param>
        /// <returns></returns>
        public bool SetStateDialog(bool clickOkButton)
        {
            return _dialogsManager.SetStateDialog(clickOkButton);
        }

        /// <summary>
        /// Clicks on entity dialog ribbon button
        /// </summary>
        /// <param name="secondSubButtonName"></param>
        /// <param name="buttonName">Name of button to click</param>
        /// <param name="subButtonName">Name of button on submenu to click</param>
        /// <param name="secondSubButtonName">Name of button on submenu (3rd level) to click</param>
        public bool ClickCommand(string buttonName, string subButtonName = null, string secondSubButtonName = null)
        {
            return _commandBarManager.ClickCommand(buttonName, subButtonName, secondSubButtonName);
        }

        /// <summary>
        /// Clicks on entity dialog ribbon button
        /// </summary>
        /// <param name="tabName">The name of the tab based on the References class</param>
        /// <param name="subtabName">The name of the subtab based on the References class</param>
        public bool SelectTab(string tabName, string subtabName = null)
        {
            return ActionHelper.SelectTab(tabName, _dialogsManager.Client, subtabName);
        }
    }
}
