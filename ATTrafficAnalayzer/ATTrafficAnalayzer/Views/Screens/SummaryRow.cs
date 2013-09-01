using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ATTrafficAnalayzer.Models;
using Newtonsoft.Json.Linq;

namespace ATTrafficAnalayzer.Views.Screens
{
    public class SummaryRow
    {
        public SummaryRow()
        {
            DetectorsIn = new List<int>();
            DetectorsOut = new List<int>();
        }

        public SummaryRow(string routeName, int intersectionIn, int intersectionOut, List<int> detectorsIn,
            List<int> detectorsOut, int dividingFactorIn, int dividingFactorOut)
        {
            RouteName = routeName;
            SelectedIntersectionIn = intersectionIn;
            SelectedIntersectionOut = intersectionOut;
            DetectorsIn = detectorsIn;
            DetectorsOut = detectorsOut;
            DividingFactorIn = dividingFactorIn;
            DividingFactorOut = dividingFactorOut;
        }

        private ObservableCollection<int> _intersections;

        public ObservableCollection<int> Intersections
        {
            get {
                return _intersections ?? (_intersections = new ObservableCollection<int>(DbHelper.GetIntersections()));
            }
        }

        public bool IsValid
        {
            get { return SelectedIntersectionIn != 0 && SelectedIntersectionOut != 0 && !RouteName.Equals(""); }
        }

        public string RouteName { get; set; }
        public int SelectedIntersectionIn { get; set; }
        public int SelectedIntersectionOut { get; set; }
        public List<int> DetectorsIn { get; set; }
        public List<int> DetectorsOut { get; set; }
        public int DividingFactorIn { get; set; }
        public int DividingFactorOut { get; set; }

        public override string ToString()
        {
            return "RouteName: " + RouteName + " Intersection In: " + SelectedIntersectionIn + " Intersection Out: " +
                   SelectedIntersectionOut +
                   " Detectors In: " + DetectorsIn + " Detectors Out: " + DetectorsOut;
        }

        public JObject ToJson()
        {
            return new JObject {{"route_name", RouteName}, 
                                    {"intersection_in", SelectedIntersectionIn},
                                    {"intersection_out", SelectedIntersectionOut},
                                    {"detectors_in", new JArray(DetectorsIn.ToArray())},
                                    {"detectors_out", new JArray(DetectorsOut.ToArray())},
                                    {"div_factor_in", DividingFactorIn},
                                    {"div_factor_out", DividingFactorOut}
                                };
        }

        public string GetDetectorsInAsString()
        {
            return String.Join(", ", DetectorsIn);
        }

        public string GetDetectorsOutAsString()
        {
            return String.Join(", ", DetectorsOut);
        }
    }
}