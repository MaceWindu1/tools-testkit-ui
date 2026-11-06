using System;
using System.Globalization;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using TALXIS.TestKit.Selectors.Browser;

namespace TALXIS.TestKit.Selectors
{
    /// <summary>
    /// The dialogs of a model-driven app.
    /// </summary>
    public class ModelDrivenDialogs : Element, IDialogs
    {
        // Dialogs open and close asynchronously, and WaitForTransaction does not cover them.
        private static readonly TimeSpan DialogStateTimeout = TimeSpan.FromSeconds(10);

        private readonly XrmApp _xrmApp;

        public ModelDrivenDialogs(XrmApp xrmApp) : base()
        {
            _xrmApp = xrmApp ?? throw new ArgumentNullException(nameof(xrmApp));
        }

        /// <inheritdoc/>
        public bool IsDialogShown(bool shown)
        {
            try
            {
                return new WebDriverWait(_xrmApp._client.Browser.Driver, DialogStateTimeout).Until(_ => _xrmApp.Dialogs.IsDialogVisible() == shown);
            }
            catch (WebDriverTimeoutException)
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public bool HasTitle(string title, out string actual)
        {
            actual = _xrmApp.Dialogs.GetCurrentDialogLable();
            return actual == title;
        }

        /// <inheritdoc/>
        public void ClickButton(string label) => _xrmApp.Dialogs.SelectButtonInDialogWindow(label);

        /// <inheritdoc/>
        public bool TryClickButton(string label) => _xrmApp.Dialogs.TryClickButton(label);

        /// <inheritdoc/>
        public bool IsButtonShown(string label, bool shown) => _xrmApp.Dialogs.IsButtonVisible(label) == shown;

        /// <inheritdoc/>
        public bool IsTextShown(string text) => _xrmApp.Dialogs.IsTextVisible(text);

        /// <inheritdoc/>
        public void CloseWithoutSaving() => _xrmApp.Dialogs.CloseDialog();

        /// <inheritdoc/>
        public void ChooseOption(string option) => throw ModelDrivenApp.NotSupported("Choosing an option of a choice group on a dialog");

        /// <inheritdoc/>
        public void SetFieldValue(string field, string value)
        {
            var fieldLogicalName = _xrmApp.Entity.GetFieldLogicalNameFromLabel(field);

            switch (ModelDrivenEntity.GetFieldType(_xrmApp, fieldLogicalName))
            {
                case "multioptionset":
                    _xrmApp.Dialogs.SetValue(new MultiValueOptionSet { Name = fieldLogicalName, Values = value.Split(',').Select(v => v.Trim()).ToArray() }, true);
                    break;
                case "optionset":
                    _xrmApp.Dialogs.SetValue(new OptionSet { Name = fieldLogicalName, Value = value });
                    break;
                case "boolean":
                    _xrmApp.Dialogs.SetValue(new BooleanItem { Name = fieldLogicalName, Value = bool.Parse(value) });
                    break;
                case "datetime":
                    _xrmApp.Dialogs.SetValue(new DateTimeControl(fieldLogicalName) { Value = DateTime.Parse(value, CultureInfo.CurrentCulture) });
                    break;
                case "lookup":
                    _xrmApp.Dialogs.SetValue(new LookupItem { Name = fieldLogicalName, Value = value });
                    break;
                case "currency":
                case "numeric":
                case "text":
                    _xrmApp.Dialogs.SetValue(fieldLogicalName, value);
                    break;
                case var fieldType:
                    throw new NotSupportedException($"Setting a '{fieldType}' field on a dialog is not supported.");
            }

            _xrmApp._client.Browser.Driver.WaitForTransaction();
        }

        /// <inheritdoc/>
        public bool HasFieldValue(string field, string value, out string actual) =>
            ModelDrivenEntity.HasValue(_xrmApp, _xrmApp.Entity.GetFieldLogicalNameFromLabel(field), value, inHeader: false, out actual);

        /// <inheritdoc/>
        public bool HasFieldError(string field, string message) => throw ModelDrivenApp.NotSupported("Checking a field's validation error");

        /// <inheritdoc/>
        public void AnswerConfirmation(bool confirm)
        {
            _xrmApp.Dialogs.ConfirmationDialog(confirm);
            _xrmApp.ThinkTime(2000);
        }

        /// <inheritdoc/>
        public void CloseAlert() => _xrmApp.Dialogs.CloseWarningDialog();

        /// <inheritdoc/>
        public bool IsAlertShown() => _xrmApp.Dialogs.GetAlertDialog() != null;

        /// <inheritdoc/>
        public bool HasAlertTitle(string title, out string actual)
        {
            actual = _xrmApp.Dialogs.GetAlertDialogMessageText();
            return actual == title;
        }

        /// <inheritdoc/>
        public bool HasConfirmationTitle(string title, out string actual)
        {
            actual = _xrmApp.Dialogs.IsDialogVisible() ? _xrmApp.Dialogs.GetCurrentDialogLable() : null;
            return actual == title;
        }
    }
}
