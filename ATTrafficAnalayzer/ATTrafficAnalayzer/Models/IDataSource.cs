using System;
using System.Collections.Generic;
using ATTrafficAnalayzer.Models.Configuration;

namespace ATTrafficAnalayzer.Models
{
    public interface IDataSource
    {

        //Volume Related methods
        int GetVolume(int intersection, int detector, DateTime dateTime);
        int GetVolumeForTimePeriod(int intersection, IList<int> detectorList, DateTime startDateTime, DateTime endDateTime);
        List<int> GetVolumes(int intersection, int detector, DateTime startDate, DateTime endDate);
        int GetTotalVolumeForDay(DateTime date, int intersection, List<int> detectors);
        
        bool RemoveVolumes(DateTime date);
        List<int> GetIntersections();
        List<int> GetDetectorsAtIntersection(int intersection);
        List<DateTime> GetImportedDates();

        Boolean VolumesExist(DateTime startDate, DateTime endDate);
        Boolean VolumesExist(DateTime startDate, DateTime endDate, int intersection);
        bool VolumesExistForMonth(int month);
       
        //Configuration Related Methods
        Report GetConfiguration(string name);
        List<Approach> GetApproaches(String configName);
        IEnumerable<SummaryRow> GetSummaryConfig(string name);

        List<String> GetSummaryNames();
        List<String> GetReportNames();
        
        void AddConfiguration(Report config);
        void SaveMonthlySummaryConfig(string configName, IEnumerable<SummaryRow> rows);

        bool SummaryExists(String name);
        bool ReportExists(String name);

        //Faults Related Methods
        Dictionary<int, List<int>> GetSuspectedFaults(DateTime startDate, DateTime endDate, int threshold);
    }
}