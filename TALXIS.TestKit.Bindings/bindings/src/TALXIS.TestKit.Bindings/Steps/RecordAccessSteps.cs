namespace TALXIS.TestKit.Bindings.Steps
{
    using FluentAssertions;
    using Reqnroll;
    using TALXIS.TestKit.Bindings.Extensions;

    /// <summary>
    /// Step bindings that check which test records the signed-in user can read, as the app's pages read them (security
    /// roles, and in a TALXIS Portal its retrieve filters). A record that cannot be read is one the query does not
    /// return: a failed query proves nothing, so it fails both checks.
    /// </summary>
    [Binding]
    public class RecordAccessSteps : PowerAppsStepDefiner
    {
        /// <summary>
        /// Asserts that the signed-in user can read a test record.
        /// </summary>
        /// <param name="alias">The alias of the test record.</param>
        [Then(@"^I should be able to read '(.*)'$")]
        public static void ThenIShouldBeAbleToRead(string alias)
        {
            var record = TestDriver.GetTestRecordReference(alias);
            var access = Driver.ReadRecordAccess(record.LogicalName, record.Id);
            access.Readable.Should().BeTrue(because: $"the signed-in user should be able to read '{alias}'{(access.Error == null ? string.Empty : $" (the query failed: {access.Error})")}");
        }

        /// <summary>
        /// Asserts that the signed-in user cannot read a test record.
        /// </summary>
        /// <param name="alias">The alias of the test record.</param>
        [Then(@"^I should not be able to read '(.*)'$")]
        public static void ThenIShouldNotBeAbleToRead(string alias)
        {
            var record = TestDriver.GetTestRecordReference(alias);
            var access = Driver.ReadRecordAccess(record.LogicalName, record.Id);
            access.Error.Should().BeNull(because: $"querying '{alias}' should show whether the signed-in user can read it");
            access.Readable.Should().BeFalse(because: $"the signed-in user should not be able to read '{alias}'");
        }
    }
}
