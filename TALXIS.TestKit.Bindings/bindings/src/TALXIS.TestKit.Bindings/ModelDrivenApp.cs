namespace TALXIS.TestKit.Bindings
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Security;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Support.UI;
    using TALXIS.TestKit.Bindings.Configuration;
    using TALXIS.TestKit.Bindings.Extensions;
    using TALXIS.TestKit.Bindings.Steps;
    using TALXIS.TestKit.Selectors;
    using TALXIS.TestKit.Selectors.Browser;

    /// <summary>
    /// A model-driven app, as the steps that act on either kind of app see it: <see cref="IAppUnderTest"/> over EasyRepro's
    /// <see cref="XrmApp"/> and the testkit's model-driven extensions.
    /// </summary>
    internal sealed class ModelDrivenApp : PowerAppsStepDefiner, IAppUnderTest
    {
        private const string AppName = "a model-driven app";

        // Dialogs open and close asynchronously, and WaitForTransaction does not cover them.
        private static readonly TimeSpan DialogStateTimeout = TimeSpan.FromSeconds(10);

        /// <inheritdoc/>
        public ILogin Login { get; } = new ModelDrivenLogin();

        /// <inheritdoc/>
        public Selectors.INavigation Navigation { get; } = new ModelDrivenNavigation();

        /// <inheritdoc/>
        public IEntity Entity { get; } = new ModelDrivenEntity();

        /// <inheritdoc/>
        public ISubGrid SubGrid { get; } = new ModelDrivenSubGrid();

        /// <inheritdoc/>
        public IDialogs Dialogs { get; } = new ModelDrivenDialogs();

        /// <inheritdoc/>
        public IDatasetGrid DatasetGrid { get; } = new ModelDrivenDatasetGrid();

        private static NotSupportedException NotSupported(string operation) =>
            new NotSupportedException($"{operation} is only supported in a TALXIS Portal, not in {AppName}.");

        // Reads a field as its type shows it and compares it with a value as the field steps take it.
        private static bool HasValue(string fieldLogicalName, string expected, bool inHeader, out string actual)
        {
            switch (MetadataHelper.GetFieldTypeFromDomByLogicalName(fieldLogicalName))
            {
                case "optionset":
                    var optionSet = new OptionSet { Name = fieldLogicalName };
                    actual = inHeader ? XrmApp.Entity.GetHeaderValue(optionSet) : XrmApp.Entity.GetValue(optionSet);
                    return actual == expected;
                case "multioptionset":
                    var values = XrmApp.Entity.GetValue(new MultiValueOptionSet { Name = fieldLogicalName }).Values;
                    actual = string.Join(", ", values);
                    return values.SequenceEqual(expected.Split(',').Select(v => v.Trim()));
                case "lookup":
                    var lookup = new LookupItem { Name = fieldLogicalName };
                    actual = inHeader ? XrmApp.Entity.GetHeaderValue(lookup) : XrmApp.Entity.GetValue(lookup);
                    return actual == expected;
                case "datetime":
                    var dateTimeControl = new DateTimeControl(fieldLogicalName);
                    var dateTime = inHeader ? XrmApp.Entity.GetHeaderValue(dateTimeControl) : XrmApp.Entity.GetValue(dateTimeControl);
                    var expectedDateTime = DateTime.Parse(expected, CultureInfo.CurrentCulture);
                    actual = dateTime?.ToString(CultureInfo.CurrentCulture);

                    // A value without a time is compared by its date.
                    return expected.Contains(':', StringComparison.Ordinal) ? dateTime == expectedDateTime : dateTime?.Date == expectedDateTime.Date;
                case "boolean":
                    var boolean = new BooleanItem { Name = fieldLogicalName };
                    var booleanValue = inHeader ? XrmApp.Entity.GetHeaderValue(boolean) : XrmApp.Entity.GetValue(boolean);
                    actual = booleanValue.ToString(CultureInfo.InvariantCulture);
                    return booleanValue == bool.Parse(expected);
                default:
                    actual = inHeader ? XrmApp.Entity.GetHeaderValue(fieldLogicalName) : XrmApp.Entity.GetValue(fieldLogicalName);
                    return actual == expected;
            }
        }

        private sealed class ModelDrivenLogin : ILogin
        {
            public void LogIn(Uri url, SecureString username, SecureString password, SecureString otpToken = null)
            {
                if (!url.Query.Contains("appid", StringComparison.Ordinal))
                {
                    throw new InvalidOperationException("Logging in without naming the app needs the app's appid in the test URL.");
                }

                LoginSteps.LogIn(
                    url,
                    new UserConfiguration { Username = username.ToUnsecureString(), Password = password?.ToUnsecureString(), OtpToken = otpToken?.ToUnsecureString() },
                    appName: null);
            }
        }

        private sealed class ModelDrivenNavigation : Selectors.INavigation
        {
            public void OpenPage(string label) => XrmApp.Navigation.OpenSubArea(label);

            public bool IsSubAreaShown(string label, bool shown) => shown
                ? XrmApp.Navigation.GetSubAreaElement(label) is { Displayed: true }
                : !XrmApp.Navigation.IsSubAreaVisible(label);

            public bool IsSubAreaDisabled(string label, string message) => throw NotSupported("Disabling a navigation item");
        }

        private sealed class ModelDrivenEntity : IEntity
        {
            public void SelectTab(string tab) => XrmApp.Entity.SelectTab(tab);

            public bool IsTabShown(string tab, bool shown) => XrmApp.Entity.IsTabVisible(tab) == shown;

            public bool IsTabSelected(string tab) => XrmApp.Entity.GetAttributeOfActiveTab("aria-label") == tab;

            public void SetValue(string field, string value)
            {
                Driver.WaitForPageToLoad();

                var fieldLogicalName = XrmApp.Entity.GetFieldLogicalNameFromLabel(Driver, field);
                var fieldType = MetadataHelper.GetFieldTypeFromDomByLogicalName(fieldLogicalName);

                // !TO-DO: WI:35121 - Remove fieldLocation
                if (MetadataHelper.GetFieldLocationFromDomByLogicalName(fieldLogicalName) == "field")
                {
                    SetFieldValue(fieldLogicalName, value, fieldType);
                }
                else
                {
                    SetHeaderFieldValue(field, value, fieldType);
                }

                Client.TryLoseFocus();
                Driver.WaitForTransaction();
            }

            public bool HasValue(string field, string value, out string actual)
            {
                var fieldLogicalName = XrmApp.Entity.GetFieldLogicalNameFromLabel(Driver, field);
                var inHeader = MetadataHelper.GetFieldLocationFromDomByLogicalName(fieldLogicalName) != "field";
                return ModelDrivenApp.HasValue(fieldLogicalName, value, inHeader, out actual);
            }

            public bool IsFieldShown(string field, bool shown) =>
                XrmApp.Entity.IsFieldVisible(Driver, XrmApp.Entity.ResolveFieldLogicalName(Driver, field)) == shown;

            public bool IsFieldRequired(string field, bool required) =>
                XrmApp.Entity.GetFieldSectionItemContainertAttribute<int>(field, "data-fieldrequirement") == (required ? 2 : 0);

            public bool IsFieldEditable(string field, bool editable)
            {
                var control = XrmApp.Entity.GetField(XrmApp.Entity.ResolveFieldLogicalName(Driver, field));

                // A hidden field cannot be read as read-only.
                return editable ? control.IsVisible && !control.IsReadOnly(Driver) : control.IsReadOnly(Driver);
            }

            public bool IsFieldEmpty(string field) => XrmApp.Entity.IsFieldEmpty(Driver, XrmApp.Entity.ResolveFieldLogicalName(Driver, field));

            private static void SetFieldValue(string fieldName, string fieldValue, string fieldType)
            {
                switch (fieldType)
                {
                    case "multioptionset":
                        XrmApp.Entity.SetValue(new MultiValueOptionSet { Name = fieldName, Values = fieldValue.Split(',').Select(v => v.Trim()).ToArray() }, true);
                        break;
                    case "optionset":
                        XrmApp.Entity.SetValue(new OptionSet { Name = fieldName, Value = fieldValue });
                        break;
                    case "boolean":
                        XrmApp.Entity.SetValue(new BooleanItem { Name = fieldName, Value = bool.Parse(fieldValue) });
                        break;
                    case "datetime":
                        // !TO-DO: Datetime formattings
                        XrmApp.Entity.SetValue(new DateTimeControl(fieldName) { Value = DateTime.Parse(fieldValue, CultureInfo.CurrentCulture) });
                        break;
                    case "lookup":
                        XrmApp.Entity.SetValue(new LookupItem { Name = fieldName, Value = fieldValue });
                        break;
                    default:
                        XrmApp.Entity.SetValue(fieldName, fieldValue);
                        break;
                }
            }

            private static void SetHeaderFieldValue(string fieldName, string fieldValue, string fieldType)
            {
                switch (fieldType)
                {
                    case "optionset":
                        XrmApp.Entity.SetHeaderValue(new OptionSet { Name = fieldName, Value = fieldValue });
                        break;
                    case "boolean":
                        XrmApp.Entity.SetHeaderValue(new BooleanItem { Name = fieldName, Value = bool.Parse(fieldValue) });
                        break;
                    case "datetime":
                        XrmApp.Entity.SetHeaderValue(new DateTimeControl(fieldName) { Value = DateTime.Parse(fieldValue, CultureInfo.CurrentCulture) });
                        break;
                    case "lookup":
                        XrmApp.Entity.SetHeaderValue(new LookupItem { Name = fieldName, Value = fieldValue });
                        break;
                    default:
                        XrmApp.Entity.SetHeaderValue(fieldName, fieldValue);
                        break;
                }
            }
        }

        private sealed class ModelDrivenSubGrid : ISubGrid
        {
            public void ClickCommand(string subGrid, string command)
            {
                // The command bar is only rendered once the subgrid's ribbon has loaded.
                Driver
                    .WaitUntilAvailable(
                        By.XPath(AppElements.Xpath[AppReference.Entity.SubGridContents].Replace("[NAME]", subGrid)),
                        $"Could not find the {subGrid} subgrid.")
                    .WaitUntilAvailable(
                        By.XPath(AppElements.Xpath[AppReference.Entity.SubGridCommandBar]),
                        $"Could not find the command bar of the {subGrid} subgrid.");

                XrmApp.Entity.SubGrid.ClickCommand(subGrid, command);
            }

            public void ClickRowCommand(string subGrid, Guid recordId, string command) => throw NotSupported("Clicking a command on a record's row");

            public void ClickRowCommand(string subGrid, string column, string value, string command) => throw NotSupported("Clicking a command on a record's row");

            public void SelectRecord(string subGrid, Guid recordId)
            {
                var index = XrmApp.Entity.SubGrid.GetRecordIndexById(subGrid, Driver, recordId);
                if (index < 0)
                {
                    throw new NoSuchElementException($"The record {recordId} is not in the '{subGrid}' subgrid.");
                }

                XrmApp.Entity.SubGrid.HighlightRecord(subGrid, Driver, index);
            }

            public void SelectRecord(string subGrid, string column, string value)
            {
                var items = XrmApp.Entity.SubGrid.GetSubGridItems(subGrid);
                var item = items.FirstOrDefault(i => i.GetAttribute<string>(column) == value)
                    ?? throw new NoSuchElementException($"Could not find record with field '{column}' with value '{value}' in the '{subGrid}' subgrid.");

                XrmApp.Entity.SubGrid.HighlightRecord(subGrid, Driver, items.IndexOf(item));
            }

            public bool ContainsRecord(string subGrid, Guid recordId, bool contained) =>
                XrmApp.Entity.SubGrid.GetRecordIndexById(subGrid, Driver, recordId) > -1 == contained;

            public bool ContainsRecord(string subGrid, string column, string value) =>
                XrmApp.Entity.SubGrid.GetSubGridItems(subGrid).Any(item => item.GetAttribute<string>(column) == value);

            // Until XrmApp.Entity.SubGrid.GetSubGridItemsCount(subGridName) works for a count of 0
            // https://github.com/microsoft/EasyRepro/issues/1318
            public long CountRecords(string subGrid) =>
                (long)Driver.ExecuteScript($"return Xrm.Page.getControl(\"{subGrid}\").getGrid().getTotalRecordCount();");

            public bool IsCommandShown(string subGrid, string command, bool shown) =>
                XrmApp.Entity.SubGrid.TryToFindAssertSubgridCommand(command, subGrid) != null == shown;

            public bool IsCommandEnabled(string subGrid, string command, bool enabled) =>
                (XrmApp.Entity.SubGrid.IsCommandEnabled(subGrid, command)
                    ?? throw new NoSuchElementException($"The '{command}' command should be shown (enabled or disabled, not hidden) on the '{subGrid}' subgrid.")) == enabled;
        }

        private sealed class ModelDrivenDialogs : IDialogs
        {
            public bool IsDialogShown(bool shown)
            {
                try
                {
                    return new WebDriverWait(Driver, DialogStateTimeout).Until(_ => XrmApp.Dialogs.IsDialogVisible() == shown);
                }
                catch (WebDriverTimeoutException)
                {
                    return false;
                }
            }

            public bool HasTitle(string title, out string actual)
            {
                actual = XrmApp.Dialogs.GetCurrentDialogLable();
                return actual == title;
            }

            public void ClickButton(string label) => XrmApp.Dialogs.SelectButtonInDialogWindow(label);

            public bool TryClickButton(string label) => XrmApp.Dialogs.TryClickButton(label);

            public bool IsButtonShown(string label, bool shown) => XrmApp.Dialogs.IsButtonVisible(label) == shown;

            public bool IsTextShown(string text) => XrmApp.Dialogs.IsTextVisible(text);

            public void CloseWithoutSaving() => XrmApp.Dialogs.CloseDialog();

            public void ChooseOption(string option) => throw NotSupported("Choosing an option of a choice group on a dialog");

            public void SetFieldValue(string field, string value)
            {
                var fieldLogicalName = XrmApp.Entity.GetFieldLogicalNameFromLabel(Driver, field);

                switch (MetadataHelper.GetFieldTypeFromDomByLogicalName(fieldLogicalName))
                {
                    case "multioptionset":
                        XrmApp.Dialogs.SetValue(new MultiValueOptionSet { Name = fieldLogicalName, Values = value.Split(',').Select(v => v.Trim()).ToArray() }, true);
                        break;
                    case "optionset":
                        XrmApp.Dialogs.SetValue(new OptionSet { Name = fieldLogicalName, Value = value });
                        break;
                    case "boolean":
                        XrmApp.Dialogs.SetValue(new BooleanItem { Name = fieldLogicalName, Value = bool.Parse(value) });
                        break;
                    case "datetime":
                        XrmApp.Dialogs.SetValue(new DateTimeControl(fieldLogicalName) { Value = DateTime.Parse(value, CultureInfo.CurrentCulture) });
                        break;
                    case "lookup":
                        XrmApp.Dialogs.SetValue(new LookupItem { Name = fieldLogicalName, Value = value });
                        break;
                    case "currency":
                    case "numeric":
                    case "text":
                        XrmApp.Dialogs.SetValue(fieldLogicalName, value);
                        break;
                    case var fieldType:
                        throw new NotSupportedException($"Setting a '{fieldType}' field on a dialog is not supported.");
                }

                Driver.WaitForTransaction();
            }

            public bool HasFieldValue(string field, string value, out string actual) =>
                ModelDrivenApp.HasValue(XrmApp.Entity.GetFieldLogicalNameFromLabel(Driver, field), value, inHeader: false, out actual);

            public bool HasFieldError(string field, string message) => throw NotSupported("Checking a field's validation error");

            public void AnswerConfirmation(bool confirm)
            {
                XrmApp.Dialogs.ConfirmationDialog(confirm);
                XrmApp.ThinkTime(2000);
            }

            public void CloseAlert() => XrmApp.Dialogs.CloseWarningDialog();

            public bool IsAlertShown() => XrmApp.Dialogs.GetAlertDialog() != null;

            public bool HasAlertTitle(string title, out string actual)
            {
                actual = XrmApp.Dialogs.GetAlertDialogMessageText();
                return actual == title;
            }

            public bool HasConfirmationTitle(string title, out string actual)
            {
                actual = XrmApp.Dialogs.IsDialogVisible() ? XrmApp.Dialogs.GetCurrentDialogLable() : null;
                return actual == title;
            }
        }

        private sealed class ModelDrivenDatasetGrid : IDatasetGrid
        {
            public bool ContainsRecord(Guid recordId) => throw NotSupported("The virtual dataset grid");

            public void ClickRowButton(Guid recordId, string button) => throw NotSupported("The virtual dataset grid");

            public bool IsRowButtonShown(Guid recordId, string button, bool shown) => throw NotSupported("The virtual dataset grid");
        }
    }
}
