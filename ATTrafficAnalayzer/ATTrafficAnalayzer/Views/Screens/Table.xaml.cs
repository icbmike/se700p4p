using System.Data;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Models.Configuration;
using ATTrafficAnalayzer.Models.Settings;
using ATTrafficAnalayzer.Models.Volume;
using ATTrafficAnalayzer.Views.Controls;
using DataGridCell = System.Windows.Controls.DataGridCell;
using System;

namespace ATTrafficAnalayzer.Views.Screens
{
    /// <summary>
    /// Interaction logic for VSSCreen.xaml
    /// </summary>
    public partial class Table : IView
    {
        private readonly VolumeMetric _maxTotal = new VolumeMetric();
        private readonly VolumeMetric _maxAm = new VolumeMetric();
        private readonly VolumeMetric _maxPm = new VolumeMetric();
        private readonly VolumeMetric _peakHourAm = new VolumeMetric();
        private readonly VolumeMetric _peakHourPm = new VolumeMetric();

        private readonly SettingsTray _settings;
        private DateTime _startDate;
        private DateTime _endDate;
        private int _interval;

        private Report _configuration;

        readonly DbHelper _dbHelper = DbHelper.GetDbHelper();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="configName"></param>
        public Table(SettingsTray settings, string configName)
        {
            _configuration = _dbHelper.GetConfiguration(configName);

            _settings = settings;
            _startDate = settings.StartDate;
            _endDate = settings.EndDate;
            _interval = settings.Interval;

            InitializeComponent();

            RenderTable();
        }


        private void RenderTable()
        {
            if (!DbHelper.GetDbHelper().VolumesExist(_startDate, _endDate))
            {
                MessageBox.Show("You haven't imported volume data for the selected date range");
                return;
            }

            if (_configuration == null)
            {
                MessageBox.Show("Select a report from the list on the left");
                return;
            }

            ScreenTitle.Content = _configuration.ConfigName;

            //Clear all the things!
            ApproachesStackPanel.Children.Clear();

            _maxAm.ClearApproaches();
            _maxPm.ClearApproaches();
            _maxTotal.ClearApproaches();
            _peakHourAm.ClearApproaches();
            _peakHourPm.ClearApproaches();

            OverallSummaryTextBlock.Inlines.Clear();

            //Add all the things!

            var timeSpan = _endDate - _startDate;
            var countsDontMatch = false;
            for (var day = 0; day < timeSpan.TotalDays; day++)
            {
                foreach (var approach in _configuration.Approaches)
                {
                    var tableApproachDisplay = CreateApproachDisplay(approach, day);
                    if (tableApproachDisplay == null)
                    {
                        countsDontMatch = true;
                        break;
                    }
                    ApproachesStackPanel.Children.Add(tableApproachDisplay);

                    _maxTotal.CheckIfMax(approach.GetTotal(), approach.Name);
                    _maxAm.CheckIfMax(approach.AmPeak.GetValue(), approach.Name);
                    _maxPm.CheckIfMax(approach.PmPeak.GetValue(), approach.Name);
                    _peakHourAm.CheckIfMax(approach.AmPeak.GetValue(),
                        string.Format("{0} ({1})", approach.Name, approach.AmPeak.GetApproachesAsString()));
                    _peakHourPm.CheckIfMax(approach.PmPeak.GetValue(),
                        string.Format("{0} ({1})", approach.Name, approach.PmPeak.GetApproachesAsString()));
                }

                if (countsDontMatch) break;
            }
            if (countsDontMatch)
            {
                if (VolumeDateCountsDontMatch != null) VolumeDateCountsDontMatch(this);
                return;
            }

            OverallSummaryTextBlock.Inlines.Add(new Bold(new Run(string.Format("{0} Overview\n", _configuration.ConfigName))));
            OverallSummaryTextBlock.Inlines.Add(new Run(string.Format("Busiest approach: {0} with {1} vehicles\n", string.Join(", ", _maxTotal.GetApproachesAsString()), _maxTotal.GetValue())));
            OverallSummaryTextBlock.Inlines.Add(new Run(string.Format("Busiest AM hour: {0} with {1} vehicles\n", string.Join(", ", _maxAm.GetApproachesAsString()), _maxAm.GetValue())));
            OverallSummaryTextBlock.Inlines.Add(new Run(string.Format("Busiest PM hour: {0} with {1} vehicles\n", string.Join(", ", _maxPm.GetApproachesAsString()), _maxPm.GetValue())));
            OverallSummaryTextBlock.Inlines.Add(new Run(string.Format("AM peak period: {0} with {1} vehicles\n", string.Join(", ", _peakHourAm.GetApproachesAsString()), _peakHourAm.GetValue())));
            OverallSummaryTextBlock.Inlines.Add(new Run(string.Format("PM peak period: {0} with {1} vehicles", string.Join(", ", _peakHourPm.GetApproachesAsString()), _peakHourPm.GetValue())));

            Logger.Info("constructed view", "VS table");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="approach"></param>
        /// <param name="day"></param>
        /// <returns></returns>
        private TableApproachDisplay CreateApproachDisplay(Approach approach, int day)
        {
            var approachDisplay = new TableApproachDisplay();

            var cellStyle = new Style(typeof(DataGridCell));
            cellStyle.Setters.Add(new Setter(BackgroundProperty, Brushes.Aqua));
            approachDisplay.ApproachDataGrid.CellStyle = cellStyle;
            var dataTable = approach.GetDataTable(_settings, _configuration.Intersection, 24, 0, day);
            if (dataTable == null)
            {
                return null;
            }
            approachDisplay.ApproachDataGrid.ItemsSource = dataTable.AsDataView();

            approachDisplay.ApproachSummary.Inlines.Add(new Bold(new Run(string.Format("Approach: {0} - Detectors: {1}\n", approach.Name, string.Join(", ", approach.Detectors)))));
            approachDisplay.ApproachSummary.Inlines.Add(new Run(string.Format("AM Peak: {0} vehicles @ {1}\n", approach.AmPeak.GetValue(), approach.AmPeak.GetApproachesAsString())));
            approachDisplay.ApproachSummary.Inlines.Add(new Run(string.Format("PM Peak: {0} vehicles @ {1}\n", approach.PmPeak.GetValue(), approach.PmPeak.GetApproachesAsString())));
            approachDisplay.ApproachSummary.Inlines.Add(new Run(string.Format("Total volume: {0} vehicles", approach.GetTotal())));

            return approachDisplay;
        }

        public void DateRangeChangedHandler(object sender, Toolbar.DateRangeChangedEventHandlerArgs args)
        {

            if (!args.StartDate.Equals(_startDate) || !args.EndDate.Equals(_endDate) || !args.Interval.Equals(_interval))
            {
                //RenderTable() is a time consuming operation.
                //We dont want to do it if we don't have to.

                _startDate = args.StartDate;
                _endDate = args.EndDate;
                _interval = args.Interval;

                RenderTable();
            }
        }

        public void ReportChangedHandler(object sender, ReportBrowser.SelectedReporChangeEventHandlerArgs args)
        {
            _configuration = _dbHelper.GetConfiguration(args.ReportName);
            RenderTable();
        }

        public event VolumeAndDateCountsDontMatchHandler VolumeDateCountsDontMatch;
    }
}