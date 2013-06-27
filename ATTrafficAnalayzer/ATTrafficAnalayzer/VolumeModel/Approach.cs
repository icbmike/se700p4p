using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ATTrafficAnalayzer.VolumeModel
{
    public class Approach
    {
        private string _name;

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        private List<int> _detectors;

        public List<int> Detectors
        {
            get { return _detectors; }
            set { _detectors = value; }
        }

        public Approach(string approachName, List<int> detectors)
        {
            this._name = approachName;
            this._detectors = detectors;
        }

        public override string ToString()
        {
            return Name;
        }

        public JSONObject toJSON()
        {
            var json = new JSONObject();
            json.Add("name", Name);
            var arr = new JSONArray();
            foreach (int detector in Detectors)
            {
                arr.Put(detector);
            }
            json.Add("detectors", arr);
            return json;
        }

        private int _id;

        public int ID
        {
            get { return _id; }
            set
            {
                if (_id == value)
                    return;
                _id = value;
            }
        }
    }
}
