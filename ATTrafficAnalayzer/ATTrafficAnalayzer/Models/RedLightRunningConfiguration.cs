using System.Collections.Generic;

namespace ATTrafficAnalayzer.Models
{
    public class RedLightRunningConfiguration
    {
        public RedLightRunningConfiguration()
        {
            Sites = new List<ReportConfiguration.ReportConfiguration>();
        }

        public string Name { get; set; }
        public List<ReportConfiguration.ReportConfiguration> Sites { get; set; }
    }
}