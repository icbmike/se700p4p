using System.Collections.Generic;
using System.Windows;
using ATTrafficAnalayzer.Models;
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

        private readonly IDataSource _dataSource;
        private DateSettings _dateSettings;
        private List<StatsTable> _statsTables;
        private bool _hasWeekends;

        /// <summary>
        /// Constructor to display a summary table with the date range at the time of construction and the specified config
        /// </summary>
        /// <param name="dateSettings"></param>
        /// <param name="configName">Name of config to be displayed</param>
        /// <param name="dataSource">The source of the data</param>
        public SummaryTable(DateSettings dateSettings, string configName, IDataSource dataSource)
        {
            _configName = configName;
            _dateSettings = dateSettings;
            _dataSource = dataSource;

            _statsTables = new List<StatsTable>
            {
                new StatsTable(_dataSource, _dateSettings, _configName, "sample", (time, row) => time.Day)
            };
            InitializeComponent();

            Render();
        }

        /// <summary>
        /// Display the table
        /// </summary>
        private void Render()
        {
            ScreenTitle.Content = _configName;

            //Remove all previous tables
            StatsStackPanel.Children.Clear();
 
            _statsTables.ForEach(statsTable => StatsStackPanel.Children.Add(statsTable));

        }

        public void DateSettingsChanged(DateSettings newDateSettings)
        {
            _dateSettings = newDateSettings;
            _statsTables.ForEach(table => table.DateSettingsChanged(newDateSettings));
            
            Render();
        }

        public void SelectedReportChanged(string newSelection)
        {
            if (newSelection != null)
            {
                if (!_configName.Equals(newSelection))
                {
                    _configName = newSelection;

                    _statsTables.ForEach(table => table.SelectedReportChanged(newSelection));
                    Render();
                }
            }
        }

        public event VolumeAndDateCountsDontMatchHandler VolumeDateCountsDontMatch;


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
