using System;
using System.Collections.ObjectModel;
using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Models.Configuration;
using ATTrafficAnalayzer.Models.Settings;
using ATTrafficAnalayzer.Views.Controls;

namespace ATTrafficAnalayzer.Views.Screens
{
    /// <summary>
    /// Interaction logic for Summary.xaml
    /// </summary>
    public partial class SummaryConfig
    {
        private readonly string _configName;
        private readonly DbHelper _dbHelper;
        private DateTime _endDate;
        private DateTime _startDate;
        private Report _configuration;

        public SummaryConfig(SettingsTray settings, string configName)
        {
            _configName = configName;
            _startDate = settings.StartDate;
            _endDate = settings.EndDate;
            _dbHelper = DbHelper.GetDbHelper();
            _configuration = _dbHelper.GetConfiguration(configName);
            Rows = new ObservableCollection<SummaryRow>();

            InitializeComponent();
            SummaryDataGrid.DataContext = this;
            FillSummary();
        }


        public ObservableCollection<SummaryRow> Rows { get; set; }

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
            ScreenTitle.Content = _configName;
            DateLabel.Content = string.Format("Dates: {0} - {1}", _startDate.ToShortDateString(),
                _endDate.Date.ToShortDateString());

            Rows.Add(new SummaryRow(){  DetectorsIn = "1, 2, 3", 
                                        DetectorsOut = "4, 5",
                                        IntersectionIn = 4012,
                                        IntersectionOut = 4013,
                                        RouteName = "FROM SAINT HELIERS TO HOWICK"});
        }
    }

    public class SummaryRow
    {
        public string RouteName { get; set; }
        public int IntersectionIn { get; set; }
        public int IntersectionOut { get; set; }
        public string DetectorsIn { get; set; }
        public string DetectorsOut { get; set; }

    }
}