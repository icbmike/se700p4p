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
        private readonly DbHelper _dbHelper;
        private readonly ReportConfiguration _configuration;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="configName"></param>
        public VsTable(SettingsTray settings, string configName)
        {
            _settings = settings;
            _dbHelper = DbHelper.GetDbHelper();
            _configuration = _dbHelper.GetConfiguration(configName);

            InitializeComponent();

            foreach (var approach in _configuration.Approaches)
            {
                // HEADER
                var header = new TextBlock { TextWrapping = TextWrapping.NoWrap };
                header.Inlines.Add(string.Format("Approach: {0} - Detectors: {1}\n", approach.Name, string.Join(", ", approach.Detectors)));
                ContainerStackPanel.Children.Add(header);

                // DATA
                CreateVolumeDisplay(approach, string.Format("AM Peak: {0}\n", GetPeak(approach, 12, 0)), CreateVsTable(approach, 12, 0));
                CreateVolumeDisplay(approach, string.Format("PM Peak: {0}\n", GetPeak(approach, 12, 12)), CreateVsTable(approach, 12, 12));
            }           

            Logger.Info("constructed view", "VS table");
        }

        private void CreateVolumeDisplay(Approach approach, string heading, DataGrid dataGrid)
        {
            var description = new TextBlock { TextWrapping = TextWrapping.NoWrap };
            description.Inlines.Add(heading);
            ContainerStackPanel.Children.Add(description);
            ContainerStackPanel.Children.Add(dataGrid);
        }

        /// <summary>
        /// Create a Data Grid to display volume store data
        /// </summary>
        /// <param name="approach"></param>
        /// <param name="limit">number of records</param>
        /// <param name="offset">starting hour</param>
        /// <returns>a datagrid to which displays the volume data</returns>
        private DataGrid CreateVsTable(Approach approach, int limit, int offset)
        {
            return new DataGrid
            {
                ItemsSource = GenerateVsTable(approach, limit, offset).AsDataView(),
                Margin = new Thickness(10),
                Width = Double.NaN,
                Height = 280,
                ColumnWidth = new DataGridLength(1, DataGridLengthUnitType.Star),
                IsReadOnly = true
            };
        }

        /// <summary>
        /// Populate the Data Grid with volume data
        /// </summary>
        /// <param name="approach"></param>
        /// <param name="limit">number of records</param>
        /// <param name="offset">starting hour</param>
        /// <returns>A DataTable which displays volume data</returns>
        private DataTable GenerateVsTable(Approach approach, int limit, int offset)
        {
            var vsDataTable = new DataTable();

            // Column headings
            for (var i = 0; i <= 12; i++)
                vsDataTable.Columns.Add(i == 0 ? "-" : string.Format("{0} hours", i - 1), typeof(string));

            // List dates
            var dates = new List<DateTime>();
            for (var date = _settings.StartDate; date < _settings.EndDate; date = date.AddMinutes(_settings.Interval))
                dates.Add(date);

            // Get volume store data 12 hours
            var approachVolumes = GetVolumesList(approach);
            for (var i = offset; i < offset + limit; i++)
            {
                var row = vsDataTable.NewRow();
                for (var j = 0; j < 13; j++)
                {
                    if (j == 0)
                        row[j] = _settings.Interval * i + " mins";
                    else
                        row[j] = approachVolumes[i * 12 + j - 1];
                }
                vsDataTable.Rows.Add(row);
            }

            //TODO totals

            return vsDataTable;
        }

        /// <summary>
        /// Retrieve volume data
        /// </summary>
        /// <param name="approach"></param>
        /// <returns>a list of volumes for a single day</returns>
        private List<int> GetVolumesList(Approach approach)
        {
            var approachVolumes = new List<int>();
            foreach (var detector in approach.Detectors)
            {
                if (approachVolumes.Count == 0)
                {
                    approachVolumes.AddRange(_dbHelper.GetVolumes(_configuration.Intersection, detector, _settings.StartDate, _settings.EndDate));
                }
                else
                {
                    var detectorVolumes = _dbHelper.GetVolumes(_configuration.Intersection, detector, _settings.StartDate, _settings.EndDate);
                    approachVolumes = approachVolumes.Zip(detectorVolumes, (i, i1) => i + i1).ToList();
                }
            }
            return approachVolumes;
        } 

        /// <summary>
        /// Find the peak values for a specified approach
        /// </summary>
        /// <param name="approach"></param>
        /// <param name="limit">number of records</param>
        /// <param name="offset">starting hour</param>
        /// <returns>max volume record</returns>
        private int GetPeak(Approach approach, int limit, int offset)
        {
            var volumesList = GetVolumesList(approach);
            return volumesList.GetRange(offset * 12, limit * 12).Max();
        }
    }
}