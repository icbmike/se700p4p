using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ATTrafficAnalayzer.Models.ReportConfiguration
{
    public class SummaryConfiguration
    {
        public SummaryConfiguration()
        {
            SummaryRows = new List<SummaryRow>();
        }

        public string Name { get; set; }
        public List<SummaryRow> SummaryRows { get; set; } 

    }
}
