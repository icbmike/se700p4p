using System;
using System.Collections.Generic;
using System.Data;
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

            var table = new TableApproachDisplay
            {
                ApproachDataGrid = { ItemsSource = GetDataTable().AsDataView() }
            };

            ApproachesStackPanel.Children.Add(table);
        }

        private DataTable GetDataTable()
        {
            var dataTable = new DataTable();

            dataTable.Columns.Add("Day", typeof(string));
            foreach (var summary in _summaryConfig)
                dataTable.Columns.Add(summary.RouteName, typeof(string));

            for (var date = _startDate; date < _endDate; date = date.AddDays(1))
            {
                var row = dataTable.NewRow();
                var j = 1;
                row[0] = string.Format(date.ToLongDateString());
                foreach (var summary in _summaryConfig)
                {
                    row[j] = _dbHelper.GetVolumeForDay(date, summary.SelectedIntersectionIn, summary.DetectorsIn) +
                             _dbHelper.GetVolumeForDay(date, summary.SelectedIntersectionOut, summary.DetectorsOut);
                    j++;
                }
                dataTable.Rows.Add(row);
            }

            return dataTable;
        }

    }
}
