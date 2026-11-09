using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;

namespace TALXIS.TestKit.Selectors.DTO.Locators
{
    internal class NavigationElementsLocator
    {
        internal static By AreaButton => By.XPath(AppElements.Xpath[AppReference.Navigation.AreaButton]);
        internal static By NewLookToggle => By.XPath(AppElements.Xpath[AppReference.Navigation.NewLookToggle]);
        internal static By AreaMenu => By.XPath(AppElements.Xpath[AppReference.Navigation.AreaMenu]);
        internal static By AreaMoreMenu => By.XPath(AppElements.Xpath[AppReference.Navigation.AreaMoreMenu]);
        internal static By SubAreaContainer => By.XPath(AppElements.Xpath[AppReference.Navigation.SubAreaContainer]);
        internal static By WebAppMenuButton => By.XPath(AppElements.Xpath[AppReference.Navigation.WebAppMenuButton]);
        internal static By UCIAppMenuButton => By.XPath(AppElements.Xpath[AppReference.Navigation.UCIAppMenuButton]);
        internal static By SiteMapLauncherButton => By.XPath(AppElements.Xpath[AppReference.Navigation.SiteMapLauncherButton]);
        internal static By SiteMapLauncherCloseButton => By.XPath(AppElements.Xpath[AppReference.Navigation.SiteMapLauncherCloseButton]);
        internal static By SiteMapAreaMoreButton => By.XPath(AppElements.Xpath[AppReference.Navigation.SiteMapAreaMoreButton]);
        internal static By SiteMapSingleArea(string name) => By.XPath(AppElements.Xpath[AppReference.Navigation.SiteMapSingleArea].Replace("[NAME]", name));
        internal static By AppMenuContainer => By.XPath(AppElements.Xpath[AppReference.Navigation.AppMenuContainer]);
        internal static By SettingsLauncherBar(string name) => By.XPath(AppElements.Xpath[AppReference.Navigation.SettingsLauncherBar].Replace("[NAME]", name));
        internal static By SettingsLauncher(string name) => By.XPath(AppElements.Xpath[AppReference.Navigation.SettingsLauncher].Replace("[NAME]", name));
        internal static By AccountManagerButton => By.XPath(AppElements.Xpath[AppReference.Navigation.AccountManagerButton]);
        internal static By AccountManagerSignOutButton => By.XPath(AppElements.Xpath[AppReference.Navigation.AccountManagerSignOutButton]);
        internal static By GuidedHelp => By.XPath(AppElements.Xpath[AppReference.Navigation.GuidedHelp]);
        internal static By AdminPortal => By.XPath(AppElements.Xpath[AppReference.Navigation.AdminPortal]);
        internal static By AdminPortalButton => By.XPath(AppElements.Xpath[AppReference.Navigation.AdminPortalButton]);
        internal static By SearchButton => By.XPath(AppElements.Xpath[AppReference.Navigation.SearchButton]);
        internal static By Search => By.XPath(AppElements.Xpath[AppReference.Navigation.Search]);
        internal static By QuickLaunchMenu => By.XPath(AppElements.Xpath[AppReference.Navigation.QuickLaunchMenu]);
        internal static By QuickLaunchButton(string name) => By.XPath(AppElements.Xpath[AppReference.Navigation.QuickLaunchButton].Replace("[NAME]", name));
        internal static By QuickCreateButton => By.XPath(AppElements.Xpath[AppReference.Navigation.QuickCreateButton]);
        internal static By QuickCreateMenuList => By.XPath(AppElements.Xpath[AppReference.Navigation.QuickCreateMenuList]);
        internal static By QuickCreateMenuItems => By.XPath(AppElements.Xpath[AppReference.Navigation.QuickCreateMenuItems]);
        internal static By PinnedSitemapEntity => By.XPath(AppElements.Xpath[AppReference.Navigation.PinnedSitemapEntity]);
        internal static By SitemapMenuGroup => By.XPath(AppElements.Xpath[AppReference.Navigation.SitemapMenuGroup]);
        internal static By SitemapMenuItems => By.XPath(AppElements.Xpath[AppReference.Navigation.SitemapMenuItems]);
        internal static By SitemapSwitcherButton => By.XPath(AppElements.Xpath[AppReference.Navigation.SitemapSwitcherButton]);
        internal static By SitemapSwitcherFlyout => By.XPath(AppElements.Xpath[AppReference.Navigation.SitemapSwitcherFlyout]);
        internal static By UCIAppContainer => By.XPath(AppElements.Xpath[AppReference.Navigation.UCIAppContainer]);
        internal static By UCIAppTile(string name) => By.XPath(AppElements.Xpath[AppReference.Navigation.UCIAppTile].Replace("[NAME]", name));
    }
}
