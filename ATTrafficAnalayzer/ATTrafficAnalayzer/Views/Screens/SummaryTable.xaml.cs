using System;
using System.Collections.Generic;
using System.Windows;
using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Models.ReportConfiguration;
using ATTrafficAnalayzer.Models.Settings;
using ATTrafficAnalayzer.Views.Controls;
    
namespace ATTrafficAnalayzer.Views.Screens
{
    /// <summary>
    /// Interaction logic for Summary.xaml
    /// </summary>
    public partial class SummaryTable
    {
        private readonly IDataSource _dataSource;
        private DateSettings _dateSettings;
        private List<StatsTable> _statsTables;
        private bool _hasWeekends;
        private SummaryConfiguration _configuration;
        private DateTime _amPeakTime;
        private DateTime _pmPeakTime;

        public SummaryConfiguration Configuration
        {
            get { return _configuration; }
            set { _configuration = value; Render(); }
        }

        public DateTime AMPeakTime
        {
            get { return _amPeakTime; }
            set
            {
                _amPeakTime = value;
                if(_configuration != null) Render();
            }
        }

        public DateTime PMPeakTime
        {
            get { return _pmPeakTime; }
            set
            {
                _pmPeakTime = value;
                if (_configuration != null) Render();
            }
        }

        /// <summary>
        /// Constructor to display a summary table with the date range at the time of construction and the specified config
        /// </summary>
        /// <param name="dateSettings"></param>
        /// <param name="configName">ApproachName of config to be displayed</param>
        /// <param name="dataSource">The source of the data</param>
        public SummaryTable(DateSettings dateSettings, IDataSource dataSource)
        {
            _dateSettings = dateSettings;
            _dataSource = dataSource;

            InitializeComponent();
        }

        private int CalulateDailyTotals(DateTime dateTime, SummaryRow summaryRow)
        {
            Console.WriteLine(dateTime);
            var totalVolumeForDay = _dataSource.GetTotalVolumeForDay(dateTime.Date, summaryRow.SelectedIntersectionIn, summaryRow.DetectorsIn);
            totalVolumeForDay += _dataSource.GetTotalVolumeForDay(dateTime.Date, summaryRow.SelectedIntersectionOut,
                summaryRow.DetectorsOut);
            return totalVolumeForDay;
        }

        /// <summary>
        /// Display the table
        /// </summary>
        private void Render()
        {
            ScreenTitle.Content = Configuration.Name;

            //Remove all previous tables
            StatsStackPanel.Children.Clear();

            new List<StatsTable>
            {
                new StatsTable(_dataSource, _dateSettings, Configuration, "sample", (time, row) => time.Day),
                new StatsTable(_dataSource, _dateSettings, Configuration, "Daily Totals", CalulateDailyTotals)
            }.ForEach(statsTable => StatsStackPanel.Children.Add(statsTable));

        }

        public void DateSettingsChanged()
        {
            Render();
        }

        public event EventHandler VolumeDateCountsDontMatch;


        //Event handler for when weekends checkbox state changes
        private void WeekendsCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            if (!_hasWeekends)
            {
                _hasWeekends = true;
                Render();
            }
        }

        //Event handler for when weekends checkbox state changes
        private void WeekendsCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_hasWeekends)
            {
                _hasWeekends = false;
                Render();
            }
        }
    }
}
