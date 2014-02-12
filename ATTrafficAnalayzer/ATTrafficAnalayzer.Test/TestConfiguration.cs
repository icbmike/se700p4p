using System;
using System.Collections.Generic;
using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Models.ReportConfiguration;
using ATTrafficAnalayzer.Models.Settings;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ATTrafficAnalayzer.Test
{
    [TestClass]
    public class TestConfiguration
    {
        private int _dummyIntersection;
        private IDataSource _mockDataSource;
        private DateSettings _dateSettings;

        private static readonly DateTime March11Th2013 = new DateTime(2013, 3, 11);
        private static readonly DateTime March12Th2013 = new DateTime(2013, 3, 12);
        private static readonly DateTime March13Th2013 = new DateTime(2013, 3, 13);


        [TestInitialize]
        public void TestInitialize()
        {
            _dummyIntersection = 4000;
            _mockDataSource = new MockDataSource(_dummyIntersection);

            _dateSettings = new DateSettings
            {
                Interval = 5,
                StartDate = March11Th2013,
                EndDate = March12Th2013
            }; 
        }

        [TestMethod]
        public void TestGetBusiestApproach()
        {
            var testConfiguration = GetTestConfigurationWithMultipleApproaches();
            var busiestApproach = testConfiguration.GetBusiestApproach(_dateSettings);
            
            Assert.AreEqual("Busiest Approach", busiestApproach.ApproachName);

        }

        [TestMethod]
        public void TestGetBusiestAMPeriod()
        {
            var testConfiguration = GetTestConfiguration();
            var amPeakPeriod = testConfiguration.GetAMPeakPeriod(_dateSettings);

            Assert.AreEqual(March11Th2013.AddHours(1).AddMinutes(40), amPeakPeriod);
        }

        [TestMethod]
        public void TestGetBusiestPMPeriod()
        {
            var testConfiguration = GetTestConfiguration();
            var pmPeakPeriod = testConfiguration.GetPMPeakPeriod(_dateSettings);

            Assert.AreEqual(March11Th2013.AddHours(20).AddMinutes(50), pmPeakPeriod);
        
        }

        [TestMethod]
        public void TestGetAMPeakVolume()
        {
            var testConfiguration = GetTestConfigurationWithMultipleApproaches();
            var amPeakVolume = testConfiguration.GetAMPeakVolume(_dateSettings);

            Assert.AreEqual(700, amPeakVolume);

        }

        [TestMethod]
        public void TestGetPMPeakVolume()
        {
            var testConfiguration = GetTestConfigurationWithMultipleApproaches();
            var pmPeakVolume = testConfiguration.GetPMPeakVolume(_dateSettings);

            Assert.AreEqual(600, pmPeakVolume);
        }

        private ReportConfiguration GetTestConfiguration()
        {
            var testApproach = new Approach("Test Approach", new List<int> { 1 }, _mockDataSource);
            var testConfiguration = new ReportConfiguration("Test Config", _dummyIntersection,
                                                      new List<Approach> { testApproach }, _mockDataSource);
            return testConfiguration;
        }

        private ReportConfiguration GetTestConfigurationWithMultipleApproaches()
        {
            return new ReportConfiguration("Test Config",
                                    _dummyIntersection,
                                    new List<Approach>
                                    {
                                        new Approach("TestApproach1", new List<int> { 1 }, _mockDataSource), 
                                        new Approach("TestApproach2", new List<int> { 2 }, _mockDataSource),
                                        new Approach("Busiest Approach", new List<int> { 3 }, _mockDataSource)
                                    }, _mockDataSource);
        }

    }
}
