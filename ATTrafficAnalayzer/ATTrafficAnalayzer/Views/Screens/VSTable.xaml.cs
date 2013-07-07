using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Models.Configuration;
using ATTrafficAnalayzer.Models.Settings;

namespace ATTrafficAnalayzer.Views.Screens
{

    /// <summary>
    /// Interaction logic for VSSCreen.xaml
    /// </summary>
    public partial class VsTable
    {
        private readonly SettingsTray _settings;

        private DbHelper _dbHelper;
        private ReportConfiguration _configuration;


        public VsTable(SettingsTray settings, string configName)
        {
            _settings = settings;
            _dbHelper = DbHelper.GetDbHelper();

            //Retrieve the config for the supplied name
            _configuration = _dbHelper.GetConfiguration(configName);

            InitializeComponent();

            var dateLabel = new Label { Content = string.Format("Day: {0} Time range: {1}", "", ""), Margin = new Thickness(10) };
            ContainerStackPanel.Children.Add(dateLabel);

            foreach (var approach in _configuration.Approaches)
            {
                var dg = new DataGrid
                {
                    ItemsSource = GenerateVsTable(approach).AsDataView(),
                    Margin = new Thickness(10),
                    Width = Double.NaN,
                    Height = 280,
                    ColumnWidth = new DataGridLength(1, DataGridLengthUnitType.Star),
                    IsReadOnly = true
                };
                var label = new Label();
                var labelText = approach.Name + " - Detectors: ";
                for (var i = 0; i < approach.Detectors.Count; i++)
                {
                    if ((i + 1) == approach.Detectors.Count)
                    {
                        labelText += approach.Detectors[i];
                    }
                    else
                    {
                        labelText += approach.Detectors[i] + ", ";
                    }
                }
                label.Content = labelText;
                ContainerStackPanel.Children.Add(label);
                ContainerStackPanel.Children.Add(dg);    
            }           

            Logger.Info("constructed view", "VS table");
        }

        public DataTable GenerateVsTable(Approach approach)
        {
            
            // Create a DataGrid
            var vsDataTable = new DataTable();

            // Set column headings
            for (var i = 0; i <= 12; i++)
            {
                if (i == 0)
                    vsDataTable.Columns.Add("_", typeof(string));
                else
                    vsDataTable.Columns.Add((i - 1).ToString(), typeof(string));
            }

            // List dates
            var dates = new List<DateTime>();
            for (var date = _settings.StartDate; date < _settings.EndDate; date = date.AddMinutes(_settings.Interval))
            {
                dates.Add(date);
            }
            var approachVolumes = new List<int>();
            foreach (var detector in approach.Detectors)
            {
                if (approachVolumes.Count == 0)
                {
                    approachVolumes.AddRange(_dbHelper.GetVolumes(_configuration.Intersection, detector, _settings.StartDate,
                                                                  _settings.EndDate));
                }
                else
                {
                    List<int> detectorVolumes = _dbHelper.GetVolumes(_configuration.Intersection, detector, _settings.StartDate,
                                                                  _settings.EndDate);
                    approachVolumes = approachVolumes.Zip(detectorVolumes, (i, i1) => i + i1).ToList();
                }

            }
            // Get volume store data //12 hours
            for (var i = 0; i < 12; i++)
            {
                var row = vsDataTable.NewRow();
                for (var j = 0; j < 13; j++)
                {
                    if (j == 0)
                        row[j] = ": " + _settings.Interval * i;
                    else
                        row[j] = approachVolumes[i * 12 + j];
                }
                vsDataTable.Rows.Add(row);
            }

            return vsDataTable;
        }
    

    }
}