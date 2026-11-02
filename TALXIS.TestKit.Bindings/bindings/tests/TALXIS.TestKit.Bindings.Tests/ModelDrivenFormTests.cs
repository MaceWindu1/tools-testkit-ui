namespace TALXIS.TestKit.Bindings.Tests
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OpenQA.Selenium;
    using TALXIS.TestKit.Bindings.Extensions;
    using TALXIS.TestKit.Selectors;

    [TestClass]
    public class ModelDrivenFormTests
    {
        // The extensions only read the browser, not the entity they extend.
        private readonly Entity entity = null!;

        private IWebDriver driver = null!;

        [TestInitialize]
        public void OpenPage() => this.driver = PortalTestPage.Open("model-driven.html");

        [TestMethod]
        public void ResolvesAFieldByItsLogicalNameOrItsLabelEvenWhenItIsHidden()
        {
            Assert.AreEqual("name", this.entity.ResolveFieldLogicalName(this.driver, "name"));
            Assert.AreEqual("name", this.entity.ResolveFieldLogicalName(this.driver, "Name"));
            Assert.AreEqual("talxis_emailfrequency", this.entity.ResolveFieldLogicalName(this.driver, "Email frequency"));
        }

        [TestMethod]
        public void LeavesANameThatIsNeitherAFieldNorAFieldLabelUnchanged()
        {
            Assert.AreEqual("Missing field", this.entity.ResolveFieldLogicalName(this.driver, "Missing field"));
            Assert.AreEqual("Documents", this.entity.ResolveFieldLogicalName(this.driver, "Documents"));
        }

        [TestMethod]
        public void TellsWhetherAFieldIsVisibleByItsLabel()
        {
            Assert.IsTrue(this.entity.IsFieldVisible(this.driver, this.entity.ResolveFieldLogicalName(this.driver, "Name")));
            Assert.IsFalse(this.entity.IsFieldVisible(this.driver, this.entity.ResolveFieldLogicalName(this.driver, "Email frequency")));
        }

        [TestMethod]
        public void TellsWhetherAShownOrHiddenFieldIsEmpty()
        {
            Assert.IsTrue(this.entity.IsFieldEmpty(this.driver, "talxis_preferredchannels"));
            Assert.IsTrue(this.entity.IsFieldEmpty(this.driver, "talxis_note"));
            Assert.IsTrue(this.entity.IsFieldEmpty(this.driver, "talxis_emailfrequency"));
            Assert.IsFalse(this.entity.IsFieldEmpty(this.driver, "name"));
            Assert.IsFalse(this.entity.IsFieldEmpty(this.driver, "talxis_phonefrequency"));
        }

        [TestMethod]
        public void RefusesToTellWhetherAFieldNotOnTheFormIsEmpty()
        {
            Assert.ThrowsException<InvalidOperationException>(() => this.entity.IsFieldEmpty(this.driver, "talxis_missing"));
        }
    }
}
