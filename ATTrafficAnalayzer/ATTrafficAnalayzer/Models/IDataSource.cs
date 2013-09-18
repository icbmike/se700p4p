using System;
using System.Collections.Generic;
using ATTrafficAnalayzer.Models.Configuration;

namespace ATTrafficAnalayzer.Models
{
    public interface IDataSource
    {
        List<int> GetDetectorsAtIntersection(int intersection);
        int GetVolume(int intersection, int detector, DateTime dateTime);
        int GetVolumeForTimePeriod(int intersection, IList<int> detectorList, DateTime startDateTime, DateTime endDateTime);
        List<int> GetVolumes(int intersection, int detector, DateTime startDate, DateTime endDate);
        Boolean VolumesExist(DateTime startDate, DateTime endDate);
        bool RemoveVolumes(DateTime date);
        Boolean VolumesExist(DateTime startDate, DateTime endDate, int intersection);
        int GetTotalVolumeForDay(DateTime date, int intersection, List<int> detectors);
        Report GetConfiguration(string name);
        void AddConfiguration(Report config);
        List<Approach> GetApproaches(String configName);
        bool ReportExists(String name);
        void SaveMonthlySummaryConfig(string configName, IEnumerable<SummaryRow> rows);
        IEnumerable<SummaryRow> GetSummaryConfig(string name);
        bool SummaryExists(String name);
        List<DateTime> GetImportedDates();
        bool VolumesExistForMonth(int month);

        Dictionary<int, List<int>> GetSuspectedFaults(DateTime startDate, DateTime endDate,int threshold);

    }
}