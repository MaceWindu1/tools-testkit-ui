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
        private const int UpdateFormType = 2;
        private const int DisabledFormType = 4;

        private IWebDriver driver = null!;
        private XrmApp xrmApp = null!;
        private ModelDrivenApp app = null!;

        [TestInitialize]
        public void OpenPage()
        {
            this.driver = PortalTestPage.Open("model-driven.html");
            this.xrmApp = new XrmApp(PortalTestPage.Client());
            this.app = new ModelDrivenApp(this.xrmApp);
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
        public void ClicksACommandInTheFlyoutOfACommand()
        {
            this.xrmApp.CommandBar.ClickCommand("Flow", "My flow");

            Assert.AreEqual("My flow", this.ClickedCommand());
        }

        [TestMethod]
        public void ClicksACommandInTheFlyoutOfACommandInAFlyout()
        {
            // The flyout of 'Flow' also lists a 'My flow', and stays open while the flyout of 'Run a flow' opens.
            this.xrmApp.CommandBar.ClickCommand("Flow", "Run a flow", "My flow");

            Assert.AreEqual("Run a flow > My flow", this.ClickedCommand());
        }

        [TestMethod]
        public void OpensTheFlyoutOfASplitButtonWithoutRunningItsCommand()
        {
            this.xrmApp.CommandBar.ClickCommand("Connect", "To Another Person");

            Assert.AreEqual("To Another Person", this.ClickedCommand());
        }

        [TestMethod]
        public void RunsTheCommandOfASplitButton()
        {
            this.xrmApp.CommandBar.ClickCommand("Connect");

            Assert.AreEqual("Connect", this.ClickedCommand());
        }

        [TestMethod]
        public void ClicksACommandInMoreCommands()
        {
            this.xrmApp.CommandBar.ClickCommand("Refresh");

            Assert.AreEqual("Refresh", this.ClickedCommand());
        }

        [TestMethod]
        public void ClicksACommandInTheFlyoutOfACommandInMoreCommands()
        {
            this.xrmApp.CommandBar.ClickCommand("Word Templates", "Invoice");

            Assert.AreEqual("Invoice", this.ClickedCommand());
        }

        [TestMethod]
        public void RefusesToClickACommandThatIsNotInTheFlyout()
        {
            var error = Assert.ThrowsException<InvalidOperationException>(() => this.xrmApp.CommandBar.ClickCommand("Flow", "Missing"));

            StringAssert.Contains(error.Message, "'Missing' exists under 'Flow'");
        }

        [TestMethod]
        public void ReadsTheStatusOfAnEditableRecordAsActive()
        {
            this.ShowForm(UpdateFormType, readOnlyNotification: null);

            Assert.AreEqual("Active", this.xrmApp.Entity.GetFormStatus());
        }

        [TestMethod]
        public void ReadsTheStatusOfAReadOnlyRecordFromItsNotification()
        {
            this.ShowForm(DisabledFormType, readOnlyNotification: "Read-only: This record's status: Inactive");

            Assert.AreEqual("Inactive", this.xrmApp.Entity.GetFormStatus());
        }

        [TestMethod]
        public void DoesNotTellTheStatusOfAReadOnlyFormWithoutItsNotification()
        {
            // E.g. an inactive record whose notification bar collapses several notifications.
            this.ShowForm(DisabledFormType, readOnlyNotification: null);

            Assert.IsNull(this.xrmApp.Entity.GetFormStatus());
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

        private string ClickedCommand() => this.driver.FindElement(By.TagName("body")).GetAttribute("data-clicked");

        // Shows the page as a form of the given type (Xrm.Page.ui.getFormType), with or without the read-only notification.
        private void ShowForm(int formType, string? readOnlyNotification)
        {
            ((IJavaScriptExecutor)this.driver).ExecuteScript(
                @"const [formType, notification] = arguments;
                  Xrm.Page.ui.getFormType = () => formType;
                  if (notification) {
                    const element = document.createElement('span');
                    element.id = 'message-formReadOnlyNotification';
                    element.textContent = notification;
                    document.body.append(element);
                  }",
                formType,
                readOnlyNotification);
        }
    }
}
