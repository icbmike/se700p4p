using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ATTrafficAnalayzer.VolumeModel
{
    class Route : AbstractApproach
    {
        private List<Intersection> _intersections = new List<Intersection>();

        public List<Intersection> Intersections
        {
            get { return _intersections; }
            set { _intersections = value; }
        }

        public void addIntersection(Intersection intersection)
        {
            _intersections.Add(intersection);
        }

        public void removeIntersection(Intersection Intersection)
        {
            _intersections.Remove(Intersection);
        }

        public void clearIntersections()
        {
            _intersections = new List<Intersection>();
        }

        public int getVolume()
        {
            //TODO for each intersection query the database and return the sum
            return 0;
        }
    }
}
