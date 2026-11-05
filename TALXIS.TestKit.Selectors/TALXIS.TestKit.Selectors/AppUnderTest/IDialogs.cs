namespace TALXIS.TestKit.Selectors
{
    /// <summary>
    /// The dialogs of the app under test: the displayed (topmost) dialog, and the alert and confirmation dialogs opened with
    /// Xrm.Navigation.openAlertDialog and openConfirmDialog.
    /// </summary>
    public interface IDialogs
    {
        /// <summary>
        /// Checks whether a dialog is displayed; when <paramref name="shown"/> is false, that none is.
        /// </summary>
        /// <param name="shown">The expected state.</param>
        /// <returns>Whether the expected state was reached.</returns>
        bool IsDialogShown(bool shown);

        /// <summary>
        /// Checks whether the displayed dialog has a title.
        /// </summary>
        /// <param name="title">The expected title.</param>
        /// <param name="actual">The title shown.</param>
        /// <returns>Whether the dialog has the title.</returns>
        bool HasTitle(string title, out string actual);

        /// <summary>
        /// Clicks a button of the displayed dialog.
        /// </summary>
        /// <param name="label">The button's label.</param>
        void ClickButton(string label);

        /// <summary>
        /// Clicks a button of the displayed dialog when it offers it, and waits until the dialog has reacted (e.g. a wizard
        /// moved to its next step). A TALXIS Portal is given the time a dialog takes to load or a wizard to move to its next
        /// step; a model-driven app looks the button up as it is.
        /// </summary>
        /// <param name="label">The button's label, title or aria-label.</param>
        /// <returns>Whether the button was clicked.</returns>
        bool TryClickButton(string label);

        /// <summary>
        /// Checks whether the displayed dialog shows a button; when <paramref name="shown"/> is false, that it does not.
        /// </summary>
        /// <param name="label">The button's label, title or aria-label.</param>
        /// <param name="shown">The expected state.</param>
        /// <returns>Whether the expected state was reached.</returns>
        bool IsButtonShown(string label, bool shown);

        /// <summary>
        /// Checks whether the displayed dialog shows a text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>Whether the text is shown.</returns>
        bool IsTextShown(string text);

        /// <summary>
        /// Closes the displayed dialog with its close control, without saving.
        /// </summary>
        void CloseWithoutSaving();

        /// <summary>
        /// Selects an option of a choice group (e.g. option set tiles) on the displayed dialog. Only a TALXIS Portal renders
        /// choice groups on dialogs.
        /// </summary>
        /// <param name="option">The option's label.</param>
        void ChooseOption(string option);

        /// <summary>
        /// Sets the value of a field on the displayed dialog.
        /// </summary>
        /// <param name="field">The field's label.</param>
        /// <param name="value">The value, as <see cref="IEntity.SetValue"/> takes it.</param>
        void SetFieldValue(string field, string value);

        /// <summary>
        /// Checks whether a field on the displayed dialog shows a value.
        /// </summary>
        /// <param name="field">The field's label.</param>
        /// <param name="value">The expected value.</param>
        /// <param name="actual">The value the field shows.</param>
        /// <returns>Whether the field shows the value.</returns>
        bool HasFieldValue(string field, string value, out string actual);

        /// <summary>
        /// Checks whether a field on the displayed dialog shows a validation error (e.g. set by a script with setIsValid).
        /// Only a TALXIS Portal is checked for field errors.
        /// </summary>
        /// <param name="field">The field's label.</param>
        /// <param name="message">The expected error message.</param>
        /// <returns>Whether the field shows the error.</returns>
        bool HasFieldError(string field, string message);

        /// <summary>
        /// Confirms or cancels the displayed confirmation dialog.
        /// </summary>
        /// <param name="confirm">Whether to confirm.</param>
        void AnswerConfirmation(bool confirm);

        /// <summary>
        /// Closes the displayed alert dialog.
        /// </summary>
        void CloseAlert();

        /// <summary>
        /// Checks whether an alert dialog is displayed.
        /// </summary>
        /// <returns>Whether an alert dialog is displayed.</returns>
        bool IsAlertShown();

        /// <summary>
        /// Checks whether the displayed alert dialog has a title (in a model-driven app, its message).
        /// </summary>
        /// <param name="title">The expected title.</param>
        /// <param name="actual">The title shown.</param>
        /// <returns>Whether the alert dialog has the title.</returns>
        bool HasAlertTitle(string title, out string actual);

        /// <summary>
        /// Checks whether a confirmation dialog with a title is displayed.
        /// </summary>
        /// <param name="title">The expected title.</param>
        /// <param name="actual">The title shown.</param>
        /// <returns>Whether the confirmation dialog has the title.</returns>
        bool HasConfirmationTitle(string title, out string actual);
    }
}
