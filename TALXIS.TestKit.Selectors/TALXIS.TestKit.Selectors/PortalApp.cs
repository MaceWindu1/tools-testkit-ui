using System;
using TALXIS.TestKit.Selectors.WebClientManagement;

namespace TALXIS.TestKit.Selectors
{
    /// <summary>
    /// A TALXIS Portal, the counterpart of <see cref="XrmApp"/> for a model-driven app: its elements implement the
    /// operations of <see cref="IAppUnderTest"/> on the portal's markup (<see cref="PortalElements"/>).
    /// </summary>
    public class PortalApp : IAppUnderTest
    {
        private readonly WebClient _client;

        public PortalApp(WebClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public PortalLogin Login => new PortalLogin(_client);
        public PortalNavigation Navigation => new PortalNavigation(_client);
        public PortalEntity Entity => new PortalEntity(_client);
        public PortalSubGrid SubGrid => new PortalSubGrid(_client);
        public PortalDialogs Dialogs => new PortalDialogs(_client);
        public PortalDatasetGrid DatasetGrid => new PortalDatasetGrid(_client);

        ILogin IAppUnderTest.Login => Login;
        INavigation IAppUnderTest.Navigation => Navigation;
        IEntity IAppUnderTest.Entity => Entity;
        ISubGrid IAppUnderTest.SubGrid => SubGrid;
        IDialogs IAppUnderTest.Dialogs => Dialogs;
        IDatasetGrid IAppUnderTest.DatasetGrid => DatasetGrid;

        /// <summary>
        /// Waits until the portal has loaded (or waits for input in a dialog), failing fast when it shows an error.
        /// </summary>
        /// <param name="timeout">How long to wait.</param>
        /// <param name="message">The message of the timeout exception.</param>
        public void WaitUntilReady(TimeSpan timeout, string message = "The portal did not finish loading.")
        {
            new PortalPageManager(_client).WaitForReady(timeout, message);
        }
    }
}
