using System;
using System.Collections.ObjectModel;
using System.Windows;
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
                Console.WriteLine(row);
            }

            //Fire saved event
            if (ConfigurationSaved != null) ConfigurationSaved(this, new ConfigurationSavedEventArgs(configName));

        }
       
    }
}