using System;
using System.Collections.Generic;
using System.Linq;
using OpenQA.Selenium;
using TALXIS.TestKit.Selectors.Browser;
using TALXIS.TestKit.Selectors.DTO.Locators;

namespace TALXIS.TestKit.Selectors.WebClientManagement
{
    /// <summary>
    /// The subgrids, their rows and their commands on TALXIS Portal pages, and field-bound dataset grids.
    /// </summary>
    /// <remarks>
    /// A subgrid is rendered in <c>div[data-id="dataSetRoot_{control name}"]</c> inside a <c>section.TALXIS__portal__view-page</c>
    /// whose ribbon (<c>.TALXIS__ribbon</c>) holds its commands. The portal renders a subgrid with one of two controls:
    /// <list type="bullet">
    /// <item>the grid (ag-grid): rows are <c>.ag-row</c> with <c>row-id</c> set to the record id (one element per pinned part
    /// of the row), cells <c>.ag-cell[col-id]</c>, headers <c>.ag-header-cell[col-id]</c>; a row is selected with the checkbox
    /// of its selection cell (<c>.ms-Checkbox</c>);</item>
    /// <item>the legacy view (Fluent DetailsList, e.g. in nested forms): rows are <c>.ms-DetailsRow</c> without the record id,
    /// cells <c>.ms-DetailsRow-cell[data-automation-key]</c> and headers <c>.ms-DetailsHeader-cell[data-item-key]</c> keyed by
    /// the column's logical name; a row is selected with its check (<c>.ms-DetailsRow-check</c>). Its records can only be
    /// found by a displayed value. A grouped legacy view starts with its groups collapsed (<c>.ms-GroupHeader</c>, inside
    /// <c>.ms-GroupedList-group</c> with the group's rows): the grouped columns' values are shown only in the group
    /// header, and the group's rows are rendered once it is expanded (<c>.ms-GroupHeader-expand</c>). A view that
    /// aggregates its records into the group header (aggregation button placement on the aggregated row) shows the
    /// commands on the header; when the page hides its expander, the header stands for the group's records.</item>
    /// </list>
    /// A command is a button with <c>data-id</c> (button id) and <c>data-command</c> (command id); an icon command shows its
    /// icon as <c>svg[data-src]</c> or <c>img[src]</c>. The portal does not render commands whose enable or display rules fail.
    /// Commands for exactly one selected record (an enable rule with a selection count of 1..1) are inline: the portal
    /// leaves them out of the subgrid's ribbon and renders them in every row of a view with the ribbon buttons column,
    /// evaluated for that row's record; such a row ribbon shows a shimmer while its rules are evaluated. A subgrid can have
    /// no record selection (no selection column); its rows still show their own commands.
    /// </remarks>
    public class PortalGridManager
    {
        private static readonly TimeSpan LoadTimeout = TimeSpan.FromSeconds(30);
        private static readonly TimeSpan CommandTimeout = TimeSpan.FromSeconds(10);
        private const string TargetRowAttribute = "data-testkit-target-row";

        public PortalGridManager(WebClient client)
        {
            Client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public WebClient Client { get; }

        internal BrowserCommandResult<bool> ClickCommand(string subGridName, string command)
        {
            return Client.Execute(Client.GetNoRetryOptions($"Click Portal Subgrid Command: {command}"), driver =>
            {
                var subGrid = FindSubGrid(driver, subGridName);
                if (!WaitForSubGridCommand(driver, subGrid, command))
                    throw new InvalidOperationException($"The '{command}' command is not available on the '{subGridName}' subgrid.");

                PortalPageManager.ClickWhenUnblocked(driver, () => FindSubGridCommand(driver, subGrid, command), LoadTimeout, $"The '{command}' command on the '{subGridName}' subgrid");
                return true;
            });
        }

        internal BrowserCommandResult<bool> ClickRowCommand(string subGridName, Guid recordId, string command)
        {
            return Client.Execute(Client.GetNoRetryOptions($"Click Portal Row Command: {command}"), driver =>
            {
                var subGrid = FindSubGrid(driver, subGridName);
                ClickRowCommand(driver, () => FindRows(subGrid, recordId), command, $"the record {recordId}");
                return true;
            });
        }

        internal BrowserCommandResult<bool> ClickRowCommand(string subGridName, string column, string value, string command)
        {
            return Client.Execute(Client.GetNoRetryOptions($"Click Portal Row Command: {command}"), driver =>
            {
                var subGrid = FindSubGrid(driver, subGridName);
                ClickRowCommand(driver, () => FindRowsByValue(subGrid, column, value), command, $"the record with '{value}' in '{column}'");
                return true;
            });
        }

        internal BrowserCommandResult<bool> SelectRecord(string subGridName, Guid recordId)
        {
            return Client.Execute(Client.GetNoRetryOptions("Select Portal Subgrid Record"), driver =>
            {
                var subGrid = FindSubGrid(driver, subGridName);
                PortalPageManager.WaitFor(driver, () => FindRows(subGrid, recordId).Count > 0, LoadTimeout);
                SelectRecord(driver, FindRows(subGrid, recordId), $"The record {recordId}");
                return true;
            });
        }

        internal BrowserCommandResult<bool> SelectRecord(string subGridName, string column, string value)
        {
            return Client.Execute(Client.GetNoRetryOptions("Select Portal Subgrid Record"), driver =>
            {
                var subGrid = FindSubGrid(driver, subGridName);
                PortalPageManager.WaitFor(driver, () => FindRowsByValue(subGrid, column, value).Count > 0, LoadTimeout);
                SelectRecord(driver, FindRowsByValue(subGrid, column, value), $"The record with '{value}' in '{column}'");
                return true;
            });
        }

        internal BrowserCommandResult<bool> ContainsRecord(string subGridName, Guid recordId, bool contained)
        {
            return Client.Execute(Client.GetNoRetryOptions("Portal Subgrid Contains Record"), driver =>
            {
                var subGrid = FindSubGrid(driver, subGridName);
                return contained
                    ? PortalPageManager.WaitFor(driver, () => FindRows(subGrid, recordId).Count > 0, LoadTimeout)
                    : PortalPageManager.BecomesAbsent(driver, () => FindRows(subGrid, recordId).Count > 0, LoadTimeout);
            });
        }

        internal BrowserCommandResult<bool> ContainsRecord(string subGridName, string column, string value)
        {
            return Client.Execute(Client.GetNoRetryOptions("Portal Subgrid Contains Record"), driver =>
            {
                var subGrid = FindSubGrid(driver, subGridName);
                return PortalPageManager.WaitFor(driver, () => FindRowsByValue(subGrid, column, value).Count > 0, LoadTimeout);
            });
        }

        internal BrowserCommandResult<long> CountRecords(string subGridName)
        {
            return Client.Execute(Client.GetNoRetryOptions("Count Portal Subgrid Records"), driver => (long)CountRecords(FindSubGrid(driver, subGridName)));
        }

        internal BrowserCommandResult<bool> IsCommandShown(string subGridName, string command, bool shown)
        {
            return Client.Execute(Client.GetNoRetryOptions($"Is Portal Subgrid Command Shown: {command}"), driver =>
            {
                var subGrid = FindSubGrid(driver, subGridName);
                return shown ? WaitForSubGridCommand(driver, subGrid, command) : IsSubGridCommandUnavailable(driver, subGrid, command);
            });
        }

        internal BrowserCommandResult<bool> ContainsDatasetRecord(Guid recordId)
        {
            return Client.Execute(Client.GetNoRetryOptions("Portal Dataset Grid Contains Record"), driver =>
                PortalPageManager.WaitFor(driver, () => FindRows(driver, recordId).Count > 0, LoadTimeout));
        }

        internal BrowserCommandResult<bool> ClickDatasetRowButton(Guid recordId, string button)
        {
            return Client.Execute(Client.GetNoRetryOptions($"Click Portal Dataset Row Button: {button}"), driver =>
            {
                if (!PortalPageManager.WaitFor(driver, () => FindRowButton(FindRows(driver, recordId), button) != null, LoadTimeout))
                    throw new NoSuchElementException($"The '{button}' button is not shown on the record {recordId}.");

                PortalPageManager.ClickWhenUnblocked(driver, () => FindRowButton(FindRows(driver, recordId), button), LoadTimeout, $"The '{button}' button on the record {recordId}");
                return true;
            });
        }

        internal BrowserCommandResult<bool> IsDatasetRowButtonShown(Guid recordId, string button, bool shown)
        {
            return Client.Execute(Client.GetNoRetryOptions($"Is Portal Dataset Row Button Shown: {button}"), driver =>
            {
                if (shown)
                    return PortalPageManager.WaitFor(driver, () => FindRowButton(FindRows(driver, recordId), button) != null, LoadTimeout);

                // An action may remove the button when it refreshes the grid.
                return PortalPageManager.WaitFor(driver, () => FindRows(driver, recordId).Count > 0, LoadTimeout)
                    && PortalPageManager.BecomesAbsent(driver, () => FindRowButton(FindRows(driver, recordId), button) != null, LoadTimeout);
            });
        }

        /// <summary>
        /// Waits for a subgrid to be rendered and its records to be loaded.
        /// </summary>
        /// <returns>The subgrid root element.</returns>
        internal static IWebElement FindSubGrid(IWebDriver driver, string subGridName)
        {
            var root = PortalPageManager.WaitUntil(
                driver,
                d => d.FindElements(PortalGridElementsLocators.SubGridRoot(subGridName)).FirstOrDefault(e => e.Displayed),
                LoadTimeout,
                $"The '{subGridName}' subgrid was not displayed.");

            PortalPageManager.WaitUntil(driver, _ => IsLoaded(root), LoadTimeout, $"The '{subGridName}' subgrid did not finish loading.");
            return root;
        }

        /// <summary>
        /// Finds the rows (one per pinned part) of a record. Rows of the legacy view do not carry the record id and are
        /// never found this way.
        /// </summary>
        /// <returns>The row elements; empty when the record is not displayed.</returns>
        internal static IReadOnlyList<IWebElement> FindRows(ISearchContext scope, Guid recordId)
        {
            return scope.FindElements(PortalGridElementsLocators.RowsOfRecord(recordId));
        }

        /// <summary>
        /// Finds the rows of the record whose cell in the given column shows the given value. In a grouped legacy view,
        /// where the grouped column's value is shown in the group header, it expands the matching group and returns its
        /// rows (empty until they are rendered).
        /// </summary>
        /// <param name="subGrid">The subgrid root element.</param>
        /// <param name="column">The column header label or the column's logical name.</param>
        /// <param name="value">The displayed value.</param>
        /// <returns>The row elements; empty when no such record is displayed.</returns>
        internal static IReadOnlyList<IWebElement> FindRowsByValue(IWebElement subGrid, string column, string value)
        {
            var header = subGrid.FindElements(PortalGridElementsLocators.Headers)
                .FirstOrDefault(h => PortalPageManager.NormalizeText(h.Text) == column);
            var columnKey = header == null ? column : header.GetAttribute("col-id") ?? header.GetAttribute("data-item-key");

            var row = subGrid.FindElements(PortalGridElementsLocators.CellsOfColumn(columnKey))
                .Where(cell => PortalPageManager.NormalizeText(cell.Text) == value)
                .Select(cell => cell.FindElement(PortalGridElementsLocators.RowOfCell))
                .FirstOrDefault();

            if (row == null)
                return FindGroupRowsByValue(subGrid, columnKey, value);

            var rowId = row.GetAttribute("row-id");
            return rowId == null ? new[] { row } : subGrid.FindElements(PortalGridElementsLocators.RowsWithRowId(rowId));
        }

        /// <summary>
        /// Selects a record by ticking its selection checkbox, and waits until it is selected. In a subgrid without record
        /// selection (no selection column), whose rows show their own commands, the record's row is marked as the one the
        /// commands are looked up on instead.
        /// </summary>
        internal static void SelectRecord(IWebDriver driver, IReadOnlyList<IWebElement> rows, string description)
        {
            if (rows.Count == 0)
                throw new NoSuchElementException($"{description} is not displayed in the subgrid.");

            // A DetailsList shows its row check only on hover; clicking the row selects it as well.
            var checkbox = rows.SelectMany(row => row.FindElements(PortalGridElementsLocators.RowChecks)).FirstOrDefault(check => check.Displayed);
            if (checkbox == null)
            {
                SelectByRowClickOrMark(driver, rows);
                return;
            }

            if (!rows.Any(IsRowSelected))
            {
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", checkbox);
                PortalPageManager.ClickWhenUnblocked(driver, () => checkbox, CommandTimeout, $"The selection checkbox of {description}");
            }

            PortalPageManager.WaitUntil(driver, _ => rows.Any(IsRowSelected), CommandTimeout, $"{description} did not become selected.");
        }

        /// <summary>
        /// Finds a displayed button in the rows of a record by its title, aria-label, text or Fluent icon name. An icon-only
        /// button that shows its label only in a tooltip (empty title, no aria-label) is found by its icon name
        /// (<c>i[data-icon-name]</c>, e.g. <c>ClipboardList</c>).
        /// </summary>
        /// <returns>The button, or null when it is not displayed.</returns>
        internal static IWebElement FindRowButton(IReadOnlyList<IWebElement> rows, string label)
        {
            return rows.SelectMany(row => row.FindElements(PortalGridElementsLocators.RowButton(label))).FirstOrDefault(button => button.Displayed);
        }

        /// <summary>
        /// Counts the records displayed in a subgrid.
        /// </summary>
        internal static int CountRecords(IWebElement subGrid)
        {
            return subGrid.FindElements(PortalGridElementsLocators.Rows).Select(RowKey).Distinct().Count();
        }

        /// <summary>
        /// Finds a command of a subgrid by its label, its button or command id, or the file name of its icon (for commands
        /// that only show an icon), looking into overflow menus when needed. Inline commands are looked up in the rows of the
        /// selected records, or in all rows when no record is selected; other commands in the subgrid's ribbon.
        /// </summary>
        /// <returns>The command, or null when it is not displayed.</returns>
        internal static IWebElement FindSubGridCommand(IWebDriver driver, IWebElement subGrid, string command)
        {
            foreach (var row in GetCommandRows(subGrid))
            {
                var inline = FindCommandInBar(driver, row, command);
                if (inline != null)
                    return inline;
            }

            var ribbon = subGrid.FindElements(PortalGridElementsLocators.SubGridRibbon).FirstOrDefault();
            return ribbon == null ? null : FindCommandInBar(driver, ribbon, command);
        }

        /// <summary>
        /// Finds an inline command in the rows of a record, the way a user clicks it on the record's row without selecting
        /// the record first, looking into the row's overflow menu when needed.
        /// </summary>
        /// <returns>The command, or null when it is not displayed.</returns>
        internal static IWebElement FindRowCommand(IWebDriver driver, IReadOnlyList<IWebElement> rows, string command)
        {
            return rows.Select(row => FindCommandInBar(driver, row, command)).FirstOrDefault(found => found != null);
        }

        /// <summary>
        /// Waits until a command of a subgrid is displayed and enabled (the ribbon re-evaluates its rules asynchronously,
        /// e.g. after the selection changes).
        /// </summary>
        /// <returns>Whether the command became available.</returns>
        internal static bool WaitForSubGridCommand(IWebDriver driver, IWebElement subGrid, string command)
        {
            var available = PortalPageManager.WaitFor(driver, () => FindSubGridCommand(driver, subGrid, command) is { } c && !PortalPageManager.IsDisabled(c), CommandTimeout);
            PortalPageManager.CloseMenus(driver);
            return available;
        }

        /// <summary>
        /// Gets a value indicating whether a command of a subgrid stays hidden or disabled, once the inline commands of the
        /// rows it is looked up in are evaluated.
        /// </summary>
        /// <returns>Whether the command is not available.</returns>
        internal static bool IsSubGridCommandUnavailable(IWebDriver driver, IWebElement subGrid, string command)
        {
            var loaded = PortalPageManager.WaitFor(driver, () => GetCommandRows(subGrid).All(row => row.FindElements(PortalGridElementsLocators.CommandsLoading).Count == 0), CommandTimeout);
            if (!loaded)
                throw new WebDriverTimeoutException($"The commands of the rows of the subgrid were still loading after {CommandTimeout.TotalSeconds} seconds.");

            var unavailable = PortalPageManager.BecomesAbsent(driver, () => FindSubGridCommand(driver, subGrid, command) is { } c && !PortalPageManager.IsDisabled(c), CommandTimeout);
            PortalPageManager.CloseMenus(driver);
            return unavailable;
        }

        private static void ClickRowCommand(IWebDriver driver, Func<IReadOnlyList<IWebElement>> findRows, string command, string description)
        {
            var available = PortalPageManager.WaitFor(driver, () => FindRowCommand(driver, findRows(), command) is { } found && !PortalPageManager.IsDisabled(found), LoadTimeout);
            PortalPageManager.CloseMenus(driver);
            if (!available)
                throw new InvalidOperationException($"The '{command}' command is not available on the row of {description}.");

            PortalPageManager.ClickWhenUnblocked(driver, () => FindRowCommand(driver, findRows(), command), LoadTimeout, $"The '{command}' command on {description}");
        }

        // Finds the group whose header shows the value in the column, expands it when collapsed and returns its rows. A group
        // that cannot be expanded (its expander hidden, e.g. an aggregated row) is represented by its header.
        private static IReadOnlyList<IWebElement> FindGroupRowsByValue(IWebElement subGrid, string columnKey, string value)
        {
            var groupHeader = subGrid.FindElements(PortalGridElementsLocators.GroupHeaderCellsOfColumn(columnKey))
                .Where(cell => PortalPageManager.NormalizeText(cell.Text) == value)
                .Select(cell => cell.FindElement(PortalGridElementsLocators.GroupHeaderOfCell))
                .FirstOrDefault();
            if (groupHeader == null)
                return Array.Empty<IWebElement>();

            var expander = groupHeader.FindElements(PortalGridElementsLocators.GroupExpander).FirstOrDefault(button => button.Displayed);
            if (expander == null)
                return new[] { groupHeader };

            if (groupHeader.GetAttribute("aria-expanded") != "true")
                expander.Click();

            return groupHeader.FindElement(PortalGridElementsLocators.GroupOfHeader).FindElements(PortalGridElementsLocators.GroupRows);
        }

        // The grid is loaded once its loading overlay is gone (ag-grid keeps its overlay elements once shown and only hides
        // them); the legacy view once it shows rows, (collapsed) groups or its "no records" message instead of its loading
        // placeholder.
        private static bool IsLoaded(IWebElement root)
        {
            if (root.FindElements(PortalGridElementsLocators.GridRoot).Count > 0)
                return !root.FindElements(PortalGridElementsLocators.GridLoadingOverlay).Any(overlay => overlay.Displayed);

            return root.FindElements(PortalGridElementsLocators.LegacyView).Count > 0
                && root.FindElements(PortalGridElementsLocators.LegacyViewLoaded).Count > 0;
        }

        // Without a selection column a row of the legacy view is still selected by clicking it: click a plain cell (not a
        // link, button or command, which would open or change something) and, when that does not select the row, mark it.
        // A row of the grid (ag-grid) cannot be selected without the column, and a click on it opens its record: mark it.
        private static void SelectByRowClickOrMark(IWebDriver driver, IReadOnlyList<IWebElement> rows)
        {
            if (rows.Any(row => row.GetAttribute("row-id") != null))
            {
                MarkAsTargetRows(driver, rows);
                return;
            }

            var plainCell = rows.SelectMany(row => row.FindElements(PortalGridElementsLocators.RowCells))
                .FirstOrDefault(cell => cell.Displayed
                    && !(cell.GetAttribute("col-id") ?? cell.GetAttribute("data-automation-key") ?? string.Empty).EndsWith(PortalGridElementsLocators.RibbonButtonsColumnSuffix, StringComparison.Ordinal)
                    && cell.FindElements(PortalGridElementsLocators.RowInteractiveContent).Count == 0);
            if (plainCell != null)
            {
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", plainCell);
                PortalPageManager.ClickWhenUnblocked(driver, () => plainCell, CommandTimeout, "The record's row");
                if (PortalPageManager.WaitFor(driver, () => rows.Any(IsRowSelected), TimeSpan.FromSeconds(2)))
                    return;
            }

            MarkAsTargetRows(driver, rows);
        }

        // Marks the rows of a record in a subgrid without record selection, replacing an earlier mark in that subgrid.
        private static void MarkAsTargetRows(IWebDriver driver, IReadOnlyList<IWebElement> rows)
        {
            ((IJavaScriptExecutor)driver).ExecuteScript(
                @"const root = arguments[0].closest('[data-id^=""dataSetRoot_""]') || document;
                  root.querySelectorAll('[" + TargetRowAttribute + @"]').forEach(row => row.removeAttribute('" + TargetRowAttribute + @"'));
                  for (const row of arguments) row.setAttribute('" + TargetRowAttribute + @"', 'true');",
                rows.Cast<object>().ToArray());
        }

        // The rows (and group headers, which show the commands of aggregated rows) whose inline commands apply: those of the
        // selected (or, without record selection, marked) records with the other pinned parts of their rows, or all of them
        // when no record is selected.
        private static IReadOnlyList<IWebElement> GetCommandRows(IWebElement subGrid)
        {
            var rows = subGrid.FindElements(PortalGridElementsLocators.RowsAndGroupHeaders);
            var selected = rows.Where(IsRowSelected).ToList();
            if (selected.Count == 0)
                return rows;

            var selectedRecordIds = selected.Select(row => row.GetAttribute("row-id")).Where(id => id != null).ToHashSet();
            return rows.Where(row => selected.Contains(row) || selectedRecordIds.Contains(row.GetAttribute("row-id") ?? string.Empty)).ToList();
        }

        private static bool IsRowSelected(IWebElement row)
        {
            var classes = (row.GetAttribute("class") ?? string.Empty).Split(' ');
            return classes.Contains("ag-row-selected") || classes.Contains("is-selected")
                || row.GetAttribute("aria-selected") == "true"
                || row.GetAttribute(TargetRowAttribute) == "true"
                || row.FindElements(PortalGridElementsLocators.RowChecks).Any(IsChecked);
        }

        // ag-grid renders a record's row once per pinned part, all with the record id; a legacy row is identified by its index.
        private static string RowKey(IWebElement row)
        {
            return row.GetAttribute("row-id") ?? $"index-{row.GetAttribute("data-item-index")}";
        }

        // Finds a command in a command bar (a ribbon or a row), including its overflow menu.
        private static IWebElement FindCommandInBar(IWebDriver driver, IWebElement bar, string command)
        {
            var found = bar.FindElements(PortalGridElementsLocators.Command).FirstOrDefault(c => IsCommand(c, command));
            if (found != null)
                return found;

            foreach (var overflow in bar.FindElements(PortalPageElementsLocators.OverflowButton).Where(b => b.Displayed))
            {
                overflow.Click();
                var menuItem = driver.FindElements(PortalGridElementsLocators.MenuCommand).FirstOrDefault(c => IsCommand(c, command));
                if (menuItem != null)
                    return menuItem;

                PortalPageManager.CloseMenus(driver);
            }

            return null;
        }

        private static bool IsCommand(IWebElement element, string command)
        {
            if (element.GetAttribute("data-id") == command || element.GetAttribute("data-command") == command
                || PortalPageManager.NormalizeText(element.Text) == command)
                return true;

            return element.FindElements(PortalGridElementsLocators.CommandIcon)
                .Select(icon => icon.GetAttribute("data-src") ?? icon.GetAttribute("src") ?? string.Empty)
                .Any(source => source.Split('?')[0].EndsWith("/" + command, StringComparison.OrdinalIgnoreCase)
                    || source.Split('?')[0].EndsWith("=" + command, StringComparison.OrdinalIgnoreCase)
                    || source.Equals(command, StringComparison.OrdinalIgnoreCase));
        }

        // A Fluent checkbox (is-checked, or its input) or a DetailsList row check (aria-checked).
        private static bool IsChecked(IWebElement checkbox)
        {
            return (checkbox.GetAttribute("class") ?? string.Empty).Split(' ').Contains("is-checked")
                || checkbox.GetAttribute("aria-checked") == "true"
                || checkbox.FindElements(PortalGridElementsLocators.CheckboxInput).Any(input => input.Selected);
        }
    }
}
