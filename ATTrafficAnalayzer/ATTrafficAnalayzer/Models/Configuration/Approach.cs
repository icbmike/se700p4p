﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ATTrafficAnalayzer.Models.Settings;
using ATTrafficAnalayzer.Models.Volume;
using Newtonsoft.Json.Linq;

namespace ATTrafficAnalayzer.Models.Configuration
{
    public class Approach
    {
        private readonly DbHelper _dbHelper;

        public Measurement AmPeak = new Measurement();
        public Measurement PmPeak = new Measurement();
        private int _approachTotal;

        public string Name { get; set; }
        public List<int> Detectors { get; set; }
        public int Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="detectors"></param>
        public Approach(string name, List<int> detectors)
        {
            Name = name;
            Detectors = detectors;
            _dbHelper = DbHelper.GetDbHelper();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public JObject ToJson()
        {
            var json = new JObject { { "name", Name } };

            var array = new JArray();
            foreach (var detector in Detectors)
                array.Add(detector);
            json.Add("detectors", array);

            return json;
        }

        /// <summary>
        /// Find the peak values for the approach
        /// </summary>
        /// <param name="list"></param>
        /// <param name="limit">number of hour columns</param>
        /// <param name="offset">starting column</param>
        /// <returns>max volume record</returns>
        public int CalculatePeakFromList(List<int> list, int limit, int offset)
        {
            return list.GetRange(offset * 12, limit * 12).Max();
        }

        /// <summary>
        /// Retrieve volume data
        /// </summary>
        /// <returns>list of volumes</returns>
        public List<int> GetVolumesList(int intersection, DateTime startDate, int day)
        {
            var volumes = new List<int>();
            foreach (var detector in Detectors)
            {
                if (volumes.Count == 0)
                {
                    volumes.AddRange(_dbHelper.GetVolumes(intersection, detector, startDate.AddDays(day), startDate.AddDays(day + 1)));
                }
                else
                {
                    var detectorVolumes = _dbHelper.GetVolumes(intersection, detector, startDate, startDate.AddDays(1));
                    volumes = volumes.Zip(detectorVolumes, (i, i1) => i + i1).ToList();
                }
            }
            return volumes;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public DataTable GetDataTable(SettingsTray settings, int intersection, int limit, int offset, int day)
        {
            var dataTable = new DataTable();

            // Column headings
            for (var i = offset; i <= offset + limit; i++)
                dataTable.Columns.Add(i == 0 ? "Time" : String.Format("{0} hrs", i - 1), typeof(string));

            // List dates
            var dates = new List<DateTime>();
            for (var date = settings.StartDate; date < settings.EndDate; date = date.AddMinutes(settings.Interval))
                dates.Add(date);

            // Get volume store data 12 hours
            var approachVolumes = GetVolumesList(intersection, settings.StartDate, day);
            for (var rowIndex = 0; rowIndex < 60; rowIndex += settings.Interval)
            {
                var row = dataTable.NewRow();
                for (var columnIndex = 0; columnIndex < limit + 1; columnIndex++)
                {
                    if (columnIndex == 0)
                        row[columnIndex] = rowIndex + " mins";
                    else
                    {
                        var cellValue = 0;
                        for (var i = 0; i < settings.Interval / 5; i++)
                        {
                            cellValue += approachVolumes[(offset + columnIndex - 1) * 12 + rowIndex / 5 + i];
                        }
                        row[columnIndex] = cellValue;
                    }
                }
                dataTable.Rows.Add(row);
            }

            var totalsRow = dataTable.NewRow();
            totalsRow[0] = "Total";
            for (var j = 0; j < limit; j++)
            {
                var total = CalculateColumnTotal(approachVolumes, j, 12);
                totalsRow[j + 1] = total;
                _approachTotal += total;
                if (j < limit / 2)
                    AmPeak.CheckIfMax(total, j + " hrs");
                else
                    PmPeak.CheckIfMax(total, j + " hrs");
            }
            dataTable.Rows.Add(totalsRow);

            return dataTable;
        }

        /// <summary>
        /// Calculates the total for each column in the datagrid
        /// </summary>
        /// <param name="data"></param>
        /// <param name="column"></param>
        /// <param name="numberOfRows"></param>
        /// <returns></returns>
        private static int CalculateColumnTotal(IList<int> data, int column, int numberOfRows)
        {
            return GetColumnData(data, column, numberOfRows).Sum();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="column"></param>
        /// <param name="numberOfRows"></param>
        /// <returns></returns>
        private static IEnumerable<int> GetColumnData(IList<int> data, int column, int numberOfRows)
        {
            var columnList = new List<int>();
            for (var i = 0; i < numberOfRows; i++)
            {
                columnList.Add(data[i + column * numberOfRows]);
            }
            return columnList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int GetTotal()
        {
            return _approachTotal;
        }
    }
}
