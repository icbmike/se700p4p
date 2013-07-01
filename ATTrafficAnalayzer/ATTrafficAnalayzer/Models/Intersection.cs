using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ATTrafficAnalayzer.VolumeModel
{
    class Intersection : AbstractApproach
    {
        private List<int> _detectors = new List<int>();

        public List<int> Detectors
        {
            get { return _detectors; }
            set { _detectors = value; }
        }

        public void addDetector(int detector) {
            _detectors.Add(detector);
        }

        public void removeDetector(int detector)
        {
            _detectors.Remove(detector);
        }

        public void clearDetectors()
        {
            _detectors = new List<int>(); 
        }

        public int getVolume()
        {
            //TODO for each detector query the database and return the sum
            return 0;
        }
    }
}
