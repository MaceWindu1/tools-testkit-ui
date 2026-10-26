namespace TALXIS.TestKit.Bindings.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Support.UI;

    /// <summary>
    /// Works with subgrids, their rows and their commands on TALXIS Portal pages.
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
    public static class PortalGridExtensions
    {
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);
        private static readonly TimeSpan CommandTimeout = TimeSpan.FromSeconds(10);
        private const string TargetRowAttribute = "data-testkit-target-row";
        private const string Rows = ".ag-row[row-id], .ms-DetailsRow";
        private const string RowCells = ".ag-cell[col-id], .ms-DetailsRow-cell[data-automation-key]";
        private const string RowChecks = ".ms-Checkbox, .ms-DetailsRow-check";
        private const string GroupHeaders = ".ms-GroupHeader";

        /// <summary>
        /// Waits for a subgrid to be rendered and its records to be loaded.
        /// </summary>
        /// <param name="driver">The web driver.</param>
        /// <param name="subGridName">The name of the subgrid control.</param>
        /// <returns>The subgrid root element.</returns>
        public static IWebElement FindPortalSubGrid(this IWebDriver driver, string subGridName)
        {
            var wait = CreateWait(driver, DefaultTimeout, $"The '{subGridName}' subgrid was not displayed.");
            var root = wait.Until(d => d.FindElements(By.CssSelector($"[data-id='dataSetRoot_{subGridName}']")).FirstOrDefault(e => e.Displayed));

            wait.Message = $"The '{subGridName}' subgrid did not finish loading.";
            wait.Until(_ => IsLoaded(root));
            return root;
        }

        /// <summary>
        /// Finds the rows (one per pinned part) of a record in a subgrid. Rows of the legacy view do not carry the record id
        /// and are never found this way.
        /// </summary>
        /// <param name="subGrid">The subgrid root element.</param>
        /// <param name="recordId">The record id.</param>
        /// <returns>The row elements; empty when the record is not displayed.</returns>
        public static IReadOnlyList<IWebElement> FindPortalGridRows(this ISearchContext subGrid, Guid recordId)
        {
            return subGrid.FindElements(By.CssSelector($".ag-row[row-id='{recordId:D}' i]"));
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
        public static IReadOnlyList<IWebElement> FindPortalGridRowsByValue(this IWebElement subGrid, string column, string value)
        {
            var header = subGrid.FindElements(By.CssSelector(".ag-header-cell[col-id], .ms-DetailsHeader-cell[data-item-key]"))
                .FirstOrDefault(h => PortalPageExtensions.NormalizeText(h.Text) == column);
            var columnKey = header == null ? column : header.GetAttribute("col-id") ?? header.GetAttribute("data-item-key");

            var row = subGrid.FindElements(By.CssSelector($".ag-row .ag-cell[col-id='{columnKey}'], .ms-DetailsRow .ms-DetailsRow-cell[data-automation-key='{columnKey}']"))
                .Where(cell => PortalPageExtensions.NormalizeText(cell.Text) == value)
                .Select(cell => cell.FindElement(By.XPath("./ancestor::div[contains(concat(' ', normalize-space(@class), ' '), ' ag-row ') or contains(concat(' ', normalize-space(@class), ' '), ' ms-DetailsRow ')][1]")))
                .FirstOrDefault();

            if (row == null)
            {
                return FindGroupRowsByValue(subGrid, columnKey, value);
            }

            var rowId = row.GetAttribute("row-id");
            return rowId == null ? new[] { row } : subGrid.FindElements(By.CssSelector($".ag-row[row-id='{rowId}']"));
        }

        // Finds the group whose header shows the value in the column, expands it when collapsed and returns its rows. A group
        // that cannot be expanded (its expander hidden, e.g. an aggregated row) is represented by its header.
        private static IReadOnlyList<IWebElement> FindGroupRowsByValue(IWebElement subGrid, string columnKey, string value)
        {
            var groupHeader = subGrid.FindElements(By.CssSelector($"{GroupHeaders} .ms-DetailsRow-cell[data-automation-key='{columnKey}']"))
                .Where(cell => PortalPageExtensions.NormalizeText(cell.Text) == value)
                .Select(cell => cell.FindElement(By.XPath("./ancestor::div[contains(concat(' ', normalize-space(@class), ' '), ' ms-GroupHeader ')][1]")))
                .FirstOrDefault();
            if (groupHeader == null)
            {
                return Array.Empty<IWebElement>();
            }

            var expander = groupHeader.FindElements(By.CssSelector(".ms-GroupHeader-expand")).FirstOrDefault(button => button.Displayed);
            if (expander == null)
            {
                return new[] { groupHeader };
            }

            if (groupHeader.GetAttribute("aria-expanded") != "true")
            {
                expander.Click();
            }

            return groupHeader
                .FindElement(By.XPath("./ancestor::div[contains(concat(' ', normalize-space(@class), ' '), ' ms-GroupedList-group ')][1]"))
                .FindElements(By.CssSelector(".ms-DetailsRow"));
        }

        /// <summary>
        /// Selects a record in a subgrid by ticking its selection checkbox, and waits until it is selected. In a subgrid
        /// without record selection (no selection column), whose rows show their own commands, the record's row is marked
        /// as the one the command steps look at instead.
        /// </summary>
        /// <param name="driver">The web driver.</param>
        /// <param name="rows">The rows of the record.</param>
        /// <param name="description">The record's description for error messages.</param>
        public static void SelectPortalGridRecord(this IWebDriver driver, IReadOnlyList<IWebElement> rows, string description)
        {
            if (rows.Count == 0)
            {
                throw new NoSuchElementException($"{description} is not displayed in the subgrid.");
            }

            // A DetailsList shows its row check only on hover; clicking the row selects it as well.
            var checkbox = rows.SelectMany(row => row.FindElements(By.CssSelector(RowChecks))).FirstOrDefault(check => check.Displayed);
            if (checkbox == null)
            {
                SelectByRowClickOrMark(driver, rows);
                return;
            }

            if (!rows.Any(IsRowSelected))
            {
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", checkbox);
                driver.ClickWhenUnblocked(() => checkbox, CommandTimeout, $"The selection checkbox of {description}");
            }

            var wait = CreateWait(driver, CommandTimeout, $"{description} did not become selected.");
            wait.Until(_ => rows.Any(IsRowSelected));
        }

        /// <summary>
        /// Finds a displayed button in the rows of a record by its title, aria-label, text or Fluent icon name. An icon-only
        /// button that shows its label only in a tooltip (empty title, no aria-label) is found by its icon name
        /// (<c>i[data-icon-name]</c>, e.g. <c>ClipboardList</c>).
        /// </summary>
        /// <param name="rows">The rows of the record.</param>
        /// <param name="label">The title, aria-label, text or icon name.</param>
        /// <returns>The button, or null when it is not displayed.</returns>
        public static IWebElement FindPortalGridRowButton(this IReadOnlyList<IWebElement> rows, string label)
        {
            var literal = PortalFormExtensions.XPathLiteral(label);
            return rows
                .SelectMany(row => row.FindElements(By.XPath(
                    $".//button[@title={literal} or @aria-label={literal} or normalize-space(.)={literal} or .//i[@data-icon-name={literal}]]")))
                .FirstOrDefault(button => button.Displayed);
        }

        /// <summary>
        /// Counts the records displayed in a subgrid.
        /// </summary>
        /// <param name="subGrid">The subgrid root element.</param>
        /// <returns>The number of records.</returns>
        public static int CountPortalGridRecords(this IWebElement subGrid)
        {
            return subGrid.FindElements(By.CssSelector(Rows)).Select(RowKey).Distinct().Count();
        }

        /// <summary>
        /// Finds a command of a subgrid by its label, its button or command id, or the file name of its icon (for commands
        /// that only show an icon), looking into overflow menus when needed. Inline commands are looked up in the rows of the
        /// selected records, or in all rows when no record is selected; other commands in the subgrid's ribbon.
        /// </summary>
        /// <param name="driver">The web driver.</param>
        /// <param name="subGrid">The subgrid root element.</param>
        /// <param name="command">The label, id or icon file name (e.g. <c>download.svg</c>).</param>
        /// <returns>The command, or null when it is not displayed.</returns>
        public static IWebElement FindPortalSubGridCommand(this IWebDriver driver, IWebElement subGrid, string command)
        {
            foreach (var row in GetCommandRows(subGrid))
            {
                var inline = FindCommandInBar(driver, row, command);
                if (inline != null)
                {
                    return inline;
                }
            }

            var ribbon = subGrid.FindElements(By.XPath(
                "./ancestor::section[contains(@class, 'TALXIS__portal__view-page')][1]//div[contains(@class, 'TALXIS__ribbon')" +
                " and not(ancestor::div[contains(concat(' ', normalize-space(@class), ' '), ' ag-row ') or contains(concat(' ', normalize-space(@class), ' '), ' ms-DetailsRow ')" +
                " or contains(concat(' ', normalize-space(@class), ' '), ' ms-GroupHeader ')])]")).FirstOrDefault();
            return ribbon == null ? null : FindCommandInBar(driver, ribbon, command);
        }

        /// <summary>
        /// Finds an inline command in the rows of a record, the way a user clicks it on the record's row without selecting
        /// the record first, looking into the row's overflow menu when needed.
        /// </summary>
        /// <param name="driver">The web driver.</param>
        /// <param name="rows">The rows of the record.</param>
        /// <param name="command">The label, id or icon file name (e.g. <c>download.svg</c>).</param>
        /// <returns>The command, or null when it is not displayed.</returns>
        public static IWebElement FindPortalRowCommand(this IWebDriver driver, IReadOnlyList<IWebElement> rows, string command)
        {
            return rows.Select(row => FindCommandInBar(driver, row, command)).FirstOrDefault(found => found != null);
        }

        /// <summary>
        /// Waits until a command of a subgrid is displayed and enabled (the ribbon re-evaluates its rules asynchronously,
        /// e.g. after the selection changes).
        /// </summary>
        /// <param name="driver">The web driver.</param>
        /// <param name="subGrid">The subgrid root element.</param>
        /// <param name="command">The label, id or icon file name.</param>
        /// <returns>Whether the command became available.</returns>
        public static bool WaitForPortalSubGridCommand(this IWebDriver driver, IWebElement subGrid, string command)
        {
            var available = driver.WaitForPortalCondition(() => driver.FindPortalSubGridCommand(subGrid, command) is { } c && !c.IsPortalElementDisabled(), CommandTimeout);
            driver.CloseMenus();
            return available;
        }

        /// <summary>
        /// Gets a value indicating whether a command of a subgrid stays hidden or disabled, once the inline commands of the
        /// rows it is looked up in are evaluated.
        /// </summary>
        /// <param name="driver">The web driver.</param>
        /// <param name="subGrid">The subgrid root element.</param>
        /// <param name="command">The label, id or icon file name.</param>
        /// <returns>Whether the command is not available.</returns>
        public static bool IsPortalSubGridCommandUnavailable(this IWebDriver driver, IWebElement subGrid, string command)
        {
            var loaded = driver.WaitForPortalCondition(() => GetCommandRows(subGrid).All(row => row.FindElements(By.CssSelector(".ms-Shimmer-container")).Count == 0), CommandTimeout);
            if (!loaded)
            {
                throw new WebDriverTimeoutException($"The commands of the rows of the subgrid were still loading after {CommandTimeout.TotalSeconds} seconds.");
            }

            var unavailable = driver.BecomesAbsent(() => driver.FindPortalSubGridCommand(subGrid, command) is { } c && !c.IsPortalElementDisabled(), CommandTimeout);
            driver.CloseMenus();
            return unavailable;
        }

        // The grid is loaded once its loading overlay is gone (ag-grid keeps its overlay elements once shown and only hides
        // them); the legacy view once it shows rows, (collapsed) groups or its "no records" message instead of its loading
        // placeholder.
        private static bool IsLoaded(IWebElement root)
        {
            if (root.FindElements(By.CssSelector(".ag-root")).Count > 0)
            {
                return !root.FindElements(By.CssSelector(".ag-overlay-loading-wrapper")).Any(overlay => overlay.Displayed);
            }

            return root.FindElements(By.CssSelector(".ms-DetailsList")).Count > 0
                && root.FindElements(By.CssSelector($".ms-DetailsRow, {GroupHeaders}, .TALXIS-Portal-View-noData")).Count > 0;
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

            var plainCell = rows.SelectMany(row => row.FindElements(By.CssSelector(RowCells)))
                .FirstOrDefault(cell => cell.Displayed
                    && !(cell.GetAttribute("col-id") ?? cell.GetAttribute("data-automation-key") ?? string.Empty).EndsWith("gridRibbonButtons", StringComparison.Ordinal)
                    && cell.FindElements(By.CssSelector("a, button, input, [role='link'], [role='button']")).Count == 0);
            if (plainCell != null)
            {
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", plainCell);
                driver.ClickWhenUnblocked(() => plainCell, CommandTimeout, "The record's row");
                if (driver.WaitForPortalCondition(() => rows.Any(IsRowSelected), TimeSpan.FromSeconds(2)))
                {
                    return;
                }
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
            var rows = subGrid.FindElements(By.CssSelector($"{Rows}, {GroupHeaders}"));
            var selected = rows.Where(IsRowSelected).ToList();
            if (selected.Count == 0)
            {
                return rows;
            }

            var selectedRecordIds = selected.Select(row => row.GetAttribute("row-id")).Where(id => id != null).ToHashSet();
            return rows.Where(row => selected.Contains(row) || selectedRecordIds.Contains(row.GetAttribute("row-id") ?? string.Empty)).ToList();
        }

        private static bool IsRowSelected(IWebElement row)
        {
            var classes = (row.GetAttribute("class") ?? string.Empty).Split(' ');
            return classes.Contains("ag-row-selected") || classes.Contains("is-selected")
                || row.GetAttribute("aria-selected") == "true"
                || row.GetAttribute(TargetRowAttribute) == "true"
                || row.FindElements(By.CssSelector(RowChecks)).Any(IsChecked);
        }

        // ag-grid renders a record's row once per pinned part, all with the record id; a legacy row is identified by its index.
        private static string RowKey(IWebElement row)
        {
            return row.GetAttribute("row-id") ?? $"index-{row.GetAttribute("data-item-index")}";
        }

        // Finds a command in a command bar (a ribbon or a row), including its overflow menu.
        private static IWebElement FindCommandInBar(IWebDriver driver, IWebElement bar, string command)
        {
            var found = bar.FindElements(By.CssSelector("[data-command]")).FirstOrDefault(c => IsCommand(c, command));
            if (found != null)
            {
                return found;
            }

            foreach (var overflow in bar.FindElements(By.CssSelector(".ms-CommandBar-overflowButton")).Where(b => b.Displayed))
            {
                overflow.Click();
                var menuItem = driver.FindElements(By.CssSelector(".ms-ContextualMenu [data-command]")).FirstOrDefault(c => IsCommand(c, command));
                if (menuItem != null)
                {
                    return menuItem;
                }

                driver.CloseMenus();
            }

            return null;
        }

        private static bool IsCommand(IWebElement element, string command)
        {
            if (element.GetAttribute("data-id") == command || element.GetAttribute("data-command") == command
                || PortalPageExtensions.NormalizeText(element.Text) == command)
            {
                return true;
            }

            return element.FindElements(By.CssSelector("svg[data-src], img[src]"))
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
                || checkbox.FindElements(By.CssSelector("input[type='checkbox']")).Any(input => input.Selected);
        }

        private static WebDriverWait CreateWait(IWebDriver driver, TimeSpan timeout, string message)
        {
            var wait = new WebDriverWait(driver, timeout) { Message = message };
            wait.IgnoreExceptionTypes(typeof(NoSuchElementException), typeof(StaleElementReferenceException));
            return wait;
        }
    }
}
