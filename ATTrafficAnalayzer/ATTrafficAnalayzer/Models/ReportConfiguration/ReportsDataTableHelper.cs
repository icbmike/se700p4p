﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Windows.Forms;
using ATTrafficAnalayzer.Models.Settings;
using ATTrafficAnalayzer.Views;
using ATTrafficAnalayzer.Views.Screens;

namespace ATTrafficAnalayzer.Models.ReportConfiguration
{
    class DataTableHelper
    {
        private static DataTableHelper _instance;
        private static readonly object SyncLock = new object();
        public static DataTableHelper GetDataTableHelper()
        {
            lock (SyncLock)
                return _instance ?? (_instance = new DataTableHelper());
        }

        private DataTableHelper()
        {
        }

        /// <summary>
        ///     Create a datatable to display the summary data
        /// </summary>
        /// <param name="calculator">Calculator wich summarieses the data</param>
        /// <param name="_startDate">Date at the start of the period</param>
        /// <param name="_endDate">Date at the end of the period</param>
        /// <param name="_summaryConfig">Summary configuration</param>
        /// <param name="hasWeekends">Include weekends in the summary</param>
        /// <returns>Data table of daily metrics</returns>
        public DataTable GetSummaryDataTable(ICalculator calculator, DateTime _startDate, DateTime _endDate, IEnumerable<SummaryRow> _summaryConfig, bool hasWeekends)
        {
            var dataTable = new DataTable();

            dataTable.Columns.Add("Date", typeof(string));
            foreach (var summary in _summaryConfig)
                dataTable.Columns.Add(summary.RouteName, typeof(string));

            for (var date = _startDate; date < _endDate; date = date.AddDays(1))
            {
                if (!hasWeekends && (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday))
                    continue;
                var row = dataTable.NewRow();
                var j = 1;
                row[0] = string.Format(date.ToLongDateString());
                foreach (var summary in _summaryConfig)
                {
                    row[j] = calculator.GetVolume(date, summary);
                    j++;
                }
                dataTable.Rows.Add(row);
            }

            return dataTable;
        }

        /// <summary>
        ///     Calculator interface
        /// </summary>
        public interface ICalculator
        {
            int? GetVolume(DateTime date, SummaryRow summary);
        }

        /// <summary>
        ///     Calculates the traffic volumes for each morning peak period
        /// </summary>
        public class AmPeakCalculator : ICalculator
        {
            private readonly int _hour;

            public AmPeakCalculator(int hour)
            {
                _hour = hour;
            }

            public int? GetVolume(DateTime date, SummaryRow summary)
            {
                var dbHelper = DataSourceFactory.GetDataSource();
                date = date.AddHours(_hour);
                if (dbHelper.VolumesExist(date,date.AddDays(1)))
                {
                    return dbHelper.GetVolumeForTimePeriod(summary.SelectedIntersectionIn, summary.DetectorsIn, date, date.AddHours(1)) +
                        dbHelper.GetVolumeForTimePeriod(summary.SelectedIntersectionOut, summary.DetectorsOut, date, date.AddHours(1));
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        ///     Calculates the traffic volumes for each afternoon peak period
        /// </summary>
        public class PmPeakCalculator : ICalculator
        {
            private readonly int _hour;

            public PmPeakCalculator(int hour)
            {
                _hour = hour;
            }

            public int? GetVolume(DateTime date, SummaryRow summary)
            {
                var dbHelper = DataSourceFactory.GetDataSource();
                date = date.AddHours(_hour + 12);
                if (dbHelper.VolumesExist(date, date.AddDays(1)))
                {
                    return dbHelper.GetVolumeForTimePeriod(summary.SelectedIntersectionIn, summary.DetectorsIn, date, date.AddHours(1)) +
                        dbHelper.GetVolumeForTimePeriod(summary.SelectedIntersectionOut, summary.DetectorsOut, date, date.AddHours(1));
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        ///     Calculates the total for each day
        /// </summary>
        public class SumCalculator : ICalculator
        {
            public int? GetVolume(DateTime date, SummaryRow summary)
            {
                var dbHelper = DataSourceFactory.GetDataSource();
                if (dbHelper.VolumesExist(date, date.AddDays(1)))
                {
                    return dbHelper.GetTotalVolumeForDay(date, summary.SelectedIntersectionIn, summary.DetectorsIn) +
                                 dbHelper.GetTotalVolumeForDay(date, summary.SelectedIntersectionOut, summary.DetectorsOut);
                }
                else
                {
                    return null;
                }
            }
        }

    }
}
