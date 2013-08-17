﻿using System;

namespace ATTrafficAnalayzer.Models.Settings
{
    public class SettingsTray
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int SummaryMonth { get; set; }
        public int SummaryYear { get; set; }
        public int Interval { get; set; }
    }
}
