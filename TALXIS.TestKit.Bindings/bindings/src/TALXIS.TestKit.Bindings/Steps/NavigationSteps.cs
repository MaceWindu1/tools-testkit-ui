namespace TALXIS.TestKit.Bindings.Steps
{
    using System;
    using System.Linq;
    using FluentAssertions;
    using OpenQA.Selenium;
    using Reqnroll;
    using TALXIS.TestKit.Bindings.Extensions;
    using TALXIS.TestKit.Selectors;
    using TALXIS.TestKit.Selectors.Browser;

    /// <summary>
    /// Step bindings relating to navigation.
    /// </summary>
    [Binding]
    public class NavigationSteps : PowerAppsStepDefiner
    {
        private static readonly TimeSpan PortalNavigationTimeout = TimeSpan.FromSeconds(10);

        /// <summary>
        /// Opens a sub-area from the navigation bar.
        /// </summary>
        /// <param name="subAreaName">The name of the sub-area.</param>
        /// <param name="areaName">The name of the area.</param>
        [When("I open the sub area '(.*)' under the '(.*)' area")]
        public static void WhenIOpenTheSubAreaUnderTheArea(string subAreaName, string areaName)
        {
            XrmApp.Navigation.OpenSubArea(areaName, subAreaName);
        }

        /// <summary>
        /// Opens global search.
        /// </summary>
        [When("I open global search")]
        public static void WhenIOpenGlobalSearch()
        {
            XrmApp.Navigation.OpenGlobalSearch();
        }

        /// <summary>
        /// Opens an area.
        /// </summary>
        /// <param name="areaName">The name of the area.</param>
        [When("I open the '(.*)' area")]
        public static void WhenIOpenTheArea(string areaName)
        {
            XrmApp.Navigation.OpenArea(areaName);
        }

        /// <summary>
        /// Opens a sub area of a group.
        /// </summary>
        /// <param name="subAreaName">The name of the sub area.</param>
        /// <param name="groupName">The name of the group.</param>
        [When("I open the '(.*)' sub area of the '(.*)' group")]
        public static void WhenIOpenTheSubAreaOfTheGroup(string subAreaName, string groupName)
        {
            XrmApp.Navigation.NavigateToSubArea(groupName, subAreaName);
        }

        /// <summary>
        /// Opens a quick create for an entity.
        /// </summary>
        /// <param name="entity">The name of the entity.</param>
        [When("I open a quick create for the '(.*)' entity")]
        public static void WhenIOpenAQuickCreateForTheEntity(string entity)
        {
            XrmApp.Navigation.QuickCreate(entity);
        }

        /// <summary>
        /// Signs out.
        /// </summary>
        [When("I sign out")]
        public static void WhenISignOut()
        {
            XrmApp.Navigation.SignOut();
        }

        /// <summary>
        /// Gets the area.
        /// </summary>
        /// <param name="area">The area name.</param>
        [Then(@"I should see the '(.*)' area")]
        public static void ThenIShouldSeeTheArea(string area)
        {
            XrmApp.Navigation.GetAreaText().Should().Contain(area);
        }

        /// <summary>
        /// Gets the sub-area.
        /// </summary>
        /// <param name="subArea">The area name.</param>
        [Then(@"I should see the '(.*)' subarea")]
        public static void ThenIShouldSeeTheSubArea(string subArea)
        {
            if (Driver.IsTalxisPortal())
            {
                Driver.WaitForPortalCondition(() => Driver.FindPortalSiteMapItem(subArea) != null, PortalNavigationTimeout)
                    .Should().BeTrue(because: $"the '{subArea}' subarea should be shown");
                return;
            }

            var subAreaElement = XrmApp.Navigation.GetSubAreaElement(subArea);

            subAreaElement.Should().NotBeNull("the subarea image should be present");
            subAreaElement.Displayed.Should().BeTrue("the subarea image should be visible");
        }

        /// <summary>
        /// Gets the sub-area.
        /// </summary>
        /// <param name="groupName">The group.</param>
        [Then(@"I should see the '(.*)' group")]
        public static void ThenIShouldSeeTheGroup(string groupName)
        {
            var groupNameWithoutWhitespace = groupName?.Replace(" ", string.Empty);

            var actualText = XrmApp.Navigation.GetGroupText(groupName);
            actualText.Should().Contain(groupName, $"the group '{groupName}' should be displayed on the sitemap");
        }
    }
}
