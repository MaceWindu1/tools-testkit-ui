using System;
using TALXIS.TestKit.Selectors.Browser;
using TALXIS.TestKit.Selectors.WebClientManagement;

namespace TALXIS.TestKit.Selectors
{
    /// <summary>
    /// A field-bound dataset grid of a TALXIS Portal (an ag-grid whose rows carry the record id as <c>row-id</c>), found
    /// wherever it is on the page.
    /// </summary>
    public class PortalDatasetGrid : Element, IDatasetGrid
    {
        private readonly PortalGridManager _gridManager;

        public PortalDatasetGrid(WebClient client) : base()
        {
            _gridManager = new PortalGridManager(client);
        }

        /// <inheritdoc/>
        public bool ContainsRecord(Guid recordId) => _gridManager.ContainsDatasetRecord(recordId).Value;

        /// <inheritdoc/>
        public void ClickRowButton(Guid recordId, string button) => _gridManager.ClickDatasetRowButton(recordId, button);

        /// <inheritdoc/>
        public bool IsRowButtonShown(Guid recordId, string button, bool shown) => _gridManager.IsDatasetRowButtonShown(recordId, button, shown).Value;
    }
}
