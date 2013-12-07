using System;
using ATTrafficAnalayzer.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ATTrafficAnalayzer.Test
{
    [TestClass]
    public class TestSQLCEDataSource
    {

        private IDataSource dataSource;

        [TestInitialize]
        public void TestInitialize()
        {
            dataSource = new SqlceDataSource();
        }

        [TestMethod]
        public void TestGetVolume()
        {
        }
    }
}
