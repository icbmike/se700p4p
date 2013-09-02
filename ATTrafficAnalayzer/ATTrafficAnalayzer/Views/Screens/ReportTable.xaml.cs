using System.ComponentModel;
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
using System.Windows.Controls;

namespace ATTrafficAnalayzer.Views.Screens
{
    /// <summary>
    /// Interaction logic for VSSCreen.xaml
    /// </summary>
    public partial class ReportTable : IView
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
        private bool _countsDontMatch;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="configName"></param>
        public ReportTable(SettingsTray settings, string configName)
        {
            _configuration = _dbHelper.GetConfiguration(configName);

            _settings = settings;
            _startDate = settings.StartDate;
            _endDate = settings.EndDate;
            _interval = settings.Interval;

            InitializeComponent();

            Render();
        }

        private void Render()
        {
            if (!DbHelper.GetDbHelper().VolumesExist(_startDate, _endDate, _configuration.Intersection))
            {
                MessageBox.Show("You haven't imported volume data for the selected date range");
                return;
            }

            _maxAm.ClearApproaches();
            _maxPm.ClearApproaches();
            _maxTotal.ClearApproaches();
            _peakHourAm.ClearApproaches();
            _peakHourPm.ClearApproaches();

            var timeSpan = _endDate - _startDate;

            _countsDontMatch = false;
            for (var day = 0; day < timeSpan.TotalDays; day++)
            {
                foreach (var approach in _configuration.Approaches)
                {
                    var dataTable = approach.GetDataTable(_settings, _configuration.Intersection, 24, 0, day);
                    if (dataTable == null)
                    {
                        _countsDontMatch = true;
                        break;
                    }

                    ApproachesStackPanel.Children.Add(CreateApproachDisplay(approach, dataTable, day));

                    _maxTotal.CheckIfMax(approach.GetTotal(), approach.Name);
                    _maxAm.CheckIfMax(approach.AmPeak.GetValue(), approach.Name);
                    _maxPm.CheckIfMax(approach.PmPeak.GetValue(), approach.Name);
                    _peakHourAm.CheckIfMax(approach.AmPeak.GetValue(),
                        string.Format("{0} ({1})", approach.Name, approach.AmPeak.GetApproachesAsString()));
                    _peakHourPm.CheckIfMax(approach.PmPeak.GetValue(),
                        string.Format("{0} ({1})", approach.Name, approach.PmPeak.GetApproachesAsString()));
                }

                if (_countsDontMatch) break;
            }

            if (_countsDontMatch)
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

            ScreenTitle.Content = _configuration.ConfigName;

            //Clear all the things!
            ApproachesStackPanel.Children.Clear();
            OverallSummaryTextBlock.Inlines.Clear();

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="approach"></param>
        /// <param name="day"></param>
        /// <returns></returns>
        private TableApproachDisplay CreateApproachDisplay(Approach approach, DataTable dataTable, int day)
        {
            var approachDisplay = new TableApproachDisplay();

            var cellStyle = new Style(typeof(DataGridCell));
            cellStyle.Setters.Add(new Setter(BackgroundProperty, Brushes.Aqua));
            approachDisplay.ApproachDataGrid.CellStyle = cellStyle;

            approachDisplay.ApproachDataGrid.ItemsSource = dataTable.AsDataView();

            approachDisplay.ApproachSummary.Inlines.Add(new Bold(new Run(string.Format("Date: {0}\n", _startDate.AddDays(day).ToLongDateString()))));
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
                _startDate = args.StartDate;
                _endDate = args.EndDate;
                _interval = args.Interval;

                Render();
            }
        }

        public void ReportChangedHandler(object sender, ReportBrowser.SelectedReportChangeEventHandlerArgs args)
        {
            if (!args.SelectionCleared)
            {
                _configuration = _dbHelper.GetConfiguration(args.ReportName);
                Render();
            }
        }

        public event VolumeAndDateCountsDontMatchHandler VolumeDateCountsDontMatch;
    }

    internal class ApproachDisplayRequirements
    {
        public Approach Approach { get; set; }
        public DataTable DataTable { get; set; }
        public int Day { get; set; }
    }
}