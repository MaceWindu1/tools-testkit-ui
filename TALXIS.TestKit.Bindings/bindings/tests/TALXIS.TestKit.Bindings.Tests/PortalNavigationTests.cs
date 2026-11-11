namespace TALXIS.TestKit.Bindings.Tests
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OpenQA.Selenium;
    using TALXIS.TestKit.Bindings.Extensions;
    using TALXIS.TestKit.Selectors;
    using TALXIS.TestKit.Selectors.WebClientManagement;

    [TestClass]
    public class PortalNavigationTests
    {
        private IWebDriver driver = null!;

        [TestInitialize]
        public void OpenPage() => this.driver = PortalTestPage.Open();

        [TestMethod]
        public void IsReadyOnceThePortalHasLoadedItsNavigationAndPage()
        {
            PortalTestPage.Run("window.TALXIS.Portal.Context = { isNavbarLoaded: true, isLoading: true };");
            Assert.ThrowsException<WebDriverTimeoutException>(() => PortalPageManager.WaitForReady(this.driver, TimeSpan.FromSeconds(1), "Not ready."));

            PortalTestPage.Run("window.TALXIS.Portal.Context.isLoading = false;");
            Assert.IsTrue(PortalPageManager.WaitForReady(this.driver, TimeSpan.FromSeconds(1), "Not ready."));
        }

        [TestMethod]
        public void FailsFastWhenThePortalShowsAnError()
        {
            PortalTestPage.Run("document.body.insertAdjacentHTML('beforeend', '<div role=\"dialog\"><i class=\"TALXIS-Portal-errorDialog-title-icon\"></i>Configuration failed</div>');");

            var error = Assert.ThrowsException<InvalidOperationException>(() => PortalPageManager.WaitForReady(this.driver, TimeSpan.FromSeconds(5), "Not ready."));
            StringAssert.Contains(error.Message, "Configuration failed");
        }

        [TestMethod]
        public void RefusesToOpenARecordByItsId()
        {
            var navigation = new PortalApp(PortalTestPage.Client()).Navigation;

            Assert.ThrowsException<NotSupportedException>(() => navigation.OpenRecord("contact", Guid.NewGuid()));
        }

        [TestMethod]
        public void FindsAnItemByItsLabelOrSubareaId()
        {
            Assert.IsNotNull(PortalNavigationManager.FindSiteMapItem(this.driver, "Requests"));
            Assert.IsNotNull(PortalNavigationManager.FindSiteMapItem(this.driver, "RequestsSubArea"));
        }

        [TestMethod]
        public void ShowsADisabledItemWithItsMessage()
        {
            var item = PortalNavigationManager.FindSiteMapItem(this.driver, "Deliveries");

            Assert.IsTrue(PortalPageManager.IsDisabled(item));
            Assert.AreEqual("Contact your account manager to get access.", item.GetAttribute("title"));
            Assert.IsFalse(PortalPageManager.IsDisabled(PortalNavigationManager.FindSiteMapItem(this.driver, "Requests")));
        }

        [TestMethod]
        public void FindsNoItemThePortalDoesNotRender()
        {
            Assert.IsNull(PortalNavigationManager.FindSiteMapItem(this.driver, "Administration"));
            Assert.IsTrue(PortalPageManager.StaysAbsent(() => PortalNavigationManager.FindSiteMapItem(this.driver, "Administration") != null));
        }

        [TestMethod]
        public void WaitsForAnItemThatAScriptHidesToBeGone()
        {
            Assert.IsNotNull(PortalNavigationManager.FindSiteMapItem(this.driver, "Settings"), "The item is shown until the page's script hides it.");
            Assert.IsTrue(PortalPageManager.BecomesAbsent(this.driver, () => PortalNavigationManager.FindSiteMapItem(this.driver, "Settings") != null, TimeSpan.FromSeconds(10)));
        }
    }
}
