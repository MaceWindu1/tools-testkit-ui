using System;
using System.Globalization;
using System.Linq;
using TALXIS.TestKit.Selectors.Browser;

namespace TALXIS.TestKit.Selectors
{
    /// <summary>
    /// The tabs and fields of a model-driven form. Fields are entered and read by their type, in the body or in the header.
    /// </summary>
    public class ModelDrivenEntity : Element, IEntity
    {
        private readonly XrmApp _xrmApp;

        public ModelDrivenEntity(XrmApp xrmApp) : base()
        {
            _xrmApp = xrmApp ?? throw new ArgumentNullException(nameof(xrmApp));
        }

        /// <inheritdoc/>
        public void SelectTab(string tab) => _xrmApp.Entity.SelectTab(tab);

        /// <inheritdoc/>
        public bool IsTabShown(string tab, bool shown) => _xrmApp.Entity.IsTabVisible(tab) == shown;

        /// <inheritdoc/>
        public bool IsTabSelected(string tab) => _xrmApp.Entity.GetAttributeOfActiveTab("aria-label") == tab;

        /// <inheritdoc/>
        public void SetValue(string field, string value)
        {
            var driver = _xrmApp._client.Browser.Driver;
            driver.WaitForPageToLoad();

            var fieldLogicalName = _xrmApp.Entity.GetFieldLogicalNameFromLabel(field);
            var fieldType = GetFieldType(_xrmApp, fieldLogicalName);

            // !TO-DO: WI:35121 - Remove fieldLocation
            if (_xrmApp.Entity.GetFieldLocation(fieldLogicalName) == "field")
                SetFieldValue(fieldLogicalName, value, fieldType);
            else
                SetHeaderFieldValue(field, value, fieldType);

            _xrmApp._client.TryLoseFocus();
            driver.WaitForTransaction();
        }

        /// <inheritdoc/>
        public bool HasValue(string field, string value, out string actual)
        {
            var fieldLogicalName = _xrmApp.Entity.GetFieldLogicalNameFromLabel(field);
            return HasValue(_xrmApp, fieldLogicalName, value, inHeader: _xrmApp.Entity.GetFieldLocation(fieldLogicalName) != "field", out actual);
        }

        /// <inheritdoc/>
        public bool IsFieldShown(string field, bool shown) =>
            _xrmApp.Entity.IsFieldVisible(_xrmApp.Entity.ResolveFieldLogicalName(field)) == shown;

        /// <inheritdoc/>
        public bool IsFieldRequired(string field, bool required) =>
            _xrmApp.Entity.GetFieldSectionItemContainertAttribute<int>(field, "data-fieldrequirement") == (required ? 2 : 0);

        /// <inheritdoc/>
        public bool IsFieldEditable(string field, bool editable)
        {
            var fieldLogicalName = _xrmApp.Entity.ResolveFieldLogicalName(field);

            // A user cannot edit a hidden field, whether it is read-only or not.
            var isEditable = _xrmApp.Entity.IsFieldVisible(fieldLogicalName) && !_xrmApp.Entity.IsFieldReadOnly(fieldLogicalName);
            return isEditable == editable;
        }

        /// <inheritdoc/>
        public bool IsFieldEmpty(string field) => _xrmApp.Entity.IsFieldEmpty(_xrmApp.Entity.ResolveFieldLogicalName(field));

        // Reads a field as its type shows it and compares it with a value as the field steps take it.
        internal static bool HasValue(XrmApp xrmApp, string fieldLogicalName, string expected, bool inHeader, out string actual)
        {
            var entity = xrmApp.Entity;
            switch (GetFieldType(xrmApp, fieldLogicalName))
            {
                case "optionset":
                    var optionSet = new OptionSet { Name = fieldLogicalName };
                    actual = inHeader ? entity.GetHeaderValue(optionSet) : entity.GetValue(optionSet);
                    return actual == expected;
                case "multioptionset":
                    var values = entity.GetValue(new MultiValueOptionSet { Name = fieldLogicalName }).Values;
                    actual = string.Join(", ", values);
                    return values.SequenceEqual(expected.Split(',').Select(v => v.Trim()));
                case "lookup":
                    var lookup = new LookupItem { Name = fieldLogicalName };
                    actual = inHeader ? entity.GetHeaderValue(lookup) : entity.GetValue(lookup);
                    return actual == expected;
                case "datetime":
                    var dateTimeControl = new DateTimeControl(fieldLogicalName);
                    var dateTime = inHeader ? entity.GetHeaderValue(dateTimeControl) : entity.GetValue(dateTimeControl);
                    var expectedDateTime = DateTime.Parse(expected, CultureInfo.CurrentCulture);
                    actual = dateTime?.ToString(CultureInfo.CurrentCulture);

                    // A value without a time is compared by its date.
                    return expected.Contains(':', StringComparison.Ordinal) ? dateTime == expectedDateTime : dateTime?.Date == expectedDateTime.Date;
                case "boolean":
                    var boolean = new BooleanItem { Name = fieldLogicalName };
                    var booleanValue = inHeader ? entity.GetHeaderValue(boolean) : entity.GetValue(boolean);
                    actual = booleanValue.ToString(CultureInfo.InvariantCulture);
                    return booleanValue == bool.Parse(expected);
                default:
                    actual = inHeader ? entity.GetHeaderValue(fieldLogicalName) : entity.GetValue(fieldLogicalName);
                    return actual == expected;
            }
        }

        // The type of a field; a label of something that is not a field (no attribute) fails rather than being read as text.
        internal static string GetFieldType(XrmApp xrmApp, string fieldLogicalName)
        {
            var fieldType = xrmApp.Entity.GetFieldType(fieldLogicalName);
            return fieldType != "attribute not found"
                ? fieldType
                : throw new InvalidOperationException($"The form has no '{fieldLogicalName}' field.");
        }

        private void SetFieldValue(string fieldName, string fieldValue, string fieldType)
        {
            switch (fieldType)
            {
                case "multioptionset":
                    _xrmApp.Entity.SetValue(new MultiValueOptionSet { Name = fieldName, Values = fieldValue.Split(',').Select(v => v.Trim()).ToArray() }, true);
                    break;
                case "optionset":
                    _xrmApp.Entity.SetValue(new OptionSet { Name = fieldName, Value = fieldValue });
                    break;
                case "boolean":
                    _xrmApp.Entity.SetValue(new BooleanItem { Name = fieldName, Value = bool.Parse(fieldValue) });
                    break;
                case "datetime":
                    // !TO-DO: Datetime formattings
                    _xrmApp.Entity.SetValue(new DateTimeControl(fieldName) { Value = DateTime.Parse(fieldValue, CultureInfo.CurrentCulture) });
                    break;
                case "lookup":
                    _xrmApp.Entity.SetValue(new LookupItem { Name = fieldName, Value = fieldValue });
                    break;
                default:
                    _xrmApp.Entity.SetValue(fieldName, fieldValue);
                    break;
            }
        }

        private void SetHeaderFieldValue(string fieldName, string fieldValue, string fieldType)
        {
            switch (fieldType)
            {
                case "optionset":
                    _xrmApp.Entity.SetHeaderValue(new OptionSet { Name = fieldName, Value = fieldValue });
                    break;
                case "boolean":
                    _xrmApp.Entity.SetHeaderValue(new BooleanItem { Name = fieldName, Value = bool.Parse(fieldValue) });
                    break;
                case "datetime":
                    _xrmApp.Entity.SetHeaderValue(new DateTimeControl(fieldName) { Value = DateTime.Parse(fieldValue, CultureInfo.CurrentCulture) });
                    break;
                case "lookup":
                    _xrmApp.Entity.SetHeaderValue(new LookupItem { Name = fieldName, Value = fieldValue });
                    break;
                default:
                    _xrmApp.Entity.SetHeaderValue(fieldName, fieldValue);
                    break;
            }
        }
    }
}
