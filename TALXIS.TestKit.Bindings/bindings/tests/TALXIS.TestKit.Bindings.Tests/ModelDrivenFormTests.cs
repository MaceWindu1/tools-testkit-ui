namespace TALXIS.TestKit.Bindings.Tests
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OpenQA.Selenium;
    using TALXIS.TestKit.Selectors;
    using TALXIS.TestKit.Selectors.WebClientManagement;

    [TestClass]
    public class ModelDrivenFormTests
    {
        private IWebDriver driver = null!;
        private ModelDrivenApp app = null!;

        [TestInitialize]
        public void OpenPage()
        {
            this.driver = PortalTestPage.Open("model-driven.html");
            this.app = new ModelDrivenApp(new XrmApp(PortalTestPage.Client()));
        }

        [TestMethod]
        public void ResolvesAFieldByItsLogicalNameOrItsLabelEvenWhenItIsHidden()
        {
            Assert.AreEqual("name", EntityManager.ResolveFieldLogicalName(this.driver, "name"));
            Assert.AreEqual("name", EntityManager.ResolveFieldLogicalName(this.driver, "Name"));
            Assert.AreEqual("talxis_emailfrequency", EntityManager.ResolveFieldLogicalName(this.driver, "Email frequency"));
        }

        [TestMethod]
        public void LeavesANameThatIsNeitherAFieldNorAFieldLabelUnchanged()
        {
            Assert.AreEqual("Missing field", EntityManager.ResolveFieldLogicalName(this.driver, "Missing field"));
            Assert.AreEqual("Documents", EntityManager.ResolveFieldLogicalName(this.driver, "Documents"));
        }

        [TestMethod]
        public void TellsWhetherAFieldIsShownByItsLabel()
        {
            Assert.IsTrue(this.app.Entity.IsFieldShown("Name", shown: true));
            Assert.IsTrue(this.app.Entity.IsFieldShown("Email frequency", shown: false));
        }

        [TestMethod]
        public void TellsWhetherAShownOrHiddenFieldIsEmpty()
        {
            Assert.IsTrue(this.app.Entity.IsFieldEmpty("Preferred channels"));
            Assert.IsTrue(this.app.Entity.IsFieldEmpty("talxis_note"));
            Assert.IsTrue(this.app.Entity.IsFieldEmpty("Email frequency"));
            Assert.IsFalse(this.app.Entity.IsFieldEmpty("Name"));
            Assert.IsFalse(this.app.Entity.IsFieldEmpty("talxis_phonefrequency"));
        }

        [TestMethod]
        public void RefusesToTellWhetherAFieldNotOnTheFormIsEmpty()
        {
            Assert.ThrowsException<InvalidOperationException>(() => this.app.Entity.IsFieldEmpty("talxis_missing"));
        }

        [TestMethod]
        public void AFieldIsEditableWhenItIsShownAndNotReadOnly()
        {
            Assert.IsTrue(this.app.Entity.IsFieldEditable("Name", editable: true));
            Assert.IsTrue(this.app.Entity.IsFieldEditable("Number", editable: false), "A read-only field is not editable.");
            Assert.IsFalse(this.app.Entity.IsFieldEditable("Number", editable: true));
        }

        [TestMethod]
        public void ReadsWhetherASubgridCommandIsEnabledFromItsButton()
        {
            Assert.IsTrue(this.app.SubGrid.IsCommandEnabled("Contacts", "Add Contact", enabled: true));
            Assert.IsTrue(this.app.SubGrid.IsCommandEnabled("Contacts", "Deactivate", enabled: false));
        }

        [TestMethod]
        public void LooksForTextsAndButtonsOnTheTopmostDialogOnly()
        {
            Assert.IsTrue(this.app.Dialogs.IsTextShown("Leave the wizard?"));
            Assert.IsFalse(this.app.Dialogs.IsTextShown("Summary"), "A text behind the dialogs is not on the dialog.");
            Assert.IsFalse(this.app.Dialogs.IsTextShown("Step 1"), "A text of the dialog below is not on the topmost dialog.");
            Assert.IsTrue(this.app.Dialogs.IsButtonShown("Leave", shown: true));
            Assert.IsTrue(this.app.Dialogs.IsButtonShown("Next", shown: false));
        }

        [TestMethod]
        public void AHiddenFieldIsNotEditableEvenWhenItIsNotReadOnly()
        {
            Assert.IsTrue(this.app.Entity.IsFieldEditable("Email frequency", editable: false));
            Assert.IsFalse(this.app.Entity.IsFieldEditable("Email frequency", editable: true));
        }
    }
}
