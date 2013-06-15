using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ATTrafficAnalayzer.VolumeModel
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

        public int Intersection
        {
            get { return _intersection; }
            set { _intersection = value; }
        }


        public ReportConfiguration(int intersection, List<Approach> approaches)
        {
            _intersection = intersection;
            _approaches = approaches;
        }

    }
}
