namespace TALXIS.TestKit.Bindings.Extensions
{
    using System;
    using System.Collections.Generic;
    using OpenQA.Selenium;

    /// <summary>
    /// Whether the signed-in user can read a record, and why not when the query failed.
    /// </summary>
    /// <param name="Readable">Whether the query returned the record.</param>
    /// <param name="Error">The error of the query, when it failed.</param>
    public sealed record RecordAccess(bool Readable, string Error);

    /// <summary>
    /// Reads records as the signed-in user through the page's Xrm.WebApi, as the app's own pages do: a model-driven app
    /// applies the user's security roles, a TALXIS Portal also its retrieve filters.
    /// </summary>
    public static class RecordAccessExtensions
    {
        // Queries the record by its id, so that a record the filters exclude comes back as no record rather than an error.
        // The result has no 'error' key: Selenium reads a returned object with one as a failed command.
        private const string ReadScript =
            @"const [entity, id] = arguments;
              return Xrm.WebApi.retrieveMultipleRecords(entity, '?$select=' + entity + 'id&$filter=' + entity + 'id eq ' + id)
                .then(result => ({ count: result.entities.length }),
                      error => ({ failure: (error && (error.message || JSON.stringify(error))) || String(error) }));";

        /// <summary>
        /// Reads a record as the signed-in user.
        /// </summary>
        /// <param name="driver">The web driver, on a page of the app.</param>
        /// <param name="logicalName">The record's table.</param>
        /// <param name="id">The record's id.</param>
        /// <returns>Whether the user can read the record.</returns>
        public static RecordAccess ReadRecordAccess(this IWebDriver driver, string logicalName, Guid id)
        {
            var result = (IDictionary<string, object>)((IJavaScriptExecutor)driver).ExecuteScript(ReadScript, logicalName, id.ToString("D"));
            return result.TryGetValue("failure", out var failure)
                ? new RecordAccess(false, failure?.ToString())
                : new RecordAccess(Convert.ToInt64(result["count"]) > 0, null);
        }
    }
}
