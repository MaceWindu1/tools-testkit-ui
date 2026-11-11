// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
using System;
using OpenQA.Selenium;
using TALXIS.TestKit.Selectors.WebClientManagement;
using TALXIS.TestKit.Selectors.Browser;

namespace TALXIS.TestKit.Selectors
{
    public class Navigation : Element
    {
        private readonly NavigationManager _navigationManager;
        private readonly TimelineManager _timelineManager;


        public Navigation(WebClient client) : base()
        {
            _navigationManager = new NavigationManager(client);
            _timelineManager = new TimelineManager(client);
        }

        /// <summary>
        /// Opens a record in its form (Xrm.Navigation.openForm) and waits for the form.
        /// </summary>
        /// <param name="entityName">The logical name of the record's table.</param>
        /// <param name="recordId">The record's id.</param>
        public void OpenRecord(string entityName, Guid recordId)
        {
            _navigationManager.OpenRecord(entityName, recordId);
        }

        /// <summary>
        /// Opens the App supplied
        /// </summary>
        /// <param name="appName">Name of the app to open</param>
        public void OpenApp(string appName)
        {
            _navigationManager.OpenApp(appName);
        }

        /// <summary>
        /// Opens a sub area from a group in the active app &amp; area
        /// This can be used to navigate within the active app/area or when the app only has a single area
        /// It will not navigate to a different app or area within the app
        /// </summary>
        /// <param name="group">Name of the group</param>
        /// <param name="subarea">Name of the subarea</param>
        /// <example>xrmApp.Navigation.OpenGroupSubArea("Customers", "Accounts");</example>
        public void OpenGroupSubArea(string group, string subarea)
        {
            _navigationManager.OpenGroupSubArea(group, subarea);
        }

        public void NavigateToSubArea(string groupName, string subAreaName)
        {
            _navigationManager.NavigateToSubArea(groupName, subAreaName);
        }


        public string GetAreaText()
        {
            return _navigationManager.GetAreaText();
        }

        public string GetGroupText(string groupName)
        {
            return _navigationManager.GetGroupText(groupName);
        }

        /// <summary>
        /// Gets a value indicating whether a subarea is shown in the sitemap, without waiting for it.
        /// </summary>
        /// <param name="subArea">The subarea's title.</param>
        public bool IsSubAreaVisible(string subArea)
        {
            return _navigationManager.IsSubAreaVisible(subArea);
        }

        public IWebElement GetSubAreaElement(string subArea)
        {
            return _navigationManager.GetSubAreaElement(subArea);
        }

        /// <summary>
        /// Opens a area in the unified client
        /// </summary>
        /// <param name="area">Name of the area</param>
        public void OpenArea(string area)
        {
            _navigationManager.OpenArea(area);
        }

        /// <summary>
        /// Opens a sub area in the unified client
        /// </summary>
        /// <param name="area">Name of the area</param>
        public void OpenSubArea(string area)
        {
            _navigationManager.OpenSubArea(area);
        }

        /// <summary>
        /// Opens a sub area in the unified client
        /// </summary>
        /// <param name="area">Name of the area</param>
        /// <param name="subarea">Name of the subarea</param>
        public void OpenSubArea(string area, string subarea)
        {
            _navigationManager.OpenSubArea(area, subarea);
        }

        /// <summary>
        /// Opens the Personalization Settings menu item in the Options menu (gear icon)
        /// </summary>
        public void OpenOptions()
        {
            _navigationManager.OpenSettingsOption("personalSettings", "SettingsMenu.PersonalSettings");
        }

        /// <summary>
        /// Opens the About menu item in the Options menu (gear icon)
        /// </summary>
        public void OpenAbout()
        {
            _navigationManager.OpenSettingsOption("personalSettings", "SettingsMenu.About");
        }

        /// <summary>
        /// Opens the Privacy and Cookies menu item in the Options menu (gear icon)
        /// </summary>
        public void OpenPrivacy()
        {
            _navigationManager.OpenSettingsOption("personalSettings", "SettingsMenu.PrivacyStatement");
        }

        /// <summary>
        /// Opens the Learning Path menu item in the Options menu (gear icon)
        /// </summary>
        public void OpenOptInForLearningPath()
        {
            _navigationManager.OpenSettingsOption("personalSettings", "SettingsMenu.LpOptIn-buttoncontainer");
        }

        /// <summary>
        /// Opens the Software license terms menu item in the Options menu (gear icon)
        /// </summary>
        public void OpenSoftwareLicensing()
        {
            _navigationManager.OpenSettingsOption("personalSettings", "SettingsMenu.SoftwareLicenseTerms");
        }

        /// <summary>
        /// Opens the Toast Notification Display Time terms menu item in the Options menu (gear icon)
        /// </summary>
        public void OpenToastNotifications()
        {
            _navigationManager.OpenSettingsOption("personalSettings", "SettingsMenu.ToastNotificationSettings");
        }

        /// <summary>
        /// Clicks the Sign Out button
        /// </summary>
        public void SignOut()
        {
            _navigationManager.SignOut();
        }

        /// <summary>
        /// Clicks the Help button
        /// </summary>
        public void OpenGuidedHelp()
        {
            _navigationManager.OpenGuidedHelp();
        }

        /// <summary>
        /// Opens the Admin Portal
        /// </summary>
        public void OpenPortalAdmin()
        {
            _navigationManager.OpenAdminPortal();
        }

        /// <summary>
        /// Clicks the Search button (magnifying glass icon)
        /// </summary>
        public void OpenGlobalSearch()
        {
            _navigationManager.OpenGlobalSearch();
        }

        /// <summary>
        /// This method will open and click on any Menu and Menuitem provided to it.
        /// </summary>
        /// <param name="menuName">This is the name of the menu reference in the XPath Dictionary in the References Class</param>
        /// <param name="menuItemName">This is the name of the menu item reference in the XPath Dictionary in the References Class</param>
        public void OpenMenu(string menuName, string menuItemName)
        {
            _timelineManager.OpenAndClickPopoutMenu(menuName, menuItemName);
        }

        /// <summary>
        /// Clicks the quick create button (+ icon)
        /// </summary>
        /// <param name="entityName"></param>
        public void QuickCreate(string entityName)
        {
            _navigationManager.SelleQuickCreate(entityName);
        }

        /// <summary>
        /// Opens the quick launch bar on the left hand side of the window
        /// </summary>
        /// <param name="toolTip">Tooltip to select</param>
        public void ClickQuickLaunchButton(string toolTip)
        {
            _navigationManager.ClickQuickLaunchButton(toolTip);
        }

        // <summary>
        /// Go back
        /// </summary>
        /// <example>xrmApp.Navigation.GoBack();</example>
        public void GoBack()
        {
            _navigationManager.GoBack();
        }
    }
}
