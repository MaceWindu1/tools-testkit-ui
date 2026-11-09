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

            var clicked = Client.Execute(Client.GetOptions($"Click Command: {name}"), driver =>
            {
                var ribbon = GetRibbon(driver);

                // Try clicking on the command
                if (TryClickCommand(ribbon, name, driver)) return true;
                if (TryClickOverflowCommand(ribbon, name, driver)) return true;

                throw new InvalidOperationException($"No command with the name '{name}' exists in the CommandBar.");
            });

            var subcommands = new[] { subname, subSecondName }.Where(subcommand => !string.IsNullOrEmpty(subcommand)).ToList();
            if (subcommands.Count == 0)
                return clicked;

            // A command with sub commands lists them in a flyout once it is clicked. They are not retried: clicking the
            // command again would close its flyout.
            return Client.Execute(Client.GetNoRetryOptions($"Click Command: {name} > {string.Join(" > ", subcommands)}"), driver =>
            {
                var parent = name;
                foreach (var subcommand in subcommands)
                {
                    ClickFlyoutCommand(driver, parent, subcommand);
                    parent = subcommand;
                }

                return true;
            });
        }

        private static void ClickFlyoutCommand(IWebDriver driver, string parent, string name)
        {
            // A flyout can load its commands from the server (e.g. flows, reports or templates).
            driver.WaitUntilAvailable(
                CommandBarElementsLocators.FlyoutCommand(name),
                $"No command with the name '{name}' exists under '{parent}' in the CommandBar, or the flyout of '{parent}' did not open.")
                .Click(true);
            driver.WaitForTransaction();
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
            if (!ribbon.TryFindElement(RelatedElementsLocators.CommandBarOverflowButton, out var moreCommands))
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
