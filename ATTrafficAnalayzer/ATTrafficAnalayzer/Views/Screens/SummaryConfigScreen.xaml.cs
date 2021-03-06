﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Models.ReportConfiguration;
using ATTrafficAnalayzer.Modes;

namespace ATTrafficAnalayzer.Views.Screens
{
    /// <summary>
    /// Interaction logic for Summary.xaml
    /// </summary>
    public partial class SummaryConfigScreen
    {
        private readonly IDataSource _dataSource;
        private bool IsNewConfig = true;
        private string _oldName;
        private SummaryConfiguration _configuration;


        /// <summary>
        /// Default constructor for when constructing a new summary config
        /// </summary>
        public SummaryConfigScreen(IDataSource dataSource)
        {
            _dataSource = dataSource;
            
            InitializeComponent();
            SummaryDataGrid.DataContext = this;
            Rows = new ObservableCollection<SummaryRow>();
        }

        /// <summary>
        /// Constructor for when editing an existing summary config
        /// </summary>
        /// <param name="summaryToBeEdited">The summary config to be edited</param>
        public SummaryConfigScreen(string summaryToBeEdited, IDataSource dataSource) : this(dataSource)
        {
            IsNewConfig = false;
            _oldName = summaryToBeEdited;
            ConfigNameTextBox.Text = summaryToBeEdited;

            foreach (var summaryRow in _dataSource.GetSummaryConfig(summaryToBeEdited).SummaryRows)
            {
                Rows.Add(summaryRow);
            }
            
        }

        public ObservableCollection<SummaryRow> Rows { get; set; }

        public SummaryConfiguration Configuration
        {
            get { return _configuration; }
            set { _configuration = value;
                Render();
            }
        }

        private void Render()
        {
            if (Configuration == null) return;

            Rows.Clear();

            IsNewConfig = false;
            _oldName = Configuration.Name;
            ConfigNameTextBox.Text = Configuration.Name;
            var intersections = _dataSource.GetIntersections();

            foreach (var summaryRow in _dataSource.GetSummaryConfig(Configuration.Name).SummaryRows)
            {
                intersections.ForEach(i => summaryRow.Intersections.Add(i));
                Rows.Add(summaryRow);
            }

        }

        /// <summary>
        /// Click handler for the save button.
        /// </summary>
        /// <param name="sender">The button</param>
        /// <param name="e"></param>
        private void SaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            //If we're editing an existing config, delete it before saving it like a regular save
            if (!IsNewConfig)
            {
                try
                {
                    _dataSource.RemoveSummary(_oldName);
                }
                catch (Exception exception)
                {
                    Logger.Error(exception, "SummaryConfigScreen");
                }
            }
            //Get the config name
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
                MessageBox.Show("Summary ApproachName is empty");
                return;
            }

            //Do save
            _dataSource.SaveMonthlySummaryConfig(configName, Rows);

            //Fire saved event
            if (ConfigurationSaved != null) ConfigurationSaved(this, new ConfigurationSavedEventArgs(configName, null));
        }

        public event ConfigurationSavedEventHandler ConfigurationSaved;

        /// <summary>
        /// Handler to automatically generate a name for a new config
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConfigNameTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (IsNewConfig)
            {
                var configTextBox = (TextBox)sender;

                for (var count = 1; ; count++)
                {
                    if (!_dataSource.SummaryExists("Summary " + count))
                    {
                        configTextBox.Text = "Summary " + count;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Handler to remove ugly red popup
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_GotFocus_1(object sender, RoutedEventArgs e)
        {
            Popup.Visibility = Visibility.Collapsed;
        }


        private void SummaryDataGridOnInitializingNewItem(object sender, InitializingNewItemEventArgs e)
        {
            var summaryRow = e.NewItem as SummaryRow;
            _dataSource.GetIntersections().ForEach(i => summaryRow.Intersections.Add(i));
        }
    }
}