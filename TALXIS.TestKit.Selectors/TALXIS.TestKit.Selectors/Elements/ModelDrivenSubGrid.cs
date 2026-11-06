using System;
using System.Linq;
using OpenQA.Selenium;
using TALXIS.TestKit.Selectors.Browser;

namespace TALXIS.TestKit.Selectors
{
    /// <summary>
    /// The subgrids of a model-driven form. A model-driven app renders no commands on a record's row.
    /// </summary>
    public class ModelDrivenSubGrid : Element, ISubGrid
    {
        private readonly XrmApp _xrmApp;

        public ModelDrivenSubGrid(XrmApp xrmApp) : base()
        {
            _xrmApp = xrmApp ?? throw new ArgumentNullException(nameof(xrmApp));
        }

        /// <inheritdoc/>
        public void ClickCommand(string subGrid, string command)
        {
            _xrmApp.Entity.SubGrid.WaitForCommandBar(subGrid);
            _xrmApp.Entity.SubGrid.ClickCommand(subGrid, command);
        }

        /// <inheritdoc/>
        public void ClickRowCommand(string subGrid, Guid recordId, string command) => throw ModelDrivenApp.NotSupported("Clicking a command on a record's row");

        /// <inheritdoc/>
        public void ClickRowCommand(string subGrid, string column, string value, string command) => throw ModelDrivenApp.NotSupported("Clicking a command on a record's row");

        /// <inheritdoc/>
        public void SelectRecord(string subGrid, Guid recordId)
        {
            var index = _xrmApp.Entity.SubGrid.GetRecordIndexById(subGrid, recordId);
            if (index < 0)
                throw new NoSuchElementException($"The record {recordId} is not in the '{subGrid}' subgrid.");

            _xrmApp.Entity.SubGrid.HighlightRecord(subGrid, index);
        }

        /// <inheritdoc/>
        public void SelectRecord(string subGrid, string column, string value)
        {
            var items = _xrmApp.Entity.SubGrid.GetSubGridItems(subGrid);
            var item = items.FirstOrDefault(i => i.GetAttribute<string>(column) == value)
                ?? throw new NoSuchElementException($"Could not find record with field '{column}' with value '{value}' in the '{subGrid}' subgrid.");

            _xrmApp.Entity.SubGrid.HighlightRecord(subGrid, items.IndexOf(item));
        }

        /// <inheritdoc/>
        public bool ContainsRecord(string subGrid, Guid recordId, bool contained) =>
            _xrmApp.Entity.SubGrid.GetRecordIndexById(subGrid, recordId) > -1 == contained;

        /// <inheritdoc/>
        public bool ContainsRecord(string subGrid, string column, string value) =>
            _xrmApp.Entity.SubGrid.GetSubGridItems(subGrid).Any(item => item.GetAttribute<string>(column) == value);

        /// <inheritdoc/>
        public long CountRecords(string subGrid) => _xrmApp.Entity.SubGrid.GetTotalRecordCount(subGrid);

        /// <inheritdoc/>
        public bool IsCommandShown(string subGrid, string command, bool shown) =>
            _xrmApp.Entity.SubGrid.TryToFindAssertSubgridCommand(command, subGrid) != null == shown;

        /// <inheritdoc/>
        public bool IsCommandEnabled(string subGrid, string command, bool enabled) =>
            (_xrmApp.Entity.SubGrid.IsCommandEnabled(subGrid, command)
                ?? throw new NoSuchElementException($"The '{command}' command should be shown (enabled or disabled, not hidden) on the '{subGrid}' subgrid.")) == enabled;
    }
}
