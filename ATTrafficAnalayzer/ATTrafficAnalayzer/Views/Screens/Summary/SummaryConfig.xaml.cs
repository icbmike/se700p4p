using System;
using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Models.Configuration;
using ATTrafficAnalayzer.Models.Settings;
using ATTrafficAnalayzer.Views.Controls;

namespace ATTrafficAnalayzer.Views.Screens.Summaries
{
    /// <summary>
    /// Interaction logic for Summary.xaml
    /// </summary>
    public partial class SummaryConfig
    {
        private readonly DbHelper _dbHelper;
        private DateTime _endDate;
        private DateTime _startDate;
        private Report _configuration;

        public SummaryConfig(SettingsTray settings, string configName)
        {
            _startDate = settings.StartDate;
            _endDate = settings.EndDate;
            _dbHelper = DbHelper.GetDbHelper();
            _configuration = _dbHelper.GetConfiguration(configName);

            InitializeComponent();

            FillSummary();
        }

        internal void DateRangeChangedHandler(object sender, Toolbar.DateRangeChangedEventHandlerArgs args)
        {
            if (!args.startDate.Equals(_startDate) || !args.endDate.Equals(_endDate))
            {
                _startDate = args.startDate;
                _endDate = args.endDate;
            }
            FillSummary();
        }

        public void ReportChangedHandler(object sender, ReportBrowser.SelectedReporChangeEventHandlerArgs args)
        {
            _configuration = _dbHelper.GetConfiguration(args.ReportName);
            FillSummary();
        }

        internal void ReportChangedHandler(object sender, Toolbar.DateRangeChangedEventHandlerArgs args)
        {
            if (!args.startDate.Equals(_startDate) || !args.endDate.Equals(_endDate))
            {
                _startDate = args.startDate;
                _endDate = args.endDate;
            }
            FillSummary();
        }

        private void FillSummary()
        {
            ScreenTitle.Content = _configuration.ConfigName;
            DateLabel.Content = string.Format("Dates: {0} - {1}", _startDate.ToShortDateString(),
                _endDate.Date.ToShortDateString());
        }
    }
}