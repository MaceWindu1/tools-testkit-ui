namespace TALXIS.TestKit.Bindings.Tests
{
    using System;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OpenQA.Selenium;
    using TALXIS.TestKit.Bindings.Extensions;

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
            this.invoices = this.driver.FindPortalSubGrid("InvoicesSubgrid");
        }

        [TestMethod]
        public void FindsTheRowsOfARecordById()
        {
            Assert.AreEqual(2, this.invoices.FindPortalGridRows(FirstInvoice).Count, "A record has a row for each pinned part.");
            Assert.AreEqual(0, this.invoices.FindPortalGridRows(Guid.Parse("33333333-3333-3333-3333-333333333333")).Count);
            Assert.AreEqual(2, this.invoices.CountPortalGridRecords());
        }

        [TestMethod]
        public void FindsTheRowsOfARecordByAValue()
        {
            Assert.AreEqual(2, this.invoices.FindPortalGridRowsByValue("Invoice number", "INV002").Count);
            Assert.AreEqual(2, this.invoices.FindPortalGridRowsByValue("talxis_name", "INV001").Count);
            Assert.AreEqual(0, this.invoices.FindPortalGridRowsByValue("talxis_name", "INV999").Count);
        }

        [TestMethod]
        public void SelectsARecordWithItsCheckbox()
        {
            this.driver.SelectPortalGridRecord(this.invoices.FindPortalGridRows(FirstInvoice), "the first invoice");

            Assert.IsTrue(this.invoices.FindPortalGridRows(FirstInvoice)[0].FindElement(By.CssSelector(".ms-Checkbox")).GetAttribute("class").Contains("is-checked"));
        }

        [TestMethod]
        public void FindsAnInlineCommandOfAnyRowWhileNoneIsSelected()
        {
            Assert.IsTrue(this.driver.IsPortalSubGridCommandUnavailable(this.invoices, "MISSING"), "The check waits for the rows' commands to load.");
            Assert.IsNotNull(this.driver.FindPortalSubGridCommand(this.invoices, "PAID"));
        }

        [TestMethod]
        public void FindsTheInlineCommandsOfTheSelectedRecordOnly()
        {
            this.driver.SelectPortalGridRecord(this.invoices.FindPortalGridRows(FirstInvoice), "the first invoice");
            this.driver.WaitForPortalCondition(() => this.invoices.FindElements(By.CssSelector("#row2-ribbon [data-command]")).Count > 0, TimeSpan.FromSeconds(5));

            Assert.IsNotNull(this.driver.FindPortalSubGridCommand(this.invoices, "PAY"));
            Assert.IsNull(this.driver.FindPortalSubGridCommand(this.invoices, "PAID"), "The second invoice's command is not the selected record's.");
        }

        [TestMethod]
        public void FindsACommandByLabelIdCommandOrIconFileName()
        {
            this.driver.SelectPortalGridRecord(this.invoices.FindPortalGridRows(FirstInvoice), "the first invoice");

            Assert.IsNotNull(this.driver.FindPortalSubGridCommand(this.invoices, "Export data"), "A ribbon command by its label.");
            Assert.IsNotNull(this.driver.FindPortalSubGridCommand(this.invoices, "talxis_downloaddocument.svg"));
            Assert.IsNotNull(this.driver.FindPortalSubGridCommand(this.invoices, "sample.invoice.subgrid.PDF.Button"));
            Assert.IsNotNull(this.driver.FindPortalSubGridCommand(this.invoices, "sample.invoice.subgrid.PDF.Command"));

            this.driver.FindPortalSubGridCommand(this.invoices, "talxis_downloaddocument.svg").Click();
            Assert.AreEqual("PDF", PortalTestPage.Clicks());
        }

        [TestMethod]
        public void TellsWhetherACommandIsAvailable()
        {
            Assert.IsTrue(this.driver.IsPortalSubGridCommandUnavailable(this.invoices, "MISSING"));
            Assert.IsTrue(this.driver.IsPortalSubGridCommandUnavailable(this.invoices, "BUSY"), "A disabled command is unavailable.");
            Assert.IsTrue(this.driver.WaitForPortalSubGridCommand(this.invoices, "LATER"), "A command the ribbon shows late is awaited.");
        }

        [TestMethod]
        public void FindsARowButtonByItsLabelOrIconName()
        {
            var rows = this.driver.FindPortalGridRows(FirstInvoice);

            Assert.IsNotNull(rows.FindPortalGridRowButton("Delete"), "By its aria-label.");
            Assert.AreEqual(string.Empty, rows.FindPortalGridRowButton("ClipboardList")?.GetAttribute("title"), "An icon-only button by its icon name.");
            Assert.IsNull(rows.FindPortalGridRowButton("it's"));
        }

        [TestMethod]
        public void UsesTheCommandsOfAllRowsOrOfTheMarkedRowInAGridWithoutSelection()
        {
            var prepayments = this.driver.FindPortalSubGrid("PrepaymentsGrid");
            Assert.IsNotNull(this.driver.FindPortalSubGridCommand(prepayments, "PAY"));
            Assert.IsNotNull(this.driver.FindPortalSubGridCommand(prepayments, "PAID"));

            this.driver.SelectPortalGridRecord(prepayments.FindPortalGridRows(SecondPrepayment), "the second prepayment");

            Assert.IsNotNull(this.driver.FindPortalSubGridCommand(prepayments, "PAID"));
            Assert.IsNull(this.driver.FindPortalSubGridCommand(prepayments, "PAY"));
            Assert.AreEqual(string.Empty, PortalTestPage.Clicks(), "The row is marked without clicking it, which would open the record.");
        }

        [TestMethod]
        public void FindsACommandInTheRowsOfARecordWithoutSelectingIt()
        {
            var rows = this.driver.FindPortalSubGrid("PrepaymentsGrid").FindPortalGridRows(SecondPrepayment);

            Assert.IsNotNull(this.driver.FindPortalRowCommand(rows, "PAID"));
            Assert.IsNull(this.driver.FindPortalRowCommand(rows, "PAY"), "The other record's command is not in its rows.");
            Assert.IsFalse(rows.Any(row => row.GetAttribute("class").Contains("ag-row-selected")), "The record stays unselected.");
        }
    }
}
