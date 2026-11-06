using System;

namespace TALXIS.TestKit.Selectors
{
    /// <summary>
    /// The app a test acts on, described by what a user does in it rather than by its markup, so that one step works on a
    /// model-driven app and on a TALXIS Portal alike.
    /// </summary>
    /// <remarks>
    /// <para>The checks (the members returning <see cref="bool"/>) wait, up to the implementation's timeout, until the app
    /// reaches the expected state and return whether it did: a TALXIS Portal renders and re-renders asynchronously, while a
    /// model-driven app is read as it is once its transactions are done.</para>
    /// <para>An operation that an app does not have throws <see cref="NotSupportedException"/> naming the operation and the
    /// app. Where an app means something else by an operation, its member says so.</para>
    /// </remarks>
    public interface IAppUnderTest
    {
        /// <summary>Gets the sign-in.</summary>
        ILogin Login { get; }

        /// <summary>Gets the navigation (site map).</summary>
        INavigation Navigation { get; }

        /// <summary>Gets the tabs and fields of the open form.</summary>
        IEntity Entity { get; }

        /// <summary>Gets the subgrids of the open page.</summary>
        ISubGrid SubGrid { get; }

        /// <summary>Gets the dialogs.</summary>
        IDialogs Dialogs { get; }

        /// <summary>Gets the field-bound dataset grid (e.g. the TALXIS VirtualDataset control).</summary>
        IDatasetGrid DatasetGrid { get; }
    }
}
