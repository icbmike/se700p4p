using System;
using System.Collections.Generic;
using ATTrafficAnalayzer.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ATTrafficAnalayzer.Test
{
    [TestClass]
    public class TestSqlceDataSource
    {

        private IDataSource _dataSource;

        [TestInitialize]
        public void TestInitialize()
        {
            _dataSource = new SqlceDataSource();
        }

        [TestMethod]
        public void TestGetVolume()
        {
            //Get volume for intersection and detector that do exist
           // var volume = _dataSource.GetVolume(1, 2, DateTime.Now);

        }

        [TestMethod]
        public void TestGetVolumeForTimePeriod()
        {
            var volumeForTimePeriod = _dataSource.GetVolumeForTimePeriod(1, new List<int>(), DateTime.Now, DateTime.Now);
        }

        [TestMethod]
        public void TestGetTotalVolumeForDay()
        {
            var totalVolumeForDay = _dataSource.GetTotalVolumeForDay(DateTime.Today, 405, new List<int>());
        }

        [TestMethod]
        public void TestGetIntersections()
        {
            var intersections = _dataSource.GetIntersections();
            Assert.AreEqual(1, intersections.Count);
        }
    }
}
