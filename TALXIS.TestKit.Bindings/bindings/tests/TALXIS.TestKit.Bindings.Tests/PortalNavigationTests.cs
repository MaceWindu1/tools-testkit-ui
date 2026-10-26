namespace TALXIS.TestKit.Bindings.Tests
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OpenQA.Selenium;
    using TALXIS.TestKit.Bindings.Extensions;

    [TestClass]
    public class PortalNavigationTests
    {
        private IWebDriver driver = null!;

        [TestInitialize]
        public void OpenPage() => this.driver = PortalTestPage.Open();

        [TestMethod]
        public void FindsAnItemByItsLabelOrSubareaId()
        {
            Assert.IsNotNull(this.driver.FindPortalSiteMapItem("Requests"));
            Assert.IsNotNull(this.driver.FindPortalSiteMapItem("RequestsSubArea"));
        }

        [TestMethod]
        public void ShowsADisabledItemWithItsMessage()
        {
            var item = this.driver.FindPortalSiteMapItem("Deliveries");

            Assert.IsTrue(item.IsPortalElementDisabled());
            Assert.AreEqual("Contact your account manager to get access.", item.GetAttribute("title"));
            Assert.IsFalse(this.driver.FindPortalSiteMapItem("Requests").IsPortalElementDisabled());
        }

        [TestMethod]
        public void FindsNoItemThePortalDoesNotRender()
        {
            Assert.IsNull(this.driver.FindPortalSiteMapItem("Administration"));
            Assert.IsTrue(PortalPageExtensions.StaysAbsent(() => this.driver.FindPortalSiteMapItem("Administration") != null));
        }

        [TestMethod]
        public void WaitsForAnItemThatAScriptHidesToBeGone()
        {
            Assert.IsNotNull(this.driver.FindPortalSiteMapItem("Settings"), "The item is shown until the page's script hides it.");
            Assert.IsTrue(this.driver.BecomesAbsent(() => this.driver.FindPortalSiteMapItem("Settings") != null, TimeSpan.FromSeconds(10)));
        }
    }
}
