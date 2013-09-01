using System;
using System.Collections.Generic;
using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Models.Configuration;
using ATTrafficAnalayzer.Models.Settings;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace ATTrafficAnalayzer.Test
{
    [TestClass]
    public class TestApproach
    {
        private IDataSource _mockDataSource;
        private DateSettings _dateSettings;
        private int _intersection;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockDataSource = new MockDataSource();
            _dateSettings = new DateSettings
                {
                    Interval = 5,
                    StartDate = new DateTime(2013, 3, 11),
                    EndDate = new DateTime(2013, 3, 12)
                };
            _intersection = 4000;
        }

        [TestMethod]
        public void TestGetPeakOneDetector()
        {
            var testApproach = new Approach("Test Approach", new List<int> { 1 }, _mockDataSource);
            var peak = testApproach.GetPeak(_dateSettings, _intersection, 0);
            Assert.AreEqual(450, peak);
        }

        [TestMethod]
        public void TestGetPeakTime()
        {
            var testApproach = new Approach("Test Approach", new List<int> { 1, 2, 3 }, _mockDataSource);
            var peakTime = testApproach.GetPeakTime(_dateSettings, _intersection, 0);
            Assert.AreEqual(new DateTime(2013, 3, 11, 7, 30, 0, 0), peakTime);
        }

        [TestMethod]
        public void TestGetAmPeakOneDetector()
        {
            var testApproach = new Approach("Test Approach", new List<int> { 1 }, _mockDataSource);
            var peak = testApproach.GetAmPeak(_dateSettings, _intersection, 0);
            Assert.AreEqual(350, peak);
        }

        [TestMethod]
        public void TestGetAmPeakTime()
        {
            var testApproach = new Approach("Test Approach", new List<int> { 1 }, _mockDataSource);
            var amPeakTime = testApproach.GetAmPeakTime(_dateSettings, _intersection, 0);
            Assert.AreEqual(new DateTime(2013, 3, 11, 7, 30, 0, 0), amPeakTime);
        }

        [TestMethod]
        public void TestGetPmPeakOneDetector()
        {
            var testApproach = new Approach("Test Approach", new List<int> { 1}, _mockDataSource);
            var peak = testApproach.GetPmPeak(_dateSettings, _intersection, 0);
            Assert.AreEqual(450, peak);
        }

        [TestMethod]
        public void TestGetPmPeakTime()
        {
            var testApproach = new Approach("Test Approach", new List<int> { 1, 2, 3 }, _mockDataSource);
            var pmPeakTime = testApproach.GetPmPeakTime(_dateSettings, _intersection, 0);
            Assert.AreEqual(new DateTime(2013, 3, 11, 19, 30, 0, 0), pmPeakTime);
        }


        [TestMethod]
        public void TestToJson()
        {
            var testApproach = new Approach("Test Approach", new List<int> {1, 2, 3}, _mockDataSource);
            var jObject = testApproach.ToJson();
            Assert.IsNotNull(jObject["name"]);
            Assert.AreEqual(3, (jObject["detectors"] as JArray).Count);

        }
    }
}
