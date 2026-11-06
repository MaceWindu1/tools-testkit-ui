namespace TALXIS.TestKit.Bindings.Tests
{
    using System;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OpenQA.Selenium;
    using TALXIS.TestKit.Bindings.Extensions;
    using TALXIS.TestKit.Selectors;
    using TALXIS.TestKit.Selectors.WebClientManagement;

    /// <summary>
    /// Subgrids shown with the grid (ag-grid): records by id or value, selection, inline and ribbon commands, row buttons.
    /// </summary>
    [TestClass]
    public class PortalGridTests
    {
        private static readonly Guid FirstInvoice = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private static readonly Guid SecondPrepayment = Guid.Parse("55555555-5555-5555-5555-555555555555");

        private IWebDriver driver = null!;
        private IWebElement invoices = null!;

        [TestInitialize]
        public void OpenPage()
        {
            this.driver = PortalTestPage.Open();
            this.invoices = PortalGridManager.FindSubGrid(this.driver, "InvoicesSubgrid");
        }

        [TestMethod]
        public void FindsTheRowsOfARecordById()
        {
            Assert.AreEqual(2, PortalGridManager.FindRows(this.invoices, FirstInvoice).Count, "A record has a row for each pinned part.");
            Assert.AreEqual(0, PortalGridManager.FindRows(this.invoices, Guid.Parse("33333333-3333-3333-3333-333333333333")).Count);
            Assert.AreEqual(2, PortalGridManager.CountRecords(this.invoices));
        }

        [TestMethod]
        public void RefusesToCheckForADisabledCommandItCannotTellFromAMissingOne()
        {
            var subGrid = new PortalApp(PortalTestPage.Client()).SubGrid;

            var error = Assert.ThrowsException<NotSupportedException>(() => subGrid.IsCommandEnabled("InvoicesSubgrid", "Aprove", enabled: false));
            StringAssert.Contains(error.Message, "I should not be able to see the 'Aprove' command on the 'InvoicesSubgrid' subgrid");
        }

        [TestMethod]
        public void FindsTheRowsOfARecordByAValue()
        {
            Assert.AreEqual(2, PortalGridManager.FindRowsByValue(this.invoices, "Invoice number", "INV002").Count);
            Assert.AreEqual(2, PortalGridManager.FindRowsByValue(this.invoices, "talxis_name", "INV001").Count);
            Assert.AreEqual(0, PortalGridManager.FindRowsByValue(this.invoices, "talxis_name", "INV999").Count);
        }

        [TestMethod]
        public void SelectsARecordWithItsCheckbox()
        {
            PortalGridManager.SelectRecord(this.driver, PortalGridManager.FindRows(this.invoices, FirstInvoice), "the first invoice");

            Assert.IsTrue(PortalGridManager.FindRows(this.invoices, FirstInvoice)[0].FindElement(By.CssSelector(".ms-Checkbox")).GetAttribute("class").Contains("is-checked"));
        }

        [TestMethod]
        public void FindsAnInlineCommandOfAnyRowWhileNoneIsSelected()
        {
            Assert.IsTrue(PortalGridManager.IsSubGridCommandUnavailable(this.driver, this.invoices, "MISSING"), "The check waits for the rows' commands to load.");
            Assert.IsNotNull(PortalGridManager.FindSubGridCommand(this.driver, this.invoices, "PAID"));
        }

        [TestMethod]
        public void FindsTheInlineCommandsOfTheSelectedRecordOnly()
        {
            PortalGridManager.SelectRecord(this.driver, PortalGridManager.FindRows(this.invoices, FirstInvoice), "the first invoice");
            PortalPageManager.WaitFor(this.driver, () => this.invoices.FindElements(By.CssSelector("#row2-ribbon [data-command]")).Count > 0, TimeSpan.FromSeconds(5));

            Assert.IsNotNull(PortalGridManager.FindSubGridCommand(this.driver, this.invoices, "PAY"));
            Assert.IsNull(PortalGridManager.FindSubGridCommand(this.driver, this.invoices, "PAID"), "The second invoice's command is not the selected record's.");
        }

        [TestMethod]
        public void FindsACommandByLabelIdCommandOrIconFileName()
        {
            PortalGridManager.SelectRecord(this.driver, PortalGridManager.FindRows(this.invoices, FirstInvoice), "the first invoice");

            Assert.IsNotNull(PortalGridManager.FindSubGridCommand(this.driver, this.invoices, "Export data"), "A ribbon command by its label.");
            Assert.IsNotNull(PortalGridManager.FindSubGridCommand(this.driver, this.invoices, "talxis_downloaddocument.svg"));
            Assert.IsNotNull(PortalGridManager.FindSubGridCommand(this.driver, this.invoices, "sample.invoice.subgrid.PDF.Button"));
            Assert.IsNotNull(PortalGridManager.FindSubGridCommand(this.driver, this.invoices, "sample.invoice.subgrid.PDF.Command"));

            PortalGridManager.FindSubGridCommand(this.driver, this.invoices, "talxis_downloaddocument.svg").Click();
            Assert.AreEqual("PDF", PortalTestPage.Clicks());
        }

        [TestMethod]
        public void TellsWhetherACommandIsAvailable()
        {
            Assert.IsTrue(PortalGridManager.IsSubGridCommandUnavailable(this.driver, this.invoices, "MISSING"));
            Assert.IsTrue(PortalGridManager.IsSubGridCommandUnavailable(this.driver, this.invoices, "BUSY"), "A disabled command is unavailable.");
            Assert.IsTrue(PortalGridManager.WaitForSubGridCommand(this.driver, this.invoices, "LATER"), "A command the ribbon shows late is awaited.");
        }

        [TestMethod]
        public void FindsARowButtonByItsLabelOrIconName()
        {
            var rows = PortalGridManager.FindRows(this.driver, FirstInvoice);

            Assert.IsNotNull(PortalGridManager.FindRowButton(rows, "Delete"), "By its aria-label.");
            Assert.AreEqual(string.Empty, PortalGridManager.FindRowButton(rows, "ClipboardList")?.GetAttribute("title"), "An icon-only button by its icon name.");
            Assert.IsNull(PortalGridManager.FindRowButton(rows, "it's"));
        }

        [TestMethod]
        public void UsesTheCommandsOfAllRowsOrOfTheMarkedRowInAGridWithoutSelection()
        {
            var prepayments = PortalGridManager.FindSubGrid(this.driver, "PrepaymentsGrid");
            Assert.IsNotNull(PortalGridManager.FindSubGridCommand(this.driver, prepayments, "PAY"));
            Assert.IsNotNull(PortalGridManager.FindSubGridCommand(this.driver, prepayments, "PAID"));

            PortalGridManager.SelectRecord(this.driver, PortalGridManager.FindRows(prepayments, SecondPrepayment), "the second prepayment");

            Assert.IsNotNull(PortalGridManager.FindSubGridCommand(this.driver, prepayments, "PAID"));
            Assert.IsNull(PortalGridManager.FindSubGridCommand(this.driver, prepayments, "PAY"));
            Assert.AreEqual(string.Empty, PortalTestPage.Clicks(), "The row is marked without clicking it, which would open the record.");
        }

        [TestMethod]
        public void FindsACommandInTheRowsOfARecordWithoutSelectingIt()
        {
            var rows = PortalGridManager.FindRows(PortalGridManager.FindSubGrid(this.driver, "PrepaymentsGrid"), SecondPrepayment);

            Assert.IsNotNull(PortalGridManager.FindRowCommand(this.driver, rows, "PAID"));
            Assert.IsNull(PortalGridManager.FindRowCommand(this.driver, rows, "PAY"), "The other record's command is not in its rows.");
            Assert.IsFalse(rows.Any(row => row.GetAttribute("class").Contains("ag-row-selected")), "The record stays unselected.");
        }
    }
}
