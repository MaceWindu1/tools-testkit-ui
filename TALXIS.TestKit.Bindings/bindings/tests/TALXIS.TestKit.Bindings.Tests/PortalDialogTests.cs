namespace TALXIS.TestKit.Bindings.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OpenQA.Selenium;
    using TALXIS.TestKit.Bindings.Extensions;

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

            Assert.AreEqual("Do you want to activate this record?", this.driver.GetPortalAlertDialogTitle());
            this.driver.AnswerPortalMessageDialog(confirm: true);

            Assert.AreEqual("confirm", PortalTestPage.Clicks());
            Assert.IsNull(this.driver.GetPortalAlertDialogTitle());
        }

        [TestMethod]
        public void CancelsAConfirmationDialogWithItsSecondaryButton()
        {
            PortalTestPage.Run("showConfirmationDialog()");

            this.driver.AnswerPortalMessageDialog(confirm: false);

            Assert.AreEqual("cancel", PortalTestPage.Clicks());
        }

        [TestMethod]
        public void AnswersTheTopmostDialog()
        {
            PortalTestPage.Run("showFormDialog(); showConfirmationDialog()");

            this.driver.AnswerPortalMessageDialog(confirm: true);

            Assert.AreEqual("confirm", PortalTestPage.Clicks());
            Assert.IsNotNull(this.driver.FindPortalDialogButton("Continue"), "The form dialog below stays open.");
        }

        [TestMethod]
        public void FindsOnlyTheShownButtonsOfAFormDialog()
        {
            PortalTestPage.Run("showFormDialog()");

            Assert.IsNotNull(this.driver.FindPortalDialogButton("Continue"));
            Assert.IsNull(this.driver.FindPortalDialogButton("Finish"));
        }

        [TestMethod]
        public void GoesThroughAWizardToItsLastStep()
        {
            PortalTestPage.Run("showFormDialog()");

            var clicks = 0;
            while (this.driver.FindPortalDialogButton("Continue") is { } next && clicks < 10)
            {
                next.Click();
                clicks++;
            }

            Assert.AreEqual(2, clicks);
            Assert.IsNotNull(this.driver.FindPortalDialogButton("Finish"));
        }

        [TestMethod]
        public void ClosesADialogWithoutSaving()
        {
            PortalTestPage.Run("showFormDialog()");

            this.driver.ClosePortalDialog();

            Assert.IsNull(this.driver.GetTopmostPortalModal());
            Assert.AreEqual("C0001", this.driver.TryFindPortalField("Customer number")?.GetPortalFieldValue(), "The page's fields are found again.");
        }

        [TestMethod]
        public void ClosesADialogThatIsStillOpening()
        {
            PortalTestPage.Run("setTimeout(showFormDialog, 1500)");

            var closed = this.driver.ClosePortalDialog();

            Assert.IsNotNull(closed);
            Assert.IsNull(this.driver.GetTopmostPortalModal());
        }
    }
}
