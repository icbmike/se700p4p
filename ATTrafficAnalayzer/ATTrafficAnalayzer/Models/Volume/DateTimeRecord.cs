using System;
using System.Collections.Generic;
using System.Windows.Documents;

namespace ATTrafficAnalayzer.Models.Volume
{
    public class DateTimeRecord
    {
        public DateTime DateTime { get; set; }
        public bool FiveMinutePeriod { get; set; }
        public List<VolumeRecord> VolumeRecords { get; set; }

        /// <summary>
        ///     Create a 
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="day"></param>
        /// <param name="hour"></param>
        /// <param name="minutes"></param>
        /// <param name="fiveMinutePeriod"></param>
        public DateTimeRecord(int year, int month, int day, int hour, int minutes, bool fiveMinutePeriod)
        {
            DateTime = new DateTime(year, month, day, hour, minutes, 0);
            FiveMinutePeriod = fiveMinutePeriod;
            VolumeRecords = new List<VolumeRecord>();
        }
    }
}
