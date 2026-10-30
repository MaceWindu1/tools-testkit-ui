namespace TALXIS.TestKit.Bindings.Steps
{
    using FluentAssertions;
    using Reqnroll;
    using TALXIS.TestKit.Bindings.Extensions;

    /// <summary>
    /// Step bindings that check which test records the signed-in TALXIS Portal user can read, as the portal's pages read
    /// them (its retrieve filters apply). A record that cannot be read is one the query does not return: a failed query
    /// proves nothing, so it fails both checks.
    /// </summary>
    [Binding]
    public class PortalRecordAccessSteps : PowerAppsStepDefiner
    {
        /// <summary>
        /// Asserts that the signed-in portal user can read a test record.
        /// </summary>
        /// <param name="alias">The alias of the test record.</param>
        [Then(@"^I should be able to read '(.*)'$")]
        public static void ThenIShouldBeAbleToRead(string alias)
        {
            var record = TestDriver.GetTestRecordReference(alias);
            var access = Driver.ReadPortalRecordAccess(record.LogicalName, record.Id);
            access.Readable.Should().BeTrue(because: $"the signed-in user should be able to read '{alias}'{(access.Error == null ? string.Empty : $" (the query failed: {access.Error})")}");
        }

        /// <summary>
        /// Asserts that the signed-in portal user cannot read a test record.
        /// </summary>
        /// <param name="alias">The alias of the test record.</param>
        [Then(@"^I should not be able to read '(.*)'$")]
        public static void ThenIShouldNotBeAbleToRead(string alias)
        {
            var record = TestDriver.GetTestRecordReference(alias);
            var access = Driver.ReadPortalRecordAccess(record.LogicalName, record.Id);
            access.Error.Should().BeNull(because: $"querying '{alias}' should show whether the signed-in user can read it");
            access.Readable.Should().BeFalse(because: $"the signed-in user should not be able to read '{alias}'");
        }
    }
}
