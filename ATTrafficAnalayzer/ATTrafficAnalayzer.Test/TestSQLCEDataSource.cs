using System.Collections.Generic;
using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Models.ReportConfiguration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ATTrafficAnalayzer.Test
{
    [TestClass()]
    public class TestSqlCeDataSource
    {
        private static IDataSource _dataSource;


        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _dataSource = new SqlceDataSource();
        }

        [TestInitialize]
        public void TestSqlceDataSource()
        {
            _dataSource.ClearData();
        }

        [TestMethod()]
        public void TestGetVolume()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void TestGetVolumeForTimePeriod()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void TestGetVolumes()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void TestGetTotalVolumeForDay()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void TestRemoveVolumes()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void TestGetIntersections()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void TestGetDetectorsAtIntersection()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void TestGetImportedDates()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void TestVolumesExist()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void TestVolumesExist1()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void TestVolumesExistForMonth()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void TestGetMostRecentImportedDate()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void TestGetConfiguration()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void TestGetApproaches()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void TestGetSummaryConfig()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void TestGetSummaryNames()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void TestAddConfiguration()
        {
            var testApproach = new Approach("test_approach", new List<int>{1, 2, 3}, _dataSource);
            var config = new Configuration("test_config", 1234, new List<Approach>{testApproach}, _dataSource);
            _dataSource.AddConfiguration(config);
            Assert.AreEqual(1, _dataSource.GetReportNames().Count);
            Assert.AreEqual("test_config", _dataSource.GetReportNames()[0]);
        }

        [TestMethod()]
        public void TestSaveMonthlySummaryConfig()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void TestRemoveReport()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void TestRemoveSummary()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void TestSummaryExists()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void TestReportExists()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void TestGetSuspectedFaults()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void TestImportFile()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void TestVolumesTableEmpty()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void TestClearData()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void TestAddIntersection()
        {
            Assert.Fail();
        }
    }
}
