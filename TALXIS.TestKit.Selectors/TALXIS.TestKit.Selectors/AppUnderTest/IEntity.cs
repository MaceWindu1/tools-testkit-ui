namespace TALXIS.TestKit.Selectors
{
    /// <summary>
    /// The tabs and fields of the open form: in a TALXIS Portal those of the topmost form dialog, when one is open. Tabs are
    /// named by their label or name, fields by their label or logical name.
    /// </summary>
    public interface IEntity
    {
        /// <summary>
        /// Selects a tab and waits until it is the selected one.
        /// </summary>
        /// <param name="tab">The tab's label or name.</param>
        void SelectTab(string tab);

        /// <summary>
        /// Checks whether a tab is shown; when <paramref name="shown"/> is false, that it disappears and stays gone.
        /// </summary>
        /// <param name="tab">The tab's label or name.</param>
        /// <param name="shown">The expected state.</param>
        /// <returns>Whether the tab reached the expected state.</returns>
        bool IsTabShown(string tab, bool shown);

        /// <summary>
        /// Checks whether a tab is the selected one.
        /// </summary>
        /// <param name="tab">The tab's label (or, in a TALXIS Portal, its name).</param>
        /// <returns>Whether the tab is selected.</returns>
        bool IsTabSelected(string tab);

        /// <summary>
        /// Sets the value of a field.
        /// </summary>
        /// <param name="field">The field's label.</param>
        /// <param name="value">The value: for an option set the option's label, for a lookup the search text, for a
        /// multi-select option set the options' labels separated by commas.</param>
        void SetValue(string field, string value);

        /// <summary>
        /// Checks whether a field shows a value.
        /// </summary>
        /// <param name="field">The field's label.</param>
        /// <param name="value">The expected value, as <see cref="SetValue"/> takes it.</param>
        /// <param name="actual">The value the field shows.</param>
        /// <returns>Whether the field shows the value.</returns>
        bool HasValue(string field, string value, out string actual);

        /// <summary>
        /// Checks whether a field is shown; when <paramref name="shown"/> is false, that it disappears and stays gone.
        /// </summary>
        /// <param name="field">The field's label or logical name.</param>
        /// <param name="shown">The expected state.</param>
        /// <returns>Whether the field reached the expected state.</returns>
        bool IsFieldShown(string field, bool shown);

        /// <summary>
        /// Checks whether a field is required (or optional).
        /// </summary>
        /// <param name="field">The field's label.</param>
        /// <param name="required">The expected state.</param>
        /// <returns>Whether the field reached the expected state.</returns>
        bool IsFieldRequired(string field, bool required);

        /// <summary>
        /// Checks whether a field can be edited: shown and not read-only.
        /// </summary>
        /// <param name="field">The field's label or logical name.</param>
        /// <param name="editable">The expected state.</param>
        /// <returns>Whether the field reached the expected state.</returns>
        bool IsFieldEditable(string field, bool editable);

        /// <summary>
        /// Checks whether a field has no value. A model-driven app reads the form's value, so the field may be hidden.
        /// </summary>
        /// <param name="field">The field's label or logical name.</param>
        /// <returns>Whether the field is empty.</returns>
        bool IsFieldEmpty(string field);
    }
}
