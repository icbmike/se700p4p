using System;
using System.Collections;
using System.Collections.Generic;
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
            foreach (var summary in _summaryConfig)
            {
                var table = new TableApproachDisplay();
                ApproachesStackPanel.Children.Add(table);

                var dataTable = summary.GetDataTable();

                table.ApproachSummary.Inlines.Add(new Bold(new Run(string.Format("Route: {0}\n", summary.RouteName))));
                table.ApproachSummary.Inlines.Add(new Run(string.Format("Inbound intersection: {0} - Detectors: {1}\n", summary.SelectedIntersectionIn, summary.GetDetectorsInAsString())));
                table.ApproachSummary.Inlines.Add(new Run(string.Format("Outbound intersection: {0} - Detectors: {1}", summary.SelectedIntersectionOut, summary.GetDetectorsOutAsString())));
            }
        }
    }
}
