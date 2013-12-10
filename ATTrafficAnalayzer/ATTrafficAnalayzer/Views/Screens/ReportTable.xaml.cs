using System.Data;
using System.Windows;
using System.Windows.Documents;
using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Models.Configuration;
using ATTrafficAnalayzer.Models.Settings;
using ATTrafficAnalayzer.Models.Volume;
using ATTrafficAnalayzer.Views.Controls;
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

        private readonly DateSettings _settings;
        private DateTime _startDate;
        private DateTime _endDate;
        private int _interval;

        private Configuration _configuration;

        readonly IDataSource _dataSource;
        private bool _countsDontMatch;

        /// <summary>
        /// Constructor create a component displaying the specified config
        /// </summary>
        /// <param name="settings">Lets the graph view get the date range at the time of construction</param>
        /// <param name="configName">The config to be displayed</param>
        /// <param name="dataSource">The dataSource to get volume information from</param>
        public ReportTable(DateSettings settings, string configName, IDataSource dataSource)
        {
            _dataSource = dataSource;
            _configuration = _dataSource.GetConfiguration(configName);
            _settings = settings;
            _startDate = settings.StartDate;
            _endDate = settings.EndDate;
            _interval = settings.Interval;

            InitializeComponent();

            Render();
        }

        /// <summary>
        /// Displays the grid
        /// </summary>
        private void Render()
        {
            ScreenTitle.Content = _configuration.ConfigName;
            OverallSummaryBorder.Visibility = Visibility.Collapsed;

            //Remove all exisitng approaches!
            ApproachesStackPanel.Children.Clear();
            OverallSummaryTextBlock.Inlines.Clear();

            _configuration.Approaches.ForEach(approach => ApproachesStackPanel.Children.Add(new ApproachTable(approach, _configuration.Intersection, _settings)));

        }                                                                                                                                                   

        /// <summary>
        /// Handler for DateRangeChanged event from Toolbar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void DateRangeChangedHandler(object sender, Toolbar.DateRangeChangedEventHandlerArgs args)
        {
            if (!args.StartDate.Equals(_startDate) || !args.EndDate.Equals(_endDate) || !args.Interval.Equals(_interval))
            {
                _startDate = args.StartDate;
                _endDate = args.EndDate;
                _interval = args.Interval;
                _configuration.Approaches.ForEach(approach => approach.Invalidate());
                Render();
            }
        }

        /// <summary>
        /// Event Handler for ReportChanged event from Configuration Browser
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void ReportChangedHandler(object sender, ReportBrowser.SelectedReportChangeEventHandlerArgs args)
        {
            if (!args.SelectionCleared && !_configuration.ConfigName.Equals(args.ReportName) && args.ReportName != null)
            {
                _configuration = _dataSource.GetConfiguration(args.ReportName);
                Render();
            }
        }

        public event VolumeAndDateCountsDontMatchHandler VolumeDateCountsDontMatch;
    }

}