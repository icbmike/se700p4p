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

            return dataTable;

        }

    }
}
