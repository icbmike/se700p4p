using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ATTrafficAnalayzer
{
    class SettingsTray
    {
        DateTime startDate;
        DateTime endDate;
        int interval;

        public int Interval
        {
            get { return interval; }
            set { interval = value; }
        }

        public DateTime EndDate
        {
            get { return endDate; }
            set { endDate = value; }
        }
        public DateTime StartDate
        {
            get { return startDate; }
            set { startDate = value; }
        }
    }
}
