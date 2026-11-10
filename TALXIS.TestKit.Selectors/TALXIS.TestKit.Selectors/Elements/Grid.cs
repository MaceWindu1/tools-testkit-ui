// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using TALXIS.TestKit.Selectors.WebClientManagement;
using TALXIS.TestKit.Selectors.Browser;

namespace TALXIS.TestKit.Selectors
{
    public class Grid : Element
    {
        private readonly GridManager _gridManager;
        public enum GridType { PowerAppsGridControl, LegacyReadOnlyGrid, ReadOnlyGrid }
        public Grid(WebClient client) : base()
        {
            _gridManager = new GridManager(client);
        }

        /// <summary>
        /// Returns HTML of Grid
        /// </summary>
        public string GetGridControl()
        {
            return _gridManager.GetGridControl();
        }

        /// <summary>
        /// Switches the view to the view supplied
        /// </summary>
        /// <param name="viewName">Name of the view to select</param>
        public void SwitchView(string viewName, string subViewName = null)
        {
            _gridManager.SwitchView(viewName, subViewName);
        }

        /// <summary>
        /// Opens a record in the grid
        /// </summary>
        /// <param name="index">The index of the row to open</param>
        public void OpenRecord(int index)
        {
            _gridManager.OpenRecord(index);
        }

        /// <summary>
        /// Checks / Ticks a record in the grid
        /// </summary>
        /// <param name="index">The index of the row to open</param>
        public void HighLightRecord(int index)
        {
            _gridManager.OpenRecord(index, checkRecord: true);
        }

        /// <summary>
        /// Performs a Quick Find on the grid
        /// </summary>
        /// <param name="searchCriteria">Search term</param>
        /// <param name="clearByDefault">Determines whether to clear the quick find field before supplying the search term</param>
        public void Search(string searchCriteria, bool clearByDefault = true)
        {
            _gridManager.Search(searchCriteria, clearByDefault);
        }

        /// <summary>
        /// Clears the quick find field
        /// </summary>
        public void ClearSearch()
        {
            _gridManager.ClearSearch();
        }

        /// <summary>
        /// Returns a list of items from the current grid
        /// </summary>
        /// <returns></returns>
        public List<GridItem> GetGridItems()
        {
            return _gridManager.GetGridItems();
        }

        /// <summary>
        /// Gets the index of a record in the grid by its id. The ids of <see cref="GetGridItems"/> are wrong
        /// (https://github.com/microsoft/EasyRepro/issues/800).
        /// </summary>
        /// <param name="recordId">The id of the record.</param>
        /// <returns>The index, or -1 when the grid does not show the record.</returns>
        public int GetRecordIndexById(Guid recordId)
        {
            return _gridManager.GetRecordIndexById(recordId);
        }

        /// <summary>
        /// Sorts the grid by the column provided
        /// </summary>
        /// <param name="columnName">Label of the column name</param>
        /// <param name="sortOptionButtonText">Sort option button text</param>
        public void Sort(string columnName, string sortOptionButtonText)
        {
            _gridManager.Sort(columnName, sortOptionButtonText);
        }

        /// <summary>
        /// Filter the grid by the Character provided
        /// </summary>
        /// <param name="filter">Label of footer filter - View records that starts with this letter</param>
        public void FilterByLetter(char filter)
        {
            _gridManager.FilterByLetter(filter);
        }
    }
}