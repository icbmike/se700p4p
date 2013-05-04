using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ATTrafficAnalayzer
{
    public class DateTimeRecord
    {
        private DateTime _dateTime;

        public DateTime dateTime
        {
            get { return _dateTime; }
            set { _dateTime = value; }
        } 
        private bool fiveMinutePeriod;

        public bool FiveMinutePeriod
        {
            get { return fiveMinutePeriod; }
            set { fiveMinutePeriod = value; }
        }

        public DateTimeRecord(int year, int month, int day, int hour, int minutes, bool fiveMinutePeriod)
        {
            _dateTime = new DateTime(year, month, day, hour, minutes, 0);

            this.fiveMinutePeriod = fiveMinutePeriod;
        }
    }
}
