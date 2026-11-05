namespace TALXIS.TestKit.Selectors
{
    /// <summary>
    /// The navigation (site map) of the app under test. Items are named by their label.
    /// </summary>
    public interface INavigation
    {
        /// <summary>
        /// Opens a page by clicking its navigation item (in a model-driven app a subarea of the current area) and waits until
        /// it has loaded.
        /// </summary>
        /// <param name="label">The item's label.</param>
        void OpenPage(string label);

        /// <summary>
        /// Checks whether a navigation item is shown; when <paramref name="shown"/> is false, that it disappears and stays
        /// gone (a TALXIS Portal hides items with scripts after the page has loaded).
        /// </summary>
        /// <param name="label">The item's label.</param>
        /// <param name="shown">The expected state.</param>
        /// <returns>Whether the item reached the expected state.</returns>
        bool IsSubAreaShown(string label, bool shown);

        /// <summary>
        /// Checks whether a navigation item is shown disabled, with a message as its tooltip. Only a TALXIS Portal disables
        /// navigation items (TALXIS.Portal.App.sitemap.getItem(name).setDisabled(disabled, message)).
        /// </summary>
        /// <param name="label">The item's label.</param>
        /// <param name="message">The expected message.</param>
        /// <returns>Whether the item is disabled with the message.</returns>
        bool IsSubAreaDisabled(string label, string message);
    }
}
