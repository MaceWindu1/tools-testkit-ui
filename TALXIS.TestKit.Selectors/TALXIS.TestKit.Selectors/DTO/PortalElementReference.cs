using System.Collections.Generic;

namespace TALXIS.TestKit.Selectors
{
    /// <summary>
    /// The keys of the TALXIS Portal selectors in <see cref="PortalElements"/>, kept apart from those of model-driven apps
    /// (<see cref="AppReference"/>) so that a change for one UI cannot break the other.
    /// </summary>
    public static class PortalReference
    {
        public static class Page
        {
            public static string ErrorIcon = "Portal_Page_ErrorIcon";
            public static string Modal = "Portal_Page_Modal";
            public static string ContextualMenu = "Portal_Page_ContextualMenu";
            public static string OverflowButton = "Portal_Page_OverflowButton";
        }

        public static class Navigation
        {
            public static string HorizontalItems = "Portal_Navigation_HorizontalItems";
            public static string VerticalItems = "Portal_Navigation_VerticalItems";
        }

        public static class Tabs
        {
            public static string Bars = "Portal_Tabs_Bars";
            public static string Tab = "Portal_Tabs_Tab";
            public static string MenuTab = "Portal_Tabs_MenuTab";
        }

        public static class Form
        {
            public static string FormDialog = "Portal_Form_FormDialog";
            public static string FieldCell = "Portal_Form_FieldCell";
            public static string RequiredIndicator = "Portal_Form_RequiredIndicator";
            public static string LockIcon = "Portal_Form_LockIcon";
            public static string LookupControl = "Portal_Form_LookupControl";
            public static string LookupValue = "Portal_Form_LookupValue";
            public static string LookupRemove = "Portal_Form_LookupRemove";
            public static string LookupInput = "Portal_Form_LookupInput";
            public static string LookupSuggestions = "Portal_Form_LookupSuggestions";
            public static string TextInput = "Portal_Form_TextInput";
            public static string ValueInput = "Portal_Form_ValueInput";
            public static string OptionSetInput = "Portal_Form_OptionSetInput";
            public static string OptionSetOption = "Portal_Form_OptionSetOption";
            public static string ReadOnlyValue = "Portal_Form_ReadOnlyValue";
        }

        public static class Dialog
        {
            public static string Modal = "Portal_Dialog_Modal";
            public static string MessageDialog = "Portal_Dialog_MessageDialog";
            public static string Button = "Portal_Dialog_Button";
            public static string FooterButton = "Portal_Dialog_FooterButton";
            public static string CloseButton = "Portal_Dialog_CloseButton";
            public static string ConfirmButton = "Portal_Dialog_ConfirmButton";
            public static string CancelButton = "Portal_Dialog_CancelButton";
            public static string ChoiceOption = "Portal_Dialog_ChoiceOption";
            public static string MessageTitle = "Portal_Dialog_MessageTitle";
            public static string Title = "Portal_Dialog_Title";
            public static string StepContent = "Portal_Dialog_StepContent";
            public static string Text = "Portal_Dialog_Text";
        }

        public static class Grid
        {
            public static string SubGridRoot = "Portal_Grid_SubGridRoot";
            public static string SubGridRibbon = "Portal_Grid_SubGridRibbon";
            public static string Rows = "Portal_Grid_Rows";
            public static string RowsOfRecord = "Portal_Grid_RowsOfRecord";
            public static string RowsWithRowId = "Portal_Grid_RowsWithRowId";
            public static string RowCells = "Portal_Grid_RowCells";
            public static string RowChecks = "Portal_Grid_RowChecks";
            public static string CheckboxInput = "Portal_Grid_CheckboxInput";
            public static string RowOfCell = "Portal_Grid_RowOfCell";
            public static string RowButton = "Portal_Grid_RowButton";
            public static string RowInteractiveContent = "Portal_Grid_RowInteractiveContent";
            public static string Headers = "Portal_Grid_Headers";
            public static string CellsOfColumn = "Portal_Grid_CellsOfColumn";
            public static string GroupHeaders = "Portal_Grid_GroupHeaders";
            public static string GroupHeaderCellsOfColumn = "Portal_Grid_GroupHeaderCellsOfColumn";
            public static string GroupHeaderOfCell = "Portal_Grid_GroupHeaderOfCell";
            public static string GroupExpander = "Portal_Grid_GroupExpander";
            public static string GroupOfHeader = "Portal_Grid_GroupOfHeader";
            public static string GroupRows = "Portal_Grid_GroupRows";
            public static string GridRoot = "Portal_Grid_GridRoot";
            public static string GridLoadingOverlay = "Portal_Grid_GridLoadingOverlay";
            public static string LegacyView = "Portal_Grid_LegacyView";
            public static string LegacyViewLoaded = "Portal_Grid_LegacyViewLoaded";
            public static string Command = "Portal_Grid_Command";
            public static string MenuCommand = "Portal_Grid_MenuCommand";
            public static string CommandIcon = "Portal_Grid_CommandIcon";
            public static string CommandsLoading = "Portal_Grid_CommandsLoading";
        }

        public static class Login
        {
            public static string SignInName = "Portal_Login_SignInName";
            public static string Password = "Portal_Login_Password";
            public static string SignIn = "Portal_Login_SignIn";
        }
    }

    /// <summary>
    /// The selectors of the TALXIS Portal markup. The portal renders Fluent UI and ag-grid, whose markup is matched most
    /// clearly by CSS selectors, so the selectors are CSS (<see cref="Css"/>) unless they need XPath (<see cref="Xpath"/>):
    /// a text, an ancestor or a negation. <c>[NAME]</c>, <c>[ID]</c> and <c>[COLUMN]</c> are replaced with a control name, a
    /// record id and a column key; <c>[LABEL]</c> and <c>[NAMEPREFIX]</c> with XPath string literals.
    /// </summary>
    public static class PortalElements
    {
        private const string Cell = "div[contains(concat(' ', normalize-space(@class), ' '), ' TALXIS__form__cell ')]";
        private const string CellMatch = "[.//label[contains(@class, 'TALXIS__form__cell__label') and @title=[LABEL]] or .//div[starts-with(@data-id, [NAMEPREFIX])]]";
        private const string Modal = "//div[@role='dialog' or @role='alertdialog']";
        private const string RowAncestor = "contains(concat(' ', normalize-space(@class), ' '), ' ag-row ') or contains(concat(' ', normalize-space(@class), ' '), ' ms-DetailsRow ')";

        public static Dictionary<string, string> Css = new Dictionary<string, string>()
        {
            // Page
            { "Portal_Page_ErrorIcon", ".TALXIS-Portal-errorDialog-title-icon" },
            { "Portal_Page_Modal", "div[role='dialog'], div[role='alertdialog']" },
            { "Portal_Page_ContextualMenu", ".ms-ContextualMenu" },
            { "Portal_Page_OverflowButton", ".ms-CommandBar-overflowButton" },

            // Navigation
            { "Portal_Navigation_HorizontalItems", "[data-id^='sitemap-entity-']" },
            { "Portal_Navigation_VerticalItems", ".TALXIS__navigation--vertical .ms-Nav-link" },

            // Tabs
            { "Portal_Tabs_Bars", ".TALXIS__form__tab" },
            { "Portal_Tabs_Tab", "[data-id^='tablist-']" },
            { "Portal_Tabs_MenuTab", ".ms-ContextualMenu [data-id^='tablist-']" },

            // Form
            { "Portal_Form_RequiredIndicator", ".TALXIS__form__cell__required-indicator" },
            { "Portal_Form_LookupControl", ".talxis__lookupControl" },
            { "Portal_Form_LookupValue", ".talxis__lookupControl button[data-entity]" },
            { "Portal_Form_LookupInput", "input.ms-BasePicker-input" },
            { "Portal_Form_TextInput", "textarea, input.ms-TextField-field" },
            { "Portal_Form_ValueInput", "textarea, input.ms-TextField-field, input.ms-ComboBox-Input" },
            { "Portal_Form_OptionSetInput", "input.ms-ComboBox-Input" },
            { "Portal_Form_ReadOnlyValue", "[data-id$='fieldControl_container'], [data-id*='.fieldControl']" },

            // Dialog
            { "Portal_Dialog_CloseButton", ".ms-Dialog-button--close, .ms-Panel-closeButton" },
            { "Portal_Dialog_ConfirmButton", ".ms-Dialog-actions .ms-Button--primary" },
            { "Portal_Dialog_CancelButton", ".ms-Dialog-actions .ms-Button--default" },
            { "Portal_Dialog_MessageTitle", ".ms-Dialog-title" },
            { "Portal_Dialog_Title", ".ms-Dialog-title, .ms-Panel-headerText" },
            { "Portal_Dialog_StepContent", ".TALXIS__form__tab-content, .ms-Dialog-content" },

            // Grid
            { "Portal_Grid_SubGridRoot", "[data-id='dataSetRoot_[NAME]']" },
            { "Portal_Grid_Rows", ".ag-row[row-id], .ms-DetailsRow" },
            { "Portal_Grid_RowsOfRecord", ".ag-row[row-id='[ID]' i]" },
            { "Portal_Grid_RowsWithRowId", ".ag-row[row-id='[ID]']" },
            { "Portal_Grid_RowCells", ".ag-cell[col-id], .ms-DetailsRow-cell[data-automation-key]" },
            { "Portal_Grid_RowChecks", ".ms-Checkbox, .ms-DetailsRow-check" },
            { "Portal_Grid_CheckboxInput", "input[type='checkbox']" },
            { "Portal_Grid_RowInteractiveContent", "a, button, input, [role='link'], [role='button']" },
            { "Portal_Grid_Headers", ".ag-header-cell[col-id], .ms-DetailsHeader-cell[data-item-key]" },
            { "Portal_Grid_CellsOfColumn", ".ag-row .ag-cell[col-id='[COLUMN]'], .ms-DetailsRow .ms-DetailsRow-cell[data-automation-key='[COLUMN]']" },
            { "Portal_Grid_GroupHeaders", ".ms-GroupHeader" },
            { "Portal_Grid_GroupHeaderCellsOfColumn", ".ms-GroupHeader .ms-DetailsRow-cell[data-automation-key='[COLUMN]']" },
            { "Portal_Grid_GroupExpander", ".ms-GroupHeader-expand" },
            { "Portal_Grid_GroupRows", ".ms-DetailsRow" },
            { "Portal_Grid_GridRoot", ".ag-root" },
            { "Portal_Grid_GridLoadingOverlay", ".ag-overlay-loading-wrapper" },
            { "Portal_Grid_LegacyView", ".ms-DetailsList" },
            { "Portal_Grid_LegacyViewLoaded", ".ms-DetailsRow, .ms-GroupHeader, .TALXIS-Portal-View-noData" },
            { "Portal_Grid_Command", "[data-command]" },
            { "Portal_Grid_MenuCommand", ".ms-ContextualMenu [data-command]" },
            { "Portal_Grid_CommandIcon", "svg[data-src], img[src]" },
            { "Portal_Grid_CommandsLoading", ".ms-Shimmer-container" },

            // Login (the Azure AD B2C local-account sign-in page)
            { "Portal_Login_SignInName", "#signInName" },
            { "Portal_Login_Password", "#password" },
            { "Portal_Login_SignIn", "#next" },
        };

        public static Dictionary<string, string> Xpath = new Dictionary<string, string>()
        {
            // Form: the innermost form cell with the label or control, since a cell can host a nested form (e.g. the detail
            // of the record selected in a subgrid), whose cells it then contains.
            { "Portal_Form_FormDialog", Modal + "[.//section[contains(@class, 'TALXIS__form')]]" },
            { "Portal_Form_FieldCell", ".//" + Cell + CellMatch + "[not(.//" + Cell + CellMatch + ")]" },
            { "Portal_Form_LockIcon", ".//i[contains(@class, 'TALXIS__form__cell__lockIcon') and not(contains(@class, 'TALXIS__form__cell__lockIcon--hidden'))]" },
            { "Portal_Form_LookupRemove", ".//button[.//i[@data-icon-name='Cancel']]" },
            { "Portal_Form_LookupSuggestions", "//div[contains(@class, 'TALXIS__tag-picker__suggestions')]//button[@role='option']" },
            { "Portal_Form_OptionSetOption", "//button[contains(@class, 'ms-ComboBox-option') and @title=[LABEL]]" },

            // Dialog
            { "Portal_Dialog_Modal", Modal },
            { "Portal_Dialog_MessageDialog", Modal + "[not(.//section[contains(@class, 'TALXIS__form')])]" },
            // A footer button, or any button labelled by its title or aria-label (e.g. an icon button in a grid on the dialog).
            { "Portal_Dialog_Button", ".//div[contains(@class, 'ms-Dialog-actions') or contains(@class, 'ms-Panel-footer')]//button[.//span[contains(@class, 'ms-Button-label') and normalize-space()=[LABEL]]] | .//button[@title=[LABEL] or @aria-label=[LABEL]]" },
            { "Portal_Dialog_FooterButton", ".//div[contains(@class, 'ms-Dialog-actions')]//button[.//span[contains(@class, 'ms-Button-label') and normalize-space()=[LABEL]]]" },
            { "Portal_Dialog_ChoiceOption", ".//label[contains(@class, 'ms-ChoiceField-field')][.//span[contains(@class, 'ms-ChoiceFieldLabel') and normalize-space()=[LABEL]]]" },
            { "Portal_Dialog_Text", ".//*[contains(text(), [LABEL])]" },

            // Grid: the subgrid's ribbon is rendered before the subgrid root, in the view page section, and the rows' own
            // ribbons are left out.
            { "Portal_Grid_SubGridRibbon", "./ancestor::section[contains(@class, 'TALXIS__portal__view-page')][1]//div[contains(@class, 'TALXIS__ribbon') and not(ancestor::div[" + RowAncestor + " or contains(concat(' ', normalize-space(@class), ' '), ' ms-GroupHeader ')])]" },
            { "Portal_Grid_RowOfCell", "./ancestor::div[" + RowAncestor + "][1]" },
            { "Portal_Grid_RowButton", ".//button[@title=[LABEL] or @aria-label=[LABEL] or normalize-space(.)=[LABEL] or .//i[@data-icon-name=[LABEL]]]" },
            { "Portal_Grid_GroupHeaderOfCell", "./ancestor::div[contains(concat(' ', normalize-space(@class), ' '), ' ms-GroupHeader ')][1]" },
            { "Portal_Grid_GroupOfHeader", "./ancestor::div[contains(concat(' ', normalize-space(@class), ' '), ' ms-GroupedList-group ')][1]" },
        };
    }
}
