namespace TALXIS.TestKit.Bindings.Steps
{
    using System;
    using Reqnroll;
    using TALXIS.TestKit.Bindings;
    using TALXIS.TestKit.Bindings.Extensions;
    using TALXIS.TestKit.Selectors.Browser;

    /// <summary>
    /// Test setup step bindings for data.
    /// </summary>
    [Binding]
    public class DataSteps : PowerAppsStepDefiner
    {
        /// <summary>
        /// Opens a test record.
        /// </summary>
        /// <param name="alias">The alias of the test record.</param>
        [Given(@"I have opened '(.*)'")]
        public static void GivenIHaveOpened(string alias)
        {
            var record = TestDriver.GetTestRecordReference(alias);
            App.Navigation.OpenRecord(record.LogicalName, record.Id);
        }

        /// <summary>
        /// Creates a test record.
        /// </summary>
        /// <param name="fileName">The name of the file containing the test record.</param>
        [Given(@"I have created '(.*)'")]
        [Given(@"'(.*)' exists")]
        public static void GivenIHaveCreated(string fileName)
        {
            TestDriver.LoadTestData(TestDataRepository.GetTestData(fileName).ReplaceParameters(TestConfig.Parameters));

            Driver.WaitForTransaction();
        }

        /// <summary>
        /// Creates multiple test records.
        /// </summary>
        /// <param name="records">The records to create.</param>
        [Given(@"I have created the following records")]
        [Given(@"the following records exist")]
        public static void GivenIHaveCreatedTheFollowingRecords(Table records)
        {
            ArgumentNullException.ThrowIfNull(records, nameof(records));

            foreach (DataTableRow row in records.Rows)
            {
                GivenIHaveCreated(row["Record"]);
            }
        }

        /// <summary>
        /// Creates a test record as a given user.
        /// </summary>
        /// <param name="alias">The user alias.</param>
        /// <param name="fileName">The name of the file containing the test record.</param>
        [Given(@"'(.*)' has created '(.*)'")]
        public static void GivenIHaveCreated(string alias, string fileName)
        {
            TestDriver.LoadTestDataAsUser(
                TestDataRepository.GetTestData(fileName).ReplaceParameters(TestConfig.Parameters),
                TestConfig.GetUser(alias).Username);
        }
    }
}
