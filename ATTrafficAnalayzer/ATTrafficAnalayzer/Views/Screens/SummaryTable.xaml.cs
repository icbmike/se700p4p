using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing.Text;
using System.Windows;
using System.Windows.Documents;
using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Models.Settings;
using ATTrafficAnalayzer.Views.Controls;

namespace ATTrafficAnalayzer.Views.Screens
{
    /// <summary>
    /// Interaction logic for Summary.xaml
    /// </summary>
    public partial class SummaryTable : IView
    {
        private readonly SettingsTray _settings;
        private DateTime _startDate;
        private DateTime _endDate;
        private readonly IEnumerable<SummaryRow> _summaryConfig;
        readonly DbHelper _dbHelper = DbHelper.GetDbHelper();

        public SummaryTable(SettingsTray settings, string configName)
        {
            _summaryConfig = _dbHelper.GetSummaryConfig(configName);
            _settings = settings;
            _startDate = settings.StartDate;
            _endDate = settings.EndDate;

            InitializeComponent();
            RenderTable(configName);
        }

        public void DateRangeChangedHandler(object sender, Toolbar.DateRangeChangedEventHandlerArgs args)
        {
            throw new System.NotImplementedException();
        }

        public void ReportChangedHandler(object sender, ReportBrowser.SelectedReporChangeEventHandlerArgs args)
        {
            throw new System.NotImplementedException();
        }

        public event VolumeAndDateCountsDontMatchHandler VolumeDateCountsDontMatch;

        private void RenderTable(string screenTitle)
        {
            if (!DbHelper.GetDbHelper().VolumesExist(_startDate, _endDate))
            {
                MessageBox.Show("You haven't imported volume data for the selected date range");
                return;
            }

            ScreenTitle.Content = screenTitle;

            var amPeakApproachDisplay = new TableApproachDisplay
            {
                ApproachDataGrid = { ItemsSource = GetDataTable(new AmPeakCalculator(10)).AsDataView() }
            };
            amPeakApproachDisplay.ApproachSummary.Inlines.Add(new Bold(new Run("AM Peak Hour Volumes")));
            ApproachesStackPanel.Children.Add(amPeakApproachDisplay);

            var pmPeakApproachDisplay = new TableApproachDisplay
            {
                ApproachDataGrid = { ItemsSource = GetDataTable(new PmPeakCalculator(18)).AsDataView() }
            };
            pmPeakApproachDisplay.ApproachSummary.Inlines.Add(new Bold(new Run("PM Peak Hour Volumes")));
            ApproachesStackPanel.Children.Add(pmPeakApproachDisplay);

            var sumApproachDisplay = new TableApproachDisplay
            {
                ApproachDataGrid = { ItemsSource = GetDataTable(new SumCalculator()).AsDataView() }
            };
            sumApproachDisplay.ApproachSummary.Inlines.Add(new Bold(new Run("Daily Volume Totals")));
            ApproachesStackPanel.Children.Add(sumApproachDisplay);
        }

        private DataTable GetDataTable(ICalculator calculator)
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

        private interface ICalculator
        {
            int GetVolume(DateTime date, SummaryRow summary);
        }

        private class AmPeakCalculator : ICalculator
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

        private class PmPeakCalculator : ICalculator
        {
            private readonly int _hour;

            public PmPeakCalculator(int hour)
            {
                _hour = hour;
            }

            public int GetVolume(DateTime date, SummaryRow summary)
            {
                var dbHelper = DbHelper.GetDbHelper();
                date = date.AddHours(_hour);
                return dbHelper.GetVolumeForTimePeriod(summary.SelectedIntersectionIn, summary.DetectorsIn, date, date.AddHours(1))+
                    dbHelper.GetVolumeForTimePeriod(summary.SelectedIntersectionOut, summary.DetectorsOut, date, date.AddHours(1));
            }
        }

        private class SumCalculator : ICalculator
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
