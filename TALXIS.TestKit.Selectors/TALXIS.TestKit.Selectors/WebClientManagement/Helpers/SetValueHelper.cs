using System;
using System.Collections.ObjectModel;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using TALXIS.TestKit.Selectors.DTO;
using TALXIS.TestKit.Selectors.DTO.Locators;
using TALXIS.TestKit.Selectors.Browser;
using TALXIS.TestKit.Selectors.Extentions;

namespace TALXIS.TestKit.Selectors.WebClientManagement.Helpers
{
    internal class SetValueHelper
    {
        internal static void SetMultiSelectOptionSetValue(WebClient client, MultiValueOptionSet option, FormContextType formContextType, bool removeExistingValues = false)
        {
            option = option ?? throw new ArgumentNullException(nameof(option));

            client.Execute<object>(client.GetOptions($"Set MultiValueOptionSet Value: {option.Name}"), driver =>
            {
                if (removeExistingValues)
                {
                    RemoveMultiOptions(driver, option, formContextType);
                }

                AddMultiOptions(driver, option, formContextType);

                return null;
            });
        }

        private static void RemoveMultiOptions(IWebDriver driver, MultiValueOptionSet option, FormContextType formContextType)
        {
            IWebElement fieldContainer = GetMultiSelectOptionSetFieldContainer(driver, option, formContextType);

            fieldContainer.Click();
            fieldContainer.FindElement(By.XPath(".//div[@class=\"msos-caret-container\"]")).Click();

            var selectedItems = fieldContainer.FindElements(By.XPath(".//li[contains(@class, \"msos-option-selected\")]"));

            foreach (IWebElement item in selectedItems)
            {
                item.Click();
            }
        }

        private static void AddMultiOptions(IWebDriver driver, MultiValueOptionSet option, FormContextType formContextType)
        {
            var fieldContainer = GetMultiSelectOptionSetFieldContainer(driver, option, formContextType);

            fieldContainer.Click();

            foreach (var optionValue in option.Values)
            {
                var input = fieldContainer.FindElement(By.TagName("input"));
                input.Clear();
                input.SendKeys(optionValue);

                var searchFlyout = fieldContainer.WaitUntilAvailable(By.XPath(".//div[contains(@class,\"msos-selection-container\")]//ul"));

                var searchResultList = searchFlyout.FindElements(By.XPath(".//li//label[@name=\"[NAME]msos-label\"]".Replace("[NAME]", option.Name)));

                if (searchResultList.Any(x => x.GetAttribute("title").Contains(optionValue, StringComparison.OrdinalIgnoreCase)))
                {
                    searchResultList.FirstOrDefault(x => x.GetAttribute("title").Contains(optionValue, StringComparison.OrdinalIgnoreCase)).Click(true);
                    driver.WaitForTransaction();
                }
                else
                {
                    throw new InvalidOperationException($"Option with text '{optionValue}' could not be found for '{option.Name}'");
                }
            }

            fieldContainer.FindElement(By.XPath(".//div[@class=\"msos-caret-container\"]"))
                .Click();
        }

        private static IWebElement GetMultiSelectOptionSetFieldContainer(IWebDriver driver, MultiValueOptionSet option, FormContextType formContextType)
        {
            IWebElement formContext;
            switch (formContextType)
            {
                case FormContextType.QuickCreate:
                    formContext = driver.WaitUntilAvailable(QuickCreateElementsLocators.QuickCreateFormContext);
                    return formContext.WaitUntilAvailable(MultiSelectElementsLocators.DivContainer(option.Name));
                case FormContextType.Entity:
                    formContext = driver.WaitUntilAvailable(EntityElementsLocators.FormContext);
                    return formContext.WaitUntilAvailable(MultiSelectElementsLocators.DivContainer(option.Name));
                case FormContextType.BusinessProcessFlow:
                    formContext = driver.WaitUntilAvailable(BusinessProcessFlowElementsLocators.BusinessProcessFlowFormContext);
                    return formContext.WaitUntilAvailable(MultiSelectElementsLocators.DivContainer(option.Name));
                default:
                    throw new Exception($"Mapping for FormContextType {formContextType} not configured.");
            }
        }

        internal static BrowserCommandResult<bool> SetValue(WebClient client, string fieldName, string value, string expectedTagName)
        {
            return client.Execute($"SetValue (Generic)", driver =>
            {
                var inputbox = driver.WaitUntilAvailable(By.XPath(Elements.Xpath[fieldName]));
                if (expectedTagName.Equals(inputbox.TagName, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (!inputbox.TagName.Contains("iframe", StringComparison.InvariantCultureIgnoreCase))
                    {
                        inputbox.Click(true);
                        inputbox.Clear();
                        inputbox.SendKeys(value);
                    }
                    else
                    {
                        driver.SwitchTo().Frame(inputbox);

                        driver.WaitUntilAvailable(By.TagName("iframe"));
                        driver.SwitchTo().Frame(0);

                        var inputBoxBody = driver.WaitUntilAvailable(By.TagName("body"));
                        inputBoxBody.Click(true);
                        inputBoxBody.SendKeys(value);

                        driver.SwitchTo().DefaultContent();
                    }

                    return true;
                }

                throw new InvalidOperationException($"Field: {fieldName} with tagname {expectedTagName} Does not exist");
            });
        }

        private static IWebElement ValidateFormContext(WebClient client, IWebDriver driver, FormContextType formContextType, string field, IWebElement fieldContainer)
        {
            switch (formContextType)
            {
                case FormContextType.QuickCreate:
                    // Initialize the quick create form context
                    var quickCreateContext = driver.WaitUntilAvailable(QuickCreateElementsLocators.QuickCreateFormContext);
                    fieldContainer = quickCreateContext.WaitUntilAvailable(EntityElementsLocators.TextFieldContainer(field));
                    break;
                case FormContextType.Entity:
                    // Initialize the entity form context
                    var entityContext = driver.WaitUntilAvailable(EntityElementsLocators.FormContext);
                    fieldContainer = entityContext.WaitUntilAvailable(EntityElementsLocators.TextFieldContainer(field));
                    break;
                case FormContextType.BusinessProcessFlow:
                    // Initialize the Business Process Flow context
                    var businessContext = driver.WaitUntilAvailable(BusinessProcessFlowElementsLocators.BusinessProcessFlowFormContext);
                    fieldContainer = businessContext.WaitUntilAvailable(EntityElementsLocators.TextFieldContainer(field));
                    break;
                case FormContextType.Header:
                    // Initialize the Header context
                    var headerContext = driver.WaitUntilAvailable(EntityElementsLocators.HeaderContext);
                    fieldContainer = headerContext.WaitUntilAvailable(EntityElementsLocators.TextFieldContainer(field));
                    break;
                case FormContextType.Dialog:
                    // Initialize the Dialog context
                    driver.WaitForTransaction();
                    var dialogContext = driver.FindElements(DialogsElementsLocators.DialogContext)
                        .LastOrDefault() ?? throw new NotFoundException("Unable to find a dialog.");
                    fieldContainer = dialogContext.WaitUntilAvailable(EntityElementsLocators.TextFieldContainer(field));
                    break;
                default:
                    throw new ArgumentException("Invalid form context type", nameof(formContextType));
            }

            return fieldContainer;
        }


        /// <summary>
        /// Set Value
        /// </summary>
        /// <param name="field">The field</param>
        /// <param name="value">The value</param>
        /// <example>xrmApp.Entity.SetValue("firstname", "Test");</example>
        internal static BrowserCommandResult<bool> SetTextFieldValue(WebClient client, string field = null,  string value = null, FormContextType formContextType = FormContextType.Entity, string label = null, int fieldIndex = 0 )
        {
            return client.Execute(client.GetOptions("Set Value"), driver =>
            {
                IWebElement fieldContainer = null;

                if (string.IsNullOrEmpty(field))
                {
                    fieldContainer = FindFieldContainerByLabelOrIndex(driver, label, fieldIndex);
                }
                else
                {
                    fieldContainer = ValidateFormContext(client, driver, formContextType, field, fieldContainer);
                }

                if (fieldContainer == null)
                    throw new NoSuchElementException($"Field container not found for label '{label}', index {fieldIndex}, or name '{field}'.");

                if (IsPCFField(fieldContainer))
                {
                    SetPCFValue(client, driver, fieldContainer, value);
                }
                else
                {
                    IWebElement input;

                    bool found = fieldContainer.TryFindElement(By.TagName("input"), out input) ||
                                 fieldContainer.TryFindElement(By.TagName("textarea"), out input);

                    if (!found)
                    {
                        found = fieldContainer.TryFindElement(By.TagName("iframe"), out input);
                        if (found)
                        {
                            SetValue(client, Reference.Timeline.NoteText, value, "iframe");
                            return true;
                        }
                        throw new NoSuchElementException($"Field with name '{field}' or index {fieldIndex} does not exist.");
                    }

                    SetInputValue(client, driver, input, value);
                }

                return true;
            });
        }

        /// <summary>
        /// Search for a field container by label or index
        /// </summary>
        private static IWebElement FindFieldContainerByLabelOrIndex(IWebDriver driver, string label, int fieldIndex)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            if (!string.IsNullOrEmpty(label))
            {
                // WebDriverWait ignores NoSuchElementException, so it retries until the field is there.
                return wait.Until(d => d.FindElement(FieldElementsLocators.LableSearch(label)));
            }
            else
            {
                var inputs = wait.Until(d =>
                {
                    var elements = d.FindElements(FieldElementsLocators.IndexSearch);
                    return elements.Count > 0 ? elements : null;
                });
                return inputs.ElementAtOrDefault(fieldIndex);
            }
        }

        /// <summary>
        /// Checking if the PCF field is
        /// </summary>
        private static bool IsPCFField(IWebElement fieldContainer)
        {
            var childInputs = fieldContainer.FindElements(By.TagName("input")).Count;
            return childInputs > 1 ||
                   fieldContainer.GetAttribute("class")?.Contains("pcf") == true ||
                   fieldContainer.GetAttribute("data-control-name") != null;
        }

        /// <summary>
        /// Setting the value for the PCF field
        /// </summary>
        private static void SetPCFValue(WebClient client, IWebDriver driver, IWebElement fieldContainer, string value)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            var inputs = fieldContainer.FindElements(By.TagName("input"));

            if (inputs.Count == 0)
                throw new NoSuchElementException("No input elements found in PCF field.");

            string[] parts = value.Split(' ');

            for (int i = 0; i < inputs.Count && i < parts.Length; i++)
            {
                var candidate = inputs[i];
                var input = wait.Until(_ =>
                {
                    try
                    {
                        return candidate.Displayed && candidate.Enabled ? candidate : null;
                    }
                    catch (StaleElementReferenceException)
                    {
                        return null;
                    }
                });
                SetInputValue(client, driver, input, parts[i]);

                if (i < inputs.Count - 1)
                {
                    var nextInput = inputs[i + 1];
                    nextInput.Click();
                }
            }
        }

        /// <summary>
        /// Setting the value for a standard field
        /// </summary>
        internal static void SetInputValue(WebClient client, IWebDriver driver, IWebElement input, string value, TimeSpan? thinktime = null)
        {
            driver.RepeatUntil(() =>
            {
                input.Clear();
                input.Click();
                input.SendKeys(Keys.Control + "a");
                input.SendKeys(Keys.Backspace);
                input.SendKeys(value);
                client.ThinkTime(1500);
                input.SendKeys(Keys.Enter);
                driver.WaitForTransaction();
            },
            d => input.GetAttribute("value").IsValueEqual(value),
                TimeSpan.FromSeconds(9), 3,
                failureCallback: () => throw new InvalidOperationException(
                    $"Timeout after 10 seconds. Expected: {value}. Actual: {input.GetAttribute("value")}")
            );

            driver.WaitForTransaction();
        }

        /// <summary>
        /// Sets the value of a Lookup, Customer, Owner or ActivityParty Lookup which accepts only a single value.
        /// </summary>
        /// <param name="control">The lookup field name, value or index of the lookup.</param>
        /// <example>xrmApp.Entity.SetValue(new Lookup { Name = "prrimarycontactid", Value = "Rene Valdes (sample)" });</example>
        /// The default index position is 0, which will be the first result record in the lookup results window. Suppy a value > 0 to select a different record if multiple are present.
        internal static BrowserCommandResult<bool> SetLookUp(WebClient client, LookupItem control, FormContextType formContextType)
        {
            return client.Execute(client.GetOptions($"Set Lookup Value: {control.Name}"), driver =>
            {
                driver.WaitForTransaction();

                IWebElement fieldContainer = null;
                fieldContainer = ValidateFormContext(client, driver, formContextType, control.Name, fieldContainer);

                LookupManager.TryRemoveLookupValue(driver, fieldContainer, control);

                IWebElement input;
                bool found = fieldContainer.TryFindElement(By.TagName("input"), out input);
                string value = control.Value?.Trim();
                if (found)
                {
                    TrySetSelectLookUpValue(client, driver, input, value);
                }

                return true;
            });
        }

        private static void TrySetSelectLookUpValue(WebClient client, IWebDriver driver, IWebElement input, string value)
        {
            string feeldId = input.GetAttribute("data-id");
            string fieldLogicalName = feeldId.Split('.')[0];

            driver.RepeatUntil(() =>
            {
                input.Clear();
                input.Click();
                input.SendKeys(value);
                client.ThinkTime(2500);
                input.SendKeys(Keys.Enter);
                driver.WaitForTransaction();
            },
                d =>
                {
                    string script = @"
                    function getLookupFieldValue(fieldLogicalName) {
                            var control = Xrm.Page.getControl(fieldLogicalName);

                            if (!control) return null;

                            var lookupValues = control.getAttribute().getValue();

                            if (lookupValues != null && lookupValues.length > 0) {
                                return lookupValues[0].name;
                            }

                            return null
                        }

                        return getLookupFieldValue(arguments[0]);
                    ";

                    IJavaScriptExecutor jsExecutor = (IJavaScriptExecutor)driver;
                    object result = jsExecutor.ExecuteScript(script, fieldLogicalName);

                    return result != null && result.ToString().IsValueEqualsTo(value);
                },
                TimeSpan.FromSeconds(9), 3,
                failureCallback: () => throw new InvalidOperationException($"Timeout after 10 seconds. Expected: {value}. Actual: {input.GetAttribute("value")}")
            );

            driver.WaitForTransaction();
        }

        /// <summary>
        /// Sets the value of an ActivityParty Lookup.
        /// </summary>
        /// <param name="controls">The lookup field name, value or index of the lookup.</param>
        /// <example>xrmApp.Entity.SetValue(new Lookup[] { Name = "to", Value = "Rene Valdes (sample)" }, { Name = "to", Value = "Alpine Ski House (sample)" } );</example>
        /// The default index position is 0, which will be the first result record in the lookup results window. Suppy a value > 0 to select a different record if multiple are present.
        internal static BrowserCommandResult<bool> SetValue(WebClient client, LookupItem[] controls, FormContextType formContextType = FormContextType.Entity, bool clearFirst = true)
        {
            var control = controls.First();
            var controlName = control.Name;
            return client.Execute(client.GetOptions($"Set ActivityParty Lookup Value: {controlName}"), driver =>
            {
                driver.WaitForTransaction();

                IWebElement fieldContainer = null;
                fieldContainer = ValidateFormContext(client, driver, formContextType, controlName, fieldContainer);

                if (clearFirst)
                    LookupManager.TryRemoveLookupValue(driver, fieldContainer, control);

                TryToSetMultiLookupValue(client, driver, fieldContainer, controls);

                return true;
            });
        }

        private static void TryToSetMultiLookupValue(WebClient client, IWebDriver driver, ISearchContext fieldContainer, LookupItem[] controls)
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
                        client.ThinkTime(3.Seconds());
                        input.SendKeys(Keys.Tab);
                        input.SendKeys(Keys.Enter);
                    }
                }

                TrySetValue(client, fieldContainer, control);
            }

            input.SendKeys(Keys.Escape); // IE wants to keep the flyout open on multi-value fields, this makes sure it closes
        }

        private static void TrySetValue(WebClient client, ISearchContext container, LookupItem control)
        {
            string value = control.Value;
            if (value == null)
                control.Value = string.Empty;

            if (control.Value == string.Empty)
                SetLookupByIndex(container, control);
            else
                SetLookUpByValue(client, container, control);
        }

        private static void SetLookUpByValue(WebClient client, ISearchContext container, LookupItem control)
        {
            var controlName = control.Name;
            var xpathToText = AppElements.Xpath[AppReference.Entity.LookupFieldNoRecordsText].Replace("[NAME]", controlName);
            var xpathToResultList = AppElements.Xpath[AppReference.Entity.LookupFieldResultList].Replace("[NAME]", controlName);
            var bypathResultList = By.XPath(xpathToText + "|" + xpathToResultList);

            container.WaitUntilAvailable(bypathResultList, TimeSpan.FromSeconds(10));

            var byPathToFlyout = EntityElementsLocators.TextFieldLookupMenu(controlName);
            var flyoutDialog = container.WaitUntilClickable(byPathToFlyout);

            var items = GetDataHelper.GetListItems(flyoutDialog, control);

            if (items.Count == 0)
                throw new InvalidOperationException($"List does not contain a record with the name:  {control.Value}");

            int index = control.Index;
            if (index >= items.Count)
                throw new InvalidOperationException($"List does not contain {index + 1} records. Please provide an index value less than {items.Count} ");

            var selectedItem = items.ElementAt(index);
            selectedItem.Click(true);
        }

        private static void SetLookupByIndex(ISearchContext container, LookupItem control)
        {
            var controlName = control.Name;
            var xpathToControl = By.XPath(AppElements.Xpath[AppReference.Entity.LookupResultsDropdown].Replace("[NAME]", controlName));
            var lookupResultsDialog = container.WaitUntilVisible(xpathToControl);

            var xpathFieldResultListItem = EntityElementsLocators.LookupFieldResultListItem(controlName);
            container.WaitUntil(d => d.FindElements(xpathFieldResultListItem).Count > 0);

            var items = GetDataHelper.GetListItems(lookupResultsDialog, control);
            if (items.Count == 0)
                throw new InvalidOperationException($"No results exist in the Recently Viewed flyout menu. Please provide a text value for {controlName}");

            int index = control.Index;
            if (index >= items.Count)
                throw new InvalidOperationException($"Recently Viewed list does not contain {index} records. Please provide an index value less than {items.Count}");

            var selectedItem = items.ElementAt(index);
            selectedItem.Click(true);
        }

        /// <summary>
        /// Sets the value of a picklist or status field.
        /// </summary>
        /// <param name="control">The option you want to set.</param>
        /// <example>xrmApp.Entity.SetValue(new OptionSet { Name = "preferredcontactmethodcode", Value = "Email" });</example>
        public static BrowserCommandResult<bool> SetOptionSetValue(WebClient client, OptionSet control, FormContextType formContextType)
        {
            var controlName = control.Name;
            return client.Execute(client.GetOptions($"Set OptionSet Value: {controlName}"), driver =>
            {
                IWebElement fieldContainer = null;
                fieldContainer = ValidateFormContext(client, driver, formContextType, controlName, fieldContainer);

                TrySetOptionSetValue(client, fieldContainer, control);
                driver.WaitForTransaction();
                return true;
            });
        }

        private static void TrySetOptionSetValue(WebClient client, IWebElement fieldContainer, OptionSet control)
        {
            var value = control.Value;
            bool success = fieldContainer.TryFindElement(By.Name("select"), out IWebElement select);

            if (success)
            {
                fieldContainer.WaitUntilAvailable(By.TagName("select"));
                var options = select.FindElements(By.TagName("option"));

                SelectOption(client, options, value);

                return;
            }

            var name = control.Name;
            var dropdown = FluentDropdownHelper.FindButton(fieldContainer, name);
            if (dropdown != null)
            {
                // Clearing a choice sets "-1": the dropdown's no-value option clears it.
                FluentDropdownHelper.SelectOption(client.Browser.Driver, dropdown, value == "-1" ? null : value);
                return;
            }

            var hasStatusCombo = fieldContainer.HasElement(EntityElementsLocators.EntityNewLookOptionsetStatusCombo(name));

            if (hasStatusCombo)
            {
                // This is for statuscode (type = status) that should act like an optionset doesn't doesn't follow the same pattern when rendered
                fieldContainer.ClickWhenAvailable(EntityElementsLocators.EntityNewLookOptionsetStatusComboButton(name));

                var listBox = fieldContainer.FindElement(EntityElementsLocators.EntityNewLookOptionsetStatusComboList(name));

                var options = listBox.FindElements(By.CssSelector(".fui-Option"));

                SelectOption(client, options, value);

                return;
            }

            throw new InvalidOperationException($"OptionSet Field: '{name}' does not exist");
        }

        private static void SelectOption(WebClient client, ReadOnlyCollection<IWebElement> options, string value)
        {
            var selectedOption = options.FirstOrDefault(op => op.Text == value || op.GetAttribute("value") == value)
                ?? throw new InvalidOperationException($"No option '{value}' to select.");
            client.ThinkTime(1500);
            selectedOption.Click(true);
            client.ThinkTime(1500);
        }

        /// <summary>
        /// Sets the value of a Boolean Item.
        /// </summary>
        /// <param name="option">The boolean field name.</param>
        /// <example>xrmApp.Entity.SetValue(new BooleanItem { Name = "donotemail", Value = true });</example>
        public static BrowserCommandResult<bool> SetBooleanValue(WebClient client, BooleanItem option, FormContextType formContextType)
        {
            return client.Execute(client.GetOptions($"Set BooleanItem Value: {option.Name}"), driver =>
            {
                // ensure that the option.Name value is lowercase -- will cause XPath lookup issues
                option.Name = option.Name.ToLowerInvariant();

                IWebElement fieldContainer = null;
                fieldContainer = ValidateFormContext(client, driver, formContextType, option.Name, fieldContainer);

                var dropdown = FluentDropdownHelper.FindButton(fieldContainer, option.Name);
                if (dropdown != null)
                {
                    FluentDropdownHelper.SetTwoOptionValue(driver, dropdown, option.Value);
                    driver.WaitForTransaction();
                    return true;
                }

                var hasRadio = fieldContainer.HasElement(EntityElementsLocators.EntityBooleanFieldRadioContainer(option.Name));
                var hasCheckbox = fieldContainer.HasElement(EntityElementsLocators.EntityBooleanFieldCheckbox(option.Name));
                var hasList = fieldContainer.HasElement(EntityElementsLocators.EntityBooleanFieldList(option.Name));
                var hasToggle = fieldContainer.HasElement(EntityElementsLocators.EntityBooleanFieldToggle(option.Name));
                var hasFlipSwitch = fieldContainer.HasElement(EntityElementsLocators.EntityBooleanFieldFlipSwitchLink(option.Name));

                // Need to validate whether control is FlipSwitch or Button
                IWebElement flipSwitchContainer = null;
                var flipSwitch = hasFlipSwitch ? fieldContainer.TryFindElement(EntityElementsLocators.EntityBooleanFieldFlipSwitchContainer(option.Name), out flipSwitchContainer) : false;
                var hasButton = flipSwitchContainer != null ? flipSwitchContainer.HasElement(EntityElementsLocators.EntityBooleanFieldButtonTrue) : false;
                hasFlipSwitch = hasButton ? false : hasFlipSwitch; //flipSwitch and button have the same container reference, so if it has a button it is not a flipSwitch
                hasFlipSwitch = hasToggle ? false : hasFlipSwitch; //flipSwitch and Toggle have the same container reference, so if it has a Toggle it is not a flipSwitch

                if (hasRadio)
                {
                    var trueRadio = fieldContainer.FindElement(EntityElementsLocators.EntityBooleanFieldRadioTrue(option.Name));
                    var falseRadio = fieldContainer.FindElement(EntityElementsLocators.EntityBooleanFieldRadioFalse(option.Name));

                    if (option.Value && bool.Parse(falseRadio.GetAttribute("aria-checked")) || !option.Value && bool.Parse(trueRadio.GetAttribute("aria-checked")))
                    {
                        driver.ClickWhenAvailable(EntityElementsLocators.EntityBooleanFieldRadioContainer(option.Name));
                    }
                }
                else if (hasCheckbox)
                {
                    var checkbox = fieldContainer.FindElement(EntityElementsLocators.EntityBooleanFieldCheckbox(option.Name));

                    if (option.Value && !checkbox.Selected || !option.Value && checkbox.Selected)
                    {
                        driver.ClickWhenAvailable(EntityElementsLocators.EntityBooleanFieldCheckboxContainer(option.Name));
                    }
                }
                else if (hasList)
                {
                    var list = fieldContainer.FindElement(EntityElementsLocators.EntityBooleanFieldList(option.Name));
                    var options = list.FindElements(By.TagName("option"));
                    var selectedOption = options.FirstOrDefault(a => a.HasAttribute("data-selected") && bool.Parse(a.GetAttribute("data-selected")));
                    var unselectedOption = options.FirstOrDefault(a => !a.HasAttribute("data-selected"));

                    var trueOptionSelected = false;
                    if (selectedOption != null)
                    {
                        trueOptionSelected = selectedOption.GetAttribute("value") == "1";
                    }

                    if (option.Value && !trueOptionSelected || !option.Value && trueOptionSelected)
                    {
                        if (unselectedOption != null)
                        {
                            driver.ClickWhenAvailable(By.Id(unselectedOption.GetAttribute("id")));
                        }
                    }
                }
                else if (hasToggle)
                {
                    var toggle = fieldContainer.FindElement(EntityElementsLocators.EntityBooleanFieldToggle(option.Name));
                    var link = toggle.FindElement(By.TagName("button"));
                    var value = bool.Parse(link.GetAttribute("aria-checked"));

                    if (value != option.Value)
                    {
                        link.Click();
                    }
                }
                else if (hasFlipSwitch)
                {
                    // flipSwitchContainer should exist based on earlier TryFindElement logic
                    var link = flipSwitchContainer.FindElement(By.TagName("a"));
                    var value = bool.Parse(link.GetAttribute("aria-checked"));

                    if (value != option.Value)
                    {
                        link.Click();
                    }
                }
                else if (hasButton)
                {
                    var container = fieldContainer.FindElement(EntityElementsLocators.EntityBooleanFieldButtonContainer(option.Name));
                    var trueButton = container.FindElement(EntityElementsLocators.EntityBooleanFieldButtonTrue);
                    var falseButton = container.FindElement(EntityElementsLocators.EntityBooleanFieldButtonFalse);

                    if (option.Value)
                    {
                        trueButton.Click();
                    }
                    else
                    {
                        falseButton.Click();
                    }
                }
                else
                    throw new InvalidOperationException($"Field: {option.Name} Does not exist");


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
        public static BrowserCommandResult<bool> SetDateTimeValue(WebClient client, string field, DateTime value, FormContextType formContext, string formatDate = null, string formatTime = null)
        {
            var control = new DateTimeControl(field)
            {
                Value = value,
                DateFormat = formatDate,
                TimeFormat = formatTime
            };
            return SetValue(client, control, formContext);
        }

        public static BrowserCommandResult<bool> SetValue(WebClient client, DateTimeControl control, FormContextType formContext)
            => client.Execute(client.GetOptions($"Set Date/Time Value: {control.Name}"),
                driver => TrySetValue(client, driver, container: driver, control: control, formContext));

        private static bool TrySetValue(WebClient client, IWebDriver driver, ISearchContext container, DateTimeControl control, FormContextType formContext)
        {
            TrySetDateValue(client, driver, container, control, formContext);
            TrySetTime(client, driver, container, control, formContext);

            if (formContext == FormContextType.Header)
            {
                EntityManager.TryCloseHeaderFlyout(driver);
            }

            return true;
        }

        private static void TrySetDateValue(WebClient client, IWebDriver driver, ISearchContext container, DateTimeControl control, FormContextType formContextType)
        {
            string controlName = control.Name;
            IWebElement fieldContainer = null;
            var xpathToInput = EntityElementsLocators.FieldControlDateTimeInputUCI(controlName);

            if (formContextType == FormContextType.QuickCreate)
            {
                // Initialize the quick create form context
                // If this is not done -- element input will go to the main form due to new flyout design
                var formContext = container.WaitUntilAvailable(QuickCreateElementsLocators.QuickCreateFormContext);
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

            TrySetDateValue(client, driver, fieldContainer, control.DateAsString, formContextType);
        }

        private static void TrySetDateValue(WebClient client, IWebDriver driver, IWebElement dateField, string date, FormContextType formContextType)
        {
            var strExpanded = dateField.GetAttribute("aria-expanded");

            if (strExpanded != null)
            {
                bool success = bool.TryParse(strExpanded, out var isCalendarExpanded);
                if (success && isCalendarExpanded)

                driver.RepeatUntil(() =>
                {
                    ClearFieldValue(client, dateField);
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

                        ClearFieldValue(client, dateField);
                        dateField.SendKeys(date);
                    }
                },
                    d => dateField.GetAttribute("value").IsValueEqualsTo(date),
                    TimeSpan.FromSeconds(9), 3,
                    failureCallback: () => throw new InvalidOperationException($"Timeout after 10 seconds. Expected: {date}. Actual: {dateField.GetAttribute("value")}")
                );
            }
        }

        private static void ClearFieldValue(WebClient client, IWebElement field)
        {
            if (field.GetAttribute("value").Length > 0)
            {
                field.SendKeys(Keys.Control + "a");
                field.SendKeys(Keys.Backspace);
            }

            client.ThinkTime(500);
        }

        private static void TrySetTime(WebClient client, IWebDriver driver, ISearchContext container, DateTimeControl control, FormContextType formContextType)
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
                TrySetTime(client, driver, timeField, control.TimeAsString);
            }
        }

        private static void TrySetTime(WebClient client, IWebDriver driver, IWebElement timeField, string time)
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
    }
}
