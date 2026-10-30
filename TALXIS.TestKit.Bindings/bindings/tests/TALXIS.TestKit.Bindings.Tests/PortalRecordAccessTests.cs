namespace TALXIS.TestKit.Bindings.Tests
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OpenQA.Selenium;
    using TALXIS.TestKit.Bindings.Extensions;

    /// <summary>
    /// Reading a record as the signed-in portal user, through the page's Xrm.WebApi (stubbed on the test page).
    /// </summary>
    [TestClass]
    public class PortalRecordAccessTests
    {
        private static readonly Guid Readable = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private static readonly Guid Filtered = Guid.Parse("22222222-2222-2222-2222-222222222222");

        private IWebDriver driver = null!;

        [TestInitialize]
        public void OpenPage() => this.driver = PortalTestPage.Open();

        [TestMethod]
        public void ReadsOnlyTheRecordsTheQueryReturns()
        {
            PortalTestPage.Run(
                "Xrm.WebApi.retrieveMultipleRecords = function (entity, options) { window.lastQuery = entity + options;" +
                " return Promise.resolve({ entities: options.indexOf('" + Readable + "') > 0 ? [{}] : [] }); }");

            Assert.IsTrue(this.driver.ReadPortalRecordAccess("talxis_contract", Readable).Readable);
            Assert.IsFalse(this.driver.ReadPortalRecordAccess("talxis_contract", Filtered).Readable);
            Assert.AreEqual(
                $"talxis_contract?$select=talxis_contractid&$filter=talxis_contractid eq {Filtered}",
                ((IJavaScriptExecutor)this.driver).ExecuteScript("return window.lastQuery"));
        }

        [TestMethod]
        public void TellsWhyTheQueryFailed()
        {
            PortalTestPage.Run("Xrm.WebApi.retrieveMultipleRecords = function () { return Promise.reject({ message: 'Forbidden' }); }");

            var access = this.driver.ReadPortalRecordAccess("talxis_contract", Readable);

            Assert.IsFalse(access.Readable);
            Assert.AreEqual("Forbidden", access.Error);
        }
    }
}
