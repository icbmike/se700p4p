using System;
using ATTrafficAnalayzer.Models.ReportConfiguration;

namespace ATTrafficAnalayzer.Views.Screens
{
    public class SummaryStatistic
    {
        public string Name { get; set; }
        public Func<DateTime, SummaryRow, int> Calculation { get; set; }

        public SummaryStatistic(string name, Func<DateTime, SummaryRow, int> calculation)
        {
            Name = name;
            Calculation = calculation;
        }
    }
}