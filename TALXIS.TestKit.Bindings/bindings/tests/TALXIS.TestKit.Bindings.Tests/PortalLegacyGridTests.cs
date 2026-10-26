namespace TALXIS.TestKit.Bindings.Tests
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OpenQA.Selenium;
    using TALXIS.TestKit.Bindings.Extensions;

    /// <summary>
    /// Subgrids shown with the legacy view (Fluent DetailsList): plain, grouped with collapsed groups, and aggregated.
    /// </summary>
    [TestClass]
    public class PortalLegacyGridTests
    {
        private static readonly TimeSpan GroupRenderTimeout = TimeSpan.FromSeconds(5);

        private IWebDriver driver = null!;

        [TestInitialize]
        public void OpenPage() => this.driver = PortalTestPage.Open();

        [TestMethod]
        public void FindsTheRowOfARecordByAValue()
        {
            var equipment = this.driver.FindPortalSubGrid("EquipmentGrid");

            Assert.AreEqual(2, equipment.CountPortalGridRecords());
            Assert.AreEqual(1, equipment.FindPortalGridRowsByValue("Equipment", "Bin 120 l").Count, "By the column's label.");
            Assert.AreEqual(1, equipment.FindPortalGridRowsByValue("sample_equipmentid", "Container 1100 l").Count, "By the column's logical name.");
        }

        [TestMethod]
        public void FindsTheCommandsOfTheSelectedRow()
        {
            var equipment = this.driver.FindPortalSubGrid("EquipmentGrid");

            this.driver.SelectPortalGridRecord(equipment.FindPortalGridRowsByValue("Equipment", "Container 1100 l"), "the container");

            Assert.IsNotNull(this.driver.FindPortalSubGridCommand(equipment, "other.svg"));
            Assert.IsNull(this.driver.FindPortalSubGridCommand(equipment, "info.svg"));
        }

        [TestMethod]
        public void FindsACommandInTheRowOfARecordFoundByAValue()
        {
            var rows = this.driver.FindPortalSubGrid("EquipmentGrid").FindPortalGridRowsByValue("Equipment", "Container 1100 l");

            Assert.IsNotNull(this.driver.FindPortalRowCommand(rows, "other.svg"));
            Assert.IsNull(this.driver.FindPortalRowCommand(rows, "info.svg"));
        }

        [TestMethod]
        public void ExpandsTheGroupThatShowsAValueAndUsesItsRows()
        {
            var stops = this.driver.FindPortalSubGrid("StopsGrid");

            Assert.IsTrue(this.driver.WaitForPortalCondition(() => stops.FindPortalGridRowsByValue("Address", "2 Main Street").Count > 0, GroupRenderTimeout));
            Assert.AreEqual(1, stops.FindPortalGridRowsByValue("Address", "2 Main Street").Count, "The expanded group is not expanded again.");
            Assert.AreEqual(0, stops.FindPortalGridRowsByValue("Address", "3 Main Street").Count);
        }

        [TestMethod]
        public void FindsTheCommandsOfTheMarkedRowOfAGroup()
        {
            var stops = this.driver.FindPortalSubGrid("StopsGrid");
            this.driver.WaitForPortalCondition(() => stops.FindPortalGridRowsByValue("Address", "2 Main Street").Count > 0, GroupRenderTimeout);
            this.driver.WaitForPortalCondition(() => stops.FindPortalGridRowsByValue("Address", "1 Main Street").Count > 0, GroupRenderTimeout);

            this.driver.SelectPortalGridRecord(stops.FindPortalGridRowsByValue("Address", "1 Main Street"), "the first stop");

            Assert.IsNotNull(this.driver.FindPortalSubGridCommand(stops, "info.svg"));
            Assert.IsNull(this.driver.FindPortalSubGridCommand(stops, "other.svg"));
        }

        [TestMethod]
        public void UsesTheHeaderOfAnAggregatedGroupThatCannotBeExpanded()
        {
            var aggregated = this.driver.FindPortalSubGrid("AggregatedGrid");
            var rows = aggregated.FindPortalGridRowsByValue("Equipment", "Bin 120 l");

            Assert.AreEqual(1, rows.Count);
            StringAssert.Contains(rows[0].GetAttribute("class"), "ms-GroupHeader");
            Assert.IsNotNull(this.driver.FindPortalSubGridCommand(aggregated, "other.svg"), "Without a marked row, the commands of all headers count.");

            this.driver.SelectPortalGridRecord(rows, "the bin");

            Assert.IsNotNull(this.driver.FindPortalSubGridCommand(aggregated, "info.svg"));
            Assert.IsNull(this.driver.FindPortalSubGridCommand(aggregated, "other.svg"));
        }
    }
}
