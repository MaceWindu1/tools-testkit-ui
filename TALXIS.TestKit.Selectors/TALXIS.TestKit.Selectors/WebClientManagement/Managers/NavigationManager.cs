using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using TALXIS.TestKit.Selectors.DTO.Locators;
using TALXIS.TestKit.Selectors.Browser;

namespace TALXIS.TestKit.Selectors.WebClientManagement
{
    public class NavigationManager
    {
        public WebClient Client { get; }

        public NavigationManager(WebClient client)
        {
            Client = client ?? throw new ArgumentNullException(nameof(client)); ;
        }

        /// <summary>
        /// Gets a value indicating whether the app shows the new look: its toggle is on, or the app has no toggle (only the
        /// new look).
        /// </summary>
        public static bool IsNewLookEnabled(IWebDriver driver)
        {
            var toggle = driver.FindElements(NavigationElementsLocator.NewLookToggle).FirstOrDefault();
            return toggle == null || toggle.GetAttribute("aria-checked") == "true";
        }

        public static Dictionary<string, IWebElement> GetSubAreaMenuItems(IWebDriver driver)
        {
            var dictionary = new Dictionary<string, IWebElement>();

            //Sitemap without enableunifiedinterfaceshellrefresh
            var hasPinnedSitemapEntity = driver.HasElement(NavigationElementsLocator.PinnedSitemapEntity);
            if (!hasPinnedSitemapEntity)
            {
                // Close SiteMap launcher since it is open
                var xpathToLauncherCloseButton = NavigationElementsLocator.SiteMapLauncherCloseButton;
                driver.ClickWhenAvailable(xpathToLauncherCloseButton);

                driver.ClickWhenAvailable(NavigationElementsLocator.SiteMapLauncherButton);

                var menuContainer = driver.WaitUntilAvailable(NavigationElementsLocator.SubAreaContainer);

                var subItems = menuContainer.FindElements(By.TagName("li"));

                foreach (var subItem in subItems)
                {
                    // Check 'Id' attribute, NULL value == Group Header
                    var id = subItem.GetAttribute("id");
                    if (string.IsNullOrEmpty(id))
                        continue;

                    // Filter out duplicate entity keys - click the first one in the list
                    var key = subItem.Text.ToLowerString();
                    if (!dictionary.ContainsKey(key))
                        dictionary.Add(key, subItem);
                }

                return dictionary;
            }

            //Sitemap with enableunifiedinterfaceshellrefresh enabled
            var menuShell = driver.FindElements(NavigationElementsLocator.SubAreaContainer);

            //The menu is broke into multiple sections. Gather all items.
            foreach (IWebElement menuSection in menuShell)
            {
                var menuItems = menuSection.FindElements(NavigationElementsLocator.SitemapMenuItems);

                foreach (var menuItem in menuItems)
                {
                    var text = menuItem.Text.ToLowerString();
                    if (string.IsNullOrEmpty(text))
                        continue;

                    if (!dictionary.ContainsKey(text))
                        dictionary.Add(text, menuItem);
                }
            }

            return dictionary;
        }

        public BrowserCommandResult<bool> GoBack()
        {
            return Client.Execute(Client.GetOptions("Go Back"), driver =>
            {
                driver.WaitForTransaction();

                var element = driver.ClickWhenAvailable(By.XPath(Elements.Xpath[Reference.Navigation.GoBack]));

                driver.WaitForTransaction();
                return element != null;
            });
        }

        public BrowserCommandResult<bool> OpenArea(string subarea)
        {
            return Client.Execute(Client.GetOptions("Open Unified Interface Area"), driver =>
            {
                var success = TryOpenArea(subarea);
                WaitForLoadArea(driver);
                return success;
            });
        }

        public Dictionary<string, IWebElement> OpenAreas(string area, int thinkTime = Constants.DefaultThinkTime)
        {
            return Client.Execute(Client.GetOptions("Open Unified Interface Area"), driver =>
            {
                //  9.1 ?? 9.0.2 <- inverted order (fallback first) run quickly
                var areas = OpenMenuFallback(area) ?? OpenMenu();

                if (!areas.ContainsKey(area))
                    throw new InvalidOperationException($"No area with the name '{area}' exists.");

                return areas;
            });
        }

        /// <summary>
        /// Opens the Guided Help
        /// </summary>
        /// <param name="thinkTime">Used to simulate a wait time between human interactions. The Default is 2 seconds.</param>
        /// <example>xrmBrowser.Navigation.OpenGuidedHelp();</example>
        public BrowserCommandResult<bool> OpenGuidedHelp(int thinkTime = Constants.DefaultThinkTime)
        {
            Client.ThinkTime(thinkTime);

            return Client.Execute(Client.GetOptions($"Open Guided Help"), driver =>
            {
                driver.ClickWhenAvailable(NavigationElementsLocator.GuidedHelp);

                return true;
            });
        }

        public Dictionary<string, IWebElement> OpenMenu(int thinkTime = Constants.DefaultThinkTime)
        {
            return Client.Execute(Client.GetOptions("Open Menu"), driver =>
            {
                driver.ClickWhenAvailable(NavigationElementsLocator.AreaButton);

                var result = GetMenuItemsFrom(driver, AppReference.Navigation.AreaMenu);
                return result;
            });
        }

        public Dictionary<string, IWebElement> OpenMenuFallback(string area, int thinkTime = Constants.DefaultThinkTime)
        {
            return Client.Execute(Client.GetOptions("Open Menu"), driver =>
            {
                //Make sure the sitemap-launcher is expanded - 9.1
                var xpathSiteMapLauncherButton = NavigationElementsLocator.SiteMapLauncherButton;
                bool success = driver.TryFindElement(xpathSiteMapLauncherButton, out IWebElement launcherButton);
                if (success)
                {
                    bool expanded = bool.Parse(launcherButton.GetAttribute("aria-expanded"));
                    if (!expanded)
                        driver.ClickWhenAvailable(xpathSiteMapLauncherButton);
                }

                var dictionary = new Dictionary<string, IWebElement>();

                //Is this the sitemap with enableunifiedinterfaceshellrefresh?
                var xpathSitemapSwitcherButton = NavigationElementsLocator.SitemapSwitcherButton;
                success = driver.TryFindElement(xpathSitemapSwitcherButton, out IWebElement switcherButton);
                if (success)
                {
                    switcherButton.Click(true);
                    driver.WaitForTransaction();

                    AddMenuItemsFrom(driver, AppReference.Navigation.SitemapSwitcherFlyout, dictionary);
                }

                var xpathSiteMapAreaMoreButton = NavigationElementsLocator.SiteMapAreaMoreButton;
                success = driver.TryFindElement(xpathSiteMapAreaMoreButton, out IWebElement moreButton);
                if (!success)
                    return dictionary;

                bool isVisible = moreButton.IsVisible();
                if (isVisible)
                {
                    moreButton.Click();
                    AddMenuItemsFrom(driver, AppReference.Navigation.AreaMoreMenu, dictionary);
                }
                else
                {
                    var singleItem = driver.FindElement(By.XPath(AppElements.Xpath[AppReference.Navigation.SiteMapSingleArea].Replace("[NAME]", area)));
                    dictionary.Add(singleItem.Text.ToLowerString(), singleItem);
                }

                return dictionary;
            });
        }

        public BrowserCommandResult<bool> OpenSubArea(string subarea)
        {
            return Client.Execute(Client.GetOptions("Open Unified Interface Sub-Area"), driver =>
            {
                var success = TryOpenSubArea(driver, subarea);
                WaitForLoadArea(driver);
                return success;
            });
        }

        internal static void AddMenuItems(IWebElement menu, Dictionary<string, IWebElement> dictionary)
        {
            var menuItems = menu.FindElements(By.TagName("li"));
            foreach (var item in menuItems)
            {
                string key = item.Text.ToLowerString();
                if (dictionary.ContainsKey(key))
                    continue;
                dictionary.Add(key, item);
            }
        }

        internal BrowserCommandResult<bool> ClickQuickLaunchButton(string toolTip, int thinkTime = Constants.DefaultThinkTime)
        {
            Client.ThinkTime(thinkTime);

            return Client.Execute(Client.GetOptions($"Quick Launch: {toolTip}"), driver =>
            {
                driver.WaitUntilClickable(NavigationElementsLocator.QuickLaunchMenu);

                //Text could be in the crumb bar.  Find the Quick launch bar buttons and click that one.
                var buttons = driver.FindElement(NavigationElementsLocator.QuickLaunchMenu);
                var launchButton = buttons.FindElement(By.XPath(AppElements.Xpath[AppReference.Navigation.QuickLaunchButton].Replace("[NAME]", toolTip)));
                launchButton.Click();

                return true;
            });
        }

        /// <summary>
        /// Opens the Admin Portal
        /// </summary>
        /// <param name="thinkTime">Used to simulate a wait time between human interactions. The Default is 2 seconds.</param>
        /// <example>xrmBrowser.Navigation.OpenAdminPortal();</example>
        internal BrowserCommandResult<bool> OpenAdminPortal(int thinkTime = Constants.DefaultThinkTime)
        {
            Client.ThinkTime(thinkTime);
            return Client.Execute(Client.GetOptions("Open Admin Portal"), driver =>
            {
                driver.WaitUntilVisible(By.XPath(AppElements.Xpath[AppReference.Application.Shell]));
                driver.FindElement(NavigationElementsLocator.AdminPortal)?.Click();
                driver.FindElement(NavigationElementsLocator.AdminPortalButton)?.Click();
                return true;
            });
        }

        internal BrowserCommandResult<bool> OpenApp(string appName, int thinkTime = Constants.DefaultThinkTime)
        {
            Client.ThinkTime(thinkTime);

            return Client.Execute(Client.GetOptions($"Open App {appName}"), driver =>
            {
                driver.WaitForPageToLoad();
                driver.SwitchTo().DefaultContent();

                var query = Client.GetUrlQueryParams(driver.Url);
                bool isSomeAppOpen = query.Get("appid") != null || query.Get("app") != null;

                bool success = false;
                if (!isSomeAppOpen)
                    success = TryToClickInAppTile(appName, driver);
                else
                    success = TryOpenAppFromMenu(driver, appName, AppReference.Navigation.UCIAppMenuButton) ||
                              TryOpenAppFromMenu(driver, appName, AppReference.Navigation.WebAppMenuButton);

                if (!success)
                    throw new InvalidOperationException($"App Name {appName} not found.");

                Client.InitializeModes();

                // Wait for app page elements to be visible (shell and sitemapLauncherButton)
                var shell = driver.WaitUntilVisible(By.XPath(AppElements.Xpath[AppReference.Application.Shell]));
                var sitemapLauncherButton = driver.WaitUntilVisible(NavigationElementsLocator.SiteMapLauncherButton);

                success = shell != null && sitemapLauncherButton != null;

                if (!success)
                    throw new InvalidOperationException($"App '{appName}' was found but app page was not loaded.");

                return true;
            });
        }

        internal bool OpenAppFromMenu(IWebDriver driver, string appName)
        {
            var container = driver.WaitUntilAvailable(NavigationElementsLocator.AppMenuContainer);
            var xpathToButton = "//nav[@aria-hidden='false']//button//*[text()='[TEXT]']".Replace("[TEXT]", appName);
            var button = container.ClickWhenAvailable(By.XPath(xpathToButton), TimeSpan.FromSeconds(1));

            var success = button != null;

            if (!success)
                Trace.TraceWarning($"App Name '{appName}' not found.");

            return success;
        }

        /// <summary>
        /// Open Global Search
        /// </summary>
        /// <param name="thinkTime">Used to simulate a wait time between human interactions. The Default is 2 seconds.</param>
        /// <example>xrmBrowser.Navigation.OpenGlobalSearch();</example>
        internal BrowserCommandResult<bool> OpenGlobalSearch(int thinkTime = Constants.DefaultThinkTime)
        {
            Client.ThinkTime(thinkTime);

            return Client.Execute(Client.GetOptions($"Open Global Search"), driver =>
            {
                driver.WaitForTransaction();

                if (driver.HasElement(By.Id("GlobalSearchBox")))
                {
                    return true;
                }

                driver.ClickWhenAvailable(
                    NavigationElementsLocator.SearchButton,
                    5.Seconds(),
                    "The Global Search button is not available.");

                driver.WaitForTransaction();

                return true;
            });
        }

        internal BrowserCommandResult<bool> OpenGroupSubArea(string group, string subarea, int thinkTime = Constants.DefaultThinkTime)
        {
            Client.ThinkTime(thinkTime);

            return Client.Execute(Client.GetOptions("Open Group Sub Area"), driver =>
            {
                //Make sure the sitemap-launcher is expanded - 9.1
                if (driver.HasElement(NavigationElementsLocator.SiteMapLauncherButton))
                {
                    var expanded = bool.Parse(driver.FindElement(NavigationElementsLocator.SiteMapLauncherButton).GetAttribute("aria-expanded"));

                    if (!expanded)
                        driver.ClickWhenAvailable(NavigationElementsLocator.SiteMapLauncherButton);
                }

                var groups = driver.FindElements(NavigationElementsLocator.SitemapMenuGroup);
                var groupList = groups.FirstOrDefault(g => g.GetAttribute("aria-label").ToLowerString() == group.ToLowerString());
                if (groupList == null)
                {
                    throw new NotFoundException($"No group with the name '{group}' exists");
                }

                var subAreaItems = groupList.FindElements(NavigationElementsLocator.SitemapMenuItems);
                var subAreaItem = subAreaItems.FirstOrDefault(a => a.GetAttribute("data-text").ToLowerString() == subarea.ToLowerString());
                if (subAreaItem == null)
                {
                    throw new NotFoundException($"No subarea with the name '{subarea}' exists inside of '{group}'");
                }

                subAreaItem.Click(true);

                WaitForLoadArea(driver);
                return true;
            });
        }

        internal BrowserCommandResult<bool> OpenSettingsOption(string command, string dataId, int thinkTime = Constants.DefaultThinkTime)
        {
            return Client.Execute(Client.GetOptions($"Open " + command + " " + dataId), driver =>
            {
                var xpathFlyout = By.XPath(AppElements.Xpath[AppReference.Navigation.SettingsLauncher].Replace("[NAME]", command));
                var xpathToFlyoutButton = By.XPath(AppElements.Xpath[AppReference.Navigation.SettingsLauncherBar].Replace("[NAME]", command));

                IWebElement flyout;
                bool success = driver.TryFindElement(xpathFlyout, out flyout);
                if (!success || !flyout.Displayed)
                {
                    driver.ClickWhenAvailable(xpathToFlyoutButton, $"No command button exists that match with: {command}.");
                    flyout = driver.WaitUntilVisible(xpathFlyout, "Flyout menu did not became visible");
                }

                var menuItems = flyout.FindElements(By.TagName("button"));
                var button = menuItems.FirstOrDefault(x => x.GetAttribute("data-id").Contains(dataId));
                if (button != null)
                {
                    button.Click();
                    return true;
                }

                throw new InvalidOperationException($"No command with data-id: {dataId} exists inside of the command menu {command}");
            });
        }

        // Xrm.Navigation.openForm opens the form within the app, as a user does. EntityManager.OpenEntity loads the app
        // again from the record's URL instead, which takes longer and drops what the page holds (e.g. the test driver).
        internal BrowserCommandResult<bool> OpenRecord(string entityName, Guid recordId, TimeSpan? timeout = null)
        {
            return Client.Execute(Client.GetNoRetryOptions($"Open Record: {entityName} {recordId}"), driver =>
            {
                var javaScript = (IJavaScriptExecutor)driver;
                object error;
                try
                {
                    // Xrm.Navigation.openForm settles once the app has navigated to the form.
                    error = javaScript.ExecuteAsyncScript(
                        @"const [entityName, entityId, done] = arguments;
                          Xrm.Navigation.openForm({ entityName, entityId })
                              .then(() => done(null), error => done((error && error.message) || String(error)));",
                        entityName,
                        recordId.ToString());
                }
                catch (WebDriverTimeoutException exception)
                {
                    throw new WebDriverTimeoutException(
                        $"Opening the {entityName} record {recordId} did not finish: the app did not navigate to its form, "
                        + "e.g. because the open form asks whether to save its unsaved changes.",
                        exception);
                }

                if (error != null)
                    throw new WebDriverException($"Opening the {entityName} record {recordId} failed: {error}");

                driver.WaitForTransaction();

                // The form can still be loading the record when openForm settles.
                new WebDriverWait(driver, timeout ?? Constants.DefaultTimeout) { Message = $"The form did not show the {entityName} record {recordId}." }
                    .Until(_ => (bool)javaScript.ExecuteScript(
                        @"const entity = window.Xrm && Xrm.Page && Xrm.Page.data && Xrm.Page.data.entity;
                          return !!entity && entity.getId().replace(/[{}]/g, '').toLowerCase() === arguments[0];",
                        recordId.ToString()));
                return true;
            });
        }

        internal BrowserCommandResult<bool> OpenSubArea(string area, string subarea, int thinkTime = Constants.DefaultThinkTime)
        {
            return Client.Execute(Client.GetOptions("Open Sub Area"), driver =>
            {
                //If the subarea is already in the left hand nav, click it
                var success = TryOpenSubArea(driver, subarea);
                if (!success)
                {
                    success = TryOpenArea(area);
                    if (!success)
                        throw new InvalidOperationException($"Area with the name '{area}' not found. ");

                    success = TryOpenSubArea(driver, subarea);
                    if (!success)
                        throw new InvalidOperationException($"No subarea with the name '{subarea}' exists inside of '{area}'.");
                }

                WaitForLoadArea(driver);
                return true;
            });
        }

        internal BrowserCommandResult<bool> SelleQuickCreate(string entityName, int thinkTime = Constants.DefaultThinkTime)
        {
            Client.ThinkTime(thinkTime);

            return Client.Execute(Client.GetOptions($"Quick Create: {entityName}"), driver =>
            {
                //Click the + button in the ribbon
                var quickCreateButton = driver.FindElement(NavigationElementsLocator.QuickCreateButton);
                quickCreateButton.Click(true);

                //Find the entity name in the list
                var entityMenuList = driver.FindElement(NavigationElementsLocator.QuickCreateMenuList);
                var entityMenuItems = entityMenuList.FindElements(NavigationElementsLocator.QuickCreateMenuItems);
                var entitybutton = entityMenuItems.FirstOrDefault(e => e.Text.Contains(entityName, StringComparison.OrdinalIgnoreCase));

                if (entitybutton == null)
                    throw new Exception(String.Format("{0} not found in Quick Create list.", entityName));

                //Click the entity name
                entitybutton.Click(true);

                driver.WaitForTransaction();

                return true;
            });
        }

        internal BrowserCommandResult<bool> SignOut()
        {
            return Client.Execute(Client.GetOptions("Sign out"), driver =>
            {
                driver.WaitUntilClickable(NavigationElementsLocator.AccountManagerButton).Click();
                driver.WaitUntilClickable(NavigationElementsLocator.AccountManagerSignOutButton).Click();

                return driver.WaitForPageToLoad();
            });
        }
        private static void AddMenuItemsFrom(IWebDriver driver, string referenceToMenuItemsContainer, Dictionary<string, IWebElement> dictionary)
        {
            driver.WaitUntilAvailable(By.XPath(AppElements.Xpath[referenceToMenuItemsContainer]),
                TimeSpan.FromSeconds(2),
                menu => AddMenuItems(menu, dictionary),
                "The Main Menu is not available."
            );
        }

        private static Dictionary<string, IWebElement> GetMenuItemsFrom(IWebDriver driver, string referenceToMenuItemsContainer)
        {
            var result = new Dictionary<string, IWebElement>();
            AddMenuItemsFrom(driver, referenceToMenuItemsContainer, result);
            return result;
        }

        private static bool TryToClickInAppTile(string appName, IWebDriver driver)
        {
            string message = "Frame AppLandingPage is not loaded.";
            driver.WaitUntil(
                d =>
                {
                    try
                    {
                        driver.SwitchTo().Frame("AppLandingPage");
                    }
                    catch (NoSuchFrameException ex)
                    {
                        message = $"{message} Exception: {ex.Message}";
                        Trace.TraceWarning(message);
                        return false;
                    }
                    return true;
                },
                5.Seconds()
                );

            var xpathToAppContainer = NavigationElementsLocator.UCIAppContainer;
            var xpathToappTile = By.XPath(AppElements.Xpath[AppReference.Navigation.UCIAppTile].Replace("[NAME]", appName));

            bool success = false;
            driver.WaitUntilVisible(xpathToAppContainer, TimeSpan.FromSeconds(5),
                appContainer => success = appContainer.ClickWhenAvailable(xpathToappTile, TimeSpan.FromSeconds(5)) != null
                );

            if (!success)
                Trace.TraceWarning(message);

            return success;
        }

        private static void WaitForLoadArea(IWebDriver driver)
        {
            driver.WaitForPageToLoad();
            driver.WaitForTransaction();
        }

        private bool TryOpenAppFromMenu(IWebDriver driver, string appName, string appMenuButton)
        {
            bool found = false;
            var xpathToAppMenu = By.XPath(AppElements.Xpath[appMenuButton]);
            driver.WaitUntilClickable(xpathToAppMenu, 5.Seconds(),
                        appMenu =>
                        {
                            appMenu.Click(true);
                            found = TryToClickInAppTile(appName, driver) || OpenAppFromMenu(driver, appName);
                        });
            return found;
        }
        private bool TryOpenArea(string area)
        {
            area = area.ToLowerString();
            var areas = OpenAreas(area);

            IWebElement menuItem;
            bool found = areas.TryGetValue(area, out menuItem);
            if (found)
            {
                var strSelected = menuItem.GetAttribute("aria-checked");
                bool selected;
                bool.TryParse(strSelected, out selected);
                if (!selected)
                    menuItem.Click(true);
            }
            return found;
        }

        private bool TryOpenSubArea(IWebDriver driver, string subarea)
        {
            subarea = subarea.ToLowerString();
            var navSubAreas = GetSubAreaMenuItems(driver);

            var found = navSubAreas.TryGetValue(subarea, out var element);
            if (found)
            {
                var strSelected = element.GetAttribute("aria-selected");
                bool.TryParse(strSelected, out var selected);
                if (!selected)
                {
                    element.Click(true);
                }
                else
                {
                    // This will result in navigating back to the desired subArea -- even if already selected.
                    // Example: If context is an Account entity record, then a call to OpenSubArea("Sales", "Accounts"),
                    // this will click on the Accounts subArea and go back to the grid view
                    element.Click(true);
                }
            }
            return found;
        }

        internal void NavigateToSubArea(string groupName, string subAreaName)
        {
            Client.Execute<object>(Client.GetOptions("Navigate To SubArea"), driver =>
            {
                // Expand the sitemap launcher if it exists and is not already expanded.
                var launcherLocator = By.XPath(AppElements.Xpath[AppReference.Navigation.SiteMapLauncherButton]);
                if (driver.HasElement(launcherLocator))
                {
                    var launcherButton = driver.FindElement(launcherLocator);
                    bool expanded = bool.Parse(launcherButton.GetAttribute("aria-expanded"));
                    if (!expanded)
                    {
                        driver.ClickWhenAvailable(launcherLocator);
                    }
                }

                // Retrieve all sitemap groups.
                var groups = driver.FindElements(By.XPath(AppElements.Xpath[AppReference.Navigation.SitemapMenuGroup]));
                var groupList = groups.FirstOrDefault(g => GetGroupName(driver, g).ToLowerString() == groupName.ToLowerString());
                if (groupList == null)
                {
                    throw new NotFoundException($"No group with the name '{groupName}' exists");
                }

                // Retrieve the subarea items within the group.
                var subAreaItems = groupList.FindElements(By.XPath("." + AppElements.Xpath[AppReference.Navigation.SitemapMenuItems]));
                var subAreaItem = subAreaItems.FirstOrDefault(a => a.GetAttribute("data-text").ToLowerString() == subAreaName.ToLowerString());
                if (subAreaItem == null)
                {
                    throw new NotFoundException($"No subarea with the name '{subAreaName}' exists inside of '{groupName}'");
                }

                // Click the subarea item and wait for the page and any transactions to load.
                subAreaItem.Click(true);
                driver.WaitForPageToLoad();
                driver.WaitForTransaction();

                return null;
            });
        }

        internal string GetAreaText()
        {
            return Client.Execute<string>(Client.GetOptions("Get Area Text"), driver =>
            {
                return driver
                    .WaitUntilAvailable(By.Id("areaSwitcherContainer"))
                    .Text;
            });
        }

        internal IWebElement GetSubAreaElement(string subArea)
        {
            return Client.Execute<IWebElement>(Client.GetOptions("Get SubArea Element"), driver =>
            {
                return driver.WaitUntilAvailable(By.XPath($"//img[@title='{subArea}']"));
            }).Value;
        }

        internal bool IsSubAreaVisible(string subArea)
        {
            return Client.Execute<bool>(Client.GetNoRetryOptions("Is SubArea Visible"), driver =>
                driver.FindElements(By.XPath(AppElements.Xpath[AppReference.Navigation.SubAreaImage].Replace("[LABEL]", subArea.ToXPathLiteral()))).Count > 0);
        }

        internal string GetGroupText(string groupName)
        {
            return Client.Execute<string>(Client.GetOptions("Get Group Text"), driver =>
            {
                var groupNameWithoutWhitespace = groupName?.Replace(" ", string.Empty);
                // An h3 in older versions of the unified interface, a span in newer ones.
                var locator = By.XPath($"//*[@data-id='sitemap-sitemapAreaGroup-{groupNameWithoutWhitespace}']");

                return driver.WaitUntilAvailable(locator).Text;
            });
        }

        // A sitemap group is named by its aria-label in older versions of the unified interface, and by the element its
        // aria-labelledby refers to in newer ones.
        private static string GetGroupName(IWebDriver driver, IWebElement group)
        {
            var label = group.GetAttribute("aria-label");
            if (!string.IsNullOrEmpty(label))
                return label;

            var labelledBy = group.GetAttribute("aria-labelledby");
            return string.IsNullOrEmpty(labelledBy)
                ? null
                : driver.FindElements(By.Id(labelledBy)).FirstOrDefault()?.GetAttribute("textContent")?.Trim();
        }
    }
}
