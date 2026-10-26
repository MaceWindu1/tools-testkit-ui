namespace TALXIS.TestKit.Bindings.Tests
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OpenQA.Selenium;
    using TALXIS.TestKit.Bindings.Extensions;

    [TestClass]
    public class PortalFormTests
    {
        private IWebDriver driver = null!;

        [TestInitialize]
        public void OpenPage() => this.driver = PortalTestPage.Open();

        [TestMethod]
        public void FindsATabByItsLabelWithoutItsIconOrByItsName()
        {
            Assert.IsNotNull(this.driver.FindPortalTab("Overview"));
            Assert.IsNotNull(this.driver.FindPortalTab("SigningTab"));
            Assert.IsNull(this.driver.FindPortalTab("Statistics"));
        }

        [TestMethod]
        public void TellsWhichTabIsSelected()
        {
            Assert.IsTrue(this.driver.FindPortalTab("Overview").IsPortalTabSelected());

            this.driver.FindPortalTab("Signing").Click();

            Assert.IsTrue(this.driver.FindPortalTab("Signing").IsPortalTabSelected());
            Assert.IsFalse(this.driver.FindPortalTab("Overview").IsPortalTabSelected());
        }

        [TestMethod]
        public void SelectsATabWhosePageDroppedTheFirstClick()
        {
            Assert.IsTrue(this.driver.SelectPortalTab("Charts", TimeSpan.FromSeconds(10)));

            Assert.IsTrue(this.driver.FindPortalTab("Charts").IsPortalTabSelected());
        }

        [TestMethod]
        public void LeavesTheSelectedTabAlone()
        {
            Assert.IsTrue(this.driver.SelectPortalTab("Overview", TimeSpan.FromSeconds(10)));
        }

        [TestMethod]
        public void SelectsATabWhileTheTabBarRerenders()
        {
            PortalTestPage.Run("rerenderTabsFor(3000)");

            Assert.IsTrue(this.driver.SelectPortalTab("Signing", TimeSpan.FromSeconds(10)));
            Assert.IsTrue(this.driver.WaitForPortalCondition(() => this.driver.FindPortalTab("Signing").IsPortalTabSelected(), TimeSpan.FromSeconds(5)));
        }

        [TestMethod]
        public void ReadsAFieldByItsLabelOrLogicalName()
        {
            Assert.AreEqual("C0001", this.driver.TryFindPortalField("Customer number")?.GetPortalFieldValue());
            Assert.IsNotNull(this.driver.TryFindPortalField("talxis_internalid"));
            Assert.IsNull(this.driver.TryFindPortalField("Missing field"));
        }

        [TestMethod]
        public void ReadsTheFieldsOfANestedForm()
        {
            Assert.AreEqual("Open request", this.driver.TryFindPortalField("Subject")?.GetPortalFieldValue());
            Assert.AreEqual("North branch", this.driver.TryFindPortalField("Operating unit")?.GetPortalFieldValue());
        }

        [TestMethod]
        public void LooksUpFieldsOnTheDialogWhileOneIsOpen()
        {
            PortalTestPage.Run("showFormDialog()");

            Assert.IsNull(this.driver.TryFindPortalField("Customer number"), "A field of the page behind the dialog is not found.");
        }
    }
}
