﻿using System;
using System.Collections.Generic;
using System.Data.SQLite;
using ATTrafficAnalayzer.Models.ReportConfiguration;
using ATTrafficAnalayzer.Modes;

namespace ATTrafficAnalayzer.Models
{
    public interface IDataSource
    {
        #region Volume Related Methods

        int GetTotalVolumeForTimePeriod(int intersection, IList<int> detectorList, DateTime startDateTime, DateTime endDateTime);

        List<int> GetVolumes(int intersection, int detector, DateTime startDate, DateTime endDate);
        
        int GetTotalVolumeForDay(DateTime date, int intersection, List<int> detectors);

        void RemoveVolumes(DateTime date);

        Boolean VolumesExistForDateRange(DateTime startDate, DateTime endDate);
        
        Boolean VolumesExistForDateRange(DateTime startDate, DateTime endDate, int intersection);
        
        bool VolumesExistForMonth(int month);

        /// <summary>
        ///     Confirms if there is no data in the volumes table
        /// </summary>
        /// <returns>True if there is no data</returns>
        bool VolumesExist();
        bool VolumesExist(DateTime dateTime);

        #endregion
        
        List<int> GetIntersections();
        List<int> GetDetectorsAtIntersection(int intersection);
        
        List<DateTime> GetImportedDates();
        DateTime GetMostRecentImportedDate();

        #region Configuration Related Methods
        
        ReportConfiguration.ReportConfiguration GetConfiguration(string name);
        
        SummaryConfiguration GetSummaryConfig(string name);

        List<String> GetSummaryNames();
        
        List<String> GetConfigurationNames();

        void AddConfiguration(ReportConfiguration.ReportConfiguration config);
        
        void SaveMonthlySummaryConfig(string configName, IEnumerable<SummaryRow> rows);

        void RemoveConfiguration(String name);
        
        void RemoveSummary(String name);

        bool SummaryExists(String name);
        
        bool ConfigurationExists(String name);

        #endregion

        #region Faults Related Methods
        Dictionary<int, List<int>> GetSuspectedFaults(DateTime startDate, DateTime endDate, int threshold);
        #endregion

        /// <summary>
        ///     Imports a single file
        /// </summary>
        /// <param name="filename">filename to import</param>
        /// <param name="updateProgress"></param>
        /// <param name="getDuplicatePolicy"></param>
        /// <param name="b">Worker thread</param>
        /// <param name="w"></param>
        /// <returns></returns>
        void ImportFile(string filename, Action<int> updateProgress, Func<DuplicatePolicy> getDuplicatePolicy);

        /// <summary>
        /// Clears the data source
        /// </summary>
        void ClearData();

        void AddIntersection(int intersection, IEnumerable<int> detectors);

        List<String> GetRedLightRunningConfigurationNames();
        void RemoveRedLightRunningConfiguration(string name);
        RedLightRunningConfiguration GetRedLightRunningConfiguration(string name);
        void SaveRedLightRunningConfiguration(RedLightRunningConfiguration configuration);
        int GetTotalVolumeForDay(DateTime date, int intersection);

        int GetVolumeForTimePeriod(int intersection, IList<int> detectorList, DateTime startDateTime,
            DateTime endDateTime);


    }
}