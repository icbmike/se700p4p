using System;
using System.Collections.Generic;

namespace ATTrafficAnalayzer.Models.Configuration
{
    class DeprecatedReport
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

        public DeprecatedReport(String name)
        {
            _name = name;
        }

        public DeprecatedReport(String name, List<AbstractApproach> approaches)
        {
            _name = name;
            _approaches = approaches;
        }
    }
}
