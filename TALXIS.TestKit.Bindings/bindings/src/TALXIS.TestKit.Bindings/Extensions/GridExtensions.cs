namespace TALXIS.TestKit.Bindings.Extensions
{
    using System;
    using OpenQA.Selenium;
    using TALXIS.TestKit.Selectors;

    /// <summary>
    /// Extensions to the <see cref="Grid"/> class.
    /// </summary>
    public static class GridExtensions
    {
        /// <summary>
        /// Gets the index of a record by ID.
        /// Accessing grid item IDs or URLs via EasyRepro is currently broken (see https://github.com/microsoft/EasyRepro/issues/800). Use this method instead.
        /// </summary>
        /// <param name="grid">The Grid.</param>
        /// <param name="driver">The Selenium WebDriver.</param>
        /// <param name="recordId">The ID of the record.</param>
        /// <returns>The index of the record.</returns>
        [Obsolete("Use Grid.GetRecordIndexById(recordId), which returns -1 when the grid does not show the record.")]
        public static int GetRecordIndexById(this Grid grid, IWebDriver driver, Guid recordId)
        {
            grid = grid ?? throw new ArgumentNullException(nameof(grid));

            var index = grid.GetRecordIndexById(recordId);
            if (index == -1)
            {
                throw new Exception($"A record with an ID of {recordId} is not present in the grid");
            }

            return index;
        }
    }
}
