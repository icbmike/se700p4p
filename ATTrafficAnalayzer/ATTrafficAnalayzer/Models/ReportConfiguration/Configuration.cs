using System;
using System.Collections.Generic;
using System.Data;
using ATTrafficAnalayzer.Models.Settings;

namespace ATTrafficAnalayzer.Models.ReportConfiguration
{
    public class Configuration
    {
        public List<Approach> Approaches { get; set; }
        public string Name { get; set; }
        public int Intersection { get; set; }

        /// <summary>
        ///     Creates a report object
        /// </summary>
        /// <param name="name">Name of the report</param>
        /// <param name="intersection">Intersection of the report</param>
        /// <param name="approaches">List of approaches contained in the report</param>
        /// <param name="dataSource"></param>
        public Configuration(string name, int intersection, List<Approach> approaches, IDataSource dataSource)
        {
            Name = name;
            Intersection = intersection;
            Approaches = approaches;
        }

        /// <summary>
        ///     Data table to configure the summary
        /// </summary>
        /// <returns>Summary data table</returns>
        public DataTable GetSummaryTable()
        {
            var dataTable = new DataTable();

            dataTable.Columns.Add("Route Name", typeof(string));
            dataTable.Columns.Add("Inbound Intersections", typeof(string));
            dataTable.Columns.Add("Inbound Detectors", typeof(string));
            dataTable.Columns.Add("Inbound Dividing Factor", typeof(string));
            dataTable.Columns.Add("Outbound Intersections", typeof(string));
            dataTable.Columns.Add("Outbound Detectors", typeof(string));
            dataTable.Columns.Add("Outbound Dividing Factor", typeof(string));

            foreach (var app in Approaches)
            {
                var newRow = dataTable.NewRow();
                newRow["Route Name"] = app.Name;
                newRow["Inbound Intersections"] = Intersection;
                newRow["Inbound Detectors"] = string.Join(", ", app.Detectors);
                newRow["Inbound Dividing Factor"] = "";
                newRow["Outbound Intersections"] = "";
                newRow["Outbound Detectors"] = "";
                newRow["Outbound Dividing Factor"] = "";
                dataTable.Rows.Add(newRow);
            }

            return dataTable;
        }

        public void Invalidate()
        {
            throw new NotImplementedException();
        }

        public Approach GetBusiestApproach(DateSettings settings)
        {
            Approach currentBusiest = null;
            foreach (var approach in Approaches)
            {
                if (currentBusiest == null ||
                    approach.GetTotal(settings, Intersection, 0) > currentBusiest.GetTotal(settings, Intersection, 0))
                {
                    currentBusiest = approach;
                }
            }

            return currentBusiest;
        }

        public DateTime GetAMPeakPeriod()
        {
            throw new NotImplementedException();
        }

        public DateTime GetPMPeakPeriod()
        {
            throw new NotImplementedException();
        }

        public void GetAMPeakVolume()
        {
            throw new NotImplementedException();
        }

        public void GetPMPeakVolume()
        {
            throw new NotImplementedException();
        }
    }
}
