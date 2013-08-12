using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Markup;
using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Models.Configuration;
using ATTrafficAnalayzer.Models.Settings;
using ATTrafficAnalayzer.Views.Controls;

namespace ATTrafficAnalayzer.Views.Screens
{
    /// <summary>
    /// Interaction logic for Summary.xaml
    /// </summary>
    public partial class SummaryConfig : IView
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

        void IView.DateRangeChangedHandler(object sender, Toolbar.DateRangeChangedEventHandlerArgs args)
        {
            DateRangeChangedHandler(sender, args);
        }

        public void ReportChangedHandler(object sender, ReportBrowser.SelectedReporChangeEventHandlerArgs args)
        {
            _configuration = _dbHelper.GetConfiguration(args.ReportName);
            FillSummary();
        }

        public event VolumeAndDateCountsDontMatchHandler VolumeDateCountsDontMatch;

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

            Rows.Add(new SummaryRow
                {  DetectorsIn = {1, 2, 3}, 
                DetectorsOut = {4, 5},
                IntersectionIn = 4012,
                IntersectionOut = 4013,
                RouteName = "FROM SAINT HELIERS TO HOWICK"});
       
        }
    }

    public class SummaryRow
    {
        public SummaryRow()
        {
            DetectorsIn = new List<int>();
            DetectorsOut = new List<int>();
        }

        public string RouteName { get; set; }
        public int IntersectionIn { get; set; }
        public int IntersectionOut { get; set; }
        public List<int> DetectorsIn { get; set; }
        public List<int> DetectorsOut { get; set; }

    }

    public class DetectorsListToStringConverter : MarkupExtension, IValueConverter
    {
        private static DetectorsListToStringConverter _converter;
        
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _converter ?? (_converter = new DetectorsListToStringConverter());
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //What the GUI sees
            var list = value as List<int>;
            Console.WriteLine("LIST: " + list);
            var sb = new StringBuilder();
            for (int i = 0; i < list.Count; i++)
            {
                sb.Append(list[i]);
                if (i < list.Count - 1)
                {
                    sb.Append(", ");
                }
            }

            return sb.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
           //What the model sees
            var str = value as String;
            Console.WriteLine("STR" + str);
            return str.Split(new[] {", "}, StringSplitOptions.None).Select(s => int.Parse(s)).ToList();
        }
    
    }
}