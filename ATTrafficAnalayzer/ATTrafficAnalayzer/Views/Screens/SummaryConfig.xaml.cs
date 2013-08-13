using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
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
    public partial class SummaryConfig
    {
        private readonly DbHelper _dbHelper;

        #region events

        public event ConfigurationSavedEventHander ConfigurationSaved;
        #endregion

        public SummaryConfig()
        {
            _dbHelper = DbHelper.GetDbHelper();

            Rows = new ObservableCollection<SummaryRow>();

            InitializeComponent();
            SummaryDataGrid.DataContext = this;

            FillSummary();
        }

        public ObservableCollection<SummaryRow> Rows { get; set; }

        private void FillSummary()
        {

            Rows.Add(new SummaryRow
                {
                    DetectorsIn = { 1, 2, 3 },
                    DetectorsOut = { 4, 5 },
                    RouteName = "FROM SAINT HELIERS TO HOWICK"
                });

        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var configName = ConfigNameTextBox.Text;

            //Do save
            foreach (var row in Rows)
            {

            }

            //Fire saved event
            if (ConfigurationSaved != null) ConfigurationSaved(this, new ConfigurationSavedEventArgs(configName));

        }

       
    }

    public class SummaryRow
    {
        public SummaryRow()
        {
            DetectorsIn = new List<int>();
            DetectorsOut = new List<int>();
        }
        private ObservableCollection<int> _intersections;

        public ObservableCollection<int> Intersections
        {
            get
            {
                if (_intersections == null)
                {
                    _intersections = new ObservableCollection<int>(DbHelper.GetIntersections());
                }
                return _intersections;
            }
        }
        public string RouteName { get; set; }
        public int SelectedIntersectionIn { get; set; }
        public int SelectedIntersectionOut { get; set; }
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
            return str.Split(new[] { ", " }, StringSplitOptions.None).Select(s => int.Parse(s)).ToList();
        }

    }
}