namespace TALXIS.TestKit.Bindings.Steps
{
    using FluentAssertions;
    using OpenQA.Selenium;
    using Reqnroll;

    /// <summary>
    /// Asserts the disabled state (and tooltip) of a TALXIS Portal sitemap item — i.e. one set via
    /// the portal framework's TALXIS.Portal.App.sitemap.getItem(name).setDisabled(true, message) API.
    ///
    /// This is TALXIS Portal's own custom navigation chrome, not the standard Dynamics UCI sitemap
    /// that NavigationSteps (XrmApp.Navigation.*) targets, so this does a generic text-based lookup
    /// for the sitemap item and reads its title/aria-disabled attributes directly rather than relying
    /// on UCI-specific selectors. Verify the selector against a live portal session for your project —
    /// it was written against one portal's rendered markup and may need adjusting if another project's
    /// theme/sitemap customization changes the DOM shape.
    /// </summary>
    [Binding]
    public class PortalSiteMapSteps : PowerAppsStepDefiner
    {
        /// <summary>
        /// Asserts that a portal sitemap item is disabled with the given tooltip message.
        /// </summary>
        /// <param name="subareaLabel">The sitemap item's visible label.</param>
        /// <param name="expectedMessage">The expected tooltip/title text.</param>
        [Then(@"the '(.*)' subarea should be disabled with the message '(.*)'")]
        public static void ThenTheSubareaShouldBeDisabledWithTheMessage(string subareaLabel, string expectedMessage)
        {
            // setDisabled(true, message) on a TALXIS.Portal sitemap item is expected to render as a
            // disabled/greyed-out nav item whose tooltip (title) is the given message.
            var item = Driver.FindElement(By.XPath($"//*[contains(text(), '{subareaLabel}')]"));

            var isDisabled = item.GetAttribute("aria-disabled") == "true"
                || item.GetAttribute("class")?.Contains("disabled") == true;

            isDisabled.Should().BeTrue(because: $"the '{subareaLabel}' subarea should be disabled");
            item.GetAttribute("title").Should().Be(expectedMessage);
        }
    }
}
