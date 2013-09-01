using System;
using System.Collections.Generic;
using System.ComponentModel;
using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Models.Configuration;

namespace ATTrafficAnalayzer.Test
{
    public class MockDataSource : IDataSource
    {
        public int GetVolume(int intersection, int detector, DateTime dateTime)
        {
            throw new NotImplementedException();
        }

        public int GetVolumeForTimePeriod(int intersection, IList<int> detectorList, DateTime startDateTime, DateTime endDateTime)
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
            volumes[20] = 450;
            volumes[250] = 350;
            return volumes;
        }

        public int GetTotalVolumeForDay(DateTime date, int intersection, List<int> detectors)
        {
            throw new NotImplementedException();
        }

        public bool RemoveVolumes(DateTime date)
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

        public bool VolumesExist(DateTime startDate, DateTime endDate)
        {
            throw new NotImplementedException();
        }

        public bool VolumesExist(DateTime startDate, DateTime endDate, int intersection)
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

        public Report GetConfiguration(string name)
        {
            throw new NotImplementedException();
        }

        public List<Approach> GetApproaches(string configName)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<SummaryRow> GetSummaryConfig(string name)
        {
            throw new NotImplementedException();
        }

        public List<string> GetSummaryNames()
        {
            throw new NotImplementedException();
        }

        public List<string> GetReportNames()
        {
            throw new NotImplementedException();
        }

        public void AddConfiguration(Report config)
        {
            throw new NotImplementedException();
        }

        public void SaveMonthlySummaryConfig(string configName, IEnumerable<SummaryRow> rows)
        {
            throw new NotImplementedException();
        }

        public void RemoveReport(string name)
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

        public bool ReportExists(string name)
        {
            throw new NotImplementedException();
        }

        public Dictionary<int, List<int>> GetSuspectedFaults(DateTime startDate, DateTime endDate, int threshold)
        {
            throw new NotImplementedException();
        }

        public DuplicatePolicy ImportFile(BackgroundWorker b, DoWorkEventArgs w, string filename, Action<int> updateProgress,
                                          Func<DuplicatePolicy> getDuplicatePolicy)
        {
            throw new NotImplementedException();
        }

        public bool VolumesTableEmpty()
        {
            throw new NotImplementedException();
        }
    }
}