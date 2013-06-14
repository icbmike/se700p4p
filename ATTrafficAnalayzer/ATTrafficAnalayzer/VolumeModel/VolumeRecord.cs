using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ATTrafficAnalayzer
{
    public class VolumeRecord
    {
        private int _intersectionNumber;


        public int IntersectionNumber
        {
            get { return _intersectionNumber; }
            set { _intersectionNumber = value; }
        }
        private Dictionary<int, int> _detectorVolumeDict;

        public VolumeRecord(int intersectionNumber)
        {
            _intersectionNumber = intersectionNumber;
            _detectorVolumeDict = new Dictionary<int, int>();
        }
        
        public void SetVolumeForDetector(int detector, int volume)
        {
            _detectorVolumeDict.Add(detector, volume);
        }

        public List<int> GetDetectors()
        {
            return _detectorVolumeDict.Keys.ToList();
        }

        public int GetVolumeForDetector(int detector)
        {
            return _detectorVolumeDict[detector];
        }
    }
}
