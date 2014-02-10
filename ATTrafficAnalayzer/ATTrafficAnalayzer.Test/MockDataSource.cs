using System;
using System.Collections.Generic;
using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Models.ReportConfiguration;

namespace ATTrafficAnalayzer.Test
{
    public class MockDataSource : IDataSource
    {
        public int DummyIntersection { get; set; }

        /// <summary>
        /// Constructor for our dataSource used in testing
        /// </summary>
        /// <param name="dummyIntersection">Intersection used to check against if a "different" intersection has been used</param>
        public MockDataSource(int dummyIntersection)
        {
            DummyIntersection = dummyIntersection;
        }

        public int GetVolume(int intersection, int detector, DateTime dateTime)
        {
            throw new NotImplementedException();
        }

        public int GetTotalVolumeForTimePeriod(int intersection, IList<int> detectorList, DateTime startDateTime, DateTime endDateTime)
        {
            throw new NotImplementedException();
        }

        public List<int> GetVolumes(int intersection, int detector, DateTime startDate, DateTime endDate)
        {
            var volumes = new List<int>();
            var i = 0;
            for (var d = startDate; d < endDate; d = d.AddMinutes(5))
            {
                volumes.Add(i++);
            }

            //Ensure AM and PM peaks
            volumes[20] = 450; //1:40am
            volumes[250] = 350; //20:50pm

            if (detector == 3)
            {
                volumes[20] = 700;
                volumes[250] = 600;
            }
           
            return volumes;
        }

        public int GetTotalVolumeForDay(DateTime date, int intersection, List<int> detectors)
        {
            throw new NotImplementedException();
        }

        public void RemoveVolumes(DateTime date)
        {
            throw new NotImplementedException();
        }

        public List<int> GetIntersections()
        {
            throw new NotImplementedException();
        }

        public List<int> GetDetectorsAtIntersection(int intersection)
        {
            throw new NotImplementedException();
        }

        public List<DateTime> GetImportedDates()
        {
            throw new NotImplementedException();
        }

        public bool VolumesExistForDateRange(DateTime startDate, DateTime endDate)
        {
            throw new NotImplementedException();
        }

        public bool VolumesExistForDateRange(DateTime startDate, DateTime endDate, int intersection)
        {
            throw new NotImplementedException();
        }

        public bool VolumesExistForMonth(int month)
        {
            throw new NotImplementedException();
        }

        public DateTime GetMostRecentImportedDate()
        {
            throw new NotImplementedException();
        }

        public ReportConfiguration GetConfiguration(string name)
        {
            throw new NotImplementedException();
        }

        public List<Approach> GetApproaches(string configName)
        {
            throw new NotImplementedException();
        }

        public SummaryConfiguration GetSummaryConfig(string name)
        {
            throw new NotImplementedException();
        }

        public List<string> GetSummaryNames()
        {
            throw new NotImplementedException();
        }

        public List<string> GetConfigurationNames()
        {
            throw new NotImplementedException();
        }

        public void AddConfiguration(ReportConfiguration config)
        {
            throw new NotImplementedException();
        }

        public void SaveMonthlySummaryConfig(string configName, IEnumerable<SummaryRow> rows)
        {
            throw new NotImplementedException();
        }

        public void RemoveConfiguration(string name)
        {
            throw new NotImplementedException();
        }

        public void RemoveSummary(string name)
        {
            throw new NotImplementedException();
        }

        public bool SummaryExists(string name)
        {
            throw new NotImplementedException();
        }

        public bool ConfigurationExists(string name)
        {
            throw new NotImplementedException();
        }

        public Dictionary<int, List<int>> GetSuspectedFaults(DateTime startDate, DateTime endDate, int threshold)
        {
            throw new NotImplementedException();
        }

        public void ImportFile(string filename, Action<int> updateProgress, Func<DuplicatePolicy> getDuplicatePolicy)
        {
            throw new NotImplementedException();
        }

        public bool VolumesExist()
        {
            throw new NotImplementedException();
        }

        public void ClearData()
        {
            throw new NotImplementedException();
        }

        public void AddIntersection(int intersection, IEnumerable<int> detectors)
        {
            throw new NotImplementedException();
        }

        public List<string> GetRedLightRunningConfigurationNames()
        {
            throw new NotImplementedException();
        }

        public void RemoveRedLightRunningConfiguration(string name)
        {
            throw new NotImplementedException();
        }

        public RedLightRunningConfiguration GetRedLightRunningConfiguration(string name)
        {
            throw new NotImplementedException();
        }

        public void SaveRedLightRunningConfiguration(RedLightRunningConfiguration configuration)
        {
            throw new NotImplementedException();
        }
    }
}