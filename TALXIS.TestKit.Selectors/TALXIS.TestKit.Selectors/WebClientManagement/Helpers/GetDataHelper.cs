
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using OpenQA.Selenium;
using TALXIS.TestKit.Selectors.DTO;
using TALXIS.TestKit.Selectors.DTO.Locators;
using TALXIS.TestKit.Selectors.Browser;

namespace TALXIS.TestKit.Selectors.WebClientManagement.Helpers
{
    internal class GetDataHelper
    {
        // The form types of Xrm.Page.ui.getFormType().
        private const long CreateFormType = 1;
        private const long UpdateFormType = 2;

        internal static BrowserCommandResult<string> GetBusinessProcessErrorText(int waitTimeInSeconds, WebClient client)
        {
            return client.Execute(client.GetOptions($"Get Business Process Error Text"), driver =>
            {
                string errorDetails = string.Empty;
                var errorDialog = driver.WaitUntilAvailable(By.XPath("//div[contains(@data-id,'errorDialogdialog')]"), new TimeSpan(0, 0, waitTimeInSeconds));

                // Is error dialog present?
                if (errorDialog != null)
                {
                    var errorDetailsElement = errorDialog.FindElement(By.XPath(".//*[contains(@data-id,'errorDialog_subtitle')]"));

                    if (errorDetailsElement != null)
                    {
                        if (!String.IsNullOrEmpty(errorDetailsElement.Text))
                            errorDetails = errorDetailsElement.Text;
                    }
                }

                return errorDetails;
            });
        }

        internal static BrowserCommandResult<IReadOnlyList<FormNotification>> GetFormNotifications(WebClient client)
        {
            return client.Execute(client.GetOptions($"Get all form notifications"), driver =>
            {
                List<FormNotification> notifications = new List<FormNotification>();

                // Look for notificationMessageAndButtons bar
                var notificationMessage = driver.WaitUntilAvailable(EntityElementsLocators.FormMessageBar, TimeSpan.FromSeconds(2));

                if (notificationMessage != null)
                {
                    IWebElement icon = null;

                    try
                    {
                        icon = driver.FindElement(EntityElementsLocators.FormMessageBarTypeIcon);
                    }
                    catch (NoSuchElementException)
                    {
                        // Swallow the exception
                    }

                    if (icon != null)
                    {
                        var notification = new FormNotification
                        {
                            Message = notificationMessage?.Text
                        };
                        string classes = icon.GetAttribute("class");
                        notification.SetTypeFromClass(classes);
                        notifications.Add(notification);
                    }
                }

                // Look for the notification wrapper, if it doesn't exist there are no notificatios
                var notificationBar = driver.WaitUntilVisible(EntityElementsLocators.FormNotifcationBar, TimeSpan.FromSeconds(2));
                if (notificationBar == null)
                    return notifications;
                else
                {
                    // If there are multiple notifications, the notifications must be expanded first.
                    if (notificationBar.TryFindElement(EntityElementsLocators.FormNotifcationExpandButton, out var expandButton))
                    {
                        if (!Convert.ToBoolean(notificationBar.GetAttribute("aria-expanded")))
                            expandButton.Click();

                        // After expansion the list of notifications are now in a different element
                        notificationBar = driver.WaitUntilAvailable(EntityElementsLocators.FormNotifcationFlyoutRoot, TimeSpan.FromSeconds(2), "Failed to open the form notifications");
                    }

                    var notificationList = notificationBar.FindElement(EntityElementsLocators.FormNotifcationList);
                    var notificationListItems = notificationList.FindElements(By.TagName("li"));

                    foreach (var item in notificationListItems)
                    {
                        var icon = item.FindElement(EntityElementsLocators.FormNotifcationTypeIcon);

                        var notification = new FormNotification
                        {
                            Message = item.Text
                        };
                        string classes = icon.GetAttribute("class");
                        notification.SetTypeFromClass(classes);
                        notifications.Add(notification);
                    }

                    if (notificationBar != null)
                    {
                        notificationBar = driver.WaitUntilVisible(EntityElementsLocators.FormNotifcationBar, TimeSpan.FromSeconds(2));
                        notificationBar.Click(true); // Collapse the notification bar
                    }
                    return notifications;
                }

            }).Value;
        }

        internal static string GetGridQueryKey(IWebDriver driver, string dataSetName = null)
        {
            Dictionary<string, object> pages = (Dictionary<string, object>)driver.ExecuteScript($"return window[Object.keys(window).find(i => !i.indexOf(\"__store$\"))].getState().pages");

            //This is the current view
            Dictionary<string, object> pageData = (Dictionary<string, object>)pages.Last().Value;
            IList<KeyValuePair<string, object>> datasets = pageData.Where(i => i.Key == "datasets").ToList();

            //Get Entity From Page List
            Dictionary<string, object> entityName = null;

            if (dataSetName != null)
            {
                foreach (KeyValuePair<string, object> dataset in datasets)
                {
                    foreach (KeyValuePair<string, object> datasetList in (Dictionary<string, object>)dataset.Value)
                    {
                        if (datasetList.Key == dataSetName)
                        {
                            entityName = (Dictionary<string, object>)datasetList.Value;
                            return (string)entityName["queryKey"];
                        }
                    }

                }
                throw new Exception("Invalid DataSet Name");
            }
            else
            {
                entityName = (Dictionary<string, object>)datasets[0].Value;
                Dictionary<string, object> entityQueryListList = (Dictionary<string, object>)entityName.First().Value;
                return (string)entityQueryListList["queryKey"];
            }
        }

        internal static ICollection<IWebElement> GetListItems(IWebElement container, LookupItem control)
        {
            var name = control.Name;
            var xpathToItems = EntityElementsLocators.LookupFieldResultListItem(name);

            //wait for complete the search
            container.WaitUntil(d => d.FindVisible(xpathToItems)?.Text?.Contains(control.Value, StringComparison.OrdinalIgnoreCase) == true);

            ICollection<IWebElement> result = container.WaitUntil(
                d => d.FindElements(xpathToItems),
                failureCallback: () => throw new InvalidOperationException($"No Results Matching {control.Value} Were Found.")
                );

            return result;
        }

        internal static bool TryGetOptionSet(string fieldName, WebClient client, out IWebElement optionSet)
        {
            var result = client.Execute(
                client.GetOptions("Get Option Set"),
                driver =>
                {
                    // Attempt to find the option set element using a CSS selector
                    if (!driver.TryFindElement(By.CssSelector($"select[data-id*=\"{fieldName}.fieldControl-option-set-select\"]"), out var element))
                    {
                        // Instead of throwing an exception, return null so that the method can indicate failure via the boolean result
                        return null;
                    }
                    return element;
                });

            optionSet = result.Value;
            return optionSet != null;
        }

        internal static BrowserCommandResult<List<GridItem>> GetSubGridItems(string subgridName, WebClient client)
        {
            return client.Execute(client.GetOptions($"Get Subgrid Items for Subgrid {subgridName}"), driver =>
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
                if (subGrid.TryFindElement(By.XPath(AppElements.Xpath[AppReference.Entity.SubGridListCells].Replace("[NAME]", subgridName)), out subGridRecordList))
                {

                    // Locate record list
                    var foundRecords = subGrid.TryFindElement(By.XPath(AppElements.Xpath[AppReference.Entity.SubGridListCells].Replace("[NAME]", subgridName)), out subGridRecordList);

                    if (foundRecords)
                    {
                        var subGridRecordRows = subGrid.FindElements(EntityElementsLocators.SubGridList(subgridName));
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
                        throw new NotFoundException($"Unable to locate record list for subgrid {subgridName}");
                }
                // Attempt to locate the editable grid list
                else if (subGrid.TryFindElement(EntityElementsLocators.EditableSubGridList(subgridName), out subGridRecordList))
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

        internal static string GetFormStatus(WebClient client)
        {
            var result = client.Execute(
                client.GetOptions("Get Option Set"),
                driver =>
                {
                    driver.WaitForTransaction();
                    if (!driver.TryFindElement(By.Id("message-formReadOnlyNotification"), out var readOnlyNotification))
                    {
                        // Without the read-only notification, only an editable form shows an active record (an inactive
                        // record opens in a disabled form with the notification).
                        return IsFormEditable(driver) ? "Active" : null;
                    }

                    var match = Regex.Match(readOnlyNotification.Text, "(?<=status:\\s)\\w+");
                    if (match.Success)
                    {
                        return match.Captures[0].Value;
                    }

                    return null;
                });

            return result.Value;
        }

        // A create (1) or update (2) form; a read-only (3) or disabled (4) form shows a record that cannot be edited.
        private static bool IsFormEditable(IWebDriver driver)
        {
            var formType = ((IJavaScriptExecutor)driver).ExecuteScript(
                "return typeof Xrm !== 'undefined' && Xrm.Page.ui && Xrm.Page.ui.getFormType ? Xrm.Page.ui.getFormType() : 0;");

            return formType is long type && (type == CreateFormType || type == UpdateFormType);
        }
    }
}
