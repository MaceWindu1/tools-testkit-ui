namespace TALXIS.TestKit.Bindings.Steps
{
    using System;
    using FluentAssertions;
    using Reqnroll;
    using TALXIS.TestKit.Bindings.Extensions;

    /// <summary>
    /// Step bindings for the navigation (sitemap) of a TALXIS Portal, whose items scripts can hide or disable with
    /// TALXIS.Portal.App.sitemap.getItem(name).setVisible(...) / setDisabled(disabled, message). See
    /// <see cref="PortalPageExtensions"/> for how the portal renders them.
    /// </summary>
    [Binding]
    public class PortalSiteMapSteps : PowerAppsStepDefiner
    {
        private static readonly TimeSpan NavigationTimeout = TimeSpan.FromSeconds(10);
        private static readonly TimeSpan PageLoadTimeout = TimeSpan.FromSeconds(60);

        /// <summary>
        /// Asserts that a navigation item is shown but disabled, with the given message as its tooltip.
        /// </summary>
        /// <param name="subareaLabel">The item's label.</param>
        /// <param name="expectedMessage">The expected message.</param>
        [Then(@"the '(.*)' subarea should be disabled with the message '(.*)'")]
        public static void ThenTheSubareaShouldBeDisabledWithTheMessage(string subareaLabel, string expectedMessage)
        {
            Driver.WaitForPortalCondition(
                () => Driver.FindPortalSiteMapItem(subareaLabel) is { } item && item.IsPortalElementDisabled() && item.GetAttribute("title") == expectedMessage,
                NavigationTimeout);

            var item = Driver.FindPortalSiteMapItem(subareaLabel);
            item.Should().NotBeNull(because: $"the '{subareaLabel}' subarea should be shown");
            item.IsPortalElementDisabled().Should().BeTrue(because: $"the '{subareaLabel}' subarea should be disabled");
            item.GetAttribute("title").Should().Be(expectedMessage);
        }

        /// <summary>
        /// Asserts that a navigation item is not shown.
        /// </summary>
        /// <param name="subareaLabel">The item's label.</param>
        [Then(@"I should not see the '(.*)' subarea")]
        public static void ThenIShouldNotSeeTheSubarea(string subareaLabel)
        {
            Driver.BecomesAbsent(() => Driver.FindPortalSiteMapItem(subareaLabel) != null, NavigationTimeout)
                .Should().BeTrue(because: $"the '{subareaLabel}' subarea should not be shown");
        }

        /// <summary>
        /// Opens a page by clicking its navigation item.
        /// </summary>
        /// <param name="subareaLabel">The item's label.</param>
        [When(@"I open the '(.*)' page from the navigation")]
        public static void WhenIOpenThePageFromTheNavigation(string subareaLabel)
        {
            Driver.WaitForPortalCondition(() => Driver.FindPortalSiteMapItem(subareaLabel) is { } item && !item.IsPortalElementDisabled(), NavigationTimeout)
                .Should().BeTrue(because: $"the '{subareaLabel}' subarea should be shown and enabled");

            Driver.ClickWhenUnblocked(() => Driver.FindPortalSiteMapItem(subareaLabel), NavigationTimeout, $"The '{subareaLabel}' subarea");
            Driver.WaitForPortalReady(PageLoadTimeout, $"The '{subareaLabel}' page did not finish loading.");
        }
    }
}
