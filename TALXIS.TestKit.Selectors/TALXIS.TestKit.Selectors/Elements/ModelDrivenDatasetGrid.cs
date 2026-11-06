using System;
using TALXIS.TestKit.Selectors.Browser;

namespace TALXIS.TestKit.Selectors
{
    /// <summary>
    /// The field-bound dataset grid, which a model-driven app does not support.
    /// </summary>
    public class ModelDrivenDatasetGrid : Element, IDatasetGrid
    {
        /// <inheritdoc/>
        public bool ContainsRecord(Guid recordId) => throw ModelDrivenApp.NotSupported("The virtual dataset grid");

        /// <inheritdoc/>
        public void ClickRowButton(Guid recordId, string button) => throw ModelDrivenApp.NotSupported("The virtual dataset grid");

        /// <inheritdoc/>
        public bool IsRowButtonShown(Guid recordId, string button, bool shown) => throw ModelDrivenApp.NotSupported("The virtual dataset grid");
    }
}
