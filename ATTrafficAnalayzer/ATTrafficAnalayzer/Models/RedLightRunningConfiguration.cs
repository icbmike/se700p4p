using System.Collections.Generic;
using System.Data;
using ATTrafficAnalayzer.Models.Settings;

namespace ATTrafficAnalayzer.Models
{
    public class RedLightRunningConfiguration
    {
        public RedLightRunningConfiguration()
        {
            Sites = new List<ReportConfiguration.ReportConfiguration>();
        }

        public string Name { get; set; }
        public List<ReportConfiguration.ReportConfiguration> Sites { get; set; }

        public DataTable GetDataTable(DateSettings dateSettings, IDataSource dataSource)
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("Site ID");
            dataTable.Columns.Add("Total Volume");
            dataTable.Columns.Add("Total Red Light Running Volume");

            foreach (var reportConfiguration in Sites)
            {
                dataTable.Rows.Add(new[] {reportConfiguration.Intersection, dataSource.GetTotalVolumeForDay(dateSettings.StartDate, reportConfiguration.Intersection), reportConfiguration.GetTotalVolume(dateSettings)});
            }
            
            return dataTable;
        }
    }
}