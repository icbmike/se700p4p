using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json.Linq;

namespace ATTrafficAnalayzer.Models.ReportConfiguration
{
    public class SummaryRow
    {

        /// <summary>
        ///     Creates a summary row
        /// </summary>
        public SummaryRow()
        {
            DetectorsIn = new List<int>();
            DetectorsOut = new List<int>();
            Intersections = new ObservableCollection<int>();
        }

        /// <summary>
        ///     Creates a summary row
        /// </summary>
        /// <param name="routeName">Name of the summary</param>
        /// <param name="intersectionIn">Name of the inbound intersection</param>
        /// <param name="intersectionOut">Name of the outbound intersection</param>
        /// <param name="detectorsIn">List of inbound detectors</param>
        /// <param name="detectorsOut">List of outbound detectors</param>
        /// <param name="dividingFactorIn">Dividing factor inbound</param>
        /// <param name="dividingFactorOut">Dividing factor outbound</param>
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
        /// <summary>
        ///     Collection of intersections
        /// </summary>
        public ObservableCollection<int> Intersections { get; set; }

        /// <summary>
        ///     Checks if the summary configuration is valid
        /// </summary>
        public bool IsValid
        {
            get { return SelectedIntersectionIn != 0 && SelectedIntersectionOut != 0 && !RouteName.Equals(""); }
        }

        /// <summary>
        ///     Summary row properties
        /// </summary>
        public string RouteName { get; set; }
        public int SelectedIntersectionIn { get; set; }
        public int SelectedIntersectionOut { get; set; }
        public List<int> DetectorsIn { get; set; }
        public List<int> DetectorsOut { get; set; }
        public int DividingFactorIn { get; set; }
        public int DividingFactorOut { get; set; }

        /// <summary>
        ///     Returns a plain text value for the summary row
        /// </summary>
        /// <returns>A string detailing the info of the summary row</returns>
        public override string ToString()
        {
            return "RouteName: " + RouteName + " Intersection In: " + SelectedIntersectionIn + " Intersection Out: " +
                   SelectedIntersectionOut +
                   " Detectors In: " + DetectorsIn + " Detectors Out: " + DetectorsOut;
        }

        /// <summary>
        ///     Outputs a JSON object for the summary row
        /// </summary>
        /// <returns>JSON object for the summary row</returns>
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

        /// <summary>
        ///     Outputs a list of outbound detectors
        /// </summary>
        /// <returns>String of detectors</returns>
        public string GetDetectorsInAsString()
        {
            return String.Join(", ", DetectorsIn);
        }

        /// <summary>
        ///     Outputs a list of inbound detectors
        /// </summary>
        /// <returns>String of detectors</returns>
        public string GetDetectorsOutAsString()
        {
            return String.Join(", ", DetectorsOut);
        }
    }
}