using System;

namespace TALXIS.TestKit.Selectors
{
    /// <summary>
    /// A field-bound dataset grid (e.g. the TALXIS VirtualDataset control), whose inline row buttons are notification
    /// expressions of the dataset client API (record.expressions.ui.setNotificationsExpression) rather than ribbon commands.
    /// A row button is named by its title, aria-label or text, or, when it shows its label only in a tooltip, by its Fluent
    /// icon name (e.g. <c>ClipboardList</c>). Only a TALXIS Portal is supported.
    /// </summary>
    public interface IDatasetGrid
    {
        /// <summary>
        /// Checks whether the grid shows a record.
        /// </summary>
        /// <param name="recordId">The record's id.</param>
        /// <returns>Whether the record is shown.</returns>
        bool ContainsRecord(Guid recordId);

        /// <summary>
        /// Clicks a button on a record's row, waiting until it is shown.
        /// </summary>
        /// <param name="recordId">The record's id.</param>
        /// <param name="button">The button's title, aria-label, text or icon name.</param>
        void ClickRowButton(Guid recordId, string button);

        /// <summary>
        /// Checks whether a record's row shows a button; when <paramref name="shown"/> is false, that the record is shown
        /// and the button disappears and stays gone.
        /// </summary>
        /// <param name="recordId">The record's id.</param>
        /// <param name="button">The button's title, aria-label, text or icon name.</param>
        /// <param name="shown">The expected state.</param>
        /// <returns>Whether the expected state was reached.</returns>
        bool IsRowButtonShown(Guid recordId, string button, bool shown);
    }
}
