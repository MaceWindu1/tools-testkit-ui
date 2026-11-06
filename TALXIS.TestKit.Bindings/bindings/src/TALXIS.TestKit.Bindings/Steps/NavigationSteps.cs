namespace TALXIS.TestKit.Bindings.Steps
{
    using FluentAssertions;
    using Reqnroll;

    /// <summary>
    /// Step bindings relating to navigation.
    /// </summary>
    [Binding]
    public class NavigationSteps : PowerAppsStepDefiner
    {
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
            App.Navigation.IsSubAreaShown(subArea, shown: true).Should().BeTrue(because: $"the '{subArea}' subarea should be shown");
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
            App.Navigation.OpenPage(subareaLabel);
        }

        /// <summary>
        /// Asserts that a navigation item is not shown.
        /// </summary>
        /// <param name="subareaLabel">The item's label.</param>
        [Then(@"I should not see the '(.*)' subarea")]
        public static void ThenIShouldNotSeeTheSubarea(string subareaLabel)
        {
            App.Navigation.IsSubAreaShown(subareaLabel, shown: false).Should().BeTrue(because: $"the '{subareaLabel}' subarea should not be shown");
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
            App.Navigation.IsSubAreaDisabled(subareaLabel, expectedMessage)
                .Should().BeTrue(because: $"the '{subareaLabel}' subarea should be shown disabled with the message '{expectedMessage}'");
        }
    }
}
