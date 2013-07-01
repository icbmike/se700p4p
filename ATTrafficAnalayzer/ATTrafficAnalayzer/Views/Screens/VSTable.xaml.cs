using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using ATTrafficAnalayzer.VolumeModel;

namespace ATTrafficAnalayzer.Views
{

    /// <summary>
    /// Interaction logic for VSSCreen.xaml
    /// </summary>
    public partial class VsTable
    {
        private readonly SettingsTray _settings;

        private VolumeDbHelper _volumeDbHelper;
        private ReportConfiguration _configuration;


        public VsTable(SettingsTray settings, string configName)
        {
            _settings = settings;
            _volumeDbHelper = VolumeDbHelper.GetDbHelper();

            //Retrieve the config for the supplied name
            _configuration = _volumeDbHelper.GetConfiguration(configName);

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
                ContainerStackPanel.Children.Add(dg);    
            }           

            Logger.Info("constructed view", "VS table");
        }

        public DataTable GenerateVsTable(Approach approach)
        {
            
            // Create a DataGrid
            var vsDataTable = new DataTable();

            // Set column headings
            for (var i = 1; i <= 12; i++)
            {
                vsDataTable.Columns.Add(i.ToString(), typeof(string));
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
                    approachVolumes.AddRange(_volumeDbHelper.GetVolumes(_configuration.Intersection, detector, _settings.StartDate,
                                                                  _settings.EndDate));
                }
                else
                {
                    List<int> detectorVolumes = _volumeDbHelper.GetVolumes(_configuration.Intersection, detector, _settings.StartDate,
                                                                  _settings.EndDate);
                    approachVolumes = approachVolumes.Zip(detectorVolumes, (i, i1) => i + i1).ToList();
                }

            }
            // Get volume store data //12 hours
            for (var i = 0; i < 12; i++)
            {
                var row = vsDataTable.NewRow();
                for (var j = 0; j < 12; j++)
                {
                   //row[j] = _volumeStore.GetVolume(intersection, detector, dates[i * 12 + j]);
                   row[j] = approachVolumes[i*12 + j];
                }
                vsDataTable.Rows.Add(row);
            }

            return vsDataTable;
        }
    

    }
}