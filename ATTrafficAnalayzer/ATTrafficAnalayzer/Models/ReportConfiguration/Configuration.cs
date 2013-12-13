using System.Collections.Generic;
using System.Data;
using Newtonsoft.Json.Linq;

namespace ATTrafficAnalayzer.Models.ReportConfiguration
{
    public class Configuration
    {
        public List<Approach> Approaches { get; set; }
        public string ConfigName { get; set; }
        public int Intersection { get; set; }

        /// <summary>
        ///     Creates a report object
        /// </summary>
        /// <param name="configName">Name of the report</param>
        /// <param name="intersection">Intersection of the report</param>
        /// <param name="approaches">List of approaches contained in the report</param>
        /// <param name="dataSource"></param>
        public Configuration(string configName, int intersection, List<Approach> approaches, IDataSource dataSource)
        {
            ConfigName = configName;
            Intersection = intersection;
            Approaches = approaches;
        }

        /// <summary>
        ///     Data table to configure the summary
        /// </summary>
        /// <returns>Summary data table</returns>
        public DataTable GetSummaryTable()
        {
            var dataTable = new DataTable();

            dataTable.Columns.Add("Route Name", typeof(string));
            dataTable.Columns.Add("Inbound Intersections", typeof(string));
            dataTable.Columns.Add("Inbound Detectors", typeof(string));
            dataTable.Columns.Add("Inbound Dividing Factor", typeof(string));
            dataTable.Columns.Add("Outbound Intersections", typeof(string));
            dataTable.Columns.Add("Outbound Detectors", typeof(string));
            dataTable.Columns.Add("Outbound Dividing Factor", typeof(string));

            foreach (var app in Approaches)
            {
                var newRow = dataTable.NewRow();
                newRow["Route Name"] = app.Name;
                newRow["Inbound Intersections"] = Intersection;
                newRow["Inbound Detectors"] = string.Join(", ", app.Detectors);
                newRow["Inbound Dividing Factor"] = "";
                newRow["Outbound Intersections"] = "";
                newRow["Outbound Detectors"] = "";
                newRow["Outbound Dividing Factor"] = "";
                dataTable.Rows.Add(newRow);
            }

            return dataTable;
        }

        public void Invalidate()
        {
            throw new System.NotImplementedException();
        }
    }
}
