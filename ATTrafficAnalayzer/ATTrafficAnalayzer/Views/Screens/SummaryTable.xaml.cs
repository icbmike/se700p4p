using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Documents;
using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Models.Configuration;
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
        private readonly DbHelper _dbHelper = DbHelper.GetDbHelper();
        private readonly DataTableHelper _dtHelper = DataTableHelper.GetDataTableHelper();
        private SettingsTray _settings;

        private DateTime _startDate;
        private DateTime _endDate;
        private bool _hasWeekends;

        private int _amPeakHour = 8;
        private int _pmPeakHour = 4;

        public SummaryTable(SettingsTray settings, string configName)
        {
            _configName = configName;
            _settings = settings;
            _startDate = _settings.StartDate;
            _endDate = _settings.EndDate;

            InitializeComponent();
            Render();
        }

        public void Render()
        {
            _summaryConfig = _dbHelper.GetSummaryConfig(_configName);
            ScreenTitle.Content = _configName;

            ApproachesStackPanel.Children.Clear();

            var amPeakApproachDisplay = new TableApproachDisplay
            {
                ApproachDataGrid = { ItemsSource = _dtHelper.GetSummaryDataTable(new ATTrafficAnalayzer.Models.Configuration.DataTableHelper.AmPeakCalculator(_amPeakHour), _startDate, _endDate, _summaryConfig, _hasWeekends).AsDataView() }

            };
            amPeakApproachDisplay.ApproachSummary.Inlines.Add(new Bold(new Run("AM Peak Hour Volumes")));
            ApproachesStackPanel.Children.Add(amPeakApproachDisplay);

            var pmPeakApproachDisplay = new TableApproachDisplay
            {
                ApproachDataGrid = { ItemsSource = _dtHelper.GetSummaryDataTable(new ATTrafficAnalayzer.Models.Configuration.DataTableHelper.PmPeakCalculator(_pmPeakHour), _startDate, _endDate, _summaryConfig, _hasWeekends).AsDataView() }
            };
            pmPeakApproachDisplay.ApproachSummary.Inlines.Add(new Bold(new Run("PM Peak Hour Volumes")));
            ApproachesStackPanel.Children.Add(pmPeakApproachDisplay);

            var sumApproachDisplay = new TableApproachDisplay
            {
                ApproachDataGrid = { ItemsSource = _dtHelper.GetSummaryDataTable(new ATTrafficAnalayzer.Models.Configuration.DataTableHelper.SumCalculator(), _startDate, _endDate, _summaryConfig, _hasWeekends).AsDataView() }
            };
            sumApproachDisplay.ApproachSummary.Inlines.Add(new Bold(new Run("Daily Volume Totals")));
            ApproachesStackPanel.Children.Add(sumApproachDisplay);
        }

        #region Event Handlers

        public void DateRangeChangedHandler(object sender, Toolbar.DateRangeChangedEventHandlerArgs args)
        {
            _startDate = _settings.StartDate;
            _endDate = _settings.EndDate;

            _amPeakHour = args.AmPeakHour;
            _pmPeakHour = args.PmPeakHour;

            Render();
        }

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

        private void WeekendsCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            if (!_hasWeekends)
            {
                _hasWeekends = true;
                Render();
            }
        }

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
