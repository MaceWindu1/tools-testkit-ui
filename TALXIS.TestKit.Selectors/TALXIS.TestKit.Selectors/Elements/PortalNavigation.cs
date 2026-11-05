using TALXIS.TestKit.Selectors.Browser;
using TALXIS.TestKit.Selectors.WebClientManagement;

namespace TALXIS.TestKit.Selectors
{
    /// <summary>
    /// The navigation of a TALXIS Portal: a horizontal command bar or a vertical navigation. Items are found by their label
    /// or sitemap subarea id.
    /// </summary>
    public class PortalNavigation : Element, INavigation
    {
        private readonly PortalNavigationManager _navigationManager;

        public PortalNavigation(WebClient client) : base()
        {
            _navigationManager = new PortalNavigationManager(client);
        }

        /// <inheritdoc/>
        public void OpenPage(string label) => _navigationManager.OpenPage(label);

        /// <inheritdoc/>
        public bool IsSubAreaShown(string label, bool shown) => _navigationManager.IsSubAreaShown(label, shown).Value;

        /// <inheritdoc/>
        public bool IsSubAreaDisabled(string label, string message) => _navigationManager.IsSubAreaDisabled(label, message).Value;
    }
}
