using System;
using System.Collections.ObjectModel;
using System.Linq;
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

        }

        public ObservableCollection<SummaryRow> Rows { get; set; }


        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var configName = ConfigNameTextBox.Text;

            if (Rows.Count == 0)
            {
                MessageBox.Show("No rows in table");
                return;
            }

            //Do save
            if (Rows.Any(row => !row.IsValid))
            {
                MessageBox.Show("Invalid data in tables");
                return;
            }

            DbHelper.GetDbHelper().SaveMonthlySummaryConfig(configName, Rows);
            Console.WriteLine("Woooo");
            //Fire saved event
            if (ConfigurationSaved != null) ConfigurationSaved(this, new ConfigurationSavedEventArgs(configName));

        }
       
    }
}