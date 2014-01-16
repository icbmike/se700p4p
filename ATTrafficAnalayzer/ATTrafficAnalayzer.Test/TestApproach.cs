using System;
using System.Collections.Generic;
using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Models.ReportConfiguration;
using ATTrafficAnalayzer.Models.Settings;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ATTrafficAnalayzer.Test
{
    [TestClass]
    public class TestApproach
    {
        private IDataSource _mockDataSource;
        private DateSettings _dateSettings;
        private int _dummyIntersection;

        [TestInitialize]
        public void TestInitialize()
        {
            _dummyIntersection = 4000;
            _mockDataSource = new MockDataSource(_dummyIntersection);
            _dateSettings = new DateSettings
                {
                    Interval = 5,
                    StartDate = new DateTime(2013, 3, 11),
                    EndDate = new DateTime(2013, 3, 12)
                }; 
        }

        [TestMethod]
        public void TestToString()
        {
            var testApproach = new Approach("Test Approach", new List<int> { 1 }, _mockDataSource);
            var toString = testApproach.ToString();
            Assert.AreEqual("Test Approach", toString);
        }



        [TestMethod]
        public void TestGetTotal()
        {
            var testApproach = new Approach("Test Approach", new List<int> {1}, _mockDataSource);
            var total = testApproach.GetTotal(_dateSettings, _dummyIntersection, 0);
            Assert.AreEqual(41858, total);
        }

        [TestMethod]
        public void TestInvalidate()
        {
            var testApproach = new Approach("Test Approach", new List<int> {1}, _mockDataSource);
            //Getting the peak will force the approach to load a datatable
            var peak = testApproach.GetPeak(_dateSettings, _dummyIntersection, 0);
            Assert.AreEqual(450, peak);
            
            var newPeak = testApproach.GetPeak(_dateSettings, _dummyIntersection + 1, 0);
            Assert.AreEqual(peak, newPeak);

            testApproach.Invalidate();

            //Provide a different intersection, simulates a change that in real application would get you different data
            newPeak = testApproach.GetPeak(_dateSettings, _dummyIntersection + 1, 0); 
            Assert.AreNotEqual(peak, newPeak);

        }

        [TestMethod]
        public void TestGetPeakOneDetector()
        {
            var testApproach = new Approach("Test Approach", new List<int> {1}, _mockDataSource);
            var peak = testApproach.GetPeak(_dateSettings, _dummyIntersection, 0);
            Assert.AreEqual(450, peak);
        }

        [TestMethod]
        public void TestGetPeakTime()
        {
            var testApproach = new Approach("Test Approach", new List<int> {1, 2, 3}, _mockDataSource);
            var peakTime = testApproach.GetPeakTime(_dateSettings, _dummyIntersection, 0);
            Assert.AreEqual(new DateTime(2013, 3, 11, 1, 40, 0, 0), peakTime);
        }

        [TestMethod]
        public void TestGetAmPeakOneDetector()
        {
            var testApproach = new Approach("Test Approach", new List<int> {1}, _mockDataSource);
            var peak = testApproach.GetAmPeak(_dateSettings, _dummyIntersection, 0);
            Assert.AreEqual(450, peak);
        }

        [TestMethod]
        public void TestGetAmPeakTime()
        {
            var testApproach = new Approach("Test Approach", new List<int> {1}, _mockDataSource);
            var amPeakTime = testApproach.GetAmPeakTime(_dateSettings, _dummyIntersection, 0);
            Assert.AreEqual(new DateTime(2013, 3, 11, 1, 40, 0, 0), amPeakTime);
        }

        [TestMethod]
        public void TestGetPmPeakOneDetector()
        {
            var testApproach = new Approach("Test Approach", new List<int> {1}, _mockDataSource);
            var peak = testApproach.GetPmPeak(_dateSettings, _dummyIntersection, 0);
            Assert.AreEqual(350, peak);
        }

        [TestMethod]
        public void TestGetPmPeakTime()
        {
            var testApproach = new Approach("Test Approach", new List<int> {1, 2, 3}, _mockDataSource);
            var pmPeakTime = testApproach.GetPmPeakTime(_dateSettings, _dummyIntersection, 0);
            Assert.AreEqual(new DateTime(2013, 3, 11, 20, 50, 0, 0), pmPeakTime);
        }
    }
}