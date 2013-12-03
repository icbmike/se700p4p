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

        private readonly SettingsTray _settings;
        private DateTime _startDate;
        private DateTime _endDate;
        private int _interval;

        private Report _configuration;

        readonly IDataSource _dataSource;
        private bool _countsDontMatch;

        /// <summary>
        /// Constructor create a component displaying the specified config
        /// </summary>
        /// <param name="settings">Lets the graph view get the date range at the time of construction</param>
        /// <param name="configName">The config to be displayed</param>
        public ReportTable(SettingsTray settings, string configName, IDataSource dataSource)
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

            //Clear the calculated stats
            _maxAm.ClearApproaches();
            _maxPm.ClearApproaches();
            _maxTotal.ClearApproaches();
            _peakHourAm.ClearApproaches();
            _peakHourPm.ClearApproaches();

            var timeSpan = _endDate - _startDate;

            //Per day
            var hasVolumes = false;
            for (var day = 0; day < timeSpan.TotalDays; day++)
            {
                //Per approach
                foreach (var approach in _configuration.Approaches)
                {
                    //Construct a datatable
                    var dataTable = approach.GetDataTable(_settings, _configuration.Intersection, 24, 0, day);
                    
                    //If there isnt a data table tell the user.
                    if (dataTable == null)
                    {
                        var missingTable = new Label()
                        {
                            Content = string.Format("No traffic volume data for intersection {0} on {1}", approach.Name, _settings.StartDate.AddDays(day).ToLongDateString()),
                            Margin = new Thickness(20, 5, 20, 5)
                        };
                        ApproachesStackPanel.Children.Add(missingTable);
                    }
                    else
                    {
                        //Display the datatable using the TableApproachDisplay control.
                        ApproachesStackPanel.Children.Add(CreateApproachDisplay(approach, dataTable, day));

                        _maxTotal.CheckIfMax(approach.GetTotal(), approach.Name);
                        _maxAm.CheckIfMax(approach.AmPeak.GetValue(), approach.Name);
                        _maxPm.CheckIfMax(approach.PmPeak.GetValue(), approach.Name);
                        _peakHourAm.CheckIfMax(approach.AmPeak.GetValue(),
                            string.Format("{0} ({1})", approach.Name, approach.AmPeak.GetApproachesAsString()));
                        _peakHourPm.CheckIfMax(approach.PmPeak.GetValue(),
                            string.Format("{0} ({1})", approach.Name, approach.PmPeak.GetApproachesAsString()));

                        hasVolumes = true;
                    }
                }
            }


            //Display overall stats
            OverallSummaryTextBlock.Inlines.Add(new Bold(new Run(string.Format("{0} Overview\n", _configuration.ConfigName))));
            OverallSummaryTextBlock.Inlines.Add(new Run(string.Format("Busiest approach: {0} with {1} vehicles\n", string.Join(", ", _maxTotal.GetApproachesAsString()), _maxTotal.GetValue())));
            OverallSummaryTextBlock.Inlines.Add(new Run(string.Format("Busiest AM hour: {0} with {1} vehicles\n", string.Join(", ", _maxAm.GetApproachesAsString()), _maxAm.GetValue())));
            OverallSummaryTextBlock.Inlines.Add(new Run(string.Format("Busiest PM hour: {0} with {1} vehicles\n", string.Join(", ", _maxPm.GetApproachesAsString()), _maxPm.GetValue())));
            OverallSummaryTextBlock.Inlines.Add(new Run(string.Format("AM peak period: {0} with {1} vehicles\n", string.Join(", ", _peakHourAm.GetApproachesAsString()), _peakHourAm.GetValue())));
            OverallSummaryTextBlock.Inlines.Add(new Run(string.Format("PM peak period: {0} with {1} vehicles", string.Join(", ", _peakHourPm.GetApproachesAsString()), _peakHourPm.GetValue())));
            OverallSummaryBorder.Visibility = hasVolumes ? Visibility.Visible : Visibility.Collapsed;

            Logger.Info("constructed view", "VS table");
        }

        /// <summary>
        /// Creates a TableApproachDisplay from the given arguments.
        /// </summary>
        /// <param name="approach">The approach for the table</param>
        /// <param name="day">The index of the day in the daterange specified in the Toolbar</param>
        /// <returns></returns>
        private TableApproachDisplay CreateApproachDisplay(Approach approach, DataTable dataTable, int day)
        {
            var approachDisplay = new TableApproachDisplay();

            approachDisplay.ApproachDataGrid.DataContext = dataTable;
            approachDisplay.ApproachDataGrid.ItemsSource = dataTable.AsDataView();

            approachDisplay.ApproachSummary.Inlines.Add(new Bold(new Run(string.Format("Date: {0}\n", _startDate.AddDays(day).ToLongDateString()))));
            approachDisplay.ApproachSummary.Inlines.Add(new Bold(new Run(string.Format("Approach: {0} - Detectors: {1}\n", approach.Name, string.Join(", ", approach.Detectors)))));
            approachDisplay.ApproachSummary.Inlines.Add(new Run(string.Format("AM Peak: {0} vehicles @ {1}\n", approach.AmPeak.GetValue(), approach.AmPeak.GetApproachesAsString())));
            approachDisplay.ApproachSummary.Inlines.Add(new Run(string.Format("PM Peak: {0} vehicles @ {1}\n", approach.PmPeak.GetValue(), approach.PmPeak.GetApproachesAsString())));
            approachDisplay.ApproachSummary.Inlines.Add(new Run(string.Format("Total volume: {0} vehicles", approach.GetTotal())));

            return approachDisplay;
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

                Render();
            }
        }

        /// <summary>
        /// Event Handler for ReportChanged event from Report Browser
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