using System;
using TALXIS.TestKit.Selectors.Browser;

namespace TALXIS.TestKit.Selectors
{
    /// <summary>
    /// The sitemap of a model-driven app: a page is a subarea of the current area.
    /// </summary>
    public class ModelDrivenNavigation : Element, INavigation
    {
        private readonly XrmApp _xrmApp;

        public ModelDrivenNavigation(XrmApp xrmApp) : base()
        {
            _xrmApp = xrmApp ?? throw new ArgumentNullException(nameof(xrmApp));
        }

        /// <inheritdoc/>
        public void OpenPage(string label) => _xrmApp.Navigation.OpenSubArea(label);

        /// <inheritdoc/>
        public bool IsSubAreaShown(string label, bool shown) => shown
            ? _xrmApp.Navigation.GetSubAreaElement(label) is { Displayed: true }
            : !_xrmApp.Navigation.IsSubAreaVisible(label);

        /// <inheritdoc/>
        public bool IsSubAreaDisabled(string label, string message) => throw ModelDrivenApp.NotSupported("Disabling a navigation item");

        /// <inheritdoc/>
        public void OpenRecord(string entityName, Guid recordId) => _xrmApp.Navigation.OpenRecord(entityName, recordId);
    }
}
