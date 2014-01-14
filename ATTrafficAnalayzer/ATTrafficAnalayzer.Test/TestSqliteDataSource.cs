using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Models.ReportConfiguration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace ATTrafficAnalayzer.Test
{
    [TestClass]
    public class TestSqliteDataSource
    {
        private static IDataSource _dataSource;
        private const string TestFile11March2013 = "../../../ATTrafficAnalayzer.Test/test_files/MANWST_20130311.VS";
        private const string TestFile12March2013 = "../../../ATTrafficAnalayzer.Test/test_files/MANWST_20130312.VS";
        private const string TestFile13March2013 = "../../../ATTrafficAnalayzer.Test/test_files/MANWST_20130313.VS";

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
        public void TestTotalGetVolumeForTimePeriod()
        {
            var totalVolumeForTimePeriod = _dataSource.GetTotalVolumeForTimePeriod(4902, new List<int>{1, 2, 3}, new DateTime(2013, 3, 11), new DateTime(2013, 3, 12));
            Assert.AreEqual(0, totalVolumeForTimePeriod);

            _dataSource.ImportFile(TestFile11March2013, i => { }, () => DuplicatePolicy.SkipAll);
            totalVolumeForTimePeriod = _dataSource.GetTotalVolumeForTimePeriod(4902, new List<int> { 1, 2, 3 }, new DateTime(2013, 3, 11), new DateTime(2013, 3, 12));
            Assert.AreEqual(4, totalVolumeForTimePeriod);
        }

        [TestMethod()]
        public void TestGetVolumes()
        {
            _dataSource.ImportFile(TestFile11March2013, i => { }, () => DuplicatePolicy.SkipAll);

            var volumes = _dataSource.GetVolumes(4902, 1, new DateTime(2013, 3, 11), new DateTime(2013, 3, 12));
            Assert.AreNotEqual(0, volumes.Count);
        }

        [TestMethod()]
        public void TestGetTotalVolumeForDay()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void TestRemoveVolumes()
        {
            _dataSource.ImportFile(TestFile11March2013, i => { }, () => DuplicatePolicy.SkipAll);

            _dataSource.RemoveVolumes(new DateTime(2013, 3, 11));
            var volumes = _dataSource.GetVolumes(4902, 1, new DateTime(2013, 3, 11), new DateTime(2013, 3, 12));
            Assert.AreEqual(0, volumes.Count);
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
            _dataSource.ImportFile(TestFile11March2013, i=>{}, () => DuplicatePolicy.SkipAll);
            importedDates = _dataSource.GetImportedDates();
            Assert.AreEqual(1, importedDates.Count);

        }

        [TestMethod()]
        public void TestVolumesExistForDateRange()
        {
            Assert.IsFalse(_dataSource.VolumesExistForDateRange(new DateTime(2013, 3, 11), new DateTime(2013, 3, 12)));
            _dataSource.ImportFile(TestFile11March2013, i => { }, () => DuplicatePolicy.SkipAll);
            Assert.IsTrue(_dataSource.VolumesExistForDateRange(new DateTime(2013, 3, 11), new DateTime(2013, 3, 12)));

        }

       

        [TestMethod()]
        public void TestVolumesExistForMonth()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void TestGetMostRecentImportedDate()
        {
            _dataSource.ImportFile(TestFile11March2013, i => { }, () => DuplicatePolicy.SkipAll);
            Assert.AreEqual(new DateTime(2013, 3, 11), _dataSource.GetMostRecentImportedDate());
            
            _dataSource.ImportFile(TestFile12March2013, i => {}, () => DuplicatePolicy.SkipAll);
            Assert.AreEqual(new DateTime(2013, 3, 12), _dataSource.GetMostRecentImportedDate());
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
            _dataSource.ImportFile(TestFile11March2013, i => {}, ()=> DuplicatePolicy.SkipAll);

            var suspectedFaults = _dataSource.GetSuspectedFaults(new DateTime(2013, 3, 11), new DateTime(2013, 3, 12), 100);
            Assert.AreNotEqual(0, suspectedFaults.Count); // I don't know how many there are, only that there are some.

            //Check that  the returned detectors actually have volumes higher than stuff.
            foreach (var intersection in suspectedFaults.Keys)
            {
                foreach (var detector in suspectedFaults[intersection])
                {
                    var volumes = _dataSource.GetVolumes(intersection, detector, new DateTime(2013, 3, 11), new DateTime(2013, 3, 12));
                    Assert.IsTrue(volumes.Max() >= 100);
                }
            }
            
            suspectedFaults = _dataSource.GetSuspectedFaults(new DateTime(2013, 3, 11), new DateTime(2013, 3, 12), 3000); //Max value is 2053 i think, cant quite remember
            Assert.AreEqual(0, suspectedFaults.Count); // I don't know how many there are, only that there are some.
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
            _dataSource.ImportFile(TestFile11March2013, i => { },
                () => DuplicatePolicy.SkipAll);
            stopwatch.Stop();
            Console.WriteLine("Took " + stopwatch.ElapsedMilliseconds + "ms");
            Assert.IsFalse(_dataSource.VolumesExist());
            Assert.AreNotEqual(0, _dataSource.GetIntersections().Count);
            Assert.AreNotEqual(0, _dataSource.GetImportedDates().Count);
        }

        [TestMethod()]
        public void TestVolumesExist()
        {
            Assert.IsTrue(_dataSource.VolumesExist());
            _dataSource.ImportFile(TestFile11March2013, i => { }, () => DuplicatePolicy.SkipAll);
            Assert.IsFalse(_dataSource.VolumesExist());
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

