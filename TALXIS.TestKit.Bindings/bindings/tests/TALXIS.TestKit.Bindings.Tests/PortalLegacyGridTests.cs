namespace TALXIS.TestKit.Bindings.Tests
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OpenQA.Selenium;
    using TALXIS.TestKit.Bindings.Extensions;
    using TALXIS.TestKit.Selectors.WebClientManagement;

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
            var equipment = PortalGridManager.FindSubGrid(this.driver, "EquipmentGrid");

            Assert.AreEqual(2, PortalGridManager.CountRecords(equipment));
            Assert.AreEqual(1, PortalGridManager.FindRowsByValue(equipment, "Equipment", "Bin 120 l").Count, "By the column's label.");
            Assert.AreEqual(1, PortalGridManager.FindRowsByValue(equipment, "sample_equipmentid", "Container 1100 l").Count, "By the column's logical name.");
        }

        [TestMethod]
        public void FindsTheCommandsOfTheSelectedRow()
        {
            var equipment = PortalGridManager.FindSubGrid(this.driver, "EquipmentGrid");

            PortalGridManager.SelectRecord(this.driver, PortalGridManager.FindRowsByValue(equipment, "Equipment", "Container 1100 l"), "the container");

            Assert.IsNotNull(PortalGridManager.FindSubGridCommand(this.driver, equipment, "other.svg"));
            Assert.IsNull(PortalGridManager.FindSubGridCommand(this.driver, equipment, "info.svg"));
        }

        [TestMethod]
        public void FindsACommandInTheRowOfARecordFoundByAValue()
        {
            var rows = PortalGridManager.FindRowsByValue(PortalGridManager.FindSubGrid(this.driver, "EquipmentGrid"), "Equipment", "Container 1100 l");

            Assert.IsNotNull(PortalGridManager.FindRowCommand(this.driver, rows, "other.svg"));
            Assert.IsNull(PortalGridManager.FindRowCommand(this.driver, rows, "info.svg"));
        }

        [TestMethod]
        public void ExpandsTheGroupThatShowsAValueAndUsesItsRows()
        {
            var stops = PortalGridManager.FindSubGrid(this.driver, "StopsGrid");

            Assert.IsTrue(PortalPageManager.WaitFor(this.driver, () => PortalGridManager.FindRowsByValue(stops, "Address", "2 Main Street").Count > 0, GroupRenderTimeout));
            Assert.AreEqual(1, PortalGridManager.FindRowsByValue(stops, "Address", "2 Main Street").Count, "The expanded group is not expanded again.");
            Assert.AreEqual(0, PortalGridManager.FindRowsByValue(stops, "Address", "3 Main Street").Count);
        }

        [TestMethod]
        public void FindsTheCommandsOfTheMarkedRowOfAGroup()
        {
            var stops = PortalGridManager.FindSubGrid(this.driver, "StopsGrid");
            PortalPageManager.WaitFor(this.driver, () => PortalGridManager.FindRowsByValue(stops, "Address", "2 Main Street").Count > 0, GroupRenderTimeout);
            PortalPageManager.WaitFor(this.driver, () => PortalGridManager.FindRowsByValue(stops, "Address", "1 Main Street").Count > 0, GroupRenderTimeout);

            PortalGridManager.SelectRecord(this.driver, PortalGridManager.FindRowsByValue(stops, "Address", "1 Main Street"), "the first stop");

            Assert.IsNotNull(PortalGridManager.FindSubGridCommand(this.driver, stops, "info.svg"));
            Assert.IsNull(PortalGridManager.FindSubGridCommand(this.driver, stops, "other.svg"));
        }

        [TestMethod]
        public void UsesTheHeaderOfAnAggregatedGroupThatCannotBeExpanded()
        {
            var aggregated = PortalGridManager.FindSubGrid(this.driver, "AggregatedGrid");
            var rows = PortalGridManager.FindRowsByValue(aggregated, "Equipment", "Bin 120 l");

            Assert.AreEqual(1, rows.Count);
            StringAssert.Contains(rows[0].GetAttribute("class"), "ms-GroupHeader");
            Assert.IsNotNull(PortalGridManager.FindSubGridCommand(this.driver, aggregated, "other.svg"), "Without a marked row, the commands of all headers count.");

            PortalGridManager.SelectRecord(this.driver, rows, "the bin");

            Assert.IsNotNull(PortalGridManager.FindSubGridCommand(this.driver, aggregated, "info.svg"));
            Assert.IsNull(PortalGridManager.FindSubGridCommand(this.driver, aggregated, "other.svg"));
        }
    }
}
