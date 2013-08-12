using System.Collections.Generic;
using System.Data;
using Newtonsoft.Json.Linq;

namespace ATTrafficAnalayzer.Models.Configuration
{
    public class Report
    {
        public List<Approach> Approaches { get; set; }
        public string ConfigName { get; set; }
        public int Intersection { get; set; }

        public Report(string configName, int intersection, List<Approach> approaches)
        {
            ConfigName = configName;
            Intersection = intersection;
            Approaches = approaches;
        }

        public JObject ToJson()
        {
            var json = new JObject { { "intersection", Intersection } };
            var array = new JArray();
            json.Add("approaches", array); // Add an empty array that will be filled in later with approach IDs once we know them

            return json;
        }

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
                newRow["Inbound Detectors"] = app.GetDetectorsAsString();
                newRow["Inbound Dividing Factor"] = "";
                newRow["Outbound Intersections"] = "";
                newRow["Outbound Detectors"] = "";
                newRow["Outbound Dividing Factor"] = "";
                dataTable.Rows.Add(newRow);
            }

            return dataTable;
        }
    }
}
