// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using OpenQA.Selenium;
using TALXIS.TestKit.Selectors.WebClientManagement;
using TALXIS.TestKit.Selectors.Browser;

namespace TALXIS.TestKit.Selectors
{
    /// <summary>
    ///  SubGrid Grid class.
    ///  </summary>
    public class SubGrid : Element
    {
        private readonly GridManager _gridManager;
        private readonly EntityManager _entityManager;
        public SubGrid(WebClient client) : base()
        {
            _gridManager = new GridManager(client);
            _entityManager = new EntityManager(client);
        }
        /// <summary>
        /// Returns HTML of Grid
        /// </summary>
        public string GetSubGridControl(string subGridName)
        {
            return _gridManager.GetSubGridControl(subGridName);
        }
        /// <summary>
        /// Clicks a button in the subgrid menu
        /// </summary>
        /// <param name="name">Name of the button to click</param>
        /// <param name="subName">Name of the submenu button to click</param>
        public void ClickCommand(string subGridName, string name, string subName = null, string subSecondName = null)
        {
            _gridManager.ClickSubGridCommand(subGridName, name, subName, subSecondName);
        }

        /// <summary>
        /// Select all records in a subgrid
        /// </summary>
        /// <param name="subGridName">Schema name of the subgrid to select all</param>
        /// <param name="thinkTime">Additional time to wait, if required</param>
        public void ClickSubgridSelectAll(string subGridName, int thinkTime = Constants.DefaultThinkTime)
        {
            _gridManager.ClickSubgridSelectAll(subGridName, thinkTime);
        }

        /// <summary>
        /// Retrieve the items from a subgrid
        /// </summary>
        /// <param name="subgridName">schemaName of the SubGrid control to retrieve items from</param>
        public List<GridItem> GetSubGridItems(string subgridName)
        {
            return _gridManager.GetSubGridItems(subgridName);
        }

        public IWebElement TryToFindAssertSubgridCommand(string commandName, string subGridName)
        {
            return _entityManager.TryToFindAssertSubgridCommand(commandName, subGridName);
        }

        /// <summary>
        /// Gets a value indicating whether a command of a subgrid is enabled: some commands stay shown, greyed out, when
        /// their enable rule fails rather than being removed by a display rule.
        /// </summary>
        /// <returns>Whether the command is enabled, or null when it is not shown.</returns>
        public bool? IsCommandEnabled(string subGridName, string commandName)
        {
            return _entityManager.IsSubgridCommandEnabled(commandName, subGridName);
        }

        public void GetFlyoutContainer()
        {
            _entityManager.GetFlyoutContainer();
        }

        public void WaitUntilFlyoutCommandVisible(string commandName, TimeSpan timeout)
        {
            _entityManager.WaitUntilFlyoutCommandVisible(commandName, timeout);
        }

        /// <summary>
        /// Open a record on a subgrid
        /// </summary>
        /// <param name="subgridName">schemaName of the SubGrid control</param>
        /// <param name="index">Index of the record to open</param>
        public void OpenSubGridRecord(string subgridName, int index = 0)
        {
            _gridManager.OpenSubGridRecord(subgridName, index);
        }

        /// <summary>
        /// Performs a Search on the subgrid
        /// </summary>
        /// <param name="searchCriteria">Search term</param>
        /// <param name="clearByDefault">Determines whether to clear the search field before supplying the search term</param>
        public void Search(string subGridName, string searchCriteria, bool clearField = false)
        {
            _gridManager.SearchSubGrid(subGridName, searchCriteria, clearField);
        }

        /// <summary>
        /// Switches the View on a SubGrid
        /// </summary>
        /// <param name="subgridName">schemaName of the SubGrid control</param>
        /// <param name="viewName">Name of the view to select</param>
        public void SwitchView(string subgridName, string viewName)
        {
            _gridManager.SwitchSubGridView(subgridName, viewName);
        }
    }
}
            