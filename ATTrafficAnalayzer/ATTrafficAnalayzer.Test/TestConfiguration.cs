﻿using System;
using System.Collections.Generic;
using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Models.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace ATTrafficAnalayzer.Test
{
    [TestClass]
    public class TestConfiguration
    {
        private int _dummyIntersection;
        private IDataSource _mockDataSource;

        [TestInitialize]
        public void TestInitialize()
        {
            _dummyIntersection = 4000;
            _mockDataSource = new MockDataSource(_dummyIntersection);
        }

        [TestMethod]
        public void TestInvalidate()
        {
            var testConfiguration = GetTestConfiguration();
            testConfiguration.Invalidate();
        }

        [TestMethod]
        public void TestToJson()
        {
            var testConfiguration = GetTestConfiguration();
            var jObject = testConfiguration.ToJson();
            var approachesJson = jObject["approaches"] as JArray;
            Assert.AreEqual(0, approachesJson.Count);
        }

        private Configuration GetTestConfiguration()
        {
            var testApproach = new Approach("Test Approach", new List<int> { 1 }, _mockDataSource);
            var testConfiguration = new Configuration("Test Config", _dummyIntersection,
                                                      new List<Approach> { testApproach });
            return testConfiguration;
        }
    }
}
