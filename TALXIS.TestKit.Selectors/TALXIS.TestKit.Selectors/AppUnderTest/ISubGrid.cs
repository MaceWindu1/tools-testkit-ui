using System;

namespace TALXIS.TestKit.Selectors
{
    /// <summary>
    /// The subgrids of the open page, named by their control name. A record is given by its id, or by a value it shows in a
    /// column (the column's header label or logical name); a TALXIS Portal's legacy view shows no record ids.
    /// </summary>
    public interface ISubGrid
    {
        /// <summary>
        /// Clicks a command of a subgrid, waiting until it is available.
        /// </summary>
        /// <param name="subGrid">The subgrid's name.</param>
        /// <param name="command">The command's label (or, in a TALXIS Portal, its button id, command id or icon file name).</param>
        void ClickCommand(string subGrid, string command);

        /// <summary>
        /// Clicks a command on a record's row without selecting the record. Only a TALXIS Portal renders commands on rows.
        /// </summary>
        /// <param name="subGrid">The subgrid's name.</param>
        /// <param name="recordId">The record's id.</param>
        /// <param name="command">The command's label, button id, command id or icon file name.</param>
        void ClickRowCommand(string subGrid, Guid recordId, string command);

        /// <summary>
        /// Clicks a command on the row of the record that shows a value in a column, without selecting the record. Only a
        /// TALXIS Portal renders commands on rows.
        /// </summary>
        /// <param name="subGrid">The subgrid's name.</param>
        /// <param name="column">The column's header label or logical name.</param>
        /// <param name="value">The value the record shows.</param>
        /// <param name="command">The command's label, button id, command id or icon file name.</param>
        void ClickRowCommand(string subGrid, string column, string value, string command);

        /// <summary>
        /// Selects a record. In a TALXIS Portal subgrid without a selection column, the record's row becomes the one the
        /// commands are looked up on.
        /// </summary>
        /// <param name="subGrid">The subgrid's name.</param>
        /// <param name="recordId">The record's id.</param>
        void SelectRecord(string subGrid, Guid recordId);

        /// <summary>
        /// Selects the record that shows a value in a column.
        /// </summary>
        /// <param name="subGrid">The subgrid's name.</param>
        /// <param name="column">The column's header label (TALXIS Portal) or logical name.</param>
        /// <param name="value">The value the record shows.</param>
        void SelectRecord(string subGrid, string column, string value);

        /// <summary>
        /// Checks whether a subgrid shows a record; when <paramref name="contained"/> is false, that it disappears and stays
        /// gone.
        /// </summary>
        /// <param name="subGrid">The subgrid's name.</param>
        /// <param name="recordId">The record's id.</param>
        /// <param name="contained">The expected state.</param>
        /// <returns>Whether the subgrid reached the expected state.</returns>
        bool ContainsRecord(string subGrid, Guid recordId, bool contained);

        /// <summary>
        /// Checks whether a subgrid shows a record with a value in a column.
        /// </summary>
        /// <param name="subGrid">The subgrid's name.</param>
        /// <param name="column">The column's header label (TALXIS Portal) or logical name.</param>
        /// <param name="value">The value.</param>
        /// <returns>Whether such a record is shown.</returns>
        bool ContainsRecord(string subGrid, string column, string value);

        /// <summary>
        /// Counts the records of a subgrid.
        /// </summary>
        /// <param name="subGrid">The subgrid's name.</param>
        /// <returns>The number of records: in a model-driven app all of them, in a TALXIS Portal those it shows.</returns>
        long CountRecords(string subGrid);

        /// <summary>
        /// Checks whether a command of a subgrid is shown; when <paramref name="shown"/> is false, that it is not, once the
        /// subgrid has evaluated its commands.
        /// </summary>
        /// <param name="subGrid">The subgrid's name.</param>
        /// <param name="command">The command's label (or, in a TALXIS Portal, its button id, command id or icon file name).</param>
        /// <param name="shown">The expected state.</param>
        /// <returns>Whether the command reached the expected state.</returns>
        bool IsCommandShown(string subGrid, string command, bool shown);

        /// <summary>
        /// Checks whether a command of a subgrid is enabled. A model-driven app shows a command whose enable rules fail as
        /// disabled; a TALXIS Portal does not render it, so there enabled means shown and disabled means not shown.
        /// </summary>
        /// <param name="subGrid">The subgrid's name.</param>
        /// <param name="command">The command's label (or, in a TALXIS Portal, its button id, command id or icon file name).</param>
        /// <param name="enabled">The expected state.</param>
        /// <returns>Whether the command reached the expected state.</returns>
        bool IsCommandEnabled(string subGrid, string command, bool enabled);
    }
}
