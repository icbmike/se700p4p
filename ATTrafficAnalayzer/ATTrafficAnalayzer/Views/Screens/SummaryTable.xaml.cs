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
    public partial class SummaryTable : IView
    {
        private string _configName;

        private IEnumerable<SummaryRow> _summaryConfig;
        private readonly IDataSource _dataSource;
        private readonly DataTableHelper _dtHelper = DataTableHelper.GetDataTableHelper();
        private readonly DateSettings _settings;

        private DateTime _startDate;
        private DateTime _endDate;
        private bool _hasWeekends;

        private int _amPeakHour = 8;
        private int _pmPeakHour = 4;

        /// <summary>
        /// Constructor to display a summary table with the date range at the time of construction and the specified config
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="configName">Name of config to be displayed</param>
        public SummaryTable(DateSettings settings, string configName, IDataSource dataSource)
        {
            _configName = configName;
            _settings = settings;
            _startDate = _settings.StartDate;
            _endDate = _settings.EndDate;
            _dataSource = dataSource;
            InitializeComponent();
            Render();
        }

        /// <summary>
        /// Display the table
        /// </summary>
        private void Render()
        {
            _summaryConfig = _dataSource.GetSummaryConfig(_configName);
            ScreenTitle.Content = _configName;

            //Remove all previous tables
            ApproachesStackPanel.Children.Clear();

//            //Create the AM Peak table
//            var amPeakApproachDisplay = new ApproachTable
//            {
//                ApproachDataGrid = { ItemsSource = _dtHelper.GetSummaryDataTable(new ATTrafficAnalayzer.Models.Configuration.DataTableHelper.AmPeakCalculator(_amPeakHour), _startDate, _endDate, _summaryConfig, _hasWeekends).AsDataView() }
//
//            };
//            amPeakApproachDisplay.ApproachSummary.Inlines.Add(new Bold(new Run("AM Peak Hour Volumes")));
//            ApproachesStackPanel.Children.Add(amPeakApproachDisplay);
//
//            //Create the PM Peak table
//            var pmPeakApproachDisplay = new ApproachTable
//            {
//                ApproachDataGrid = { ItemsSource = _dtHelper.GetSummaryDataTable(new ATTrafficAnalayzer.Models.Configuration.DataTableHelper.PmPeakCalculator(_pmPeakHour), _startDate, _endDate, _summaryConfig, _hasWeekends).AsDataView() }
//            };
//            pmPeakApproachDisplay.ApproachSummary.Inlines.Add(new Bold(new Run("PM Peak Hour Volumes")));
//            ApproachesStackPanel.Children.Add(pmPeakApproachDisplay);
//
//            //Creat the totals table
//            var sumApproachDisplay = new ApproachTable
//            {
//                ApproachDataGrid = { ItemsSource = _dtHelper.GetSummaryDataTable(new ATTrafficAnalayzer.Models.Configuration.DataTableHelper.SumCalculator(), _startDate, _endDate, _summaryConfig, _hasWeekends).AsDataView() }
//            };
//            sumApproachDisplay.ApproachSummary.Inlines.Add(new Bold(new Run("Daily Volume Totals")));
//            ApproachesStackPanel.Children.Add(sumApproachDisplay);
        }

        #region Event Handlers

        /// <summary>
        /// Handler for when the date range changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void DateRangeChangedHandler(object sender, Toolbar.DateRangeChangedEventHandlerArgs args)
        {
            _startDate = _settings.StartDate;
            _endDate = _settings.EndDate;

            _amPeakHour = args.AmPeakHour;
            _pmPeakHour = args.PmPeakHour;

            Render();
        }

        /// <summary>
        /// Handler for when the selected report changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void ReportChangedHandler(object sender, ReportBrowser.SelectedReportChangeEventHandlerArgs args)
        {
            if (!args.SelectionCleared)
            {
                if (!_configName.Equals(args.ReportName))
                {
                    _configName = args.ReportName;
                    Render();
                }
            }
        }

        public event VolumeAndDateCountsDontMatchHandler VolumeDateCountsDontMatch;

        #endregion

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
