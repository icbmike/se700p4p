using System;
using System.Collections.Generic;

namespace ATTrafficAnalayzer.VolumeModel
{
    class Report
    {
        private String _name;
        private List<AbstractApproach> _approaches = new List<AbstractApproach>();

        public String Name {
            get { return _name; }
            set { _name = value; }
        }

        public List<AbstractApproach> Approaches
        {
            get { return _approaches; }
            set { _approaches = value; }
        }

        public Report(String name)
        {
            _name = name;
        }

        public Report(String name, List<AbstractApproach> approaches)
        {
            _name = name;
            _approaches = approaches;
        }
    }
}
