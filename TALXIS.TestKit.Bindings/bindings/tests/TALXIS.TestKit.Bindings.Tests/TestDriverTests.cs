namespace TALXIS.TestKit.Bindings.Tests
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OpenQA.Selenium;

    /// <summary>
    /// The references of the test records, from a test driver stubbed on the test page (it is injected only when missing).
    /// </summary>
    [TestClass]
    public class TestDriverTests
    {
        private static readonly Guid Account = new("a1b2c3d4-0000-0000-0000-000000000001");

        private TestDriver testDriver = null!;

        [TestInitialize]
        public void OpenPage()
        {
            var driver = PortalTestPage.Open();
            PortalTestPage.Run(
                "top.driver = { getRecordReference: alias => alias === 'an account' ? { entityType: 'account', id: '{A1B2C3D4-0000-0000-0000-000000000001}' } : undefined };");
            this.testDriver = new TestDriver((IJavaScriptExecutor)driver);
        }

        [TestMethod]
        public void GetsTheReferenceOfATestRecord()
        {
            var reference = this.testDriver.GetTestRecordReference("an account");

            Assert.AreEqual("account", reference.LogicalName);
            Assert.AreEqual(Account, reference.Id);
        }

        [TestMethod]
        public void NamesAnAliasWithoutATestRecord()
        {
            var error = Assert.ThrowsException<ArgumentException>(() => this.testDriver.GetTestRecordReference("a missing account"));

            StringAssert.Contains(error.Message, "Test record with alias 'a missing account' does not exist.");
        }
    }
}
