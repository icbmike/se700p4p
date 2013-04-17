using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ATTrafficAnalayzer
{
    public class DateTimeRecord
    {
        private int year;

        public int Year
        {
            get { return year; }
            set { year = value; }
        }
        private int month;

        public int Month
        {
            get { return month; }
            set { month = value; }
        }
        private int day;

        public int Day
        {
            get { return day; }
            set { day = value; }
        }
        private int hour;

        public int Hour
        {
            get { return hour; }
            set { hour = value; }
        }
        private int minutes;

        public int Minutes
        {
            get { return minutes; }
            set { minutes = value; }
        }
        private bool fiveMinutePeriod;

        public bool FiveMinutePeriod
        {
            get { return fiveMinutePeriod; }
            set { fiveMinutePeriod = value; }
        }

        public DateTimeRecord(int year, int month, int day, int hour, int minutes, bool fiveMinutePeriod)
        {
            // TODO: Complete member initialization
            this.year = year;
            this.month = month;
            this.day = day;
            this.hour = hour;
            this.minutes = minutes;
            this.fiveMinutePeriod = fiveMinutePeriod;
        }
    }
}
