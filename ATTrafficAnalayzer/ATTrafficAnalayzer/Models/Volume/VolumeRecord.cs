using System.Collections.Generic;
using System.Linq;

namespace ATTrafficAnalayzer.Models.Volume
{
    public class VolumeRecord
    {
        public int IntersectionNumber { get; set; }

        private readonly Dictionary<int, int> _detectorVolumeDict;

        public VolumeRecord(int intersectionNumber)
        {
            IntersectionNumber = intersectionNumber;
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
