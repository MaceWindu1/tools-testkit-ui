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
        private static readonly TimeSpan PortalPageLoadTimeout = TimeSpan.FromSeconds(60);

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
            if (TargetsPortal)
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

        /// <summary>
        /// Opens a page by clicking its navigation item (a subarea of the current area in a model-driven app).
        /// </summary>
        /// <param name="subareaLabel">The item's label.</param>
        [When(@"I open the '(.*)' page from the navigation")]
        public static void WhenIOpenThePageFromTheNavigation(string subareaLabel)
        {
            if (!TargetsPortal)
            {
                XrmApp.Navigation.OpenSubArea(subareaLabel);
                return;
            }

            Driver.WaitForPortalCondition(() => Driver.FindPortalSiteMapItem(subareaLabel) is { } item && !item.IsPortalElementDisabled(), PortalNavigationTimeout)
                .Should().BeTrue(because: $"the '{subareaLabel}' subarea should be shown and enabled");

            Driver.ClickWhenUnblocked(() => Driver.FindPortalSiteMapItem(subareaLabel), PortalNavigationTimeout, $"The '{subareaLabel}' subarea");
            Driver.WaitForPortalReady(PortalPageLoadTimeout, $"The '{subareaLabel}' page did not finish loading.");
        }

        /// <summary>
        /// Asserts that a navigation item is not shown.
        /// </summary>
        /// <param name="subareaLabel">The item's label.</param>
        [Then(@"I should not see the '(.*)' subarea")]
        public static void ThenIShouldNotSeeTheSubarea(string subareaLabel)
        {
            var shown = TargetsPortal
                ? !Driver.BecomesAbsent(() => Driver.FindPortalSiteMapItem(subareaLabel) != null, PortalNavigationTimeout)
                : Driver.FindElements(By.XPath($"//img[@title={PortalFormExtensions.XPathLiteral(subareaLabel)}]")).Count > 0;

            shown.Should().BeFalse(because: $"the '{subareaLabel}' subarea should not be shown");
        }

        /// <summary>
        /// Asserts that a navigation item is shown but disabled, with the given message as its tooltip. Only a TALXIS Portal
        /// can disable navigation items (TALXIS.Portal.App.sitemap.getItem(name).setDisabled(disabled, message)).
        /// </summary>
        /// <param name="subareaLabel">The item's label.</param>
        /// <param name="expectedMessage">The expected message.</param>
        [Then(@"the '(.*)' subarea should be disabled with the message '(.*)'")]
        public static void ThenTheSubareaShouldBeDisabledWithTheMessage(string subareaLabel, string expectedMessage)
        {
            if (!TargetsPortal)
            {
                throw new NotSupportedException("Only a TALXIS Portal can disable a navigation item.");
            }

            Driver.WaitForPortalCondition(
                () => Driver.FindPortalSiteMapItem(subareaLabel) is { } item && item.IsPortalElementDisabled() && item.GetAttribute("title") == expectedMessage,
                PortalNavigationTimeout);

            var item = Driver.FindPortalSiteMapItem(subareaLabel);
            item.Should().NotBeNull(because: $"the '{subareaLabel}' subarea should be shown");
            item.IsPortalElementDisabled().Should().BeTrue(because: $"the '{subareaLabel}' subarea should be disabled");
            item.GetAttribute("title").Should().Be(expectedMessage);
        }
    }
}
