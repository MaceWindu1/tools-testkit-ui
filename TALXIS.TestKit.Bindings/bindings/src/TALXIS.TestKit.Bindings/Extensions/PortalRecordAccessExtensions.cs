namespace TALXIS.TestKit.Bindings.Extensions
{
    using System;
    using System.Collections.Generic;
    using OpenQA.Selenium;

    /// <summary>
    /// Whether the signed-in user of a TALXIS Portal can read a record, and why not when the query failed.
    /// </summary>
    /// <param name="Readable">Whether the portal returned the record.</param>
    /// <param name="Error">The error of the query, when it failed.</param>
    public sealed record PortalRecordAccess(bool Readable, string Error);

    /// <summary>
    /// Reads records as the signed-in TALXIS Portal user: through the page's Xrm.WebApi, to which the portal applies its
    /// retrieve filters, as it does to every query of its pages.
    /// </summary>
    public static class PortalRecordAccessExtensions
    {
        // Queries the record by its id, so that a record the filters exclude comes back as no record rather than an error.
        // The result has no 'error' key: Selenium reads a returned object with one as a failed command.
        private const string ReadScript =
            @"const [entity, id] = arguments;
              return Xrm.WebApi.retrieveMultipleRecords(entity, '?$select=' + entity + 'id&$filter=' + entity + 'id eq ' + id)
                .then(result => ({ count: result.entities.length }),
                      error => ({ failure: (error && (error.message || JSON.stringify(error))) || String(error) }));";

        /// <summary>
        /// Reads a record as the signed-in portal user.
        /// </summary>
        /// <param name="driver">The web driver, on a portal page.</param>
        /// <param name="logicalName">The record's table.</param>
        /// <param name="id">The record's id.</param>
        /// <returns>Whether the user can read the record.</returns>
        public static PortalRecordAccess ReadPortalRecordAccess(this IWebDriver driver, string logicalName, Guid id)
        {
            var result = (IDictionary<string, object>)((IJavaScriptExecutor)driver).ExecuteScript(ReadScript, logicalName, id.ToString("D"));
            return result.TryGetValue("failure", out var failure)
                ? new PortalRecordAccess(false, failure?.ToString())
                : new PortalRecordAccess(Convert.ToInt64(result["count"]) > 0, null);
        }
    }
}
