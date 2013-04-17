using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ATTrafficAnalayzer
{
    public class VolumeRecord
    {
        private int _intersectionNumber;
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

        public int GetVolumeForDetector(int detector)
        {
            return _detectorVolumeDict[detector];
        }
    }
}
