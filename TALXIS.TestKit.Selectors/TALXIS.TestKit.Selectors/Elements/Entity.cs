// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Options;
using OpenQA.Selenium;
using TALXIS.TestKit.Selectors.DTO;
using TALXIS.TestKit.Selectors.WebClientManagement;
using TALXIS.TestKit.Selectors.WebClientManagement.Helpers;
using TALXIS.TestKit.Selectors.Browser;

namespace TALXIS.TestKit.Selectors
{
    public class Entity : Element
    {
        private readonly EntityManager _entityManager;
        private readonly TimelineManager _timelineManager;

        public SubGrid SubGrid => this.GetElement<SubGrid>(_entityManager.Client);
        public RelatedGrid RelatedGrid => this.GetElement<RelatedGrid>(_entityManager.Client);

        public Entity(WebClient client) : base()
        {
            _entityManager = new EntityManager(client);
            _timelineManager = new TimelineManager(client);
        }

        public T GetElement<T>(WebClient client)
        where T : Element
        {
            return (T)Activator.CreateInstance(typeof(T), new object[] { client });
        }

        /// <summary>
        /// Clicks the Assign button and completes the Assign dialog
        /// </summary>
        /// <param name="userOrTeam">Name of the user or team to assign the record to</param>
        public void Assign(string userOrTeam)
        {
            _timelineManager.Assign(userOrTeam);
        }

        /// <summary>
        /// Gets the logical name of a field from the label the form shows.
        /// </summary>
        /// <param name="label">The field's label.</param>
        public string GetFieldLogicalNameFromLabel(string label) => _entityManager.GetFieldLogicalNameFromLabel(label);

        /// <summary>
        /// Gets the logical name of a field from its logical name or its label, through the form's controls, so that also a
        /// hidden field is found by its label. A name that matches neither is returned unchanged.
        /// </summary>
        /// <param name="field">The field's logical name or label.</param>
        public string ResolveFieldLogicalName(string field) => _entityManager.ResolveFieldLogicalName(field);

        /// <summary>
        /// Gets a value indicating whether a field is shown on the form, without waiting for it.
        /// </summary>
        /// <param name="fieldLogicalName">The field's logical name.</param>
        public bool IsFieldVisible(string fieldLogicalName) => _entityManager.IsFieldVisible(fieldLogicalName);

        /// <summary>
        /// Gets a value indicating whether a shown field is read-only.
        /// </summary>
        /// <param name="fieldLogicalName">The field's logical name.</param>
        public bool IsFieldReadOnly(string fieldLogicalName) => _entityManager.IsFieldReadOnly(fieldLogicalName);

        /// <summary>
        /// Gets a value indicating whether a field has no value, whether it is shown or hidden.
        /// </summary>
        /// <param name="fieldLogicalName">The field's logical name.</param>
        public bool IsFieldEmpty(string fieldLogicalName) => _entityManager.IsFieldEmpty(fieldLogicalName);

        /// <summary>
        /// Gets the type of a field: boolean, datetime, lookup, optionset, multioptionset or text.
        /// </summary>
        /// <param name="fieldLogicalName">The field's logical name.</param>
        public string GetFieldType(string fieldLogicalName) => _entityManager.GetFieldType(fieldLogicalName);

        /// <summary>
        /// Gets where a field is on the form: 'field' in the body or 'header' in the header.
        /// </summary>
        /// <param name="fieldLogicalName">The field's logical name.</param>
        public string GetFieldLocation(string fieldLogicalName) => _entityManager.GetFieldLocation(fieldLogicalName);

        public TAttributeType GetFieldSectionItemContainertAttribute<TAttributeType>(string fieldName, string attributeName)
        {
            var fieldContainer = _entityManager.GetFieldSectionItemContainer(fieldName);

            if (fieldContainer == null)
            {
                throw new InvalidOperationException($"The {fieldName} field is not visible on the form.");
            }

            return fieldContainer.GetAttribute<TAttributeType>(attributeName);
        }

        public string GetAttributeOfActiveTab(string attributeName)
        {
            return _entityManager.GetCurrentTab().GetAttribute(attributeName);
        }

        public bool IsSectionVisible(string logicalName)
        {
            ICollection<IWebElement> webElements = _entityManager.FindSectionByLogicalName(logicalName);

            return webElements.Count > 0;
        }

        public string GetFormStatus()
        {
            return GetDataHelper.GetFormStatus(_entityManager.Client);
        }

        public void SaveTheRecord()
        {
            ActionHelper.SaveTheRecord(_entityManager.Client);
        }

        public IEnumerable<IWebElement> GetAllOptionSetValue(string fieldName)
        {
            if (!GetDataHelper.TryGetOptionSet(fieldName, _entityManager.Client, out IWebElement optionSet))
            {
                throw new InvalidOperationException($"Unable to find option set field {fieldName}.");
            }

            return optionSet.FindElements(By.CssSelector("option")).Where(e => e.GetAttribute("value") != "-1");
        }

        /// <summary>
        /// Clears a value from the text or date field provided
        /// </summary>
        /// <param name="field"></param>
        public void ClearValue(string field)
        {
            _entityManager.ClearValue(field, FormContextType.Entity);
        }

        /// <summary>
        /// Clears a value from the LookupItem provided
        /// Can be used on a lookup, customer, owner, or activityparty field
        /// </summary>
        /// <param name="control"></param>
        /// <example>xrmApp.Entity.ClearValue(new LookupItem { Name = "parentcustomerid" });</example>
        /// <example>xrmApp.Entity.ClearValue(new LookupItem { Name = "to" });</example>
        public void ClearValue(LookupItem control)
        {
            _entityManager.ClearValue(control, FormContextType.Entity);
        }

        /// <summary>
        /// Clears a value from the OptionSet provided
        /// </summary>
        /// <param name="control"></param>
        public void ClearValue(OptionSet control)
        {
            _entityManager.ClearValue(control, FormContextType.Entity);
        }

        /// <summary>
        /// Clears a value from the MultiValueOptionSet provided
        /// </summary>
        /// <param name="control"></param>
        public void ClearValue(MultiValueOptionSet control)
        {
            _entityManager.ClearValue(control, FormContextType.Entity);
        }


        /// <summary>
        /// Clears a value from the DateTimeControl provided
        /// </summary>
        /// <param name="control"></param>
        public void ClearHeaderValue(DateTimeControl control)
        {
            _entityManager.ClearHeaderValue(control);
        }

        /// <summary>
        /// Clears a value from the DateTimeControl provided
        /// </summary>
        /// <param name="control"></param>
        public void ClearValue(DateTimeControl control)
        {
            _entityManager.ClearValue(control, FormContextType.Entity);
        }

        /// <summary>
        /// Close Record Set Navigator
        /// </summary>
        /// <param name="thinkTime"></param>
        /// <example>xrmApp.Entity.CloseRecordSetNavigator();</example>
        public void CloseRecordSetNavigator()
        {
            _entityManager.CloseRecordSetNavigator();
        }

        /// <summary>
        /// Clicks the Delete button on the command bar for an entity
        /// </summary>
        public void Delete()
        {
            _timelineManager.Delete();
        }

        public Field GetField(string field)
        {
            return _entityManager.GetField(field);
        }

        /// <summary>
        /// Gets test from a Business Process Error, if present
        /// <paramref name="waitTimeInSeconds"/>Number of seconds to wait for the error dialog. Default value is 120 seconds</param>
        /// </summary>
        /// <example>var errorText = xrmApp.Entity.GetBusinessProcessError(int waitTimeInSeconds);</example>
        public string GetBusinessProcessError(int waitTimeInSeconds = 120)
        {
            _entityManager.Client.Browser.Driver.WaitForTransaction();

            return GetDataHelper.GetBusinessProcessErrorText(waitTimeInSeconds, _entityManager.Client);
        }

        /// <summary>
        /// Gets the state from the form.
        /// </summary>
        /// <returns>The state of the record.</returns>
        public string GetFormState()
        {
            return _entityManager.GetStateFromForm();
        }

        [Obsolete("Forms no longer have footers. Use Entity.GetFormState() instead.")]
        public string GetFooterStatusValue()
        {
            return this.GetFormState();
        }

        public IReadOnlyList<FormNotification> GetFormNotifications()
        {
            return GetDataHelper.GetFormNotifications(_entityManager.Client).Value;
        }

        /// <summary>
        /// Gets the value of a LookupItem from the header
        /// </summary>
        /// <param name="control">The lookup field name of the lookup.</param>
        /// <example>xrmApp.Entity.GetValue(new Lookup { Name = "primarycontactid" });</example>
        public string GetHeaderValue(LookupItem control)
        {
            return _entityManager.GetHeaderValue(control);
        }

        /// <summary>
        /// Gets the value of an ActivityParty Lookup from the header
        /// </summary>
        /// <param name="controls">The activityparty lookup field name, value or index of the lookup.</param>
        /// <example>xrmApp.Entity.GetHeaderValue(new LookupItem[] { new LookupItem { Name = "to" } });</example>
        public string[] GetHeaderValue(LookupItem[] controls)
        {
            return _entityManager.GetValue(controls);
        }

        /// <summary>
        /// Gets the value of a picklist or status field from the header
        /// </summary>
        /// <param name="option">The option you want to Get.</param>
        /// <example>xrmBrowser.Entity.GetValue(new OptionSet { Name = "preferredcontactmethodcode"}); </example>
        public string GetHeaderValue(OptionSet control)
        {
            return _entityManager.GetHeaderValue(control);
        }

        /// <summary>
        /// Gets the value of a Boolean Item from the header
        /// </summary>
        /// <param name="control">The boolean field you want to Get.</param>
        /// <example>xrmApp.Entity.GetHeaderValue(new BooleanItem { Name = "preferredcontactmethodcode"}); </example>
        public bool GetHeaderValue(BooleanItem control)
        {
            return _entityManager.GetHeaderValue(control);
        }

        /// <summary>
        /// Gets the value of a text or date field from the header
        /// </summary>
        /// <param name="control">The schema name of the field</param>
        /// <example>xrmApp.Entity.GetHeaderValue("emailaddress1");</example>
        public string GetHeaderValue(string control)
        {
            return _entityManager.GetHeaderValue(control);
        }

        /// <summary>
        /// Gets the value of a  MultiValueOptionSet from the header
        /// </summary>
        /// <param name="option">The option you want to Get.</param>
        public MultiValueOptionSet GetHeaderValue(MultiValueOptionSet control)
        {
            return _entityManager.GetHeaderValue(control);
        }

        /// <summary>
        /// Gets the value of a DateTime Control from the header
        /// </summary>
        /// <param name="control">The date time field name of the lookup.</param>
        /// <example>xrmApp.Entity.GetValue(new DateTimeControl { Name = "estimatedclosedate" });</example>
        public DateTime? GetHeaderValue(DateTimeControl control)
        {
            return _entityManager.GetHeaderValue(control);
        }

        /// <summary>
        /// Get the object id of the current entity
        /// </summary>
        public Guid GetObjectId()
        {
            return _entityManager.GetObjectId();
        }

        /// <summary>
        /// Get the Entity Name of the current entity
        /// </summary>
        public string GetEntityName()
        {
            return _entityManager.GetEntityName();
        }

        /// <summary>
        /// Get the Form Name of the current entity
        /// </summary>
        public string GetFormName()
        {
            return _entityManager.GetFormName();
        }

        /// <summary>
        /// Get the Header Title of the current entity
        /// </summary>
        public string GetHeaderTitle()
        {
            return _entityManager.GetHeaderTitle();
        }

        /// <summary>
        /// Retrieve the items from a subgrid
        /// </summary>
        /// <param name="subgridName">Label of the subgrid to retrieve items from</param>
        [Obsolete("GetSubGridItems(string subgridName)is deprecated, please use the equivalent Entity.SubGrid.<Method> instead.")]
        public List<GridItem> GetSubGridItems(string subgridName)
        {
            return GetDataHelper.GetSubGridItems(subgridName, _entityManager.Client);
        }

        /// <summary>
        /// Retrieves the number of rows from a subgrid
        /// </summary>
        /// <param name="subgridName">Label of the subgrid to retrieve items from</param>
        /// <returns></returns>
        [Obsolete("GetSubGridItemsCount(string subgridName) is deprecated, please use the equivalent Entity.SubGrid.<Method> instead.")]
        public int GetSubGridItemsCount(string subgridName)
        {
            return _entityManager.GetSubGridItemsCount(subgridName);
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
        /// Open Entity
        /// </summary>
        /// <param name="entityName">The entity name</param>
        /// <param name="id">The Id</param>
        public void OpenEntity(string entityname, Guid id)
        {
            _entityManager.OpenEntity(entityname, id);
        }

        /// <summary>
        /// Open record set and navigate record index.
        /// This method supersedes Navigate Up and Navigate Down outside of UCI 
        /// </summary>
        /// <param name="index">The index.</param>
        public void OpenRecordSetNavigator(int index = 0)
        {
            _entityManager.OpenRecordSetNavigator(index);
        }


        /// <summary>
        /// Saves the entity
        /// </summary>
        public void Save()
        {
            _entityManager.Save();
            ActionHelper.HandleSaveDialog(_entityManager.Client);
            _entityManager.Client.Browser.Driver.WaitForTransaction();
        }

        /// <summary>
        /// Selects a Lookup Field
        /// </summary>
        /// <param name="control">LookupItem with the schema name of the field</param>
        public void SelectLookup(LookupItem control)
        {
            _entityManager.SelectLookup(control);
        }

        /// <summary>
        /// Opens any tab on the web page.
        /// </summary>
        /// <param name="tabName">The name of the tab based on the References class</param>
        /// <param name="subtabName">The name of the subtab based on the References class</param>
        public void SelectTab(string tabName, string subTabName = "")
        {
            _timelineManager.SelectTab(tabName, subTabName);
        }

        


        public void SetHeaderValue(string field, string value)
        {
            _entityManager.SetHeaderValue(field, value);
        }

        public bool IsTabVisible(string tabName)
        {
            return _entityManager.IsTabVisible(tabName);
        }

        /// <summary>
        /// Sets the value of a Lookup in the header
        /// </summary>
        /// <param name="control">The lookup field name, value or index of the lookup.</param>
        public void SetHeaderValue(LookupItem control)
        {
            _entityManager.SetHeaderValue(control);
        }

        /// <summary>
        /// Sets the value of am ActivityParty Lookup in the header
        /// </summary>
        /// <param name="controls">The activityparty lookup field name, value or index of the lookup.</param>
        /// <example>xrmApp.Entity.SetHeaderValue(new LookupItem[] { new LookupItem { Name = "to", Value = "A. Datum Corporation (sample)" } });</example>
        public void SetHeaderValue(LookupItem[] controls)
        {
            _entityManager.SetHeaderValue(controls);
        }

        /// <summary>
        /// Sets/Removes the value from the multselect type control in the header
        /// </summary>
        /// <param name="option">Object of type MultiValueOptionSet containing name of the Field and the values to be set/removed</param>
        public void SetHeaderValue(MultiValueOptionSet control)
        {
            _entityManager.SetHeaderValue(control);
        }

        /// <summary>
        /// Sets the value of a picklist or status field in the header
        /// </summary>
        /// <param name="option">The option you want to set.</param>
        public void SetHeaderValue(OptionSet control)
        {
            _entityManager.SetHeaderValue(control);
        }

        /// <summary>
        /// Sets the value of a BooleanItem in the header
        /// </summary>
        /// <param name="control">The boolean field you want to set.</param>
        public void SetHeaderValue(BooleanItem control)
        {
            _entityManager.SetHeaderValue(control);
        }

        /// <summary>
        /// Sets the value of a Date field in the header
        /// </summary>
        /// <param name="field">Date field name.</param>
        /// <param name="date">DateTime value.</param>
        /// <param name="formatDate">Datetime format matching Short Date formatting personal options.</param>
        /// <param name="formatTime">Datetime format matching Short Time formatting personal options.</param>
        /// <example>xrmApp.Entity.SetHeaderValue("birthdate", DateTime.Parse("11/1/1980"));</example>
        /// <example>xrmApp.Entity.SetHeaderValue("new_actualclosedatetime", DateTime.Now, "MM/dd/yyyy", "hh:mm tt");</example>
        /// <example>xrmApp.Entity.SetHeaderValue("estimatedclosedate", DateTime.Now);</example>
        public void SetHeaderValue(string field, DateTime date, string formatDate = null, string formatTime = null)
        {
            _entityManager.SetHeaderValue(field, date, formatDate, formatTime);
        }

        /// <summary>
        /// Sets the value of a BooleanItem in the header
        /// </summary>
        /// <param name="control">The boolean field you want to set.</param>
        public void SetHeaderValue(DateTimeControl control)
        {
            _entityManager.SetHeaderValue(control);
        }

        /// <summary>
        /// Sets the value of a field
        /// </summary>
        /// <param name="field">The field</param>
        /// <param name="value">The value</param>
        public void SetValue(string field, string value)
        {
            SetValueHelper.SetTextFieldValue(_entityManager.Client, field, value, FormContextType.Entity);
        }

        /// <summary>
        /// Sets the value of a Lookup.
        /// </summary>
        /// <param name="control">The lookup field name, value or index of the lookup.</param>
        public void SetValue(LookupItem control)
        {
            SetValueHelper.SetLookUp(_entityManager.Client, control, FormContextType.Entity);
        }

        /// <summary>
        /// Sets the value of an ActivityParty Lookup.
        /// </summary>
        /// <param name="controls">The activityparty lookup field name, value or index of the lookup.</param>
        /// <example>xrmApp.Entity.SetValue(new LookupItem[] { new LookupItem { Name = "to", Value = "A. Datum Corporation (sample)" } });</example>
        public void SetValue(LookupItem[] controls)
        {
            SetValueHelper.SetValue(_entityManager.Client, controls, FormContextType.Entity);
        }

        /// <summary>
        /// Sets the value of a picklist or status field.
        /// </summary>
        /// <param name="option">The option you want to set.</param>
        public void SetValue(OptionSet optionSet)
        {
            SetValueHelper.SetOptionSetValue(_entityManager.Client, optionSet, FormContextType.Entity);
        }

        /// <summary>
        /// Sets the value of a Boolean Item.
        /// </summary>
        /// <param name="option">The boolean field name.</param>
        public void SetValue(BooleanItem option)
        {
            SetValueHelper.SetBooleanValue(_entityManager.Client, option, FormContextType.Entity);
        }

        /// <summary>
        /// Sets the value of a Date Field.
        /// </summary>
        /// <param name="field">Date field name.</param>
        /// <param name="date">DateTime value.</param>
        /// <param name="formatDate">Datetime format matching Short Date formatting personal options.</param>
        /// <param name="formatTime">Datetime format matching Short Time formatting personal options.</param>
        /// <example>xrmApp.Entity.SetValue("birthdate", DateTime.Parse("11/1/1980"));</example>
        /// <example>xrmApp.Entity.SetValue("new_actualclosedatetime", DateTime.Now, "MM/dd/yyyy", "hh:mm tt");</example>
        /// <example>xrmApp.Entity.SetValue("estimatedclosedate", DateTime.Now);</example>
        public void SetValue(string field, DateTime date, string formatDate = null, string formatTime = null)
        {
            SetValueHelper.SetDateTimeValue(_entityManager.Client, field, date, FormContextType.Entity, formatDate, formatTime);
        }

        /// <summary>
        /// Sets the value of a Date Field.
        /// </summary>
        /// <param name="control">Date field control.</param>
        public void SetValue(DateTimeControl control)
        {
            SetValueHelper.SetValue(_entityManager.Client, control, FormContextType.Entity);
        }

        /// <summary>
        /// Sets/Removes the value from the multselect type control
        /// </summary>
        /// <param name="option">Object of type MultiValueOptionSet containing name of the Field and the values to be set/removed</param>
        /// <param name="removeExistingValues">False - Values will be set. True - Values will be removed</param>
        public void SetValue(MultiValueOptionSet option, bool removeExistingValues = false)
        {
            SetValueHelper.SetMultiSelectOptionSetValue(_entityManager.Client, option, FormContextType.Entity, removeExistingValues);
        }

        /// <summary>
        /// Click Process>Switch Process
        /// </summary>
        /// <param name="processToSwitchTo">Name of the process to switch to</param>
        public void SwitchProcess(string processToSwitchTo)
        {
            _timelineManager.SwitchProcess(processToSwitchTo);
        }

        /// <summary>
        /// Switches forms using the form selector
        /// </summary>
        /// <param name="formName">Name of the form</param>
        /// <example>xrmApp.Entity.SelectForm("AI for Sales");</example>
        public void SelectForm(string formName)
        {
            _entityManager.SelectForm(formName);
        }

        /// <summary>
        /// Adds values to an ActivityParty Lookup.
        /// </summary>
        /// <param name="controls">The activityparty lookup field name, value or index of the lookup.</param>
        /// <example>xrmApp.Entity.AddValues(new LookupItem[] { new LookupItem { Name = "to", Value = "A. Datum Corporation (sample)" } });</example>
        public void AddValues(LookupItem[] controls)
        {
            _entityManager.AddValues(controls);
        }

        /// <summary>
        /// Removes values from an ActivityParty Lookup.
        /// </summary>
        /// <param name="controls">The activityparty lookup field name, value or index of the lookup.</param>
        /// <example>xrmApp.Entity.RemoveValues(new LookupItem[] { new LookupItem { Name = "to", Value = "A. Datum Corporation (sample)" } });</example>
        public void RemoveValues(LookupItem[] controls)
        {
            _entityManager.RemoveValues(controls);
        }
    }
}
