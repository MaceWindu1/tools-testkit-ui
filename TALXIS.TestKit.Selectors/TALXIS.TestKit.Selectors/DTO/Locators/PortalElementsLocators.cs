using System;
using OpenQA.Selenium;
using TALXIS.TestKit.Selectors.Browser;

namespace TALXIS.TestKit.Selectors.DTO.Locators
{
    internal static class PortalPageElementsLocators
    {
        internal static string ErrorIcon => PortalElements.Css[PortalReference.Page.ErrorIcon];
        internal static string ModalSelector => PortalElements.Css[PortalReference.Page.Modal];
        internal static By Modal => By.CssSelector(ModalSelector);
        internal static By ContextualMenu => By.CssSelector(PortalElements.Css[PortalReference.Page.ContextualMenu]);
        internal static By OverflowButton => By.CssSelector(PortalElements.Css[PortalReference.Page.OverflowButton]);
    }

    internal static class PortalNavigationElementsLocators
    {
        internal const string ItemIdPrefix = "sitemap-entity-";
        internal static By HorizontalItems => By.CssSelector(PortalElements.Css[PortalReference.Navigation.HorizontalItems]);
        internal static By VerticalItems => By.CssSelector(PortalElements.Css[PortalReference.Navigation.VerticalItems]);
    }

    internal static class PortalTabElementsLocators
    {
        internal const string TabIdPrefix = "tablist-";
        internal static By Bars => By.CssSelector(PortalElements.Css[PortalReference.Tabs.Bars]);
        internal static By Tab => By.CssSelector(PortalElements.Css[PortalReference.Tabs.Tab]);
        internal static By MenuTab => By.CssSelector(PortalElements.Css[PortalReference.Tabs.MenuTab]);
    }

    internal static class PortalFormElementsLocators
    {
        internal static By FormDialog => By.XPath(PortalElements.Xpath[PortalReference.Form.FormDialog]);
        internal static By FieldCell(string labelOrName) => By.XPath(PortalElements.Xpath[PortalReference.Form.FieldCell]
            .Replace("[LABEL]", labelOrName.ToXPathLiteral())
            .Replace("[NAMEPREFIX]", (labelOrName + ".").ToXPathLiteral()));
        internal static By RequiredIndicator => By.CssSelector(PortalElements.Css[PortalReference.Form.RequiredIndicator]);
        internal static By LockIcon => By.XPath(PortalElements.Xpath[PortalReference.Form.LockIcon]);
        internal static By LookupControl => By.CssSelector(PortalElements.Css[PortalReference.Form.LookupControl]);
        internal static By LookupValue => By.CssSelector(PortalElements.Css[PortalReference.Form.LookupValue]);
        internal static By LookupRemove => By.XPath(PortalElements.Xpath[PortalReference.Form.LookupRemove]);
        internal static By LookupInput => By.CssSelector(PortalElements.Css[PortalReference.Form.LookupInput]);
        internal static By LookupSuggestions => By.XPath(PortalElements.Xpath[PortalReference.Form.LookupSuggestions]);
        internal static By TextInput => By.CssSelector(PortalElements.Css[PortalReference.Form.TextInput]);
        internal static By ValueInput => By.CssSelector(PortalElements.Css[PortalReference.Form.ValueInput]);
        internal static By OptionSetInput => By.CssSelector(PortalElements.Css[PortalReference.Form.OptionSetInput]);
        internal static By OptionSetOption(string label) => By.XPath(PortalElements.Xpath[PortalReference.Form.OptionSetOption].Replace("[LABEL]", label.ToXPathLiteral()));
        internal static By ReadOnlyValue => By.CssSelector(PortalElements.Css[PortalReference.Form.ReadOnlyValue]);
    }

    internal static class PortalDialogElementsLocators
    {
        internal static By Modal => By.XPath(PortalElements.Xpath[PortalReference.Dialog.Modal]);
        internal static By MessageDialog => By.XPath(PortalElements.Xpath[PortalReference.Dialog.MessageDialog]);
        internal static By Button(string label) => By.XPath(PortalElements.Xpath[PortalReference.Dialog.Button].Replace("[LABEL]", label.ToXPathLiteral()));
        internal static By FooterButton(string label) => By.XPath(PortalElements.Xpath[PortalReference.Dialog.FooterButton].Replace("[LABEL]", label.ToXPathLiteral()));
        internal static By CloseButton => By.CssSelector(PortalElements.Css[PortalReference.Dialog.CloseButton]);
        internal static By ConfirmButton => By.CssSelector(PortalElements.Css[PortalReference.Dialog.ConfirmButton]);
        internal static By CancelButton => By.CssSelector(PortalElements.Css[PortalReference.Dialog.CancelButton]);
        internal static By ChoiceOption(string label) => By.XPath(PortalElements.Xpath[PortalReference.Dialog.ChoiceOption].Replace("[LABEL]", label.ToXPathLiteral()));
        internal static By MessageTitle => By.CssSelector(PortalElements.Css[PortalReference.Dialog.MessageTitle]);
        internal static By Title => By.CssSelector(PortalElements.Css[PortalReference.Dialog.Title]);
        internal static By StepContent => By.CssSelector(PortalElements.Css[PortalReference.Dialog.StepContent]);
        internal static By Text(string text) => By.XPath(PortalElements.Xpath[PortalReference.Dialog.Text].Replace("[LABEL]", text.ToXPathLiteral()));
    }

    internal static class PortalGridElementsLocators
    {
        // The column of the row ribbons (the inline commands) of a grid.
        internal const string RibbonButtonsColumnSuffix = "gridRibbonButtons";

        internal static By SubGridRoot(string name) => By.CssSelector(PortalElements.Css[PortalReference.Grid.SubGridRoot].Replace("[NAME]", name));
        internal static By SubGridRibbon => By.XPath(PortalElements.Xpath[PortalReference.Grid.SubGridRibbon]);
        internal static string RowsSelector => PortalElements.Css[PortalReference.Grid.Rows];
        internal static string GroupHeadersSelector => PortalElements.Css[PortalReference.Grid.GroupHeaders];
        internal static By Rows => By.CssSelector(RowsSelector);
        internal static By RowsAndGroupHeaders => By.CssSelector($"{RowsSelector}, {GroupHeadersSelector}");
        internal static By RowsOfRecord(Guid recordId) => By.CssSelector(PortalElements.Css[PortalReference.Grid.RowsOfRecord].Replace("[ID]", recordId.ToString("D")));
        internal static By RowsWithRowId(string rowId) => By.CssSelector(PortalElements.Css[PortalReference.Grid.RowsWithRowId].Replace("[ID]", rowId));
        internal static By RowCells => By.CssSelector(PortalElements.Css[PortalReference.Grid.RowCells]);
        internal static By RowChecks => By.CssSelector(PortalElements.Css[PortalReference.Grid.RowChecks]);
        internal static By CheckboxInput => By.CssSelector(PortalElements.Css[PortalReference.Grid.CheckboxInput]);
        internal static By RowOfCell => By.XPath(PortalElements.Xpath[PortalReference.Grid.RowOfCell]);
        internal static By RowButton(string label) => By.XPath(PortalElements.Xpath[PortalReference.Grid.RowButton].Replace("[LABEL]", label.ToXPathLiteral()));
        internal static By RowInteractiveContent => By.CssSelector(PortalElements.Css[PortalReference.Grid.RowInteractiveContent]);
        internal static By Headers => By.CssSelector(PortalElements.Css[PortalReference.Grid.Headers]);
        internal static By CellsOfColumn(string columnKey) => By.CssSelector(PortalElements.Css[PortalReference.Grid.CellsOfColumn].Replace("[COLUMN]", columnKey));
        internal static By GroupHeaderCellsOfColumn(string columnKey) => By.CssSelector(PortalElements.Css[PortalReference.Grid.GroupHeaderCellsOfColumn].Replace("[COLUMN]", columnKey));
        internal static By GroupHeaderOfCell => By.XPath(PortalElements.Xpath[PortalReference.Grid.GroupHeaderOfCell]);
        internal static By GroupExpander => By.CssSelector(PortalElements.Css[PortalReference.Grid.GroupExpander]);
        internal static By GroupOfHeader => By.XPath(PortalElements.Xpath[PortalReference.Grid.GroupOfHeader]);
        internal static By GroupRows => By.CssSelector(PortalElements.Css[PortalReference.Grid.GroupRows]);
        internal static By GridRoot => By.CssSelector(PortalElements.Css[PortalReference.Grid.GridRoot]);
        internal static By GridLoadingOverlay => By.CssSelector(PortalElements.Css[PortalReference.Grid.GridLoadingOverlay]);
        internal static By LegacyView => By.CssSelector(PortalElements.Css[PortalReference.Grid.LegacyView]);
        internal static By LegacyViewLoaded => By.CssSelector(PortalElements.Css[PortalReference.Grid.LegacyViewLoaded]);
        internal static By Command => By.CssSelector(PortalElements.Css[PortalReference.Grid.Command]);
        internal static By MenuCommand => By.CssSelector(PortalElements.Css[PortalReference.Grid.MenuCommand]);
        internal static By CommandIcon => By.CssSelector(PortalElements.Css[PortalReference.Grid.CommandIcon]);
        internal static By CommandsLoading => By.CssSelector(PortalElements.Css[PortalReference.Grid.CommandsLoading]);
    }

    internal static class PortalLoginElementsLocators
    {
        internal static By SignInName => By.CssSelector(PortalElements.Css[PortalReference.Login.SignInName]);
        internal static By Password => By.CssSelector(PortalElements.Css[PortalReference.Login.Password]);
        internal static By SignIn => By.CssSelector(PortalElements.Css[PortalReference.Login.SignIn]);
    }
}
