using System;
using TALXIS.TestKit.Selectors.Browser;
using TALXIS.TestKit.Selectors.WebClientManagement;

namespace TALXIS.TestKit.Selectors
{
    /// <summary>
    /// The subgrids of a TALXIS Portal page, rendered as a grid (ag-grid) or as the legacy view (Fluent DetailsList).
    /// </summary>
    public class PortalSubGrid : Element, ISubGrid
    {
        private readonly PortalGridManager _gridManager;

        public PortalSubGrid(WebClient client) : base()
        {
            _gridManager = new PortalGridManager(client);
        }

        /// <inheritdoc/>
        /// <remarks>Commands for one selected record are rendered on each record's row: they are looked up on the rows of
        /// the selected records (all rows when none is selected), other commands on the subgrid's ribbon.</remarks>
        public void ClickCommand(string subGrid, string command) => _gridManager.ClickCommand(subGrid, command);

        /// <inheritdoc/>
        public void ClickRowCommand(string subGrid, Guid recordId, string command) => _gridManager.ClickRowCommand(subGrid, recordId, command);

        /// <inheritdoc/>
        public void ClickRowCommand(string subGrid, string column, string value, string command) => _gridManager.ClickRowCommand(subGrid, column, value, command);

        /// <inheritdoc/>
        public void SelectRecord(string subGrid, Guid recordId) => _gridManager.SelectRecord(subGrid, recordId);

        /// <inheritdoc/>
        public void SelectRecord(string subGrid, string column, string value) => _gridManager.SelectRecord(subGrid, column, value);

        /// <inheritdoc/>
        public bool ContainsRecord(string subGrid, Guid recordId, bool contained) => _gridManager.ContainsRecord(subGrid, recordId, contained).Value;

        /// <inheritdoc/>
        public bool ContainsRecord(string subGrid, string column, string value) => _gridManager.ContainsRecord(subGrid, column, value).Value;

        /// <inheritdoc/>
        public long CountRecords(string subGrid) => _gridManager.CountRecords(subGrid).Value;

        /// <inheritdoc/>
        public bool IsCommandShown(string subGrid, string command, bool shown) => _gridManager.IsCommandShown(subGrid, command, shown).Value;

        /// <inheritdoc/>
        /// <remarks>The portal does not render a command whose rules fail: enabled means shown, disabled not shown.</remarks>
        public bool IsCommandEnabled(string subGrid, string command, bool enabled) => IsCommandShown(subGrid, command, enabled);
    }
}
