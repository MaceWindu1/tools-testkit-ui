using System;

namespace TALXIS.TestKit.Selectors
{
    /// <summary>
    /// A model-driven app, as the steps that act on either kind of app see it: <see cref="IAppUnderTest"/> over the elements
    /// of <see cref="XrmApp"/>, the counterpart of <see cref="PortalApp"/> for a TALXIS Portal.
    /// </summary>
    public class ModelDrivenApp : IAppUnderTest
    {
        private readonly XrmApp _xrmApp;

        public ModelDrivenApp(XrmApp xrmApp)
        {
            _xrmApp = xrmApp ?? throw new ArgumentNullException(nameof(xrmApp));
        }

        public ModelDrivenLogin Login => new ModelDrivenLogin(_xrmApp);
        public ModelDrivenNavigation Navigation => new ModelDrivenNavigation(_xrmApp);
        public ModelDrivenEntity Entity => new ModelDrivenEntity(_xrmApp);
        public ModelDrivenSubGrid SubGrid => new ModelDrivenSubGrid(_xrmApp);
        public ModelDrivenDialogs Dialogs => new ModelDrivenDialogs(_xrmApp);
        public ModelDrivenDatasetGrid DatasetGrid => new ModelDrivenDatasetGrid();

        ILogin IAppUnderTest.Login => Login;
        INavigation IAppUnderTest.Navigation => Navigation;
        IEntity IAppUnderTest.Entity => Entity;
        ISubGrid IAppUnderTest.SubGrid => SubGrid;
        IDialogs IAppUnderTest.Dialogs => Dialogs;
        IDatasetGrid IAppUnderTest.DatasetGrid => DatasetGrid;

        internal static NotSupportedException NotSupported(string operation) =>
            new NotSupportedException($"{operation} is only supported in a TALXIS Portal, not in a model-driven app.");
    }
}
