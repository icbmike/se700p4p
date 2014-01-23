using System;
using System.Collections.Generic;
using System.Data;
using ATTrafficAnalayzer.Models.Settings;

namespace ATTrafficAnalayzer.Models.ReportConfiguration
{
    public class Configuration
    {
        private Approach _currentBusiest;
        private DateTime? _amPeakPeriod;
        private DateTime? _pmPeakPeriod;
        private int _amPeakVolume;
        private int _pmPeakVolume;
        public List<Approach> Approaches { get; set; }
        public string Name { get; set; }
        public int Intersection { get; set; }

        /// <summary>
        ///     Creates a report object
        /// </summary>
        /// <param name="name">ApproachName of the report</param>
        /// <param name="intersection">Intersection of the report</param>
        /// <param name="approaches">List of approaches contained in the report</param>
        /// <param name="dataSource"></param>
        public Configuration(string name, int intersection, List<Approach> approaches, IDataSource dataSource)
        {
            Name = name;
            Intersection = intersection;
            Approaches = approaches;
            
            _currentBusiest = null;
            _amPeakPeriod = null;
            _pmPeakPeriod = null;
            _amPeakVolume = -1;
            _pmPeakVolume = -1;
        }

        /// <summary>
        ///     Data table to configure the summary
        /// </summary>
        /// <returns>Summary data table</returns>
        public DataTable GetSummaryTable()
        {
            var dataTable = new DataTable();

            dataTable.Columns.Add("Route ApproachName", typeof(string));
            dataTable.Columns.Add("Inbound Intersections", typeof(string));
            dataTable.Columns.Add("Inbound Detectors", typeof(string));
            dataTable.Columns.Add("Inbound Dividing Factor", typeof(string));
            dataTable.Columns.Add("Outbound Intersections", typeof(string));
            dataTable.Columns.Add("Outbound Detectors", typeof(string));
            dataTable.Columns.Add("Outbound Dividing Factor", typeof(string));

            foreach (var app in Approaches)
            {
                var newRow = dataTable.NewRow();
                newRow["Route ApproachName"] = app.ApproachName;
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

        private void Invalidate()
        {
            _currentBusiest = null;
            _amPeakPeriod = null;
            _pmPeakPeriod = null;
            _amPeakVolume = -1;
            _pmPeakVolume = -1;

        }

        public Approach GetBusiestApproach(DateSettings settings)
        {
            if (_currentBusiest != null) return _currentBusiest;

            foreach (var approach in Approaches)
            {
                if (_currentBusiest == null ||
                    approach.GetTotal(settings, Intersection, 0) >
                    _currentBusiest.GetTotal(settings, Intersection, 0))
                {
                    _currentBusiest = approach;
                }
            }
            return _currentBusiest;
        }

        public DateTime GetAMPeakPeriod(DateSettings settings)
        {
            if (_amPeakPeriod != null) return _amPeakPeriod.Value;
            
            //This sets the peak period anyway
            GetAMPeakVolume(settings);

            return _amPeakPeriod.Value;

        }

        public DateTime GetPMPeakPeriod(DateSettings settings)
        {
            if (_pmPeakPeriod != null) return _pmPeakPeriod.Value;
            
            //This sets the peak period anyway
            GetPMPeakVolume(settings);
            
            return _pmPeakPeriod.Value;
        }

        public int GetAMPeakVolume(DateSettings settings)
        {
            if (_amPeakVolume != -1) return _amPeakVolume;

            foreach (var approach in Approaches)
            {
                if (approach.GetAmPeak(settings, Intersection, 0) > _amPeakVolume)
                {
                    _amPeakVolume = approach.GetAmPeak(settings, Intersection, 0);
                    _amPeakPeriod = approach.GetAmPeakTime(settings, Intersection, 0);
                }
            }

            return _amPeakVolume;
        }

        public int GetPMPeakVolume(DateSettings settings)
        {
            if (_pmPeakVolume != -1) return _pmPeakVolume;

            foreach (var approach in Approaches)
            {
                if (approach.GetPmPeak(settings, Intersection, 0) > _pmPeakVolume)
                {
                    _pmPeakVolume = approach.GetPmPeak(settings, Intersection, 0);
                    _pmPeakPeriod = approach.GetPmPeakTime(settings, Intersection, 0);
                }
            }

            return _pmPeakVolume;
        }
    }
}
