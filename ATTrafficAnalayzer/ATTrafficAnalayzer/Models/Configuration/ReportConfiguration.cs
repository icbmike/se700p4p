using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace ATTrafficAnalayzer.Models.Configuration
{
    public class ReportConfiguration
    {
        private List<Approach> _approaches;

        public List<Approach> Approaches
        {
            get { return _approaches; }
            set { _approaches = value; }
        }
        
        private int _intersection;
        private string _configName;

        public String ConfigName
        {
            get { return _configName; }
            set { _configName = value; }
        }

        public int Intersection
        {
            get { return _intersection; }
            set { _intersection = value; }
        }


        public ReportConfiguration(string configName, int intersection, List<Approach> approaches)
        {
             this._configName = configName;
             this._intersection = intersection;
             this._approaches = approaches;
        }

        public JObject ToJson()
        {
            var json = new JObject();
            json.Add("intersection", Intersection);
            var arr = new JArray();
            json.Add("approaches", arr); // Add an empty array that will be filled in later with approach IDs once we know them
           
            return json;
        }
    }
}
