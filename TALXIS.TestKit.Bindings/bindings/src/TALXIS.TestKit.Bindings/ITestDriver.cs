namespace TALXIS.TestKit.Bindings
{
    using System;
    using Microsoft.Xrm.Sdk;

    /// <summary>
    /// An interface for a test driver used to perform setup and teardown via the Power Apps Client APIs.
    /// </summary>
    public interface ITestDriver
    {
        /// <summary>
        /// Injects the driver onto the current page.
        /// </summary>
        /// <param name="authToken">The application user auth token (if configured).</param>
        /// <param name="dataverseUrl">The URL of a separate Dataverse environment holding the test data (if configured).</param>
        /// <param name="suppressFlows">Whether test data operations of the application user should not trigger Power Automate flows.</param>
        void InjectOnPage(string authToken, string dataverseUrl, bool suppressFlows);

        /// <summary>
        /// Loads scenario test data.
        /// </summary>
        /// <param name="data">The data to load.</param>
        /// <param name="username">The username of the user to impersonate.</param>
        void LoadTestDataAsUser(string data, string username);

        /// <summary>
        /// Loads scenario test data.
        /// </summary>
        /// <param name="data">The data to load.</param>
        void LoadTestData(string data);

        /// <summary>
        /// Deletes scenario test data.
        /// </summary>
        void DeleteTestData();

        /// <summary>
        /// Registers a record created outside the test data (e.g. through the UI) so that it is deleted with the test data.
        /// </summary>
        /// <param name="logicalName">The logical name of the record's table.</param>
        /// <param name="id">The ID of the record.</param>
        void TrackTestRecord(string logicalName, Guid id);

        /// <summary>
        /// Open a test record.
        /// </summary>
        /// <param name="recordAlias">The alias of the record.</param>
        void OpenTestRecord(string recordAlias);

        /// <summary>
        /// Gets an entity reference to a previously created test record.
        /// </summary>
        /// <param name="recordAlias">The alias of the test record.</param>
        /// <returns>A reference to the created record.</returns>
        /// <exception cref="System.ArgumentException">No test record has the alias.</exception>
        EntityReference GetTestRecordReference(string recordAlias);
    }
}
