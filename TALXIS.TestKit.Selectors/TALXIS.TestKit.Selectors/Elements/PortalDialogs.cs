using System;
using TALXIS.TestKit.Selectors.Browser;
using TALXIS.TestKit.Selectors.WebClientManagement;

namespace TALXIS.TestKit.Selectors
{
    /// <summary>
    /// The dialogs of a TALXIS Portal.
    /// </summary>
    public class PortalDialogs : Element, IDialogs
    {
        // Dialog fields are re-read for a shorter time than page fields: a dialog's values are set while it is open.
        private static readonly TimeSpan FieldValueTimeout = TimeSpan.FromSeconds(10);

        private readonly PortalDialogsManager _dialogsManager;
        private readonly PortalFormManager _formManager;

        public PortalDialogs(WebClient client) : base()
        {
            _dialogsManager = new PortalDialogsManager(client);
            _formManager = new PortalFormManager(client);
        }

        /// <inheritdoc/>
        public bool IsDialogShown(bool shown) => _dialogsManager.IsDialogShown(shown).Value;

        /// <inheritdoc/>
        public bool HasTitle(string title, out string actual)
        {
            actual = _dialogsManager.GetTitle(title).Value;
            return actual == title;
        }

        /// <inheritdoc/>
        public void ClickButton(string label) => _dialogsManager.ClickButton(label);

        /// <inheritdoc/>
        public bool TryClickButton(string label) => _dialogsManager.TryClickButton(label).Value;

        /// <inheritdoc/>
        public bool IsButtonShown(string label, bool shown) => _dialogsManager.IsButtonShown(label, shown).Value;

        /// <inheritdoc/>
        public bool IsTextShown(string text) => _dialogsManager.IsTextShown(text).Value;

        /// <inheritdoc/>
        public void CloseWithoutSaving() => _dialogsManager.CloseWithoutSaving();

        /// <inheritdoc/>
        public void ChooseOption(string option) => _dialogsManager.ChooseOption(option);

        /// <inheritdoc/>
        public void SetFieldValue(string field, string value) => _formManager.SetValue(field, value);

        /// <inheritdoc/>
        public bool HasFieldValue(string field, string value, out string actual)
        {
            actual = _formManager.GetValue(field, value, FieldValueTimeout).Value;
            return actual == value;
        }

        /// <inheritdoc/>
        public bool HasFieldError(string field, string message) => _formManager.HasFieldError(field, message).Value;

        /// <inheritdoc/>
        public void AnswerConfirmation(bool confirm) => _dialogsManager.AnswerMessageDialog(confirm);

        /// <inheritdoc/>
        public void CloseAlert() => _dialogsManager.AnswerMessageDialog(confirm: true);

        /// <inheritdoc/>
        public bool IsAlertShown() => _dialogsManager.GetMessageTitle(expected: null).Value != null;

        /// <inheritdoc/>
        public bool HasAlertTitle(string title, out string actual)
        {
            actual = _dialogsManager.GetMessageTitle(title).Value;
            return actual == title;
        }

        /// <inheritdoc/>
        /// <remarks>An alert and a confirmation dialog are both modals without a form, told apart by nothing else.</remarks>
        public bool HasConfirmationTitle(string title, out string actual) => HasAlertTitle(title, out actual);
    }
}
