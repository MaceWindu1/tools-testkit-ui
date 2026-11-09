using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using TALXIS.TestKit.Selectors.DTO.Locators;
using TALXIS.TestKit.Selectors.WebClientManagement.Helpers;
using TALXIS.TestKit.Selectors.Browser;

namespace TALXIS.TestKit.Selectors.WebClientManagement
{
    public class GridManager
    {
        public WebClient Client { get; }

        public GridManager(WebClient client)
        {
            Client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public BrowserCommandResult<bool> ClickRelatedCommand(string name, string subName = null, string subSecondName = null)
        {
            return this.Client.Execute(Client.GetOptions("Click Related Tab Command"), driver =>
            {
                // Locate Related Command Bar Button List
                var relatedCommandBarButtonList = driver.WaitUntilAvailable(RelatedElementsLocators.CommandBarButtonList);

                // Validate list has provided command bar button
                if (relatedCommandBarButtonList.HasElement(RelatedElementsLocators.CommandBarButton( name)))
                {
                    relatedCommandBarButtonList.FindElement(RelatedElementsLocators.CommandBarButton( name)).Click(true);

                    driver.WaitForTransaction();

                    if (subName != null)
                    {
                        //Look for Overflow flyout
                        if (driver.HasElement(RelatedElementsLocators.CommandBarOverflowContainer))
                        {
                            var overFlowContainer = driver.FindElement(RelatedElementsLocators.CommandBarOverflowContainer);

                            if (!overFlowContainer.HasElement(RelatedElementsLocators.CommandBarSubButton( subName)))
                                throw new NotFoundException($"{subName} button not found");

                            overFlowContainer.FindElement(RelatedElementsLocators.CommandBarSubButton( subName)).Click(true);

                            driver.WaitForTransaction();
                        }

                        if (subSecondName != null)
                        {
                            //Look for Overflow flyout
                            if (driver.HasElement(RelatedElementsLocators.CommandBarOverflowContainer))
                            {
                                var overFlowContainer = driver.FindElement(RelatedElementsLocators.CommandBarOverflowContainer);

                                if (!overFlowContainer.HasElement(RelatedElementsLocators.CommandBarSubButton( subName)))
                                    throw new NotFoundException($"{subName} button not found");

                                overFlowContainer.FindElement(RelatedElementsLocators.CommandBarSubButton( subName)).Click(true);

                                driver.WaitForTransaction();
                            }
                        }
                    }

                    return true;
                }
                else
                {
                    // Button was not found, check if we should be looking under More Commands (OverflowButton)
                    var moreCommands = relatedCommandBarButtonList.HasElement(RelatedElementsLocators.CommandBarOverflowButton);

                    if (moreCommands)
                    {
                        var overFlowButton = relatedCommandBarButtonList.FindElement(RelatedElementsLocators.CommandBarOverflowButton);
                        overFlowButton.Click(true);

                        if (driver.HasElement(RelatedElementsLocators.CommandBarOverflowContainer)) //Look for Overflow
                        {
                            var overFlowContainer = driver.FindElement(RelatedElementsLocators.CommandBarOverflowContainer);

                            if (overFlowContainer.HasElement(RelatedElementsLocators.CommandBarButton( name)))
                            {
                                overFlowContainer.FindElement(RelatedElementsLocators.CommandBarButton( name)).Click(true);

                                driver.WaitForTransaction();

                                if (subName != null)
                                {
                                    overFlowContainer = driver.FindElement(RelatedElementsLocators.CommandBarOverflowContainer);

                                    if (!overFlowContainer.HasElement(RelatedElementsLocators.CommandBarSubButton(subName)))
                                        throw new NotFoundException($"{subName} button not found");

                                    overFlowContainer.FindElement(RelatedElementsLocators.CommandBarSubButton(subName)).Click(true);

                                    driver.WaitForTransaction();

                                    if (subSecondName != null)
                                    {
                                        overFlowContainer = driver.FindElement(RelatedElementsLocators.CommandBarOverflowContainer);

                                        if (!overFlowContainer.HasElement(RelatedElementsLocators.CommandBarSubButton( subName)))
                                            throw new NotFoundException($"{subName} button not found");

                                        overFlowContainer.FindElement(RelatedElementsLocators.CommandBarSubButton( subName)).Click(true);

                                        driver.WaitForTransaction();
                                    }
                                }

                                return true;
                            }
                        }
                        else
                        {
                            throw new NotFoundException($"{name} button not found in the More Commands container. Button names are case sensitive. Please check for proper casing of button name.");
                        }

                    }
                    else
                    {
                        throw new NotFoundException($"{name} button not found. Button names are case sensitive. Please check for proper casing of button name.");
                    }
                }

                return true;
            });
        }

        /// This method is obsolete. Do not use.
        public BrowserCommandResult<bool> ClickSubgridAddButton(string subgridName, int thinkTime = Constants.DefaultThinkTime)
        {
            Client.ThinkTime(thinkTime);

            return Client.Execute(Client.GetOptions($"Click add button of subgrid: {subgridName}"), driver =>
            {
                driver.FindElement(EntityElementsLocators.SubGridAddButton( subgridName))?.Click();

                return true;
            });
        }

        public BrowserCommandResult<bool> FilterByAll(int thinkTime = Constants.DefaultThinkTime)
        {
            Client.ThinkTime(thinkTime);

            return this.Client.Execute(Client.GetOptions("Filter by All Records"), driver =>
            {
                var jumpBar = driver.FindElement(GridElementsLocators.JumpBar);
                var link = jumpBar.FindElement(GridElementsLocators.FilterByAll);

                if (link != null)
                {
                    link.Click();

                    driver.WaitForTransaction();
                }
                else
                {
                    throw new Exception($"Filter by All link does not exist");
                }

                return true;
            });
        }

        public BrowserCommandResult<bool> FilterByLetter(char filter, int thinkTime = Constants.DefaultThinkTime)
        {
            Client.ThinkTime(thinkTime);

            if (!Char.IsLetter(filter) && filter != '#')
                throw new InvalidOperationException("Filter criteria is not valid.");

            return this.Client.Execute(Client.GetOptions("Filter by Letter"), driver =>
            {
                var jumpBar = driver.FindElement(GridElementsLocators.JumpBar);
                var link = jumpBar.FindElement(By.Id(filter + "_link"));

                if (link != null)
                {
                    link.Click();

                    driver.WaitForTransaction();
                }
                else
                {
                    throw new Exception($"Filter with letter: {filter} link does not exist");
                }

                return true;
            });
        }

        public BrowserCommandResult<string> GetGridControl()
        {

            return Client.Execute(Client.GetOptions($"Get Grid Control"), driver =>
            {
                var gridContainer = driver.FindElement(By.XPath("//div[contains(@data-lp-id,'MscrmControls.Grid')]"));

                return gridContainer.GetAttribute("innerHTML");
            });
        }

        public BrowserCommandResult<string> GetSubGridControl(string subGridName)
        {

            return Client.Execute(Client.GetOptions($"Get Sub Grid Control"), driver =>
            {
                var subGrid = driver.FindElement(EntityElementsLocators.SubGridContents( subGridName));

                return subGrid.GetAttribute("innerHTML");
            });
        }

        public BrowserCommandResult<bool> HideChart(int thinkTime = Constants.DefaultThinkTime)
        {
            Client.ThinkTime(thinkTime);

            return this.Client.Execute(Client.GetOptions("Hide Chart"), driver =>
            {
                if (driver.HasElement(GridElementsLocators.HideChart))
                {
                    driver.ClickWhenAvailable(GridElementsLocators.HideChart);

                    driver.WaitForTransaction();
                }
                else
                {
                    throw new Exception("The Hide Chart button does not exist.");
                }

                return true;
            });
        }

        /// <summary>
        /// Opens the grid record.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="thinkTime">Used to simulate a wait time between human interactions. The Default is 2 seconds.</param>
        public BrowserCommandResult<bool> OpenRelatedGridRow(int index, int thinkTime = Constants.DefaultThinkTime)
        {
            Client.ThinkTime(thinkTime);

            return this.Client.Execute(Client.GetOptions("Open Grid Item"), driver =>
            {
                var grid = driver.FindElement(GridElementsLocators.Container);
                var rows = grid
                    .FindElement(GridElementsLocators.CellContainer)
                    .FindElements(GridElementsLocators.Rows);

                if (rows.Count <= 0)
                {
                    return true;
                }
                else if (index + 1 > rows.Count)
                {
                    throw new IndexOutOfRangeException($"Grid record count: {rows.Count}. Expected: {index + 1}");
                }

                var row = rows.ElementAt(index + 1);
                var cell = row.FindElements(EntityElementsLocators.SubGridCells).ElementAt(1);

                new Actions(driver).DoubleClick(cell).Perform();
                driver.WaitForTransaction();

                return true;
            });
        }

        public BrowserCommandResult<Dictionary<string, IWebElement>> OpenViewPicker(int thinkTime = Constants.DefaultThinkTime)
        {
            Client.ThinkTime(thinkTime);

            return Client.Execute(Client.GetOptions("Open View Picker"), driver =>
            {
                driver.ClickWhenAvailable(GridElementsLocators.ViewSelector,
                    TimeSpan.FromSeconds(20),
                    "Unable to click the View Picker"
                );

                var viewContainer = driver.FindElement(GridElementsLocators.ViewContainer);
                var viewItems = viewContainer.FindElements(By.TagName("li"));

                var result = new Dictionary<string, IWebElement>();
                foreach (var viewItem in viewItems)
                {
                    var role = viewItem.GetAttribute("role");

                    if (role != "presentation")
                        continue;

                    //var key = viewItem.FindElement(GridElementsLocators.ViewSelectorMenuItem).Text.ToLowerString();
                    var key = viewItem.Text.ToLowerString();

                    if (string.IsNullOrWhiteSpace(key))
                        continue;

                    if (!result.ContainsKey(key))
                        result.Add(key, viewItem);
                }
                return result;
            });
        }

        public BrowserCommandResult<bool> SelectRecord(int index, int thinkTime = Constants.DefaultThinkTime)
        {
            Client.ThinkTime(thinkTime);

            return this.Client.Execute(Client.GetOptions("Select Grid Record"), driver =>
            {
                var container = driver.WaitUntilAvailable(GridElementsLocators.RowsContainer, "Grid Container does not exist.");

                var row = container.FindElement(By.Id("id-cell-" + index + "-1"));

                if (row == null)
                    throw new Exception($"Row with index: {index} does not exist.");

                row.Click();

                return true;
            });
        }

        public BrowserCommandResult<bool> ShowChart(int thinkTime = Constants.DefaultThinkTime)
        {
            Client.ThinkTime(thinkTime);

            return this.Client.Execute(Client.GetOptions("Show Chart"), driver =>
            {
                if (driver.HasElement(GridElementsLocators.ShowChart))
                {
                    driver.ClickWhenAvailable(GridElementsLocators.ShowChart);

                    driver.WaitForTransaction();
                }
                else
                {
                    throw new Exception("The Show Chart button does not exist.");
                }

                return true;
            });
        }

        public BrowserCommandResult<bool> Sort(string columnName, string sortOptionButtonText, int thinkTime = Constants.DefaultThinkTime)
        {
            Client.ThinkTime(thinkTime);

            return this.Client.Execute(Client.GetOptions($"Sort by {columnName}"), driver =>
            {
                var sortCol = driver.FindElement(By.XPath(AppElements.Xpath[AppReference.Grid.GridSortColumn].Replace("[COLNAME]", columnName)));

                if (sortCol == null)
                    throw new InvalidOperationException($"Column: {columnName} Does not exist");
                else
                {
                    sortCol.Click(true);
                    driver.WaitUntilClickable(By.XPath($@"//button[@name='{sortOptionButtonText}']")).Click(true);
                }

                driver.WaitForTransaction();

                return true;
            });
        }

        public BrowserCommandResult<bool> SwitchChart(string chartName, int thinkTime = Constants.DefaultThinkTime)
        {
            Client.ThinkTime(thinkTime);

            if (!Client.Browser.Driver.IsVisible(GridElementsLocators.ChartSelector))
                ShowChart();

            Client.ThinkTime(1000);

            return this.Client.Execute(Client.GetOptions("Switch Chart"), driver =>
            {
                driver.ClickWhenAvailable(GridElementsLocators.ChartSelector);

                var list = driver.FindElement(GridElementsLocators.ChartViewList);

                driver.ClickWhenAvailable(By.XPath("//li[contains(@title,'" + chartName + "')]"));

                return true;
            });
        }

        internal BrowserCommandResult<bool> ClearSearch(int thinkTime = Constants.DefaultThinkTime)
        {
            Client.ThinkTime(thinkTime);

            return this.Client.Execute(Client.GetOptions($"Clear Search"), driver =>
            {
                driver.WaitUntilClickable(GridElementsLocators.QuickFind);

                driver.FindElement(GridElementsLocators.QuickFind).Clear();

                return true;
            });
        }

        internal BrowserCommandResult<bool> ClickSubGridCommand(string subGridName, string name, string subName = null, string subSecondName = null)
        {
            return Client.Execute(Client.GetOptions($"Click SubGrid Command: {name}"), driver =>
            {
                // Find SubGrid
                var subGrid = driver.FindElement(EntityElementsLocators.SubGridContents( subGridName))
                    ?? throw new NotFoundException($"Unable to locate subgrid contents for {subGridName}.");

                // Find the command panel
                if (!subGrid.TryFindElement(EntityElementsLocators.SubGridCommandBar( subGridName), out var subGridCommandBar))
                    throw new InvalidOperationException($"Unable to locate the Commandbar for {subGridName}.");

                // Click on a command (if it is in the CommandBar)
                if (TryClickCommand(subGridCommandBar, name, driver))
                    return true;

                // Click on a command in “More Commands”
                if (!TryClickMoreCommands(subGridCommandBar, name, driver, subGridName))
                    throw new InvalidOperationException($"No command with the name '{name}' exists inside {subGridName} Commandbar.");

                // Click on nested commands (if `subName` and `subSecondName` are passed)
                return ClickSubMenuCommands(driver, name, subName, subSecondName, subGridName);
            });
        }

        internal BrowserCommandResult<bool> ClickSubgridSelectAll(string subGridName, int thinkTime = Constants.DefaultThinkTime)
        {
            Client.ThinkTime(thinkTime);

            return this.Client.Execute(Client.GetOptions($"Click Select All Button on subgrid: {subGridName}"), driver =>
            {
                // Find the SubGrid
                var subGrid = driver.WaitUntilAvailable(
                    EntityElementsLocators.SubGridContents( subGridName),
                    5.Seconds(),
                    $"Unable to find subgrid named {subGridName}.");

                subGrid.ClickWhenAvailable(By.XPath("//div[@role='columnheader']//span[@role='checkbox']"));
                driver.WaitForTransaction();

                return true;
            });
        }

        internal BrowserCommandResult<bool> FirstPage(int thinkTime = Constants.DefaultThinkTime)
        {
            Client.ThinkTime(thinkTime);

            return this.Client.Execute(Client.GetOptions($"First Page"), driver =>
            {
                driver.ClickWhenAvailable(GridElementsLocators.FirstPage);

                driver.WaitForTransaction();

                return true;
            });
        }

        internal BrowserCommandResult<int> GetRecordIndexById(Guid recordId)
        {
            return Client.Execute(Client.GetNoRetryOptions($"Get Grid Record Index: {recordId}"), driver =>
            {
                driver.WaitUntilAvailable(GridElementsLocators.Container);

                // The app's grid state has the records by their ids.
                var index = (long)((IJavaScriptExecutor)driver).ExecuteScript(
                    "return Object.keys(getCurrentXrmStatus().mainGrid._grid.getRows()._collection).indexOf(arguments[0]);",
                    recordId.ToString());

                return Convert.ToInt32(index);
            });
        }

        internal BrowserCommandResult<List<GridItem>> GetGridItems(int thinkTime = Constants.DefaultThinkTime)
        {
            Client.ThinkTime(thinkTime);

            return this.Client.Execute(Client.GetOptions("Get Grid Items"), driver =>
            {
                var returnList = new List<GridItem>();
                //#1294
                var gridContainer = driver.FindElement(By.XPath("//div[contains(@data-id,'data-set-body-container')]/div"));
                string[] gridDataId = gridContainer.GetAttribute("data-lp-id").Split('|');
                Dictionary<string, object> WindowStateData = (Dictionary<string, object>)driver.ExecuteScript($"return window[Object.keys(window).find(i => !i.indexOf(\"__store$\"))].getState().data");
                string keyForData = GetGridQueryKey(driver, null);
                //Get Data Store

                Dictionary<string, object> WindowStateDataLists = (Dictionary<string, object>)WindowStateData["lists"];

                //Find Data by Key
                Dictionary<string, object> WindowStateDataKeyedForData = (Dictionary<string, object>)WindowStateDataLists[keyForData];
                //Find Record Ids for Key Data Set
                ReadOnlyCollection<object> WindowStateDataKeyedForDataRecordsIds = (ReadOnlyCollection<object>)WindowStateDataKeyedForData["records"];

                //Get Data
                Dictionary<string, object> WindowStateEntityData = (Dictionary<string, object>)WindowStateData["entities"];

                if (!WindowStateEntityData.ContainsKey(gridDataId[2])) return returnList;

                Dictionary<string, object> WindowStateEntityDataEntity = (Dictionary<string, object>)WindowStateEntityData[gridDataId[2]];

                foreach (Dictionary<string, object> record in WindowStateDataKeyedForDataRecordsIds)
                {
                    Dictionary<string, object> recordId = (Dictionary<string, object>)record["id"];
                    Dictionary<string, object> definedRecord = (Dictionary<string, object>)WindowStateEntityDataEntity[(string)recordId["guid"]];
                    Dictionary<string, object> attributes = (Dictionary<string, object>)definedRecord["fields"];
                    GridItem gridItem = new GridItem()
                    {
                        EntityName = gridDataId[2],
                        Id = new Guid((string)recordId["guid"])
                    };
                    ProcessGridRowAttributes(attributes, gridItem);
                    returnList.Add(gridItem);
                }

                return returnList;
            });
        }

        internal BrowserCommandResult<List<GridItem>> GetSubGridItems(string subgridName)
        {
            return this.Client.Execute(Client.GetOptions($"Get Subgrid Items for Subgrid {subgridName}"), driver =>
            {
                // Initialize return object
                List<GridItem> subGridRows = new List<GridItem>();

                // Initialize required local variables
                IWebElement subGridRecordList = null;
                List<string> columns = new List<string>();
                List<string> cellValues = new List<string>();
                GridItem item = new GridItem();
                Dictionary<string, object> WindowStateData = (Dictionary<string, object>)driver.ExecuteScript($"return JSON.parse(JSON.stringify(window[Object.keys(window).find(i => !i.indexOf(\"__store$\"))].getState().data))");

                // Find the SubGrid
                var subGrid = driver.FindElement(EntityElementsLocators.SubGridContents(subgridName));

                if (subGrid == null)
                    throw new NotFoundException($"Unable to locate subgrid contents for {subgridName} subgrid.");

                // Check if ReadOnlyGrid was found
                if (subGrid.TryFindElement(EntityElementsLocators.SubGridListCells( subgridName), out subGridRecordList))
                {

                    // Locate record list
                    var foundRecords = subGrid.TryFindElement(EntityElementsLocators.SubGridListCells( subgridName), out subGridRecordList);

                    if (foundRecords)
                    {
                        var subGridRecordRows = subGrid.FindElements(EntityElementsLocators.SubGridList( subgridName));
                        var SubGridContainer = driver.FindElement(EntityElementsLocators.SubGridContents(subgridName));
                        string[] gridDataId = SubGridContainer.FindElement(By.XPath($"//div[contains(@data-lp-id,'{subgridName}')]")).GetAttribute("data-lp-id").Split('|');
                        //Need to add entity name
                        string keyForData = GetGridQueryKey(driver, gridDataId[2] + ":" + subgridName);

                        Dictionary<string, object> WindowStateDataLists = (Dictionary<string, object>)WindowStateData["lists"];

                        //Find Data by Key
                        Dictionary<string, object> WindowStateDataKeyedForData = (Dictionary<string, object>)WindowStateDataLists[keyForData];
                        //Find Record Ids for Key Data Set
                        ReadOnlyCollection<object> WindowStateDataKeyedForDataRecordsIds = (ReadOnlyCollection<object>)WindowStateDataKeyedForData["records"];

                        //Get Data
                        Dictionary<string, object> WindowStateEntityData = (Dictionary<string, object>)WindowStateData["entities"];
                        Dictionary<string, object> WindowStateEntityDataEntity = (Dictionary<string, object>)WindowStateEntityData[gridDataId[2]];
                        foreach (Dictionary<string, object> record in WindowStateDataKeyedForDataRecordsIds)
                        {
                            Dictionary<string, object> recordId = (Dictionary<string, object>)record["id"];
                            //Dictionary<string, object> definedRecord = (Dictionary<string, object>)WindowStateEntityDataEntity[(string)recordId["guid"]];
                            //Dictionary<string, object> attributes = (Dictionary<string, object>)definedRecord["fields"];
                            GridItem gridItem = new GridItem()
                            {
                                EntityName = gridDataId[2],
                                Id = new Guid((string)recordId["guid"])
                            };
                            //ProcessGridRowAttributes(attributes, gridItem);
                            subGridRows.Add(gridItem);
                        }
                    }
                    else
                    {
                        throw new NotFoundException($"Unable to locate record list for subgrid {subgridName}");
                    }
                }
                // Attempt to locate the editable grid list
                else if (subGrid.TryFindElement(EntityElementsLocators.EditableSubGridList( subgridName), out subGridRecordList))
                {
                    //Find the columns
                    var headerCells = subGrid.FindElements(EntityElementsLocators.SubGridHeadersEditable);

                    foreach (IWebElement headerCell in headerCells)
                    {
                        var headerTitle = headerCell.GetAttribute("title");
                        columns.Add(headerTitle);
                    }

                    //Find the rows
                    var rows = subGrid.FindElements(EntityElementsLocators.SubGridDataRowsEditable);

                    //Process each row
                    foreach (IWebElement row in rows)
                    {
                        var cells = row.FindElements(EntityElementsLocators.SubGridCells);

                        if (cells.Count > 0)
                        {
                            foreach (IWebElement thisCell in cells)
                                cellValues.Add(thisCell.Text);

                            for (int i = 0; i < columns.Count; i++)
                            {
                                //The first cell is always a checkbox for the record.  Ignore the checkbox.
                                if (i == 0)
                                {
                                    // Do Nothing
                                }
                                else
                                {
                                    item[columns[i]] = cellValues[i];
                                }
                            }

                            subGridRows.Add(item);

                            // Flush Item and Cell Values To Get New Rows
                            cellValues = new List<string>();
                            item = new GridItem();
                        }
                    }

                    return subGridRows;
                }
                // Special 'Related' high density grid control for entity forms
                else if (subGrid.TryFindElement(EntityElementsLocators.SubGridHighDensityList( subgridName), out subGridRecordList))
                {
                    //Find the columns
                    var headerCells = subGrid.FindElements(EntityElementsLocators.SubGridHeadersHighDensity);

                    foreach (IWebElement headerCell in headerCells)
                    {
                        var headerTitle = headerCell.GetAttribute("data-id");
                        columns.Add(headerTitle);
                    }

                    //Find the rows
                    var rows = subGrid.FindElements(EntityElementsLocators.SubGridRowsHighDensity);

                    //Process each row
                    foreach (IWebElement row in rows)
                    {
                        //Get the entityId and entity Type
                        if (row.GetAttribute("data-lp-id") != null)
                        {
                            var rowAttributes = row.GetAttribute("data-lp-id").Split('|');
                            item.EntityName = rowAttributes[4];
                            //The row record IDs are not in the DOM. Must be retrieved via JavaScript
                            var getId = $"return Xrm.Page.getControl(\"{subgridName}\").getGrid().getRows().get({rows.IndexOf(row)}).getData().entity.getId()";
                            item.Id = new Guid((string)driver.ExecuteScript(getId));
                        }

                        var cells = row.FindElements(EntityElementsLocators.SubGridCells);

                        if (cells.Count > 0)
                        {
                            foreach (IWebElement thisCell in cells)
                                cellValues.Add(thisCell.Text);

                            for (int i = 0; i < columns.Count; i++)
                            {
                                //The first cell is always a checkbox for the record.  Ignore the checkbox.
                                if (i == 0)
                                {
                                    // Do Nothing
                                }
                                else
                                {
                                    item[columns[i]] = cellValues[i];
                                }

                            }

                            subGridRows.Add(item);

                            // Flush Item and Cell Values To Get New Rows
                            cellValues = new List<string>();
                            item = new GridItem();
                        }
                    }

                    return subGridRows;
                }

                // Return rows object
                return subGridRows;
            });
        }

        internal BrowserCommandResult<bool> NextPage(int thinkTime = Constants.DefaultThinkTime)
        {
            Client.ThinkTime(thinkTime);

            return this.Client.Execute(Client.GetOptions($"Next Page"), driver =>
            {
                driver.ClickWhenAvailable(GridElementsLocators.NextPage);

                driver.WaitForTransaction();

                return true;
            });
        }

        internal BrowserCommandResult<bool> OpenRecord(int index, int thinkTime = Constants.DefaultThinkTime, bool checkRecord = false)
        {
            Client.ThinkTime(thinkTime);
            return Client.Execute(Client.GetOptions("Open Grid Record"), driver =>
            {
                var grid = driver.FindElement(GridElementsLocators.PcfContainer);
                bool lastRow = false;
                IWebElement gridRow = null;
                Grid.GridType gridType = Grid.GridType.PowerAppsGridControl;
                int lastRowInCurrentView = 0;
                while (!lastRow)
                {
                    //determine which grid
                    if (driver.HasElement(GridElementsLocators.Rows))
                    {
                        gridType = Grid.GridType.PowerAppsGridControl;
                        Trace.WriteLine("Found Power Apps Grid.");
                    }
                    else if (driver.HasElement(GridElementsLocators.LegacyReadOnlyRows))
                    {
                        gridType = Grid.GridType.LegacyReadOnlyGrid;
                        Trace.WriteLine("Found Legacy Read Only Grid.");
                    }


                    if (!driver.HasElement(By.XPath(AppElements.Xpath[AppReference.Grid.Row].Replace("[INDEX]", (index).ToString()))))
                    {
                        lastRowInCurrentView = ClickGridAndPageDown(driver, grid, lastRowInCurrentView, gridType);
                    }
                    else
                    {
                        gridRow = driver.FindElement(GridElementsLocators.Row( index.ToString()));
                        lastRow = true;
                    }
                    if (driver.HasElement(GridElementsLocators.LastRow))
                    {
                        lastRow = true;
                    }
                }
                if (gridRow == null) throw new NotFoundException($"Grid Row {index} not found.");
                var xpathToGrid = By.XPath("//div[contains(@data-id,'DataSetHostContainer')]");
                IWebElement control = driver.WaitUntilAvailable(xpathToGrid);

                Func<Actions, Actions> action;
                if (checkRecord)
                    action = e => e.Click();
                else
                    action = e => e.DoubleClick();
                var xpathToCell = By.XPath(AppElements.Xpath[AppReference.Grid.Row].Replace("[INDEX]", index.ToString()));
                control.WaitUntilClickable(xpathToCell,
                    cell =>
                    {
                        var emptyDiv = cell.FindElement(By.TagName("div"));
                        switch (gridType)
                        {
                            case Grid.GridType.LegacyReadOnlyGrid:
                                driver.Perform(action, emptyDiv, null);
                                break;
                            case Grid.GridType.ReadOnlyGrid:
                                driver.Perform(action, emptyDiv, null);
                                break;
                            case Grid.GridType.PowerAppsGridControl:
                                try
                                {
                                    cell.FindElement(By.XPath("//a[contains(@aria-label,'Read only')]")).Click();
                                }
                                catch
                                {
                                    cell.FindElement(By.XPath("//a[contains(@role,'link')]")).Click();
                                }

                                break;
                            default: throw new InvalidSelectorException("Did not find Read Only or Power Apps Grid.");
                        }
                        Trace.WriteLine("Clicked record.");
                    },
                    $"An error occur trying to open the record at position {index}"
                );

                driver.WaitForTransaction();
                Trace.WriteLine("Click Record transaction complete.");
                return true;
            });
        }

        internal BrowserCommandResult<bool> OpenSubGridRecord(string subgridName, int index = 0)
        {
            return this.Client.Execute(Client.GetOptions($"Open Subgrid record for subgrid {subgridName}"), driver =>
            {
                // Find the SubGrid
                var subGrid = driver.FindElement(EntityElementsLocators.SubGridContents(subgridName));

                // Find list of SubGrid records
                IWebElement subGridRecordList = null;
                var foundGrid = subGrid.TryFindElement(EntityElementsLocators.SubGridListCells( subgridName), out subGridRecordList);

                // Read Only Grid Found
                if (subGridRecordList != null && foundGrid)
                {
                    var subGridRecordRows = subGrid.FindElements(EntityElementsLocators.SubGridListCells( subgridName));
                    if (subGridRecordRows == null)
                        throw new NoSuchElementException($"No records were found for subgrid {subgridName}");
                    Actions actions = new Actions(driver);
                    actions.MoveToElement(subGrid).Perform();

                    IWebElement gridRow = null;
                    if (index + 1 < subGridRecordRows.Count)
                    {
                        gridRow = subGridRecordRows[index];
                    }
                    else
                    {
                        var grid = driver.FindElement(By.XPath("//div[@ref='eViewport']"));
                        actions.DoubleClick(gridRow).Perform();
                        driver.WaitForTransaction();
                    }
                    if (gridRow == null)
                        throw new IndexOutOfRangeException($"Subgrid {subgridName} record count: {subGridRecordRows.Count}. Expected: {index + 1}");


                    actions.DoubleClick(gridRow).Perform();
                    driver.WaitForTransaction();
                    return true;
                }
                else if (!foundGrid)
                {
                    // Read Only Grid Not Found
                    var foundEditableGrid = subGrid.TryFindElement(EntityElementsLocators.EditableSubGridList( subgridName), out subGridRecordList);

                    if (foundEditableGrid)
                    {
                        var editableGridListCells = subGridRecordList.FindElement(EntityElementsLocators.EditableSubGridListCells);

                        var editableGridCellRows = editableGridListCells.FindElements(EntityElementsLocators.EditableSubGridListCellRows);

                        var editableGridCellRow = editableGridCellRows[index + 1].FindElements(By.XPath("./div"));

                        Actions actions = new Actions(driver);
                        actions.DoubleClick(editableGridCellRow[0]).Perform();

                        driver.WaitForTransaction();

                        return true;
                    }
                    else
                    {
                        // Editable Grid Not Found
                        // Check for special 'Related' grid form control
                        // This opens a limited form view in-line on the grid

                        //Get the GridName
                        string subGridName = subGrid.GetAttribute("data-id").Replace("dataSetRoot_", String.Empty);

                        //cell-0 is the checkbox for each record
                        var checkBox = driver.FindElement(EntityElementsLocators.SubGridRecordCheckbox(subGridName, index.ToString()));

                        driver.DoubleClick(checkBox);

                        driver.WaitForTransaction();
                    }
                }

                return true;
            });
        }

        internal BrowserCommandResult<bool> PreviousPage(int thinkTime = Constants.DefaultThinkTime)
        {
            Client.ThinkTime(thinkTime);

            return this.Client.Execute(Client.GetOptions($"Previous Page"), driver =>
            {
                driver.ClickWhenAvailable(GridElementsLocators.PreviousPage);

                driver.WaitForTransaction();

                return true;
            });
        }

        internal BrowserCommandResult<bool> Search(string searchCriteria, bool clearByDefault = true, int thinkTime = Constants.DefaultThinkTime)
        {
            Client.ThinkTime(thinkTime);

            return this.Client.Execute(Client.GetOptions($"Search"), driver =>
            {
                driver.WaitUntilClickable(GridElementsLocators.QuickFind);

                if (clearByDefault)
                {
                    driver.FindElement(GridElementsLocators.QuickFind).Clear();
                }

                driver.FindElement(GridElementsLocators.QuickFind).SendKeys(searchCriteria);
                driver.FindElement(GridElementsLocators.QuickFind).SendKeys(Keys.Enter);

                return true;
            });
        }

        internal BrowserCommandResult<bool> SearchSubGrid(string subGridName, string searchCriteria, bool clearByDefault = false)
        {
            return this.Client.Execute(Client.GetOptions($"Search SubGrid {subGridName}"), driver =>
            {
                IWebElement subGridSearchField = null;

                // Find the SubGrid
                var subGrid = driver.FindElement(EntityElementsLocators.SubGridContents( subGridName));

                if (subGrid != null)
                {
                    var foundSearchField = subGrid.TryFindElement(EntityElementsLocators.SubGridSearchBox, out subGridSearchField);
                    if (foundSearchField)
                    {
                        var inputElement = subGridSearchField.FindElement(By.TagName("input"));

                        if (clearByDefault)
                        {
                            inputElement.Clear();
                        }

                        inputElement.SendKeys(searchCriteria);

                        var startSearch = subGridSearchField.FindElement(By.TagName("button"));
                        startSearch.Click(true);

                        driver.WaitForTransaction();
                    }
                    else
                    {
                        throw new NotFoundException($"Unable to locate the search box for subgrid {subGridName}. Please validate that view search is enabled for this subgrid");
                    }
                }
                else
                {
                    throw new NotFoundException($"Unable to locate subgrid with name {subGridName}");
                }

                return true;
            });
        }

        internal BrowserCommandResult<bool> SelectAll(int thinkTime = Constants.DefaultThinkTime)
        {
            Client.ThinkTime(thinkTime);

            return this.Client.Execute(Client.GetOptions($"Select All"), driver =>
            {
                driver.ClickWhenAvailable(GridElementsLocators.SelectAll);

                driver.WaitForTransaction();

                return true;
            });
        }

        internal BrowserCommandResult<bool> SwitchSubGridView(string subGridName, string viewName, int thinkTime = Constants.DefaultThinkTime)
        {
            Client.ThinkTime(thinkTime);

            return Client.Execute(Client.GetOptions($"Switch SubGrid View"), driver =>
            {
                // Initialize required variables
                IWebElement viewPicker = null;

                // Find the SubGrid
                var subGrid = driver.FindElement(EntityElementsLocators.SubGridContents( subGridName));

                var foundPicker = subGrid.TryFindElement(EntityElementsLocators.SubGridViewPickerButton, out viewPicker);

                if (foundPicker)
                {
                    viewPicker.Click(true);

                    // Locate the ViewSelector flyout
                    var viewPickerFlyout = driver.WaitUntilAvailable(EntityElementsLocators.SubGridViewPickerFlyout, new TimeSpan(0, 0, 2));

                    var viewItems = viewPickerFlyout.FindElements(By.TagName("li"));


                    //Is the button in the ribbon?
                    if (viewItems.Any(x => x.GetAttribute("aria-label").Equals(viewName, StringComparison.OrdinalIgnoreCase)))
                    {
                        viewItems.FirstOrDefault(x => x.GetAttribute("aria-label").Equals(viewName, StringComparison.OrdinalIgnoreCase)).Click(true);
                    }

                }
                else
                    throw new NotFoundException($"Unable to locate the viewPicker for SubGrid {subGridName}");

                driver.WaitForTransaction();

                return true;
            });
        }

        internal BrowserCommandResult<bool> SwitchView(string viewName, string subViewName = null, int thinkTime = Constants.DefaultThinkTime)
        {
            Client.ThinkTime(thinkTime);

            return Client.Execute(Client.GetOptions($"Switch View"), driver =>
            {
                var views = OpenViewPicker().Value;
                Thread.Sleep(500);
                var key = viewName.ToLowerString();
                bool success = views.TryGetValue(key, out var view);
                if (!success)
                    throw new InvalidOperationException($"No view with the name '{key}' exists.");

                view.Click(true);

                if (subViewName != null)
                {
                    // TBD
                }

                driver.WaitForTransaction();

                return true;
            });
        }

        private static int ClickGridAndPageDown(IWebDriver driver, IWebElement grid, int lastKnownFloor, Grid.GridType gridType)
        {
            Actions actions = new Actions(driver);
            By rowGroupLocator = null;
            By topRowLocator = null;

            switch (gridType)
            {
                case Grid.GridType.LegacyReadOnlyGrid:
                    rowGroupLocator = GridElementsLocators.LegacyReadOnlyRows;
                    topRowLocator = GridElementsLocators.Rows;
                    break;
                case Grid.GridType.ReadOnlyGrid:
                    rowGroupLocator = GridElementsLocators.Rows;
                    topRowLocator = GridElementsLocators.Rows;
                    break;
                case Grid.GridType.PowerAppsGridControl:
                    rowGroupLocator = GridElementsLocators.Rows;
                    topRowLocator = GridElementsLocators.Rows;
                    break;
                default:
                    break;
            }

            var CurrentRows = driver.FindElements(rowGroupLocator);
            var lastFloor = CurrentRows.Where(x => Convert.ToInt32(x.GetAttribute("row-index")) == lastKnownFloor).First();
            //var topRow = driver.FindElement(topRowLocator);
            var topRow = CurrentRows.First();
            var firstCell = lastFloor.FindElement(By.XPath("//div[@aria-colindex='1']"));
            lastFloor.Click();
            actions.SendKeys(OpenQA.Selenium.Keys.PageDown).Perform();
            return Convert.ToInt32(driver.FindElements(rowGroupLocator).Last().GetAttribute("row-index"));
        }

        private static Actions ClickSubGridAndPageDown(IWebDriver driver, IWebElement grid)
        {
            Actions actions = new Actions(driver);
            //var topRow = driver.FindElement(By.XPath("//div[@data-id='entity_control-pcf_grid_control_container']//div[@ref='centerContainer']//div[@role='rowgroup']//div[@role='row']"));
            var topRow = driver.FindElement(GridElementsLocators.Rows);
            //topRow.Click();
            //actions.SendKeys(OpenQA.Selenium.Keys.PageDown).Perform();

            actions.MoveToElement(topRow.FindElement(By.XPath("//div[@role='listitem']//button"))).Perform();
            actions.KeyDown(OpenQA.Selenium.Keys.Alt).SendKeys(OpenQA.Selenium.Keys.ArrowDown).Build().Perform();
            return actions;
        }

        private static bool ClickSubMenuCommands(IWebDriver driver, string name, string subName, string subSecondName, string subGridName)
        {
            if (subName == null)
                return true; // If there is no `subName`, just terminate execution

            var overflowContainer = driver.FindElement(EntityElementsLocators.SubGridOverflowContainer);

            if (!overflowContainer.TryFindElement(EntityElementsLocators.SubGridOverflowButton( subName), out var overflowButton))
                throw new InvalidOperationException($"No command '{subName}' under '{name}' in {subGridName} Commandbar.");

            overflowButton.Click(true);
            driver.WaitForTransaction();

            // Checking `subSecondName`
            if (subSecondName != null)
            {
                if (!overflowContainer.TryFindElement(EntityElementsLocators.SubGridOverflowButton(subSecondName), out var secondOverflowCommand))
                    throw new InvalidOperationException($"No command '{subSecondName}' under '{subName}' in {subGridName} Commandbar.");

                secondOverflowCommand.Click(true);
                driver.WaitForTransaction();
            }

            return true;
        }

        private static string GetGridQueryKey(IWebDriver driver, string dataSetName = null) => GetDataHelper.GetGridQueryKey(driver, dataSetName);

        private static void ProcessGridRowAttributes(Dictionary<string, object> attributes, GridItem gridItem)
        {
            foreach (string attributeKey in attributes.Keys)
            {
                var serializedString = JsonConvert.SerializeObject(attributes[attributeKey]);
                var deserializedRecord = JsonConvert.DeserializeObject<SerializedGridItem>(serializedString);
                if (deserializedRecord.value != null)
                {
                    gridItem[attributeKey] = deserializedRecord.value;
                }
                else if (deserializedRecord.label != null)
                {
                    gridItem[attributeKey] = deserializedRecord.label;
                }
                else if (deserializedRecord.id != null)
                {
                    gridItem[attributeKey] = deserializedRecord.id.guid;
                }
                else if (deserializedRecord.reference != null)
                {
                    gridItem[attributeKey] = deserializedRecord.reference.id.guid;
                }
            }
        }

        private static bool TryClickCommand(IWebElement commandBar, string commandName, IWebDriver driver)
        {
            if (commandBar.TryFindElement(EntityElementsLocators.SubGridCommandLabel( commandName), out var command))
            {
                command.Click(true);
                driver.WaitForTransaction();
                return true;
            }
            return false;
        }

        private static bool TryClickMoreCommands(IWebElement commandBar, string commandName, IWebDriver driver, string subGridName)
        {
            if (!commandBar.TryFindElement(EntityElementsLocators.SubGridOverflowButton( "More commands"), out var moreCommands))
                return false;

            moreCommands.Click(true);
            driver.WaitForTransaction();

            var overflowContainer = driver.FindElement(EntityElementsLocators.SubGridOverflowContainer);

            if (overflowContainer.TryFindElement(EntityElementsLocators.SubGridOverflowButton( commandName), out var overflowCommand))
            {
                overflowCommand.Click(true);
                driver.WaitForTransaction();
                return true;
            }

            return false;
        }

        internal IWebElement GetButtonInFlyout(string button)
        {
            return Client.Execute(Client.GetOptions($"Get Button '{button}' in Flyout on the Related Grid"), driver =>
            {
                var overFlowContainer = driver.FindElement(By.XPath(AppElements.Xpath[AppReference.Related.CommandBarOverflowContainer]));

                return overFlowContainer.FindElement(By.XPath(AppElements.Xpath[AppReference.Related.CommandBarSubButton].Replace("[NAME]", button)));
            }).Value;
        }
    }
    }
