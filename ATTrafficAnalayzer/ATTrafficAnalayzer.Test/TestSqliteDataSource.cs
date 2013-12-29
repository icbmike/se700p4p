using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Models.ReportConfiguration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ATTrafficAnalayzer.Test
{
    [TestClass]
    public class TestSqliteDataSource
    {
        private static IDataSource _dataSource;


        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _dataSource = new SqliteDataSource();
        }

        [TestInitialize]
        public void TestInitialize()
        {
            _dataSource.ClearData();
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
            _dataSource.AddIntersection(1234, new List<int>{1, 2, 3});
            _dataSource.AddIntersection(4321, new List<int> { 1, 2, 3 });
            
            var intersections = _dataSource.GetIntersections();
            
            Assert.AreEqual(2, intersections.Count);
            Assert.AreEqual(1234, intersections[0]);
            Assert.AreEqual(4321, intersections[1]);
        }

        [TestMethod()]
        public void TestGetDetectorsAtIntersection()
        {
            var testDetectors = new List<int> {1, 2, 3, 4, 5, 6};
            _dataSource.AddIntersection(1234, testDetectors);

            var detectorsAtIntersection = _dataSource.GetDetectorsAtIntersection(1234);
            
            CollectionAssert.AreEqual(testDetectors, detectorsAtIntersection);
        }

        [TestMethod()]
        public void TestGetImportedDates()
        {
            var importedDates = _dataSource.GetImportedDates();
            Assert.AreEqual(0, importedDates.Count);

            //This file only contains one day's worth of volume data
            _dataSource.ImportFile("../../../ATTrafficAnalayzer.Test/test_files/MANWST_20130311.VS", i=>{}, () => DuplicatePolicy.SkipAll);
            importedDates = _dataSource.GetImportedDates();
            Assert.AreEqual(1, importedDates.Count);

        }

        [TestMethod()]
        public void TestVolumesExist()
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
            var testConfiguration = CreateTestConfiguration();
            _dataSource.AddConfiguration(testConfiguration);
            var configuration = _dataSource.GetConfiguration(testConfiguration.Name);

            //TODO: Maybe should replace with just a straight comparison and then base validity of test from TestConfiguration suite.
            Assert.AreEqual(testConfiguration.Name, configuration.Name);
            Assert.AreEqual(testConfiguration.Intersection, configuration.Intersection);
            Assert.AreEqual(testConfiguration.Approaches.Count, configuration.Approaches.Count);
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
            var config = CreateTestConfiguration();
            _dataSource.AddConfiguration(config);
            Assert.AreEqual(1, _dataSource.GetConfigurationNames().Count);
            Assert.AreEqual("test_config", _dataSource.GetConfigurationNames()[0]);
            
        }

        [TestMethod()]
        public void TestSaveMonthlySummaryConfig()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void TestRemoveConfig()
        {
            var testConfig = CreateTestConfiguration();
            
            _dataSource.AddConfiguration(testConfig);
            Assert.AreEqual(1,_dataSource.GetConfigurationNames().Count);

            _dataSource.RemoveConfiguration(testConfig.Name);
            Assert.AreEqual(0, _dataSource.GetConfigurationNames().Count);
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
        public void TestConfigurationExists()
        {
            var config = CreateTestConfiguration();
            _dataSource.AddConfiguration(config);

            Assert.IsTrue(_dataSource.ConfigurationExists("test_config"));
        }

        [TestMethod()]
        public void TestGetSuspectedFaults()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void TestImportFile()
        {
            try
            {
                _dataSource.ImportFile("bad_filename.vs", i => { System.Console.WriteLine(i); }, () => DuplicatePolicy.SkipAll);
                Assert.Fail();
            }
            catch (FileNotFoundException e)
            {
                
            }
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            _dataSource.ImportFile("../../../ATTrafficAnalayzer.Test/test_files/MANWST_20130311.VS", i => { },
                () => DuplicatePolicy.SkipAll);
            stopwatch.Stop();
            Console.WriteLine("Took " + stopwatch.ElapsedMilliseconds + "ms");
            Assert.IsFalse(_dataSource.VolumesTableEmpty());
            Assert.AreNotEqual(0, _dataSource.GetIntersections().Count);
            Assert.AreNotEqual(0, _dataSource.GetImportedDates().Count);
        }

        [TestMethod()]
        public void TestVolumesTableEmpty()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void TestAddIntersection()
        {
            _dataSource.AddIntersection(1234, new List<int>{1, 2, 3});
            var intersections = _dataSource.GetIntersections();
            Assert.AreEqual(1, intersections.Count);
            Assert.AreEqual(3, _dataSource.GetDetectorsAtIntersection(1234).Count);
        }

        private static Configuration CreateTestConfiguration()
        {
            var testApproach = new Approach("test_approach", new List<int> {1, 2, 3}, _dataSource);
            var config = new Configuration("test_config", 1234, new List<Approach> {testApproach}, _dataSource);
            return config;
        }
    }
}

