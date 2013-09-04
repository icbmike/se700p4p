using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Models.Configuration;
using ATTrafficAnalayzer.Models.Settings;

namespace ATTrafficAnalayzer.Views.Screens
{
    /// <summary>
    /// Interaction logic for Summary.xaml
    /// </summary>
    public partial class SummaryConfig : IConfigScreen
    {
        private readonly IDataSource _dbHelper;
        private bool IsNewConfig = true;
        private string _oldName;

        public event ConfigurationSavedEventHander ConfigurationSaved;

        public SummaryConfig()
        {
            
            if (_dbHelper == null) _dbHelper = DbHelper.GetDbHelper();

            Rows = new ObservableCollection<SummaryRow>();

            InitializeComponent();
            SummaryDataGrid.DataContext = this;
        }

        public SummaryConfig(string summaryToBeEdited) : this()
        {
            IsNewConfig = false;
            _oldName = summaryToBeEdited;
            ConfigNameTextBox.Text = summaryToBeEdited;

            foreach (var summaryRow in _dbHelper.GetSummaryConfig(summaryToBeEdited))
            {
                Rows.Add(summaryRow);
            }
            
        }

        public ObservableCollection<SummaryRow> Rows { get; set; }

        private void SaveButton_OnClick(object sender, RoutedEventArgs e)
        {

            if (!IsNewConfig)
            {
                try
                {
                    DataTableHelper.GetDataTableHelper().RemoveReport(_oldName, Mode.Summary);
                }
                catch (Exception exception)
                {
                    Logger.Error(exception, "SummaryConfig");
                }
            }

            var configName = ConfigNameTextBox.Text;

            //Check we're in a valid state
            if (Rows.Count == 0)
            {
                MessageBox.Show("No rows in table");
                return;
            }
            if (Rows.Any(row => !row.IsValid))
            {
                MessageBox.Show("Invalid data in tables");
                return;
            }
            if (configName.Equals(""))
            {
                MessageBox.Show("Summary Name is empty");
                return;
            }

            //Do save
            DbHelper.GetDbHelper().SaveMonthlySummaryConfig(configName, Rows);

            //Fire saved event
            if (ConfigurationSaved != null) ConfigurationSaved(this, new ConfigurationSavedEventArgs(configName));
        }

        private void ConfigNameTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (IsNewConfig)
            {
                var configTextBox = (TextBox)sender;

                for (var count = 1; ; count++)
                {
                    if (!_dbHelper.SummaryExists("Summary " + count))
                    {
                        configTextBox.Text = "Summary " + count;
                        break;
                    }
                }
            }
        }

        private void UserControl_GotFocus_1(object sender, RoutedEventArgs e)
        {
            Popup.Visibility = Visibility.Collapsed;
        }
    }
}