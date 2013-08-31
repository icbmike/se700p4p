using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Windows.Forms;
using ATTrafficAnalayzer.Models.Settings;
using ATTrafficAnalayzer.Views;
using ATTrafficAnalayzer.Views.Screens;

namespace ATTrafficAnalayzer.Models.Configuration
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

        private readonly DbHelper _dbHelper = DbHelper.GetDbHelper();

        private MainWindow _mainWindow;

        private DataTableHelper()
        {
            CreateConfigDataSet();
        }

        #region Report Related Methods

        private SQLiteDataAdapter _reportDataAdapter;
        private SQLiteDataAdapter _summaryDataAdapter;

        private readonly DataTable _reportsDataTable = new DataTable();
        private readonly DataTable _summaryDataTable = new DataTable();

        private void CreateConfigDataSet()
        {
            //Regular reports
            _reportDataAdapter = _dbHelper.GetConfigsDataAdapter();
            _reportDataAdapter.Fill(_reportsDataTable);
            new SQLiteCommandBuilder(_reportDataAdapter);
            _reportsDataTable.PrimaryKey = new[] { _reportsDataTable.Columns["name"] };

            //Monthly Summaries
            _summaryDataAdapter = _dbHelper.GetMonthlySummariesDataAdapter();
            _summaryDataAdapter.Fill(_summaryDataTable);
            new SQLiteCommandBuilder(_summaryDataAdapter);
            _summaryDataTable.PrimaryKey = new[] { _summaryDataTable.Columns["name"] };
        }

        public DataView GetReportDataView()
        {
            return _reportsDataTable.DefaultView;
        }

        public void RemoveReport(string configToDelete, Mode selectedMode)
        {
            RemoveConfig(configToDelete, selectedMode);
            SyncConfigs(selectedMode);
        }

        #endregion

        private void RemoveConfig(String configToDelete, Mode selectedMode)
        {
            var configs = selectedMode.Equals(Mode.Report) ? _reportsDataTable.Rows : _summaryDataTable.Rows;
            var rowToDelete = configs.Find(configToDelete);
            rowToDelete.Delete();
        }

        public void SyncConfigs(Mode selectedMode)
        {
            if (selectedMode.Equals(Mode.Report))
            {
                _reportDataAdapter.Update(_reportsDataTable);
                _reportDataAdapter.Fill(_reportsDataTable);
            }
            if (selectedMode.Equals(Mode.Summary))
            {
                _summaryDataAdapter.Update(_summaryDataTable);
                _summaryDataAdapter.Fill(_summaryDataTable);
            }
        }

        public DataView GetSummaryDataView()
        {
            return _summaryDataTable.DefaultView;
        }

        public DataTable GetSummaryDataTable(ICalculator calculator, DateTime _startDate, DateTime _endDate, IEnumerable<SummaryRow> _summaryConfig)
        {
            var dataTable = new DataTable();

            dataTable.Columns.Add("Date", typeof(string));
            foreach (var summary in _summaryConfig)
                dataTable.Columns.Add(summary.RouteName, typeof(string));

            for (var date = _startDate; date < _endDate; date = date.AddDays(1))
            {
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
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

        public interface ICalculator
        {
            int GetVolume(DateTime date, SummaryRow summary);
        }

        public class AmPeakCalculator : ICalculator
        {
            private readonly int _hour;

            public AmPeakCalculator(int hour)
            {
                _hour = hour;
            }

            public int GetVolume(DateTime date, SummaryRow summary)
            {
                var dbHelper = DbHelper.GetDbHelper();
                date = date.AddHours(_hour);
                return dbHelper.GetVolumeForTimePeriod(summary.SelectedIntersectionIn, summary.DetectorsIn, date, date.AddHours(1)) +
                    dbHelper.GetVolumeForTimePeriod(summary.SelectedIntersectionOut, summary.DetectorsOut, date, date.AddHours(1));
            }
        }

        public class PmPeakCalculator : ICalculator
        {
            private readonly int _hour;

            public PmPeakCalculator(int hour)
            {
                _hour = hour;
            }

            public int GetVolume(DateTime date, SummaryRow summary)
            {
                var dbHelper = DbHelper.GetDbHelper();
                date = date.AddHours(_hour + 12);
                return dbHelper.GetVolumeForTimePeriod(summary.SelectedIntersectionIn, summary.DetectorsIn, date, date.AddHours(1)) +
                    dbHelper.GetVolumeForTimePeriod(summary.SelectedIntersectionOut, summary.DetectorsOut, date, date.AddHours(1));
            }
        }

        public class SumCalculator : ICalculator
        {
            public int GetVolume(DateTime date, SummaryRow summary)
            {
                var dbHelper = DbHelper.GetDbHelper();
                return dbHelper.GetTotalVolumeForDay(date, summary.SelectedIntersectionIn, summary.DetectorsIn) +
                             dbHelper.GetTotalVolumeForDay(date, summary.SelectedIntersectionOut, summary.DetectorsOut);
            }
        }

    }
}
