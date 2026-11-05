using TALXIS.TestKit.Selectors.Browser;
using TALXIS.TestKit.Selectors.WebClientManagement;

namespace TALXIS.TestKit.Selectors
{
    /// <summary>
    /// The tabs and fields of a TALXIS Portal form: of the topmost form dialog when one is open, otherwise of the page. Tabs
    /// are found by their label without the <c>#Icon#</c> prefix or by their name, fields by their label or logical name.
    /// </summary>
    public class PortalEntity : Element, IEntity
    {
        private readonly PortalFormManager _formManager;

        public PortalEntity(WebClient client) : base()
        {
            _formManager = new PortalFormManager(client);
        }

        /// <inheritdoc/>
        public void SelectTab(string tab) => _formManager.SelectTab(tab);

        /// <inheritdoc/>
        public bool IsTabShown(string tab, bool shown) => _formManager.IsTabShown(tab, shown).Value;

        /// <inheritdoc/>
        public bool IsTabSelected(string tab) => _formManager.IsTabSelected(tab).Value;

        /// <inheritdoc/>
        /// <remarks>Sets text fields, option sets (by the option's label) and lookups; date fields are not supported yet.</remarks>
        public void SetValue(string field, string value) => _formManager.SetValue(field, value);

        /// <inheritdoc/>
        public bool HasValue(string field, string value, out string actual)
        {
            actual = _formManager.GetValue(field, value, PortalFormManager.RenderTimeout).Value;
            return actual == value;
        }

        /// <inheritdoc/>
        public bool IsFieldShown(string field, bool shown) => _formManager.IsFieldShown(field, shown).Value;

        /// <inheritdoc/>
        public bool IsFieldRequired(string field, bool required) => _formManager.IsFieldRequired(field, required).Value;

        /// <inheritdoc/>
        public bool IsFieldEditable(string field, bool editable) => _formManager.IsFieldEditable(field, editable).Value;

        /// <inheritdoc/>
        public bool IsFieldEmpty(string field) => _formManager.IsFieldEmpty(field).Value;
    }
}
