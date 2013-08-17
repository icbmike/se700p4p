using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Documents;
using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Models.Settings;
using ATTrafficAnalayzer.Views.Controls;
using Newtonsoft.Json.Linq;

namespace ATTrafficAnalayzer.Views.Screens
{
    /// <summary>
    /// Interaction logic for Summary.xaml
    /// </summary>
    public partial class Summary : IView
    {
        private readonly SettingsTray _settings;
        private DateTime _startDate;
        private readonly IEnumerable<SummaryRow> _summaryConfig;
        readonly DbHelper _dbHelper = DbHelper.GetDbHelper();

        public Summary(SettingsTray settings, string configName)
        {
            _summaryConfig = _dbHelper.GetSummaryConfig(configName);
            _settings = settings;
            _startDate = settings.StartDate;

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

            ScreenTitle.Content = screenTitle;

            //            table.ApproachSummary.Inlines.Add(new Bold(new Run(string.Format("Route: {0}\n", summary.RouteName))));
            //            table.ApproachSummary.Inlines.Add(new Run(string.lFormat("Inbound intersection: {0} - Detectors: {1}\n", summary.SelectedIntersectionIn, summary.GetDetectorsInAsString())));
            //            table.ApproachSummary.Inlines.Add(new Run(string.Format("Outbound intersection: {0} - Detectors: {1}", summary.SelectedIntersectionOut, summary.GetDetectorsOutAsString())));

            var table = new TableApproachDisplay
            {
                ApproachDataGrid = { ItemsSource = GetDataTable().AsDataView() }
            };
            ApproachesStackPanel.Children.Add(table);
        }

        private DataTable GetDataTable()
        {
            var dataTable = new DataTable();
            var summaryDate = new DateTime(2013, 3, 1);

            dataTable.Columns.Add("-", typeof(string));
            foreach (var summary in _summaryConfig)
                dataTable.Columns.Add(summary.RouteName, typeof(string));

            for (var i = 1; i <= DateTime.DaysInMonth(summaryDate.Year, summaryDate.Month); i++)
            {
                var row = dataTable.NewRow();
                var j = 1;
                row[0] = string.Format("Day {0}", i);
                foreach (var summary in _summaryConfig)
                {
                    Logger.Debug(summaryDate.Date.ToString(), "Date");
                    row[j] = _dbHelper.GetVolumeForDay(summaryDate, summary.SelectedIntersectionIn, summary.DetectorsIn);
                    summaryDate = summaryDate.AddDays(1);
                    j++;
                }
                dataTable.Rows.Add(row);
            }

            return dataTable;
        }

    }
}
