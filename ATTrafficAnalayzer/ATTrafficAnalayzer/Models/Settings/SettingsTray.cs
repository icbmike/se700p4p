using System;

namespace ATTrafficAnalayzer.Models.Settings
{
    public class SettingsTray
    {
        public int Interval { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime StartDate { get; set; }

        public override string ToString()
        {
            return "Interval: " + Interval + ", Start Date: " + StartDate + ", End Date: " + EndDate; 
        }
    }

    
}
