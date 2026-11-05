namespace TALXIS.TestKit.Bindings.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OpenQA.Selenium;
    using TALXIS.TestKit.Bindings.Extensions;
    using TALXIS.TestKit.Selectors.WebClientManagement;

    [TestClass]
    public class PortalDialogTests
    {
        private IWebDriver driver = null!;

        [TestInitialize]
        public void OpenPage() => this.driver = PortalTestPage.Open();

        [TestMethod]
        public void ConfirmsAConfirmationDialogWithItsPrimaryButton()
        {
            PortalTestPage.Run("showConfirmationDialog()");

            Assert.AreEqual("Do you want to activate this record?", PortalDialogsManager.GetMessageTitle(this.driver));
            PortalDialogsManager.AnswerMessageDialog(this.driver, confirm: true);

            Assert.AreEqual("confirm", PortalTestPage.Clicks());
            Assert.IsNull(PortalDialogsManager.GetMessageTitle(this.driver));
        }

        [TestMethod]
        public void CancelsAConfirmationDialogWithItsSecondaryButton()
        {
            PortalTestPage.Run("showConfirmationDialog()");

            PortalDialogsManager.AnswerMessageDialog(this.driver, confirm: false);

            Assert.AreEqual("cancel", PortalTestPage.Clicks());
        }

        [TestMethod]
        public void AnswersTheTopmostDialog()
        {
            PortalTestPage.Run("showFormDialog(); showConfirmationDialog()");

            PortalDialogsManager.AnswerMessageDialog(this.driver, confirm: true);

            Assert.AreEqual("confirm", PortalTestPage.Clicks());
            Assert.IsNotNull(PortalDialogsManager.FindButton(this.driver, "Continue"), "The form dialog below stays open.");
        }

        [TestMethod]
        public void FindsOnlyTheShownButtonsOfAFormDialog()
        {
            PortalTestPage.Run("showFormDialog()");

            Assert.IsNotNull(PortalDialogsManager.FindButton(this.driver, "Continue"));
            Assert.IsNull(PortalDialogsManager.FindButton(this.driver, "Finish"));
        }

        [TestMethod]
        public void GoesThroughAWizardToItsLastStep()
        {
            PortalTestPage.Run("showFormDialog()");

            var clicks = 0;
            while (PortalDialogsManager.FindButton(this.driver, "Continue") is { } next && clicks < 10)
            {
                next.Click();
                clicks++;
            }

            Assert.AreEqual(2, clicks);
            Assert.IsNotNull(PortalDialogsManager.FindButton(this.driver, "Finish"));
        }

        [TestMethod]
        public void ClosesADialogWithoutSaving()
        {
            PortalTestPage.Run("showFormDialog()");

            PortalDialogsManager.CloseDialog(this.driver);

            Assert.IsNull(PortalPageManager.GetTopmostModal(this.driver));
            Assert.AreEqual("C0001", PortalFormManager.GetValue(PortalFormManager.TryFindField(this.driver, "Customer number")), "The page's fields are found again.");
        }

        [TestMethod]
        public void ClosesADialogThatIsStillOpening()
        {
            PortalTestPage.Run("setTimeout(showFormDialog, 1500)");

            var closed = PortalDialogsManager.CloseDialog(this.driver);

            Assert.IsNotNull(closed);
            Assert.IsNull(PortalPageManager.GetTopmostModal(this.driver));
        }
    }
}
