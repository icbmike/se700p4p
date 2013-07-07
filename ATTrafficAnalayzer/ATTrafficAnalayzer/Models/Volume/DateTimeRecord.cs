using System;

namespace ATTrafficAnalayzer.Models.Volume
{
    public class DateTimeRecord
    {
        public DateTime DateTime { get; set; }
        public bool FiveMinutePeriod { get; set; }

        public DateTimeRecord(int year, int month, int day, int hour, int minutes, bool fiveMinutePeriod)
        {
            DateTime = new DateTime(year, month, day, hour, minutes, 0);
            FiveMinutePeriod = fiveMinutePeriod;
        }
    }
}
