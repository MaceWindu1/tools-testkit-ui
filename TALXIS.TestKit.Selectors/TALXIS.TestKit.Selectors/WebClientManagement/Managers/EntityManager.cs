using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using AngleSharp.Dom;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using TALXIS.TestKit.Selectors.DTO;
using TALXIS.TestKit.Selectors.DTO.Locators;
using TALXIS.TestKit.Selectors.WebClientManagement.Helpers;
using TALXIS.TestKit.Selectors.Browser;

namespace TALXIS.TestKit.Selectors.WebClientManagement
{
    internal class EntityManager
    {
        public WebClient Client { get; }

        private enum BooleanControlType
        {
            Unknown,
            Radio,
            Checkbox,
            List,
            Toggle,
            FlipSwitch,
            Button
        }

        internal EntityManager(WebClient client)
        {
            Client = client ?? throw new ArgumentNullException(nameof(client));
        }

        /// <summary>
        /// Close Record Set Navigator
        /// </summary>
        /// <param name="thinkTime"></param>
        /// <example>xrmApp.Entity.CloseRecordSetNavigator();</example>
        public BrowserCommandResult<bool> CloseRecordSetNavigator(int thinkTime = Constants.DefaultThinkTime)
        {
            Client.ThinkTime(thinkTime);

            return this.Client.Execute(Client.GetOptions("Close Record Set Navigator"), driver =>
            {
                var closeSpan = driver.HasElement(EntityElementsLocators.RecordSetNavCollapseIcon);

                if (closeSpan)
                {
                    driver.FindElement(EntityElementsLocators.RecordSetNavCollapseIconParent).Click();
                }

                return true;
            });
        }

        /// <summary>
        /// Gets the value of a Lookup.
        /// </summary>
        /// <param name="control">The lookup field name of the lookup.</param>
        /// <example>xrmApp.Entity.GetValue(new Lookup { Name = "primarycontactid" });</example>
        public BrowserCommandResult<string> GetValue(LookupItem control)
        {
            var controlName = control.Name;

            return Client.Execute($"Get Lookup Value: {controlName}", driver =>
            {
                var xpathToContainer = EntityElementsLocators.TextFieldLookupFieldContainer(controlName);
                IWebElement fieldContainer = driver.WaitUntilAvailable(xpathToContainer);
                string lookupValue = TryGetValue(fieldContainer, control);

                return lookupValue;
            });
        }

        /// <summary>
        /// Gets the value of an ActivityParty Lookup.
        /// </summary>
        /// <param name="controls">The lookup field name of the lookup.</param>
        /// <example>xrmApp.Entity.GetValue(new LookupItem[] { new LookupItem { Name = "to" } });</example>
        public BrowserCommandResult<string[]> GetValue(LookupItem[] controls)
        {
            var controlName = controls.First().Name;

            return Client.Execute($"Get ActivityParty Lookup Value: {controlName}", driver =>
            {
                var xpathToContainer = EntityElementsLocators.TextFieldLookupFieldContainer(controlName);
                var fieldContainer = driver.WaitUntilAvailable(xpathToContainer);
                string[] result = TryGetValue(fieldContainer, controls);

                return result;
            });
        }

        /// <summary>
        /// Gets the value of a Lookup.
        /// </summary>
        /// <param name="control">The lookup field name of the lookup.</param>
        /// <example>xrmApp.Entity.GetValue(new DateTimeControl { Name = "scheduledstart" });</example>
        public BrowserCommandResult<DateTime?> GetValue(DateTimeControl control)
            => Client.Execute($"Get DateTime Value: {control.Name}", driver => TryGetValue(driver, container: driver, control: control));

        /// <summary>
        /// Open record set and navigate record index.
        /// This method supersedes Navigate Up and Navigate Down outside of UCI 
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="thinkTime">Used to simulate a wait time between human interactions. The Default is 2 seconds.</param>
        /// <example>xrmBrowser.Entity.OpenRecordSetNavigator();</example>
        public BrowserCommandResult<bool> OpenRecordSetNavigator(int index = 0, int thinkTime = Constants.DefaultThinkTime)
        {
            Client.ThinkTime(thinkTime);

            return this.Client.Execute(Client.GetOptions("Open Record Set Navigator"), driver =>
            {
                // check if record set navigator parent div is set to open
                driver.WaitForTransaction();

                if (!driver.TryFindElement(EntityElementsLocators.RecordSetNavList, out var navList))
                {
                    driver.FindElement(EntityElementsLocators.RecordSetNavigator).Click();
                    driver.WaitForTransaction();
                    navList = driver.FindElement(EntityElementsLocators.RecordSetNavList);
                }

                var links = navList.FindElements(By.TagName("li"));

                try
                {
                    links[index].Click();
                }
                catch
                {
                    throw new InvalidOperationException($"No record with the index '{index}' exists.");
                }

                driver.WaitForTransaction();

                return true;
            });
        }

        /// <summary>
        /// Sets the value of a Boolean Item.
        /// </summary>
        /// <param name="option">The boolean field name.</param>
        /// <example>xrmApp.Entity.SetValue(new BooleanItem { Name = "donotemail", Value = true });</example>
        public BrowserCommandResult<bool> SetValue(BooleanItem option, FormContextType formContextType)
        {
            if (option == null) throw new ArgumentNullException(nameof(option));

            return this.Client.Execute(Client.GetOptions($"Set BooleanItem Value: {option.Name}"), driver =>
            {
                // Make sure the name is lower case.
                option.Name = option.Name.ToLowerInvariant();

                // Find a container for the form element
                var fieldContainer = ValidateFormContext(driver, formContextType, option.Name, null);

                // Check which control element is used
                var controlType = GetBooleanControlType(fieldContainer, option.Name);

                switch (controlType)
                {
                    case BooleanControlType.Radio:
                        ClickRadioButton(fieldContainer, option);
                        break;

                    case BooleanControlType.Checkbox:
                        ClickCheckbox(fieldContainer, option);
                        break;

                    case BooleanControlType.List:
                        ClickListOption(fieldContainer, option, driver);
                        break;

                    case BooleanControlType.Toggle:
                    case BooleanControlType.FlipSwitch:
                        ClickToggleOrFlipSwitch(fieldContainer, option);
                        break;

                    case BooleanControlType.Button:
                        ClickButton(fieldContainer, option);
                        break;

                    default:
                        throw new InvalidOperationException($"Field: {option.Name} does not exist or has an unsupported control type.");
                }

                return true;
            });
        }

        /// <summary>
        /// Sets the value of a Date Field.
        /// </summary>
        /// <param name="field">Date field name.</param>
        /// <param name="value">DateTime value.</param>
        /// <param name="formatDate">Datetime format matching Short Date formatting personal options.</param>
        /// <param name="formatTime">Datetime format matching Short Time formatting personal options.</param>
        /// <example>xrmApp.Entity.SetValue("birthdate", DateTime.Parse("11/1/1980"));</example>
        /// <example>xrmApp.Entity.SetValue("new_actualclosedatetime", DateTime.Now, "MM/dd/yyyy", "hh:mm tt");</example>
        /// <example>xrmApp.Entity.SetValue("estimatedclosedate", DateTime.Now);</example>
        public BrowserCommandResult<bool> SetValue(string field, DateTime value, FormContextType formContext, string formatDate = null, string formatTime = null)
        {
            var control = new DateTimeControl(field)
            {
                Value = value,
                DateFormat = formatDate,
                TimeFormat = formatTime
            };
            return SetValue(control, formContext);
        }

        public BrowserCommandResult<bool> SetValue(DateTimeControl control, FormContextType formContext)
            => Client.Execute(Client.GetOptions($"Set Date/Time Value: {control.Name}"),
                driver => TrySetValue(driver, container: driver, control: control, formContext));

        internal static void TryCloseHeaderFlyout(IWebDriver driver)
        {
            bool hasHeader = driver.HasElement(EntityElementsLocators.Header.Container);
            if (!hasHeader)
                throw new NotFoundException("Unable to find header on the form");

            var xPath = EntityElementsLocators.Header.FlyoutButton;
            var headerFlyoutButton = driver.FindElement(xPath);
            bool expanded = bool.Parse(headerFlyoutButton.GetAttribute("aria-expanded"));

            if (expanded)
                headerFlyoutButton.Click(true);
        }

        internal static void TryRemoveLookupValue(IWebDriver driver, IWebElement fieldContainer, LookupItem control, bool removeAll = true, bool isHeader = false)
        {
            var controlName = control.Name;
            fieldContainer.Hover(driver);

            var xpathDeleteExistingValues = EntityElementsLocators.LookupFieldDeleteExistingValue(controlName);
            var existingValues = fieldContainer.FindElements(xpathDeleteExistingValues);

            var xpathToExpandButton = EntityElementsLocators.LookupFieldExpandCollapseButton(controlName);
            bool success = fieldContainer.TryFindElement(xpathToExpandButton, out var expandButton);

            if (success)
            {
                expandButton.Click(true);

                var count = existingValues.Count;
                fieldContainer.WaitUntil(x => x.FindElements(xpathDeleteExistingValues).Count > count);
            }

            fieldContainer.WaitUntilAvailable(EntityElementsLocators.TextFieldLookupSearchButton(controlName));

            existingValues = fieldContainer.FindElements(xpathDeleteExistingValues);
            if (existingValues.Count == 0)
                return;

            if (removeAll)
            {
                // Removes all selected items

                while (existingValues.Count > 0)
                {
                    foreach (var v in existingValues)
                        v.Click(true);

                    existingValues = fieldContainer.FindElements(xpathDeleteExistingValues);
                }

                return;
            }

            // Removes an individual item by value or index
            var value = control.Value;
            if (value == null)
                throw new InvalidOperationException($"No value or index has been provided for the LookupItem {controlName}. Please provide an value or an empty string or an index and try again.");

            if (value == string.Empty)
            {
                var index = control.Index;
                if (index >= existingValues.Count)
                    throw new InvalidOperationException($"Field '{controlName}' does not contain {index + 1} records. Please provide an index value less than {existingValues.Count}");

                existingValues[index].Click(true);
                return;
            }

            var existingValue = existingValues.FirstOrDefault(v => v.GetAttribute("aria-label").EndsWith(value));
            if (existingValue == null)
                throw new InvalidOperationException($"Field '{controlName}' does not contain a record with the name:  {value}");

            existingValue.Click(true);
            driver.WaitForTransaction();
        }

        internal BrowserCommandResult<bool> AddValues(LookupItem[] controls)
        {
            return Client.Execute(Client.GetOptions($"Add values {controls.First().Name}"), driver =>
            {
                SetValue(controls, FormContextType.Entity, false);

                return true;
            });
        }

        internal BrowserCommandResult<bool> CancelQuickCreate(int thinkTime = Constants.DefaultThinkTime)
        {
            Client.ThinkTime(thinkTime);

            return this.Client.Execute(Client.GetOptions($"Cancel Quick Create"), driver =>
            {
                var save = driver.WaitUntilAvailable(
                    QuickCreateElementsLocators.CancelButton,
                    "Quick Create Cancel Button is not available");
                save?.Click(true);

                driver.WaitForTransaction();

                return true;
            });
        }

        internal BrowserCommandResult<bool> ClearHeaderValue(DateTimeControl control)
        {
            var controlName = control.Name;
            return Client.Execute(Client.GetOptions($"Clear Header Date/Time Value: {controlName}"),
                driver => TrySetHeaderValue(driver, new DateTimeControl(controlName)));
        }

        internal BrowserCommandResult<bool> ClearValue(DateTimeControl control, FormContextType formContextType)
                    => Client.Execute(Client.GetOptions($"Clear Field: {control.Name}"),
                        driver => TrySetValue(driver, container: driver, control: new DateTimeControl(control.Name), formContextType));

        internal BrowserCommandResult<bool> ClearValue(string fieldName, FormContextType formContextType)
        {
            return this.Client.Execute(Client.GetOptions($"Clear Field {fieldName}"), driver =>
            {
                SetValue(fieldName, string.Empty, formContextType);

                return true;
            });
        }

        // Pass an empty control
        internal BrowserCommandResult<bool> ClearValue(LookupItem control, FormContextType formContextType, bool removeAll = true)
        {
            var controlName = control.Name;
            return Client.Execute(Client.GetOptions($"Clear Field {controlName}"), driver =>
            {
                var fieldContainer = driver.WaitUntilAvailable(EntityElementsLocators.TextFieldLookupFieldContainer(controlName));
                TryRemoveLookupValue(driver, fieldContainer, control, removeAll);
                return true;
            });
        }

        internal BrowserCommandResult<bool> ClearValue(OptionSet control, FormContextType formContextType)
        {
            return this.Client.Execute(Client.GetOptions($"Clear Field {control.Name}"), driver =>
            {
                control.Value = "-1";
                SetValueHelper.SetOptionSetValue(Client, control, formContextType);

                return true;
            });
        }

        internal BrowserCommandResult<bool> ClearValue(MultiValueOptionSet control, FormContextType formContextType)
        {
            return this.Client.Execute(Client.GetOptions($"Clear Field {control.Name}"), driver =>
            {
                RemoveMultiOptions(control, formContextType);

                return true;
            });
        }

        internal TResult ExecuteInHeaderContainer<TResult>(IWebDriver driver, By xpathToContainer, Func<IWebElement, TResult> function)
        {
            TResult lookupValue = default(TResult);

            TryExpandHeaderFlyout(driver);

            var xpathToFlyout = EntityElementsLocators.Header.Flyout;
            driver.WaitUntilVisible(xpathToFlyout, TimeSpan.FromSeconds(5),
                flyout =>
                {
                    IWebElement container = flyout.FindElement(xpathToContainer);
                    lookupValue = function(container);
                });

            return lookupValue;
        }

        /// <summary>
        /// Returns the Entity Name of the entity
        /// </summary>
        /// <returns>Entity Name of the Entity</returns>
        internal BrowserCommandResult<string> GetEntityName(int thinkTime = Constants.DefaultThinkTime)
        {
            return this.Client.Execute(Client.GetOptions($"Get Entity Name"), driver =>
            {
                var entityName = driver.ExecuteScript("return Xrm.Page.data.entity.getEntityName();").ToString();

                if (string.IsNullOrEmpty(entityName))
                {
                    throw new NotFoundException("Unable to retrieve Entity Name for this entity");
                }

                return entityName;
            });
        }

        internal BrowserCommandResult<Field> GetField(string field)
        {
            return this.Client.Execute(Client.GetOptions($"Get Field"), driver =>
            {
                var fieldElement = driver.WaitUntilAvailable(EntityElementsLocators.TextFieldContainer(field));
                Field returnField = new Field(fieldElement);
                returnField.Name = field;

                IWebElement fieldLabel = null;
                try
                {
                    fieldLabel = fieldElement.FindElement(EntityElementsLocators.TextFieldLabel(field));
                }
                catch (NoSuchElementException)
                {
                    // Swallow
                }

                if (fieldLabel != null)
                {
                    returnField.Label = fieldLabel.Text;
                }

                return returnField;
            });
        }

        /// <summary>
        /// Returns the Form Name of the entity
        /// </summary>
        /// <returns>Form Name of the Entity</returns>
        internal BrowserCommandResult<string> GetFormName(int thinkTime = Constants.DefaultThinkTime)
        {
            return this.Client.Execute(Client.GetOptions($"Get Form Name"), driver =>
            {
                // Wait for form selector visible
                driver.WaitUntilVisible(EntityElementsLocators.FormSelector);

                string formName = driver.ExecuteScript("return Xrm.Page.ui.formContext.ui.formSelector.getCurrentItem().getLabel();").ToString();

                if (string.IsNullOrEmpty(formName))
                {
                    throw new NotFoundException("Unable to retrieve Form Name for this entity");
                }

                return formName;
            });
        }

        /// <summary>
        /// Returns the Header Title of the entity
        /// </summary>
        /// <returns>Header Title of the Entity</returns>
        internal BrowserCommandResult<string> GetHeaderTitle(int thinkTime = Constants.DefaultThinkTime)
        {
            return this.Client.Execute(Client.GetOptions($"Get Header Title"), driver =>
            {
                // Wait for form selector visible
                var headerTitle = driver.WaitUntilVisible(EntityElementsLocators.HeaderTitle, new TimeSpan(0, 0, 5));

                var headerTitleName = headerTitle?.GetAttribute("title");

                if (string.IsNullOrEmpty(headerTitleName))
                {
                    throw new NotFoundException("Unable to retrieve Header Title for this entity");
                }

                return headerTitleName;
            });
        }

        internal BrowserCommandResult<string> GetHeaderValue(LookupItem control)
        {
            var controlName = control.Name;
            return Client.Execute(Client.GetOptions($"Get Header LookupItem Value {controlName}"), driver =>
            {
                var xpathToContainer = EntityElementsLocators.Header.LookupFieldContainer(controlName);
                string lookupValue = ExecuteInHeaderContainer(driver, xpathToContainer, container => TryGetValue(container, control));

                return lookupValue;
            });
        }

        internal BrowserCommandResult<string[]> GetHeaderValue(LookupItem[] controls)
        {
            var controlName = controls.First().Name;
            var xpathToContainer = EntityElementsLocators.Header.LookupFieldContainer(controlName);
            return Client.Execute(Client.GetOptions($"Get Header Activityparty LookupItem Value {controlName}"), driver =>
            {
                string[] lookupValues = ExecuteInHeaderContainer(driver, xpathToContainer, container => TryGetValue(container, controls));

                return lookupValues;
            });
        }

        internal BrowserCommandResult<string> GetHeaderValue(string control)
        {
            return this.Client.Execute(Client.GetOptions($"Get Header Value {control}"), driver =>
            {
                TryExpandHeaderFlyout(driver);

                return GetValue(control);
            });
        }

        internal BrowserCommandResult<MultiValueOptionSet> GetHeaderValue(MultiValueOptionSet control)
        {
            return this.Client.Execute(Client.GetOptions($"Get Header MultiValueOptionSet Value {control.Name}"), driver =>
            {
                TryExpandHeaderFlyout(driver);

                return GetValue(control);
            });
        }

        internal BrowserCommandResult<string> GetHeaderValue(OptionSet control)
        {
            var controlName = control.Name;
            var xpathToContainer = EntityElementsLocators.Header.OptionSetFieldContainer(controlName);
            return Client.Execute(Client.GetOptions($"Get Header OptionSet Value {controlName}"),
                driver => ExecuteInHeaderContainer(driver, xpathToContainer, container => TryGetValue(container, control))
            );
        }

        internal BrowserCommandResult<bool> GetHeaderValue(BooleanItem control)
        {
            return this.Client.Execute(Client.GetOptions($"Get Header BooleanItem Value {control}"), driver =>
            {
                TryExpandHeaderFlyout(driver);

                return GetValue(control);
            });
        }

        internal BrowserCommandResult<DateTime?> GetHeaderValue(DateTimeControl control)
        {
            var xpathToContainer = EntityElementsLocators.Header.DateTimeFieldContainer(control.Name);
            return Client.Execute(Client.GetOptions($"Get Header DateTime Value {control.Name}"),
                driver => ExecuteInHeaderContainer(driver, xpathToContainer,
                    container => TryGetValue(driver, container, control)));
        }

        /// <summary>
        /// Returns the ObjectId of the entity
        /// </summary>
        /// <returns>Guid of the Entity</returns>
        internal BrowserCommandResult<Guid> GetObjectId(int thinkTime = Constants.DefaultThinkTime)
        {
            return this.Client.Execute(Client.GetOptions($"Get Object Id"), driver =>
            {
                var objectId = driver.ExecuteScript("return Xrm.Page.data.entity.getId();");

                Guid oId;

                if (!Guid.TryParse(objectId.ToString(), out oId))
                    throw new NotFoundException("Unable to retrieve object Id for this entity");

                return oId;
            });
        }

        internal BrowserCommandResult<string> GetStateFromForm()
        {
            return this.Client.Execute(Client.GetOptions($"Get Status value from form"), driver =>
            {
                driver.WaitForTransaction();
                if (!driver.TryFindElement(By.Id("message-formReadOnlyNotification"), out var readOnlyNotification))
                {
                    return "Active";
                }

                var match = Regex.Match(readOnlyNotification.Text, "This record’s status: (.*)");
                if (match.Success)
                {
                    return match.Captures[1].Value;
                }

                try
                {
                    return GetHeaderValue(new OptionSet { Name = "statecode" }).Value;
                }
                catch (Exception ex)
                {
                    throw new NotFoundException("Unable to determine the status from the form. This can happen if you do not have access to edit the record and the state is not in the header.", ex);
                }
            });
        }

        internal BrowserCommandResult<int> GetSubGridItemsCount(string subgridName)
        {
            return this.Client.Execute(Client.GetOptions($"Get Subgrid Items Count for subgrid {subgridName}"), driver =>
            {
                List<GridItem> rows = GetDataHelper.GetSubGridItems(subgridName, Client);

                return rows.Count;
            });
        }

        internal BrowserCommandResult<string> GetValue(string field)
        {
            if (string.IsNullOrWhiteSpace(field))
                throw new ArgumentException("Field name cannot be null or empty", nameof(field));

            return Client.Execute(Client.GetOptions($"Get Value: {field}"), driver =>
            {
                var fieldContainer = driver.WaitUntilAvailable(EntityElementsLocators.TextFieldContainer(field));

                // Search for input, textarea, iframe in one place
                var inputElement = fieldContainer.FindElements(By.TagName("input")).FirstOrDefault();
                var textareaElement = fieldContainer.FindElements(By.TagName("textarea")).FirstOrDefault();
                var iframeElement = fieldContainer.FindElements(By.TagName("iframe")).FirstOrDefault();

                // If an input is found
                if (inputElement != null)
                {
                    string text = inputElement.GetAttribute("value");

                    //  Processing of the date + time field
                    var timeField = driver.FindElements(EntityElementsLocators.FieldControlDateTimeTimeInputUCI(field)).FirstOrDefault();
                    if (timeField != null)
                    {
                        text += $" {timeField.GetAttribute("value")}";
                    }

                    return text;
                }

                // If a textarea is found
                if (textareaElement != null)
                {
                    return textareaElement.GetAttribute("value");
                }

                // If an iframe is found
                if (iframeElement != null)
                {
                    return GetIframeText(driver);
                }

                throw new Exception($"Field with name '{field}' does not exist.");
            });
        }

        /// <summary>
        /// Gets the value of a picklist or status field.
        /// </summary>
        /// <param name="control">The option you want to set.</param>
        /// <example>xrmApp.Entity.GetValue(new OptionSet { Name = "preferredcontactmethodcode"}); </example>
        internal BrowserCommandResult<string> GetValue(OptionSet control)
        {
            var controlName = control.Name;

            return this.Client.Execute($"Get OptionSet Value: {controlName}", driver =>
            {
                var xpathToFieldContainer = EntityElementsLocators.OptionSetFieldContainer(controlName);
                var fieldContainer = driver.WaitUntilAvailable(xpathToFieldContainer);
                string result = TryGetValue(fieldContainer, control);

                return result;
            });
        }

        /// <summary>
        /// Sets the value of a Boolean Item.
        /// </summary>
        /// <param name="option">The boolean field name.</param>
        /// <example>xrmApp.Entity.GetValue(new BooleanItem { Name = "creditonhold" });</example>
        internal BrowserCommandResult<bool> GetValue(BooleanItem option)
        {
            return this.Client.Execute($"Get BooleanItem Value: {option.Name}", driver =>
            {
                var check = false;

                var fieldContainer = driver.WaitUntilAvailable(EntityElementsLocators.TextFieldContainer(option.Name));

                var hasRadio = fieldContainer.HasElement(EntityElementsLocators.EntityBooleanFieldRadioContainer(option.Name));
                var hasCheckbox = fieldContainer.HasElement(EntityElementsLocators.EntityBooleanFieldCheckbox(option.Name));
                var hasList = fieldContainer.HasElement(EntityElementsLocators.EntityBooleanFieldList(option.Name));
                var hasToggle = fieldContainer.HasElement(EntityElementsLocators.EntityBooleanFieldToggle(option.Name));

                if (hasRadio)
                {
                    var trueRadio = fieldContainer.FindElement(EntityElementsLocators.EntityBooleanFieldRadioTrue(option.Name));

                    check = bool.Parse(trueRadio.GetAttribute("aria-checked"));
                }
                else if (hasCheckbox)
                {
                    var checkbox = fieldContainer.FindElement(EntityElementsLocators.EntityBooleanFieldCheckbox(option.Name));

                    check = bool.Parse(checkbox.GetAttribute("aria-checked"));
                }
                else if (hasList)
                {
                    var list = fieldContainer.FindElement(EntityElementsLocators.EntityBooleanFieldList(option.Name));
                    var options = list.FindElements(By.TagName("option"));
                    var selectedOption = options.FirstOrDefault(a => a.HasAttribute("data-selected") && bool.Parse(a.GetAttribute("data-selected")));

                    if (selectedOption != null)
                    {
                        check = int.Parse(selectedOption.GetAttribute("value")) == 1;
                    }
                }
                else if (hasToggle)
                {
                    var toggle = fieldContainer.FindElement(EntityElementsLocators.EntityBooleanFieldToggle(option.Name));
                    var link = toggle.FindElement(By.TagName("button"));

                    check = bool.Parse(link.GetAttribute("aria-checked"));
                }
                else
                    throw new InvalidOperationException($"Field: {option.Name} Does not exist");

                return check;
            });
        }

        /// <summary>
        /// Gets the value from the multselect type control
        /// </summary>
        /// <param name="option">Object of type MultiValueOptionSet containing name of the Field</param>
        /// <returns>MultiValueOptionSet object where the values field contains all the contact names</returns>
        internal BrowserCommandResult<MultiValueOptionSet> GetValue(MultiValueOptionSet option)
        {
            return this.Client.Execute(Client.GetOptions($"Get Multi Select Value: {option.Name}"), driver =>
            {
                var containerXPath = MultiSelectElementsLocators.DivContainer(option.Name);
                var container = driver.WaitUntilAvailable(containerXPath, $"Multi-select option set {option.Name} not found.");

                container.Hover(driver, true);
                var expandButtonXPath = MultiSelectElementsLocators.ExpandCollapseButton;
                if (container.TryFindElement(expandButtonXPath, out var expandButton) && expandButton.IsClickable())
                {
                    expandButton.Click();
                }

                var selectedOptionsXPath = MultiSelectElementsLocators.SelectedRecordLabel;
                var selectedOptions = container.FindElements(selectedOptionsXPath);

                return new MultiValueOptionSet
                {
                    Name = option.Name,
                    Values = selectedOptions.Select(o => o.Text).ToArray()
                };
            });
        }

        /// <summary>
        /// Open Entity
        /// </summary>
        /// <param name="entityName">The entity name</param>
        /// <param name="id">The Id</param>
        /// <param name="thinkTime">The think time</param>
        internal BrowserCommandResult<bool> OpenEntity(string entityName, Guid id, int thinkTime = Constants.DefaultThinkTime)
        {
            Client.ThinkTime(thinkTime);

            return this.Client.Execute(Client.GetOptions($"Open: {entityName} {id}"), driver =>
            {
                //https:///main.aspx?appid=98d1cf55-fc47-e911-a97c-000d3ae05a70&pagetype=entityrecord&etn=lead&id=ed975ea3-531c-e511-80d8-3863bb3ce2c8
                var uri = new Uri(Client.Browser.Driver.Url);
                var qs = HttpUtility.ParseQueryString(uri.Query.ToLower());
                var appId = qs.Get("appid");
                var link = $"{uri.Scheme}://{uri.Authority}/main.aspx?appid={appId}&etn={entityName}&pagetype=entityrecord&id={id}";

                if (Client.Browser.Options.UCITestMode)
                {
                    link += "&flags=testmode=true";
                }
                if (Client.Browser.Options.UCIPerformanceMode)
                {
                    link += "&perf=true";
                }

                driver.Navigate().GoToUrl(link);

                //SwitchToContent();
                driver.WaitForPageToLoad();
                driver.WaitForTransaction();
                driver.WaitUntilClickable(By.XPath(Elements.Xpath[Reference.Entity.Form]),
                    TimeSpan.FromSeconds(30),
                    "CRM Record is Unavailable or not finished loading. Timeout Exceeded"
                );

                return true;
            });
        }

        internal BrowserCommandResult<bool> RemoveValues(LookupItem[] controls)
        {
            return Client.Execute(Client.GetOptions($"Remove values {controls.First().Name}"), driver =>
            {
                foreach (var control in controls)
                    ClearValue(control, FormContextType.Entity, false);

                return true;
            });
        }

        /// <summary>
        /// Saves the entity
        /// </summary>
        /// <param name="thinkTime"></param>
        internal BrowserCommandResult<bool> Save(int thinkTime = Constants.DefaultThinkTime)
        {
            Client.ThinkTime(thinkTime);

            return this.Client.Execute(Client.GetOptions($"Save"), driver =>
            {
                Actions action = new Actions(driver);
                action.KeyDown(Keys.Control).SendKeys("S").Perform();

                return true;
            });
        }

        internal BrowserCommandResult<bool> SaveQuickCreate(int thinkTime = Constants.DefaultThinkTime)
        {
            Client.ThinkTime(thinkTime);

            return this.Client.Execute(Client.GetOptions($"SaveQuickCreate"), driver =>
            {
                var save = driver.WaitUntilAvailable(By.XPath(AppElements.Xpath[AppReference.QuickCreate.SaveAndCloseButton]),
                    "Quick Create Save Button is not available");
                save?.Click(true);

                driver.WaitForTransaction();

                return true;
            });
        }

        internal BrowserCommandResult<bool> SelectForm(string formName)
        {
            return this.Client.Execute(Client.GetOptions($"Select Form {formName}"), driver =>
            {
                driver.WaitForTransaction();

                if (!driver.HasElement(By.XPath(Elements.Xpath[Reference.Entity.FormSelector])))
                    throw new NotFoundException("Unable to find form selector on the form");

                var formSelector = driver.WaitUntilAvailable(By.XPath(Elements.Xpath[Reference.Entity.FormSelector]));
                // Click didn't work with IE
                formSelector.SendKeys(Keys.Enter);

                driver.WaitUntilVisible(By.XPath(Elements.Xpath[Reference.Entity.FormSelectorFlyout]));

                var flyout = driver.FindElement(By.XPath(Elements.Xpath[Reference.Entity.FormSelectorFlyout]));
                var forms = flyout.FindElements(By.XPath(Elements.Xpath[Reference.Entity.FormSelectorItem]));

                var form = forms.FirstOrDefault(a => a.GetAttribute("data-text").EndsWith(formName, StringComparison.OrdinalIgnoreCase));
                if (form == null)
                    throw new NotFoundException($"Form {formName} is not in the form selector");

                driver.ClickWhenAvailable(By.Id(form.GetAttribute("id")));

                driver.WaitForPageToLoad();
                driver.WaitForTransaction();

                return true;
            });
        }

        /// <summary>
        /// Click the magnifying glass icon for the lookup control supplied
        /// </summary>
        /// <param name="control">The LookupItem field on the form</param>
        /// <returns></returns>
        internal BrowserCommandResult<bool> SelectLookup(LookupItem control)
        {
            return Client.Execute(Client.GetOptions($"Select Lookup Field {control.Name}"), driver =>
            {
                if (driver.HasElement(EntityElementsLocators.FieldLookupButton(control.Name)))
                {
                    var lookupButton = driver.FindElement(EntityElementsLocators.FieldLookupButton(control.Name));

                    lookupButton.Hover(driver);

                    driver.WaitForTransaction();

                    driver.FindElement(EntityElementsLocators.SearchButtonIcon).Click(true);
                }
                else
                    throw new NotFoundException($"Lookup field {control.Name} not found");

                driver.WaitForTransaction();

                return true;
            });
        }

        internal BrowserCommandResult<bool> SetHeaderValue(string field, string value)
        {
            return this.Client.Execute(Client.GetOptions($"Set Header Value {field}"), driver =>
            {
                TryExpandHeaderFlyout(driver);
                SetValue(field, value, FormContextType.Header);
                TryCloseHeaderFlyout(driver);

                return true;
            });
        }

        internal BrowserCommandResult<bool> SetHeaderValue(LookupItem control)
        {
            var controlName = control.Name;
            bool isHeader = true;
            bool removeAll = true;
            var xpathToContainer = EntityElementsLocators.Header.LookupFieldContainer(controlName);

            return Client.Execute(Client.GetOptions($"Set Header LookupItem Value {controlName}"),
                driver => ExecuteInHeaderContainer(driver, xpathToContainer,
                    fieldContainer =>
                    {
                        TryRemoveLookupValue(driver, fieldContainer, control, removeAll, isHeader);
                        TrySetValue(driver, fieldContainer, control);
                        TryCloseHeaderFlyout(driver);

                        return true;
                    }));
        }

        internal BrowserCommandResult<bool> SetHeaderValue(LookupItem[] controls, bool clearFirst = true)
        {
            var control = controls.First();
            var controlName = control.Name;
            var xpathToContainer = EntityElementsLocators.Header.LookupFieldContainer(controlName);

            return Client.Execute(Client.GetOptions($"Set Header Activityparty LookupItem Value {controlName}"),
                driver => ExecuteInHeaderContainer(driver, xpathToContainer,
                    container =>
                    {
                        if (clearFirst)
                            TryRemoveLookupValue(driver, container, control);

                        TryToSetValue(driver, container, controls);

                        TryCloseHeaderFlyout(driver);

                        return true;
                    }));
        }

        internal BrowserCommandResult<bool> SetHeaderValue(OptionSet control)
        {
            var controlName = control.Name;
            var xpathToContainer = EntityElementsLocators.Header.OptionSetFieldContainer(controlName);

            return Client.Execute(Client.GetOptions($"Set Header OptionSet Value {controlName}"),
                driver => ExecuteInHeaderContainer(driver, xpathToContainer,
                    container =>
                    {
                        TrySetValue(container, control);

                        TryCloseHeaderFlyout(driver);

                        return true;
                    }));
        }

        internal BrowserCommandResult<bool> SetHeaderValue(MultiValueOptionSet control)
        {
            return this.Client.Execute(Client.GetOptions($"Set Header MultiValueOptionSet Value {control.Name}"), driver =>
            {
                TryExpandHeaderFlyout(driver);

                SetValue(control, FormContextType.Header);

                TryCloseHeaderFlyout(driver);

                return true;
            });
        }

        internal BrowserCommandResult<bool> SetHeaderValue(BooleanItem control)
        {
            return this.Client.Execute(Client.GetOptions($"Set Header BooleanItem Value {control.Name}"), driver =>
            {
                TryExpandHeaderFlyout(driver);

                SetValue(control, FormContextType.Header);

                TryCloseHeaderFlyout(driver);

                return true;
            });
        }

        internal BrowserCommandResult<bool> SetHeaderValue(string field, DateTime value, string formatDate = null, string formatTime = null)
        {
            var control = new DateTimeControl(field)
            {
                Value = value,
                DateFormat = formatDate,
                TimeFormat = formatTime
            };

            return SetHeaderValue(control);
        }

        internal BrowserCommandResult<bool> SetHeaderValue(DateTimeControl control)
                    => Client.Execute(Client.GetOptions($"Set Header Date/Time Value: {control.Name}"), driver => TrySetHeaderValue(driver, control));

        /// <summary>
        /// Set Value
        /// </summary>
        /// <param name="field">The field</param>
        /// <param name="value">The value</param>
        /// <example>xrmApp.Entity.SetValue("firstname", "Test");</example>
        internal BrowserCommandResult<bool> SetValue(string field, string value, FormContextType formContextType = FormContextType.Entity)
        {
            return Client.Execute(Client.GetOptions("Set Value"), driver =>
            {
                IWebElement fieldContainer = null;
                fieldContainer = ValidateFormContext(driver, formContextType, field, fieldContainer);

                IWebElement input;
                bool found = fieldContainer.TryFindElement(By.TagName("input"), out input);

                if (!found)
                    found = fieldContainer.TryFindElement(By.TagName("textarea"), out input);

                if (!found)
                {
                    found = fieldContainer.TryFindElement(By.TagName("iframe"), out input);
                    SetValueHelper.SetValue(Client, Reference.Timeline.NoteText, value, "iframe");
                    return true;
                }

                if (!found)
                    throw new NoSuchElementException($"Field with name {field} does not exist.");

                SetInputValue(driver, input, value);

                return true;
            });
        }

        /// <summary>
        /// Sets the value of a Lookup, Customer, Owner or ActivityParty Lookup which accepts only a single value.
        /// </summary>
        /// <param name="control">The lookup field name, value or index of the lookup.</param>
        /// <example>xrmApp.Entity.SetValue(new Lookup { Name = "prrimarycontactid", Value = "Rene Valdes (sample)" });</example>
        /// The default index position is 0, which will be the first result record in the lookup results window. Suppy a value > 0 to select a different record if multiple are present.
        internal BrowserCommandResult<bool> SetValue(LookupItem control, FormContextType formContextType)
        => SetValueHelper.SetLookUp(Client, control, formContextType);

        /// <summary>
        /// Sets the value of an ActivityParty Lookup.
        /// </summary>
        /// <param name="controls">The lookup field name, value or index of the lookup.</param>
        /// <example>xrmApp.Entity.SetValue(new Lookup[] { Name = "to", Value = "Rene Valdes (sample)" }, { Name = "to", Value = "Alpine Ski House (sample)" } );</example>
        /// The default index position is 0, which will be the first result record in the lookup results window. Suppy a value > 0 to select a different record if multiple are present.
        internal BrowserCommandResult<bool> SetValue(LookupItem[] controls, FormContextType formContextType = FormContextType.Entity, bool clearFirst = true)
        {
            var control = controls.First();
            var controlName = control.Name;
            return Client.Execute(Client.GetOptions($"Set ActivityParty Lookup Value: {controlName}"), driver =>
            {
                driver.WaitForTransaction();

                IWebElement fieldContainer = null;
                fieldContainer = ValidateFormContext(driver, formContextType, controlName, fieldContainer);

                if (clearFirst)
                    TryRemoveLookupValue(driver, fieldContainer, control);

                TryToSetValue(driver, fieldContainer, controls);

                return true;
            });
        }

        /// <summary>
        /// Sets/Removes the value from the multselect type control
        /// </summary>
        /// <param name="option">Object of type MultiValueOptionSet containing name of the Field and the values to be set/removed</param>
        /// <param name="removeExistingValues">False - Values will be set. True - Values will be removed</param>
        /// <returns>True on success</returns>
        internal BrowserCommandResult<bool> SetValue(MultiValueOptionSet option, FormContextType formContextType = FormContextType.Entity, bool removeExistingValues = false)
        {
            return this.Client.Execute(Client.GetOptions($"Set MultiValueOptionSet Value: {option.Name}"), driver =>
            {
                if (removeExistingValues)
                {
                    RemoveMultiOptions(option, formContextType);
                }


                AddMultiOptions(option, formContextType);

                return true;
            });
        }

        internal void TryExpandHeaderFlyout(IWebDriver driver)
        {
            driver.WaitUntilAvailable(
                EntityElementsLocators.Header.Container,
                "Unable to find header on the form");

            var xPath = EntityElementsLocators.Header.FlyoutButton;
            var headerFlyoutButton = driver.FindElement(xPath);
            bool expanded = bool.Parse(headerFlyoutButton.GetAttribute("aria-expanded"));

            if (!expanded)
                headerFlyoutButton.Click(true);
        }

        private static void ClickButton(IWebElement fieldContainer, BooleanItem option)
        {
            var container = fieldContainer.FindElement(EntityElementsLocators.EntityBooleanFieldButtonContainer(option.Name));
            var trueButton = container.FindElement(EntityElementsLocators.EntityBooleanFieldButtonTrue);
            var falseButton = container.FindElement(EntityElementsLocators.EntityBooleanFieldButtonFalse);

            if (option.Value)
                trueButton.Click();
            else
                falseButton.Click();
        }

        private static void ClickCheckbox(IWebElement fieldContainer, BooleanItem option)
        {
            var checkbox = fieldContainer.FindElement(EntityElementsLocators.EntityBooleanFieldCheckbox(option.Name));

            bool shouldClick = (option.Value && !checkbox.Selected) || (!option.Value && checkbox.Selected);

            if (shouldClick)
                checkbox.Click();
        }

        private static void ClickListOption(IWebElement fieldContainer, BooleanItem option, IWebDriver driver)
        {
            var list = fieldContainer.FindElement(EntityElementsLocators.EntityBooleanFieldList(option.Name));
            var options = list.FindElements(By.TagName("option"));

            var selectedOption = options.FirstOrDefault(a => a.HasAttribute("data-selected") && bool.Parse(a.GetAttribute("data-selected")));
            var unselectedOption = options.FirstOrDefault(a => !a.HasAttribute("data-selected"));

            bool shouldClick = selectedOption != null && ((option.Value && selectedOption.GetAttribute("value") != "1") || (!option.Value && selectedOption.GetAttribute("value") == "1"));

            if (shouldClick && unselectedOption != null)
                driver.ClickWhenAvailable(By.Id(unselectedOption.GetAttribute("id")));
        }

        private static void ClickRadioButton(IWebElement fieldContainer, BooleanItem option)
        {
            var trueRadio = fieldContainer.FindElement(EntityElementsLocators.EntityBooleanFieldRadioTrue(option.Name));
            var falseRadio = fieldContainer.FindElement(EntityElementsLocators.EntityBooleanFieldRadioFalse(option.Name));

            bool shouldClick = (option.Value && bool.Parse(falseRadio.GetAttribute("aria-checked"))) ||
                               (!option.Value && bool.Parse(trueRadio.GetAttribute("aria-checked")));

            if (shouldClick)
                trueRadio.Click();
        }

        private static void ClickToggleOrFlipSwitch(IWebElement fieldContainer, BooleanItem option)
        {
            var toggle = fieldContainer.FindElement(EntityElementsLocators.EntityBooleanFieldToggle(option.Name));
            var link = toggle.FindElement(By.TagName("button"));
            bool value = bool.Parse(link.GetAttribute("aria-checked"));

            if (value != option.Value)
                link.Click();
        }

        private static BooleanControlType GetBooleanControlType(IWebElement fieldContainer, string name)
        {
            if (fieldContainer.HasElement(EntityElementsLocators.EntityBooleanFieldRadioContainer(name)))
                return BooleanControlType.Radio;

            if (fieldContainer.HasElement(EntityElementsLocators.EntityBooleanFieldCheckbox(name)))
                return BooleanControlType.Checkbox;

            if (fieldContainer.HasElement(EntityElementsLocators.EntityBooleanFieldList(name)))
                return BooleanControlType.List;

            if (fieldContainer.HasElement(EntityElementsLocators.EntityBooleanFieldToggle(name)))
                return BooleanControlType.Toggle;

            if (fieldContainer.HasElement(EntityElementsLocators.EntityBooleanFieldFlipSwitchLink(name)))
                return BooleanControlType.FlipSwitch;

            if (fieldContainer.HasElement(EntityElementsLocators.EntityBooleanFieldButtonContainer(name)))
                return BooleanControlType.Button;

            return BooleanControlType.Unknown;
        }

        private static string GetIframeText(IWebDriver driver)
        {
            var inputBox = driver.WaitUntilAvailable(By.XPath(Elements.Xpath[Reference.Timeline.NoteText]));
            driver.SwitchTo().Frame(inputBox);

            driver.WaitUntilAvailable(By.TagName("iframe"));
            driver.SwitchTo().Frame(0);

            string text = driver.WaitUntilAvailable(By.TagName("body")).Text;

            driver.SwitchTo().DefaultContent();

            return text;
        }
        private static string GetSelectedOption(ReadOnlyCollection<IWebElement> options)
        {
            var selectedOption = options.FirstOrDefault(op => op.Selected);

            return selectedOption?.Text ?? string.Empty;
        }

        private static void SelectOption(ReadOnlyCollection<IWebElement> options, string value)
        {
            var selectedOption = options.FirstOrDefault(op => op.Text == value || op.GetAttribute("value") == value);
            selectedOption.Click(true);
        }

        private static string TryGetValue(IWebElement fieldContainer, OptionSet control)
        {
            bool success = fieldContainer.TryFindElement(By.TagName("select"), out IWebElement select);
            if (success)
            {
                var options = select.FindElements(By.TagName("option"));
                string result = GetSelectedOption(options);

                return result;
            }

            var name = control.Name;
            var hasStatusCombo = fieldContainer.HasElement(EntityElementsLocators.EntityNewLookOptionsetStatusCombo(name));
            if (hasStatusCombo)
            {
                // This is for statuscode (type = status) that should act like an optionset doesn't doesn't follow the same pattern when rendered
                var valueSpan = fieldContainer.FindElement(EntityElementsLocators.EntityOptionsetStatusTextValue(name));

                return valueSpan.Text;
            }

            throw new InvalidOperationException($"OptionSet Field: '{name}' does not exist");
        }

        private static DateTime? TryGetValue(IWebDriver driver, ISearchContext container, DateTimeControl control)
        {
            string field = control.Name;
            driver.WaitForTransaction();

            string script = @"
                function getDateTimeFieldValue(fieldLogicalName) {
                    var control = Xrm.Page.getControl(fieldLogicalName);

                    if (!control) return null;

                    var dateTimeValue = control.getAttribute().getValue();

                    return dateTimeValue ? dateTimeValue.toISOString() : null;
                }
                return getDateTimeFieldValue(arguments[0]);
            ";

            IJavaScriptExecutor jsExecutor = (IJavaScriptExecutor)driver;
            object result = jsExecutor.ExecuteScript(script, control.Name);

            var dateTime = DateTime.Parse(result.ToString());

            return dateTime;
        }

        private static void TrySetTime(IWebDriver driver, ISearchContext container, DateTimeControl control, FormContextType formContextType)
        {
            By timeFieldXPath = EntityElementsLocators.FieldControlDateTimeTimeInputUCI(control.Name);

            IWebElement formContext = null;

            if (formContextType == FormContextType.QuickCreate)
            {
                //IWebDriver formContext;
                // Initialize the quick create form context
                // If this is not done -- element input will go to the main form due to new flyout design
                formContext = container.WaitUntilAvailable(By.XPath(AppElements.Xpath[AppReference.QuickCreate.QuickCreateFormContext]), new TimeSpan(0, 0, 1));
            }
            else if (formContextType == FormContextType.Entity)
            {
                // Initialize the entity form context
                formContext = container.WaitUntilAvailable(EntityElementsLocators.FormContext, new TimeSpan(0, 0, 1));
            }
            else if (formContextType == FormContextType.BusinessProcessFlow)
            {
                // Initialize the Business Process Flow context
                formContext = container.WaitUntilAvailable(BusinessProcessFlowElementsLocators.BusinessProcessFlowFormContext, new TimeSpan(0, 0, 1));
            }
            else if (formContextType == FormContextType.Header)
            {
                // Initialize the Header context
                formContext = container as IWebElement;
            }
            else if (formContextType == FormContextType.Dialog)
            {
                // Initialize the Header context
                formContext = container.WaitUntilAvailable(By.XPath(AppElements.Xpath[AppReference.Dialogs.DialogContext]), new TimeSpan(0, 0, 1));
            }

            driver.WaitForTransaction();

            if (formContext.TryFindElement(timeFieldXPath, out var timeField))
            {
                TrySetTime(driver, timeField, control.TimeAsString);
            }
        }

        private static void TrySetTime(IWebDriver driver, IWebElement timeField, string time)
        {
            // click & wait until the time get updated after change/clear the date
            timeField.Click();
            driver.WaitForTransaction();

            driver.RepeatUntil(() =>
            {
                timeField.Clear();
                timeField.Click();
                timeField.SendKeys(time);
                timeField.SendKeys(Keys.Tab);
                driver.WaitForTransaction();
            },
            d => timeField.GetAttribute("value").IsValueEqualsTo(time),
            TimeSpan.FromSeconds(9), 3,
            failureCallback: () => throw new InvalidOperationException($"Timeout after 10 seconds. Expected: {time}. Actual: {timeField.GetAttribute("value")}")
            );
        }

        private static void TrySetValue(IWebElement fieldContainer, OptionSet control)
        {
            var value = control.Value;
            bool success = fieldContainer.TryFindElement(By.TagName("select"), out IWebElement select);

            if (success)
            {
                fieldContainer.WaitUntilAvailable(By.TagName("select"));
                var options = select.FindElements(By.TagName("option"));
                SelectOption(options, value);

                return;
            }

            var name = control.Name;
            var hasStatusCombo = fieldContainer.HasElement(EntityElementsLocators.EntityNewLookOptionsetStatusCombo(name));

            if (hasStatusCombo)
            {
                // This is for statuscode (type = status) that should act like an optionset doesn't doesn't follow the same pattern when rendered
                fieldContainer.ClickWhenAvailable(EntityElementsLocators.EntityNewLookOptionsetStatusComboButton(name));

                var listBox = fieldContainer.FindElement(EntityElementsLocators.EntityNewLookOptionsetStatusComboList(name));

                var options = listBox.FindElements(By.TagName("li"));
                SelectOption(options, value);
                return;
            }

            throw new InvalidOperationException($"OptionSet Field: '{name}' does not exist");
        }

        /// <summary>
        /// Sets the value from the multselect type control
        /// </summary>
        /// <param name="option">Object of type MultiValueOptionSet containing name of the Field and the values to be set</param>
        /// <returns></returns>
        private BrowserCommandResult<bool> AddMultiOptions(MultiValueOptionSet option, FormContextType formContextType)
        {
            return this.Client.Execute(Client.GetOptions($"Add Multi Select Value: {option.Name}"), driver =>
            {
                IWebElement fieldContainer = null;

                if (formContextType == FormContextType.QuickCreate)
                {
                    // Initialize the quick create form context
                    // If this is not done -- element input will go to the main form due to new flyout design
                    var formContext = driver.WaitUntilAvailable(By.XPath(AppElements.Xpath[AppReference.QuickCreate.QuickCreateFormContext]));
                    fieldContainer = formContext.WaitUntilAvailable(MultiSelectElementsLocators.DivContainer(option.Name));
                }
                else if (formContextType == FormContextType.Entity)
                {
                    // Initialize the entity form context
                    var formContext = driver.WaitUntilAvailable(EntityElementsLocators.FormContext);
                    fieldContainer = formContext.WaitUntilAvailable(MultiSelectElementsLocators.DivContainer(option.Name));
                }
                else if (formContextType == FormContextType.BusinessProcessFlow)
                {
                    // Initialize the Business Process Flow context
                    var formContext = driver.WaitUntilAvailable(BusinessProcessFlowElementsLocators.BusinessProcessFlowFormContext);
                    fieldContainer = formContext.WaitUntilAvailable(MultiSelectElementsLocators.DivContainer(option.Name));
                }
                else if (formContextType == FormContextType.Header)
                {
                    // Initialize the Header context
                    var formContext = driver.WaitUntilAvailable(EntityElementsLocators.HeaderContext);
                    fieldContainer = formContext.WaitUntilAvailable(MultiSelectElementsLocators.DivContainer(option.Name));
                }
                else if (formContextType == FormContextType.Dialog)
                {
                    // Initialize the Header context
                    var formContext = driver.WaitUntilAvailable(By.XPath(AppElements.Xpath[AppReference.Dialogs.DialogContext]));
                    fieldContainer = formContext.WaitUntilAvailable(MultiSelectElementsLocators.DivContainer(option.Name));
                }

                var inputXPath = MultiSelectElementsLocators.InputSearch;
                fieldContainer.FindElement(inputXPath).SendKeys(string.Empty);

                var flyoutCaretXPath = MultiSelectElementsLocators.FlyoutCaret;
                fieldContainer.FindElement(flyoutCaretXPath).Click();

                foreach (var optionValue in option.Values)
                {
                    var flyoutOptionXPath = MultiSelectElementsLocators.FlyoutOption(optionValue);
                    if (fieldContainer.TryFindElement(flyoutOptionXPath, out var flyoutOption))
                    {
                        var ariaSelected = flyoutOption.GetAttribute<string>("aria-selected");
                        var selected = !string.IsNullOrEmpty(ariaSelected) && bool.Parse(ariaSelected);

                        if (!selected)
                        {
                            flyoutOption.Click();
                        }
                    }
                }

                return true;
            });
        }

        private void ClearFieldValue(IWebElement field)
        {
            if (field.GetAttribute("value").Length > 0)
            {
                field.SendKeys(Keys.Control + "a");
                field.SendKeys(Keys.Backspace);
            }

            Client.ThinkTime(500);
        }

        /// <summary>
        /// Removes the value from the multselect type control
        /// </summary>
        /// <param name="option">Object of type MultiValueOptionSet containing name of the Field and the values to be removed</param>
        /// <returns></returns>
        private BrowserCommandResult<bool> RemoveMultiOptions(MultiValueOptionSet option, FormContextType formContextType)
        {
            return this.Client.Execute(Client.GetOptions($"Remove Multi Select Value: {option.Name}"), driver =>
            {
                IWebElement fieldContainer = null;

                if (formContextType == FormContextType.QuickCreate)
                {
                    // Initialize the quick create form context
                    // If this is not done -- element input will go to the main form due to new flyout design
                    var formContext = driver.WaitUntilAvailable(By.XPath(AppElements.Xpath[AppReference.QuickCreate.QuickCreateFormContext]));
                    fieldContainer = formContext.WaitUntilAvailable(MultiSelectElementsLocators.DivContainer(option.Name));
                }
                else if (formContextType == FormContextType.Entity)
                {
                    // Initialize the entity form context
                    var formContext = driver.WaitUntilAvailable(EntityElementsLocators.FormContext);
                    fieldContainer = formContext.WaitUntilAvailable(MultiSelectElementsLocators.DivContainer(option.Name));
                }
                else if (formContextType == FormContextType.BusinessProcessFlow)
                {
                    // Initialize the Business Process Flow context
                    var formContext = driver.WaitUntilAvailable(BusinessProcessFlowElementsLocators.BusinessProcessFlowFormContext);
                    fieldContainer = formContext.WaitUntilAvailable(MultiSelectElementsLocators.DivContainer(option.Name));
                }
                else if (formContextType == FormContextType.Header)
                {
                    // Initialize the Header context
                    var formContext = driver.WaitUntilAvailable(EntityElementsLocators.HeaderContext);
                    fieldContainer = formContext.WaitUntilAvailable(MultiSelectElementsLocators.DivContainer(option.Name));
                }
                else if (formContextType == FormContextType.Dialog)
                {
                    // Initialize the Header context
                    var formContext = driver.WaitUntilAvailable(By.XPath(AppElements.Xpath[AppReference.Dialogs.DialogContext]));
                    fieldContainer = formContext.WaitUntilAvailable(MultiSelectElementsLocators.DivContainer(option.Name));
                }

                fieldContainer.Hover(driver, true);

                var selectedRecordXPath = MultiSelectElementsLocators.SelectedRecord;
                //change to .//li
                var selectedRecords = fieldContainer.FindElements(selectedRecordXPath);

                var initialCountOfSelectedOptions = selectedRecords.Count;
                var deleteButtonXpath = MultiSelectElementsLocators.SelectedOptionDeleteButton;
                //button[contains(@data-id, 'delete')]
                for (int i = 0; i < initialCountOfSelectedOptions; i++)
                {
                    // With every click of the button, the underlying DOM changes and the
                    // entire collection becomes stale, hence we only click the first occurance of
                    // the button and loop back to again find the elements and anyother occurance
                    selectedRecords[0].FindElement(deleteButtonXpath).Click(true);
                    driver.WaitForTransaction();
                    selectedRecords = fieldContainer.FindElements(selectedRecordXPath);
                }

                return true;
            });
        }

        private void SetInputValue(IWebDriver driver, IWebElement input, string value, TimeSpan? thinktime = null)
        {
            // Repeat set value if expected value is not set
            // Do this to ensure that the static placeholder '---' is removed 
            driver.RepeatUntil(() =>
            {
                input.Clear();
                input.Click();
                input.SendKeys(Keys.Control + "a");
                input.SendKeys(Keys.Control + "a");
                input.SendKeys(Keys.Backspace);
                input.SendKeys(value);
                driver.WaitForTransaction();
            },
                d => input.GetAttribute("value").IsValueEqualsTo(value),
                TimeSpan.FromSeconds(9), 3,
                failureCallback: () => throw new InvalidOperationException($"Timeout after 10 seconds. Expected: {value}. Actual: {input.GetAttribute("value")}")
            );

            driver.WaitForTransaction();
        }

        private void SetLookupByIndex(ISearchContext container, LookupItem control)
        {
            var controlName = control.Name;
            var xpathToControl = EntityElementsLocators.LookupResultsDropdown(controlName);
            var lookupResultsDialog = container.WaitUntilVisible(xpathToControl);

            var xpathFieldResultListItem = EntityElementsLocators.LookupFieldResultListItem(controlName);
            container.WaitUntil(d => d.FindElements(xpathFieldResultListItem).Count > 0);

            var items = DialogsManager.GetListItems(lookupResultsDialog, control);
            if (items.Count == 0)
                throw new InvalidOperationException($"No results exist in the Recently Viewed flyout menu. Please provide a text value for {controlName}");

            int index = control.Index;
            if (index >= items.Count)
                throw new InvalidOperationException($"Recently Viewed list does not contain {index} records. Please provide an index value less than {items.Count}");

            var selectedItem = items.ElementAt(index);
            selectedItem.Click(true);
        }

        private void SetLookUpByValue(ISearchContext container, LookupItem control)
        {
            var controlName = control.Name;
            var xpathToText = AppElements.Xpath[AppReference.Entity.LookupFieldNoRecordsText].Replace("[NAME]", controlName);
            var xpathToResultList = AppElements.Xpath[AppReference.Entity.LookupFieldResultList].Replace("[NAME]", controlName);
            var bypathResultList = By.XPath(xpathToText + "|" + xpathToResultList);

            container.WaitUntilAvailable(bypathResultList, TimeSpan.FromSeconds(10));

            var byPathToFlyout = EntityElementsLocators.TextFieldLookupMenu(controlName);
            var flyoutDialog = container.WaitUntilClickable(byPathToFlyout);

            var items = DialogsManager.GetListItems(flyoutDialog, control);

            if (items.Count == 0)
                throw new InvalidOperationException($"List does not contain a record with the name:  {control.Value}");

            int index = control.Index;
            if (index >= items.Count)
                throw new InvalidOperationException($"List does not contain {index + 1} records. Please provide an index value less than {items.Count} ");

            var selectedItem = items.ElementAt(index);
            selectedItem.Click(true);
        }

        private string TryGetValue(IWebElement fieldContainer, LookupItem control)
        {
            string[] lookupValues = TryGetValue(fieldContainer, new[] { control });
            string result = string.Join("; ", lookupValues);

            return result;
        }

        private string[] TryGetValue(IWebElement fieldContainer, LookupItem[] controls)
        {
            var controlName = controls.First().Name;
            var xpathToExistingValues = EntityElementsLocators.LookupFieldExistingValue(controlName);
            var existingValues = fieldContainer.FindElements(xpathToExistingValues);

            var xpathToExpandButton = EntityElementsLocators.LookupFieldExpandCollapseButton(controlName);
            bool expandButtonFound = fieldContainer.TryFindElement(xpathToExpandButton, out var expandButton);
            if (expandButtonFound)
            {
                expandButton.Click(true);

                int count = existingValues.Count;
                fieldContainer.WaitUntil(fc => fc.FindElements(xpathToExistingValues).Count > count);

                existingValues = fieldContainer.FindElements(xpathToExistingValues);
            }

            Exception ex = null;
            try
            {
                if (existingValues.Count > 0)
                {
                    string[] lookupValues = existingValues.Select(v => v.GetAttribute("innerText").TrimSpecialCharacters()).ToArray(); //IE can return line breaks
                    return lookupValues;
                }

                if (fieldContainer.FindElements(By.TagName("input")).Any())
                    return new string[0];
            }
            catch (Exception e)
            {
                ex = e;
            }

            throw new InvalidOperationException($"Field: {controlName} Does not exist", ex);
        }

        private void TrySetDateValue(IWebDriver driver, ISearchContext container, DateTimeControl control, FormContextType formContextType)
        {
            string controlName = control.Name;
            IWebElement fieldContainer = null;
            var xpathToInput = EntityElementsLocators.FieldControlDateTimeInputUCI(controlName);

            if (formContextType == FormContextType.QuickCreate)
            {
                // Initialize the quick create form context
                // If this is not done -- element input will go to the main form due to new flyout design
                var formContext = container.WaitUntilAvailable(By.XPath(AppElements.Xpath[AppReference.QuickCreate.QuickCreateFormContext]));
                fieldContainer = formContext.WaitUntilAvailable(xpathToInput, $"DateTime Field: '{controlName}' does not exist");

                var strExpanded = fieldContainer.GetAttribute("aria-expanded");

                if (strExpanded == null)
                {
                    fieldContainer = formContext.FindElement(EntityElementsLocators.TextFieldContainer(controlName));
                }
            }
            else if (formContextType == FormContextType.Entity)
            {
                // Initialize the entity form context
                var formContext = container.WaitUntilAvailable(EntityElementsLocators.FormContext);
                fieldContainer = formContext.WaitUntilAvailable(xpathToInput, $"DateTime Field: '{controlName}' does not exist");

                var strExpanded = fieldContainer.GetAttribute("aria-expanded");

                if (strExpanded == null)
                {
                    fieldContainer = formContext.FindElement(EntityElementsLocators.TextFieldContainer(controlName));
                }
            }
            else if (formContextType == FormContextType.BusinessProcessFlow)
            {
                // Initialize the Business Process Flow context
                var formContext = driver.WaitUntilAvailable(BusinessProcessFlowElementsLocators.BusinessProcessFlowFormContext);
                fieldContainer = formContext.WaitUntilAvailable(xpathToInput, $"DateTime Field: '{controlName}' does not exist");

                var strExpanded = fieldContainer.GetAttribute("aria-expanded");

                if (strExpanded == null)
                {
                    fieldContainer = formContext.FindElement(EntityElementsLocators.TextFieldContainer(controlName));
                }
            }
            else if (formContextType == FormContextType.Header)
            {
                // Initialize the Header context
                var formContext = driver.WaitUntilAvailable(EntityElementsLocators.HeaderContext);
                fieldContainer = formContext.WaitUntilAvailable(xpathToInput, $"DateTime Field: '{controlName}' does not exist");

                var strExpanded = fieldContainer.GetAttribute("aria-expanded");

                if (strExpanded == null)
                {
                    fieldContainer = formContext.FindElement(EntityElementsLocators.TextFieldContainer(controlName));
                }
            }
            else if (formContextType == FormContextType.Dialog)
            {
                // Initialize the Dialog context
                var formContext = driver.WaitUntilAvailable(By.XPath(AppElements.Xpath[AppReference.Dialogs.DialogContext]));
                fieldContainer = formContext.WaitUntilAvailable(xpathToInput, $"DateTime Field: '{controlName}' does not exist");

                var strExpanded = fieldContainer.GetAttribute("aria-expanded");

                if (strExpanded == null)
                {
                    fieldContainer = formContext.FindElement(EntityElementsLocators.TextFieldContainer(controlName));
                }

            }

            TrySetDateValue(driver, fieldContainer, control.DateAsString, formContextType);
        }

        private void TrySetDateValue(IWebDriver driver, IWebElement dateField, string date, FormContextType formContextType)
        {
            var strExpanded = dateField.GetAttribute("aria-expanded");

            if (strExpanded != null)
            {
                bool success = bool.TryParse(strExpanded, out var isCalendarExpanded);
                if (success && isCalendarExpanded)
                    dateField.Click(); // close calendar

                driver.RepeatUntil(() =>
                {
                    ClearFieldValue(dateField);
                    if (date != null)
                    {
                        dateField.SendKeys(date);
                        dateField.SendKeys(Keys.Tab);
                    }
                },
                d => dateField.GetAttribute("value").IsValueEqualsTo(date),
                TimeSpan.FromSeconds(9), 3,
                failureCallback: () => throw new InvalidOperationException($"Timeout after 10 seconds. Expected: {date}. Actual: {dateField.GetAttribute("value")}")
                );
            }
            else
            {
                driver.RepeatUntil(() =>
                {
                    dateField.Click(true);
                    if (date != null)
                    {
                        dateField = dateField.FindElement(By.TagName("input"));

                        // Only send Keys.Escape to avoid element not interactable exceptions with calendar flyout on forms.
                        // This can cause the Header or BPF flyouts to close unexpectedly
                        if (formContextType == FormContextType.Entity || formContextType == FormContextType.QuickCreate)
                        {
                            dateField.SendKeys(Keys.Escape);
                        }

                        ClearFieldValue(dateField);
                        dateField.SendKeys(date);
                    }
                },
                    d => dateField.GetAttribute("value").IsValueEqualsTo(date),
                    TimeSpan.FromSeconds(9), 3,
                    failureCallback: () => throw new InvalidOperationException($"Timeout after 10 seconds. Expected: {date}. Actual: {dateField.GetAttribute("value")}")
                );
            }
        }

        private bool TrySetHeaderValue(IWebDriver driver, DateTimeControl control)
        {
            var xpathToContainer = EntityElementsLocators.Header.DateTimeFieldContainer(control.Name);

            return ExecuteInHeaderContainer(driver, xpathToContainer,
                container => TrySetValue(driver, container, control, FormContextType.Header));
        }

        private void TrySetValue(IWebDriver driver, IWebElement fieldContainer, LookupItem control)
        {
            IWebElement input;
            bool found = fieldContainer.TryFindElement(By.TagName("input"), out input);
            string value = control.Value?.Trim();

            if (found)
                SetInputValue(driver, input, value);

            TrySetValue(driver, control);
        }

        private void TrySetValue(ISearchContext container, LookupItem control)
        {
            string value = control.Value;
            if (value == null)
                control.Value = string.Empty;
            // throw new InvalidOperationException($"No value has been provided for the LookupItem {control.Name}. Please provide a value or an empty string and try again.");

            if (control.Value == string.Empty)
                SetLookupByIndex(container, control);
            else
                SetLookUpByValue(container, control);
        }

        private bool TrySetValue(IWebDriver driver, ISearchContext container, DateTimeControl control, FormContextType formContext)
        {
            TrySetDateValue(driver, container, control, formContext);
            TrySetTime(driver, container, control, formContext);

            if (formContext == FormContextType.Header)
            {
                TryCloseHeaderFlyout(driver);
            }

            return true;
        }

        private void TryToSetValue(IWebDriver driver, ISearchContext fieldContainer, LookupItem[] controls)
        {
            IWebElement input;
            bool found = fieldContainer.TryFindElement(By.TagName("input"), out input);

            foreach (var control in controls)
            {
                var value = control.Value?.Trim();
                if (found)
                {
                    if (string.IsNullOrWhiteSpace(value))
                        input.Click();
                    else
                    {
                        input.SendKeys(value, true);
                        driver.WaitForTransaction();
                        Client.ThinkTime(3.Seconds());
                        input.SendKeys(Keys.Tab);
                        input.SendKeys(Keys.Enter);
                    }
                }

                TrySetValue(fieldContainer, control);
            }

            input.SendKeys(Keys.Escape); // IE wants to keep the flyout open on multi-value fields, this makes sure it closes
        }

        // Used by SetValue methods to determine the field context
        private IWebElement ValidateFormContext(IWebDriver driver, FormContextType formContextType, string field, IWebElement fieldContainer)
        {
            if (formContextType == FormContextType.QuickCreate)
            {
                // Initialize the quick create form context
                // If this is not done -- element input will go to the main form due to new flyout design
                var formContext = driver.WaitUntilAvailable(By.XPath(AppElements.Xpath[AppReference.QuickCreate.QuickCreateFormContext]));
                fieldContainer = formContext.WaitUntilAvailable(EntityElementsLocators.TextFieldContainer(field));
            }
            else if (formContextType == FormContextType.Entity)
            {
                // Initialize the entity form context
                var formContext = driver.WaitUntilAvailable(EntityElementsLocators.FormContext);
                fieldContainer = formContext.WaitUntilAvailable(EntityElementsLocators.TextFieldContainer(field));
            }
            else if (formContextType == FormContextType.BusinessProcessFlow)
            {
                // Initialize the Business Process Flow context
                var formContext = driver.WaitUntilAvailable(BusinessProcessFlowElementsLocators.BusinessProcessFlowFormContext);
                fieldContainer = formContext.WaitUntilAvailable(BusinessProcessFlowElementsLocators.TextFieldContainer(field));
            }
            else if (formContextType == FormContextType.Header)
            {
                // Initialize the Header context
                var formContext = driver.WaitUntilAvailable(EntityElementsLocators.HeaderContext);
                fieldContainer = formContext.WaitUntilAvailable(EntityElementsLocators.TextFieldContainer(field));
            }
            else if (formContextType == FormContextType.Dialog)
            {
                // Initialize the Dialog context
                driver.WaitForTransaction();
                var formContext = driver
                    .FindElements(By.XPath(AppElements.Xpath[AppReference.Dialogs.DialogContext]))
                    .LastOrDefault() ?? throw new NotFoundException("Unable to find a dialog.");
                fieldContainer = formContext.WaitUntilAvailable(EntityElementsLocators.TextFieldContainer(field));
            }

            return fieldContainer;
        }

        internal bool IsTabVisible(string tabName)
        {
            return Client.Execute<bool>(Client.GetOptions("Check If Tab Is Visible"), driver =>
            {
                return driver.IsVisible(By.CssSelector($"li[title=\"{tabName}\"]"));
            });
        }

        internal IWebElement GetFieldSectionItemContainer(string fieldName)
        {
            var result = Client.Execute(
                Client.GetOptions("Get Field Section Item Container"),
                driver =>
                {
                    return driver.TryFindElement(By.CssSelector($"div[data-id=\"{fieldName}-FieldSectionItemContainer\"]"), out var fieldContainer)
                        ? fieldContainer
                        : null;
                });

            return result?.Value;
        }

        internal IWebElement GetCurrentTab()
        {
            var result = Client.Execute(
                Client.GetOptions("Get Current Tab"),
                driver =>
                {
                    return driver.FindElement(By.XPath("//li[@role='tab' and @aria-selected='true']"));
                });

            return result.Value;
        }

        internal ICollection<IWebElement> FindSectionByLogicalName(string logicalName)
        {
            var result = Client.Execute(
                Client.GetOptions("Find Section By Logical Name"),
                driver =>
                {
                    return driver.FindElements(By.CssSelector($"*[data-id='{logicalName}']"));
                });

            return result.Value;
        }

        internal IWebElement TryToFindAssertSubgridCommand(string commandName, string subGridName)
        {
            var result = Client.Execute<IWebElement>(
                Client.GetOptions("Try To Find Assert Subgrid Command"),
                driver =>
                {
                    var subGrid = driver
                        .WaitUntilAvailable(By.XPath(AppElements.Xpath[AppReference.Entity.SubGridContents].Replace("[NAME]", subGridName)))
                        .WaitUntilAvailable(By.XPath(AppElements.Xpath[AppReference.Entity.SubGridCommandBar]));

                    var commandLocator = By.XPath(AppElements.Xpath[AppReference.Entity.SubGridCommandLabel].Replace("[NAME]", commandName));
                    return  subGrid.WaitUntilVisible(commandLocator, TimeSpan.FromSeconds(5));
                });

            return result.Value;
        }

        internal string GetFieldLogicalNameFromLabel(string label) =>
            Client.Execute(Client.GetNoRetryOptions($"Get Field Logical Name From Label: {label}"), driver => GetFieldLogicalNameFromLabel(driver, label)).Value;

        internal string ResolveFieldLogicalName(string field) =>
            Client.Execute(Client.GetNoRetryOptions($"Resolve Field Logical Name: {field}"), driver => ResolveFieldLogicalName(driver, field)).Value;

        internal bool IsFieldVisible(string fieldLogicalName) =>
            Client.Execute(Client.GetNoRetryOptions($"Is Field Visible: {fieldLogicalName}"), driver => IsFieldVisible(driver, fieldLogicalName)).Value;

        internal bool IsFieldReadOnly(string fieldLogicalName) =>
            Client.Execute(Client.GetNoRetryOptions($"Is Field Read-Only: {fieldLogicalName}"), driver => IsFieldReadOnly(driver, fieldLogicalName)).Value;

        internal bool IsFieldEmpty(string fieldLogicalName) =>
            Client.Execute(Client.GetNoRetryOptions($"Is Field Empty: {fieldLogicalName}"), driver => IsFieldEmpty(driver, fieldLogicalName)).Value;

        internal string GetFieldType(string fieldLogicalName) =>
            Client.Execute(Client.GetNoRetryOptions($"Get Field Type: {fieldLogicalName}"), driver => GetFieldType(driver, fieldLogicalName)).Value;

        internal string GetFieldLocation(string fieldLogicalName) =>
            Client.Execute(Client.GetNoRetryOptions($"Get Field Location: {fieldLogicalName}"), driver => GetFieldLocation(driver, fieldLogicalName)).Value;

        internal int GetSubGridRecordIndexById(string subGridName, Guid recordId) =>
            Client.Execute(Client.GetNoRetryOptions($"Get Subgrid Record Index: {subGridName}"), driver => GetSubGridRecordIndexById(driver, subGridName, recordId)).Value;

        internal int GetSubGridRecordIndexByLookup(string subGridName, string lookup, Guid recordId) =>
            Client.Execute(Client.GetNoRetryOptions($"Get Subgrid Record Index By Lookup: {subGridName}"), driver => GetSubGridRecordIndexByLookup(driver, subGridName, lookup, recordId)).Value;

        internal bool HighlightSubGridRecord(string subGridName, int index) =>
            Client.Execute(Client.GetNoRetryOptions($"Highlight Subgrid Record: {subGridName}"), driver => HighlightSubGridRecord(driver, subGridName, index)).Value;

        internal long GetSubGridTotalRecordCount(string subGridName) =>
            Client.Execute(Client.GetNoRetryOptions($"Get Subgrid Total Record Count: {subGridName}"), driver => GetSubGridTotalRecordCount(driver, subGridName)).Value;

        internal bool WaitForSubGridCommandBar(string subGridName)
        {
            return Client.Execute(Client.GetNoRetryOptions($"Wait For Subgrid Command Bar: {subGridName}"), driver =>
            {
                // The command bar is only rendered once the subgrid's ribbon has loaded.
                driver
                    .WaitUntilAvailable(EntityElementsLocators.SubGridContents(subGridName), $"Could not find the {subGridName} subgrid.")
                    .WaitUntilAvailable(By.XPath(AppElements.Xpath[AppReference.Entity.SubGridCommandBar]), $"Could not find the command bar of the {subGridName} subgrid.");
                return true;
            }).Value;
        }

        /// <summary>
        /// Gets the logical name of a field from the label the form shows.
        /// </summary>
        internal static string GetFieldLogicalNameFromLabel(IWebDriver driver, string label)
        {
            return driver.FindElement(EntityElementsLocators.FieldLabelContainer(label)).GetAttribute("data-id");
        }

        /// <summary>
        /// Gets the logical name of a field from its logical name or its label. Unlike <see cref="GetFieldLogicalNameFromLabel(IWebDriver, string)"/>,
        /// which reads the labels shown, it reads the form's controls, so it also finds hidden fields. A name that matches
        /// neither is returned unchanged.
        /// </summary>
        internal static string ResolveFieldLogicalName(IWebDriver driver, string field)
        {
            return (string)((IJavaScriptExecutor)driver).ExecuteScript(
                @"var field = arguments[0];
                  if (Xrm.Page.getAttribute(field)) { return field; }
                  var labelled = Xrm.Page.ui.controls.get(function (control) {
                      return typeof control.getLabel === 'function' && control.getLabel() === field
                          && typeof control.getAttribute === 'function' && control.getAttribute();
                  });
                  return labelled.length > 0 ? labelled[0].getAttribute().getName() : field;",
                field);
        }

        /// <summary>
        /// Gets a value indicating whether a field is shown on the form: a hidden field renders no container.
        /// </summary>
        internal static bool IsFieldVisible(IWebDriver driver, string fieldLogicalName)
        {
            return driver.FindElements(EntityElementsLocators.FieldContainer(fieldLogicalName)).Count > 0;
        }

        /// <summary>
        /// Gets a value indicating whether a shown field is read-only (shows a lock icon); Field.IsReadOnly does not work.
        /// </summary>
        internal static bool IsFieldReadOnly(IWebDriver driver, string fieldLogicalName)
        {
            return driver.FindElements(EntityElementsLocators.FieldLockedIcon(fieldLogicalName)).Count > 0;
        }

        /// <summary>
        /// Gets a value indicating whether a field on the form has no value, whether it is shown or hidden.
        /// </summary>
        internal static bool IsFieldEmpty(IWebDriver driver, string fieldLogicalName)
        {
            var empty = ((IJavaScriptExecutor)driver).ExecuteScript(
                @"var attribute = Xrm.Page.getAttribute(arguments[0]);
                  if (!attribute) { return null; }
                  var value = attribute.getValue();
                  return value === null || value === '' || (Array.isArray(value) && value.length === 0);",
                fieldLogicalName);

            return empty as bool? ?? throw new InvalidOperationException($"The form has no '{fieldLogicalName}' field.");
        }

        /// <summary>
        /// Gets the type of a field as the field steps tell them apart: boolean, datetime, lookup, optionset, multioptionset
        /// or text ('attribute not found' when the form has no such field).
        /// </summary>
        internal static string GetFieldType(IWebDriver driver, string fieldLogicalName)
        {
            try
            {
                return ((IJavaScriptExecutor)driver).ExecuteScript(
                    @"var attribute = Xrm.Page.getAttribute(arguments[0]);
                      if (!attribute) return 'attribute not found';
                      switch (attribute.getAttributeType()) {
                          case 'boolean': return 'boolean';
                          case 'datetime': return 'datetime';
                          case 'lookup': return 'lookup';
                          case 'optionset': return 'optionset';
                          case 'multiselectoptionset': return 'multioptionset';
                          default: return 'text';
                      }",
                    fieldLogicalName).ToString();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error when retrieving field type '{fieldLogicalName}': {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets where a field is on the form: 'field' in the body, 'header' in the header ('control not found' when the form
        /// has no such control).
        /// </summary>
        internal static string GetFieldLocation(IWebDriver driver, string fieldLogicalName)
        {
            try
            {
                return ((IJavaScriptExecutor)driver).ExecuteScript(
                    @"var control = Xrm.Page.getControl(arguments[0]);
                      if (!control) return 'control not found';
                      var header = Xrm.Page.ui.tabs.get('header_section');
                      return header && header.controls.get(arguments[0]) ? 'header' : 'field';",
                    fieldLogicalName).ToString();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error when retrieving field location '{fieldLogicalName}': {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets the index of a record in a subgrid by its id (EasyRepro's grid item ids are broken, see
        /// https://github.com/microsoft/EasyRepro/issues/800).
        /// </summary>
        /// <returns>The index, or -1 when the subgrid does not show the record.</returns>
        internal static int GetSubGridRecordIndexById(IWebDriver driver, string subGridName, Guid recordId)
        {
            driver.WaitUntilAvailable(EntityElementsLocators.SubGridContents(subGridName));

            var index = (long)((IJavaScriptExecutor)driver).ExecuteScript(
                "return Xrm.Page.getControl(arguments[0]).getGrid().getRows().get().findIndex(row => row.getData().getEntity().getId() == arguments[1]);",
                subGridName,
                recordId.ToString("B").ToUpperInvariant());

            return Convert.ToInt32(index);
        }

        /// <summary>
        /// Gets the index of the first record in a subgrid whose lookup references a record.
        /// </summary>
        /// <returns>The index, or -1 when no record of the subgrid references the record in the lookup.</returns>
        internal static int GetSubGridRecordIndexByLookup(IWebDriver driver, string subGridName, string lookup, Guid recordId)
        {
            driver.WaitUntilAvailable(EntityElementsLocators.SubGridContents(subGridName));

            var index = (long)((IJavaScriptExecutor)driver).ExecuteScript(
                @"const [subGridName, lookup, id] = arguments;
                  return Xrm.Page.getControl(subGridName).getGrid().getRows().get().findIndex(row =>
                      row.getData().getEntity().attributes.get().some(a => a.getName() === lookup && a.getValue() && a.getValue()[0].id === id));",
                subGridName,
                lookup,
                recordId.ToString("B").ToUpperInvariant());

            return Convert.ToInt32(index);
        }

        /// <summary>
        /// Selects the record at an index of a subgrid by clicking its first cell.
        /// </summary>
        internal static bool HighlightSubGridRecord(IWebDriver driver, string subGridName, int index)
        {
            var rows = driver.FindElement(EntityElementsLocators.SubGridContents(subGridName)).FindElements(EntityElementsLocators.SubGridRows);

            if (rows.Count == 0)
                throw new NoSuchElementException($"No records were found for subgrid {subGridName}");

            if (index + 1 > rows.Count)
                throw new IndexOutOfRangeException($"Subgrid {subGridName} record count: {rows.Count}. Expected: {index + 1}");

            rows[index].FindElements(EntityElementsLocators.SubGridCells)[0].Click();
            driver.WaitForTransaction();
            return true;
        }

        /// <summary>
        /// Counts all records of a subgrid; GetSubGridItemsCount fails for a count of 0
        /// (https://github.com/microsoft/EasyRepro/issues/1318).
        /// </summary>
        internal static long GetSubGridTotalRecordCount(IWebDriver driver, string subGridName)
        {
            return (long)((IJavaScriptExecutor)driver).ExecuteScript("return Xrm.Page.getControl(arguments[0]).getGrid().getTotalRecordCount();", subGridName);
        }

        internal bool? IsSubgridCommandEnabled(string commandName, string subGridName)
        {
            var label = TryToFindAssertSubgridCommand(commandName, subGridName);
            if (label == null)
                return null;

            var command = label.FindElement(EntityElementsLocators.SubGridCommandButtonOfLabel);
            return command.GetAttribute("aria-disabled") != "true" && string.IsNullOrEmpty(command.GetAttribute("disabled"));
        }

        /// <summary>
        /// Gets the flyout container element.
        /// </summary>
        /// <returns>The flyout container element.</returns>
        internal IWebElement GetFlyoutContainer()
        {
            var result = Client.Execute<IWebElement>(
                Client.GetOptions("Get Flyout Container"),
                driver =>
                {
                    return driver.WaitUntilAvailable(By.XPath(AppElements.Xpath[AppReference.Entity.SubGridOverflowContainer]));
                });

            return result.Value;
        }

        /// <summary>
        /// Waits until the specified command in the flyout is visible.
        /// </summary>
        /// <param name="commandName">The name of the command.</param>
        /// <param name="timeout">The maximum time to wait.</param>
        /// <returns>The flyout command element.</returns>
        internal IWebElement WaitUntilFlyoutCommandVisible(string commandName, TimeSpan timeout)
        {
            var container = GetFlyoutContainer();
            var commandLocator = By.XPath(AppElements.Xpath[AppReference.Entity.SubGridOverflowButton].Replace("[NAME]", commandName));

            return container.WaitUntilVisible(commandLocator, timeout, $"Could not find the {commandName} command on the flyout of the subgrid.");
        }
    }
}
