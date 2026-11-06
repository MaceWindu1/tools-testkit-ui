namespace TALXIS.TestKit.Bindings.Extensions
{
    using System;
    using System.Globalization;
    using OpenQA.Selenium;
    using TALXIS.TestKit.Selectors;
    using TALXIS.TestKit.Selectors.Browser;

    /// <summary>
    /// Extensions to the <see cref="SubGrid"/> class.
    /// </summary>
    public static class SubGridExtensions
    {
        /// <summary>
        /// Gets the index of a record by ID.
        /// Accessing grid item IDs or URLs via EasyRepro is currently broken (see https://github.com/microsoft/EasyRepro/issues/800). Use this method instead.
        /// </summary>
        /// <param name="subGrid">The SubGrid.</param>
        /// <param name="subgridName">The name of the subgrid.</param>
        /// <param name="driver">The Selenium WebDriver.</param>
        /// <param name="recordId">The ID of the record.</param>
        /// <returns>The index of the record.</returns>
        public static int GetRecordIndexById(this SubGrid subGrid, string subgridName, IWebDriver driver, Guid recordId)
        {
            subGrid = subGrid ?? throw new ArgumentNullException(nameof(subGrid));

            return subGrid.GetRecordIndexById(subgridName, recordId);
        }

        /// <summary>
        /// Gets the index of a record by ID.
        /// Accessing grid item IDs or URLs via EasyRepro is currently broken (see https://github.com/microsoft/EasyRepro/issues/800). Use this method instead.
        /// </summary>
        /// <param name="subGrid">The SubGrid.</param>
        /// <param name="subgridName">The name of the subgrid.</param>
        /// <param name="driver">The Selenium WebDriver.</param>
        /// <param name="index">The index of the record.</param>
        public static void HighlightRecord(this SubGrid subGrid, string subgridName, IWebDriver driver, int index)
        {
            subGrid = subGrid ?? throw new ArgumentNullException(nameof(subGrid));

            subGrid.HighlightRecord(subgridName, index);
        }
    }
}