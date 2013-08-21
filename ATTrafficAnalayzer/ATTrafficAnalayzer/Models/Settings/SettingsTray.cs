using System;

namespace ATTrafficAnalayzer.Models.Settings
{
    public class SettingsTray
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int SummaryAmPeak { get; set; }
        public int SummaryPmPeak { get; set; }
        public int Interval { get; set; }
    }
}
