using System.Collections.Generic;
using System.Collections.ObjectModel;
using ATTrafficAnalayzer.Models;

namespace ATTrafficAnalayzer.Views.Screens
{
    public class SummaryRow
    {
        public SummaryRow()
        {
            DetectorsIn = new List<int>();
            DetectorsOut = new List<int>();
        }
        private ObservableCollection<int> _intersections;

        public ObservableCollection<int> Intersections
        {
            get
            {
                if (_intersections == null)
                {
                    _intersections = new ObservableCollection<int>(DbHelper.GetIntersections());
                }
                return _intersections;
            }
        }
        public string RouteName { get; set; }
        public int SelectedIntersectionIn { get; set; }
        public int SelectedIntersectionOut { get; set; }
        public List<int> DetectorsIn { get; set; }
        public List<int> DetectorsOut { get; set; }

        public override string ToString()
        {
            return "RouteName: " + RouteName + " Intersection In: " + SelectedIntersectionIn + " Intersection Out: " +
                   SelectedIntersectionOut +
                   " Detectors In: " + DetectorsIn + " Detectors Out: " + DetectorsOut;
        }

    }
}