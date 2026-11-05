namespace TALXIS.TestKit.Bindings.Tests
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OpenQA.Selenium;
    using TALXIS.TestKit.Bindings.Extensions;
    using TALXIS.TestKit.Selectors.WebClientManagement;

    [TestClass]
    public class PortalFormTests
    {
        private IWebDriver driver = null!;

        [TestInitialize]
        public void OpenPage() => this.driver = PortalTestPage.Open();

        [TestMethod]
        public void FindsATabByItsLabelWithoutItsIconOrByItsName()
        {
            Assert.IsNotNull(PortalFormManager.FindTab(this.driver, "Overview"));
            Assert.IsNotNull(PortalFormManager.FindTab(this.driver, "SigningTab"));
            Assert.IsNull(PortalFormManager.FindTab(this.driver, "Statistics"));
        }

        [TestMethod]
        public void TellsWhichTabIsSelected()
        {
            Assert.IsTrue(PortalFormManager.IsTabSelected(PortalFormManager.FindTab(this.driver, "Overview")));

            PortalFormManager.FindTab(this.driver, "Signing").Click();

            Assert.IsTrue(PortalFormManager.IsTabSelected(PortalFormManager.FindTab(this.driver, "Signing")));
            Assert.IsFalse(PortalFormManager.IsTabSelected(PortalFormManager.FindTab(this.driver, "Overview")));
        }

        [TestMethod]
        public void SelectsATabWhosePageDroppedTheFirstClick()
        {
            Assert.IsTrue(PortalFormManager.SelectTab(this.driver, "Charts", TimeSpan.FromSeconds(10)));

            Assert.IsTrue(PortalFormManager.IsTabSelected(PortalFormManager.FindTab(this.driver, "Charts")));
        }

        [TestMethod]
        public void LeavesTheSelectedTabAlone()
        {
            Assert.IsTrue(PortalFormManager.SelectTab(this.driver, "Overview", TimeSpan.FromSeconds(10)));
        }

        [TestMethod]
        public void SelectsATabWhileTheTabBarRerenders()
        {
            PortalTestPage.Run("rerenderTabsFor(3000)");

            Assert.IsTrue(PortalFormManager.SelectTab(this.driver, "Signing", TimeSpan.FromSeconds(10)));
            Assert.IsTrue(PortalPageManager.WaitFor(this.driver, () => PortalFormManager.IsTabSelected(PortalFormManager.FindTab(this.driver, "Signing")), TimeSpan.FromSeconds(5)));
        }

        [TestMethod]
        public void ReadsAFieldByItsLabelOrLogicalName()
        {
            Assert.AreEqual("C0001", PortalFormManager.GetValue(PortalFormManager.TryFindField(this.driver, "Customer number")));
            Assert.IsNotNull(PortalFormManager.TryFindField(this.driver, "talxis_internalid"));
            Assert.IsNull(PortalFormManager.TryFindField(this.driver, "Missing field"));
        }

        [TestMethod]
        public void ReadsTheFieldsOfANestedForm()
        {
            Assert.AreEqual("Open request", PortalFormManager.GetValue(PortalFormManager.TryFindField(this.driver, "Subject")));
            Assert.AreEqual("North branch", PortalFormManager.GetValue(PortalFormManager.TryFindField(this.driver, "Operating unit")));
        }

        [TestMethod]
        public void TakesTheOnlySuggestionOfALookupOnceTheSearchNarrowsItDown()
        {
            PortalFormManager.SetValue(this.driver, PortalFormManager.FindField(this.driver, "Contract"), "Main contract");

            Assert.AreEqual("C-002", PortalFormManager.GetValue(PortalFormManager.FindField(this.driver, "Contract")));
        }

        [TestMethod]
        public void LooksUpFieldsOnTheDialogWhileOneIsOpen()
        {
            PortalTestPage.Run("showFormDialog()");

            Assert.IsNull(PortalFormManager.TryFindField(this.driver, "Customer number"), "A field of the page behind the dialog is not found.");
        }
    }
}
