using System;
using System.Collections.Generic;
using System.ComponentModel;
using ATTrafficAnalayzer.Models.Configuration;

namespace ATTrafficAnalayzer.Models
{
    public interface IDataSource
    {

        #region Volume Related Methods
        
        int GetVolume(int intersection, int detector, DateTime dateTime);

        int GetVolumeForTimePeriod(int intersection, IList<int> detectorList, DateTime startDateTime, DateTime endDateTime);

        List<int> GetVolumes(int intersection, int detector, DateTime startDate, DateTime endDate);
        
        int GetTotalVolumeForDay(DateTime date, int intersection, List<int> detectors);

        bool RemoveVolumes(DateTime date);

        Boolean VolumesExist(DateTime startDate, DateTime endDate);
        
        Boolean VolumesExist(DateTime startDate, DateTime endDate, int intersection);
        
        bool VolumesExistForMonth(int month);

        /// <summary>
        ///     Confirms if there is no data in the volumes table
        /// </summary>
        /// <returns>True if there is no data</returns>
        bool VolumesTableEmpty();

        #endregion
        
        List<int> GetIntersections();
        List<int> GetDetectorsAtIntersection(int intersection);
        
        List<DateTime> GetImportedDates();
        DateTime GetMostRecentImportedDate();

        #region Configuration Related Methods
        
        Configuration.Configuration GetConfiguration(string name);
        
        List<Approach> GetApproaches(String configName);
        
        IEnumerable<SummaryRow> GetSummaryConfig(string name);

        List<String> GetSummaryNames();
        
        List<String> GetReportNames();

        void AddConfiguration(Configuration.Configuration config);
        
        void SaveMonthlySummaryConfig(string configName, IEnumerable<SummaryRow> rows);

        void RemoveReport(String name);
        
        void RemoveSummary(String name);

        bool SummaryExists(String name);
        
        bool ReportExists(String name);

        #endregion

        #region Faults Related Methods
        Dictionary<int, List<int>> GetSuspectedFaults(DateTime startDate, DateTime endDate, int threshold);
        #endregion
        
        
        /// <summary>
        ///     Imports a single file
        /// </summary>
        /// <param name="b">Worker thread</param>
        /// <param name="w"></param>
        /// <param name="filename">filename to import</param>
        /// <param name="updateProgress"></param>
        /// <param name="getDuplicatePolicy"></param>
        /// <returns></returns>
        DuplicatePolicy ImportFile(string filename, Action<int> updateProgress, Func<DuplicatePolicy> getDuplicatePolicy);

        /// <summary>
        /// Clears the data source
        /// </summary>
        void ClearData();

        void AddIntersection(int intersection, IEnumerable<int> detectors);

    }
}