using System;
using System.Collections.Generic;
using System.Linq;
using OpenQA.Selenium;
using TALXIS.TestKit.Selectors.DTO.Locators;
using TALXIS.TestKit.Selectors.Browser;

namespace TALXIS.TestKit.Selectors.WebClientManagement
{
    public class CommandBarManager
    {
        public WebClient Client { get; }

        public CommandBarManager(WebClient client)
        {
            Client = client ?? throw new ArgumentNullException(nameof(client));
        }

        internal BrowserCommandResult<bool> ClickCommand(string name, string subname = null, string subSecondName = null, int thinkTime = Constants.DefaultThinkTime)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Command name cannot be empty", nameof(name));

            var subcommands = new[] { subname, subSecondName }.Where(subcommand => !string.IsNullOrEmpty(subcommand)).ToList();
            if (subcommands.Count == 0)
            {
                return Client.Execute(Client.GetOptions($"Click Command: {name}"), driver =>
                {
                    var ribbon = GetRibbon(driver);

                    // Try clicking on the command
                    if (TryClickCommand(ribbon, name, driver)) return true;
                    if (TryClickOverflowCommand(ribbon, name, driver)) return true;

                    throw new InvalidOperationException($"No command with the name '{name}' exists in the CommandBar.");
                });
            }

            // Each command opens the flyout that lists the next one. Clicking a command again closes its flyout, so this is
            // not retried.
            return Client.Execute(Client.GetNoRetryOptions($"Click Command: {name} > {string.Join(" > ", subcommands)}"), driver =>
            {
                var command = FindFlyoutOpener(driver, GetRibbon(driver), name);
                var parent = name;
                for (var level = 0; level < subcommands.Count; level++)
                {
                    var subcommand = subcommands[level];
                    var locator = level < subcommands.Count - 1
                        ? CommandBarElementsLocators.FlyoutOpener(subcommand)
                        : CommandBarElementsLocators.FlyoutCommand(subcommand);

                    // A flyout can load its commands from the server (e.g. flows, reports or templates).
                    command = OpenFlyout(driver, command, parent)
                        .WaitUntilAvailable(locator, $"No command with the name '{subcommand}' exists under '{parent}' in the CommandBar.");
                    parent = subcommand;
                }

                command.Click();
                driver.WaitForTransaction();
                return true;
            });
        }

        // A command that opens a flyout, in the command bar or in its More commands flyout.
        private static IWebElement FindFlyoutOpener(IWebDriver driver, IWebElement ribbon, string name)
        {
            var command = ribbon.FindElements(CommandBarElementsLocators.FlyoutOpener(name)).FirstOrDefault();
            if (command != null)
                return command;

            var notFound = $"No command with the name '{name}' that has sub commands exists in the CommandBar.";
            var moreCommands = ribbon.FindElements(CommandBarElementsLocators.OverflowButton).FirstOrDefault()
                ?? throw new InvalidOperationException(notFound);

            return OpenFlyout(driver, moreCommands, "More commands").WaitUntilAvailable(CommandBarElementsLocators.FlyoutOpener(name), notFound);
        }

        // Clicks a command that opens a flyout and waits for its flyout.
        private static IWebElement OpenFlyout(IWebDriver driver, IWebElement command, string name)
        {
            var commandItem = command.FindElement(CommandBarElementsLocators.CommandItem).GetAttribute("id");
            command.Click();

            return driver.WaitUntilAvailable(CommandBarElementsLocators.Flyout(commandItem), $"The flyout of '{name}' did not open.");
        }

        /// <summary>
        /// Returns the values of CommandBar objects
        /// </summary>
        /// <param name="includeMoreCommandsValues">Whether or not to check the more commands overflow list</param>
        /// <param name="thinkTime">Used to simulate a wait time between human interactions. The Default is 2 seconds.</param>
        /// <example>xrmApp.CommandBar.GetCommandValues();</example>
        internal BrowserCommandResult<List<string>> GetCommandValues(bool includeMoreCommandsValues = false, int thinkTime = Constants.DefaultThinkTime)
        {
            Client.ThinkTime(thinkTime);

            return Client.Execute(Client.GetOptions("Get CommandBar Command Count"), driver => TryGetCommandValues(includeMoreCommandsValues, driver));
        }

        internal void SelectInQuickCreate(string entityName)
        {
            Client.Execute<object>(Client.GetOptions("Select option in quick create"), driver =>
            {
                driver.WaitForPageToLoad(15.Seconds());

                var plusIconButtonElement = driver.FindElement(NavigationElementsLocator.QuickCreateButton);
                plusIconButtonElement.Click();

                driver.WaitForTransaction(30.Seconds());

                var dialogTitleElement = driver.FindElement(By.XPath($"//button[@role='menuitem' and @aria-label='{entityName}']"));
                dialogTitleElement.Click();

                driver.WaitForTransaction(15.Seconds());
                driver.WaitForPageToLoad(15.Seconds());

                return null;
            });
        }

        private static List<string> GetCommandNames(IEnumerable<IWebElement> commandBarItems)
        {
            var result = new List<string>();

            foreach (var value in commandBarItems)
            {
                string commandText = value.Text.Trim();

                if (string.IsNullOrWhiteSpace(commandText))
                    continue;

                if (commandText.Contains("\r\n"))
                {
                    commandText = commandText.Substring(0, commandText.IndexOf("\r\n", StringComparison.Ordinal));
                }

                result.Add(commandText);
            }

            return result;
        }

        private static Dictionary<string, IWebElement> GetMenuItems(IWebElement menu)
        {
            var result = new Dictionary<string, IWebElement>();

            NavigationManager.AddMenuItems(menu, result);

            return result;
        }

        private static bool TryClickCommand(IWebElement ribbon, string name, IWebDriver driver)
        {
            if (ribbon.TryFindElement(EntityElementsLocators.SubGridCommandLabel( name), out var command))
            {
                command.Click(true);
                driver.WaitForTransaction();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Trying to click on a button in the menu "More Commands".
        /// </summary>
        private static bool TryClickOverflowCommand(IWebElement ribbon, string name, IWebDriver driver)
        {
            if (!ribbon.TryFindElement(CommandBarElementsLocators.OverflowButton, out var moreCommands))
                return false;

            moreCommands.Click(true);
            driver.WaitForTransaction();

            var flyOutMenu = driver.WaitUntilAvailable(RelatedElementsLocators.CommandBarFlyoutButtonList);
            if (flyOutMenu.TryFindElement(EntityElementsLocators.SubGridCommandLabel( name), out var overflowCommand))
            {
                overflowCommand.Click(true);
                driver.WaitForTransaction();
                return true;
            }
            return false;
        }

        private static IWebElement GetRibbon(IWebDriver driver)
        {
            return driver.WaitUntilAvailable(CommandBarElementsLocators.Container, 5.Seconds())
                ?? driver.WaitUntilAvailable(CommandBarElementsLocators.ContainerGrid, 5.Seconds())
                ?? throw new InvalidOperationException("Unable to find the ribbon.");
        }

        private static List<string> TryGetCommandValues(bool includeMoreCommandsValues, IWebDriver driver)
        {
            const string moreCommandsLabel = "more commands";

            //Find the button in the CommandBar
            IWebElement ribbon = GetRibbon(driver);

            //Get the CommandBar buttons
            Dictionary<string, IWebElement> commandBarItems = GetMenuItems(ribbon);
            bool hasMoreCommands = commandBarItems.TryGetValue(moreCommandsLabel, out var moreCommandsButton);
            if (includeMoreCommandsValues && hasMoreCommands)
            {
                moreCommandsButton.Click(true);

                driver.WaitUntilVisible(CommandBarElementsLocators.MoreCommandsMenu,
                    menu => NavigationManager.AddMenuItems(menu, commandBarItems),
                    "Unable to locate the 'More Commands' menu"
                    );
            }

            return GetCommandNames(commandBarItems.Values);
        }

        

    }
}
