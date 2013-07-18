using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using System.Windows.Documents;
using System.Windows.Media;
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
                var approachSummary = new TextBlock
                {
                    TextWrapping = TextWrapping.NoWrap,
                    Background = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    Margin = new Thickness(20, 15, 20, 5),
                    Padding = new Thickness(5)
                };
                approachSummary.Inlines.Add(new Bold(new Run(string.Format("Approach: {0} - Detectors: {1}\n", approach.Name, string.Join(", ", approach.Detectors)))));
                approachSummary.Inlines.Add(new Italic(new Run(string.Format("Combined Peak: {0}\n", CalculatePeak(approach, 24, 0)))));
                ContainerStackPanel.Children.Add(approachSummary);

                ContainerStackPanel.Children.Add(CreateVsTable(approach, 24, 0));
            }

            Logger.Info("constructed view", "VS table");
        }

        /// <summary>
        /// Create a Data Grid to display volume store data
        /// </summary>
        /// <param name="approach"></param>
        /// <param name="limit">number of records</param>
        /// <param name="offset">starting column</param>
        /// <returns>a datagrid to which displays the volume data</returns>
        private DataGrid CreateVsTable(Approach approach, int limit, int offset)
        {
            return new DataGrid
            {
                ItemsSource = GenerateVsTable(approach, limit, offset).AsDataView(),
                Margin = new Thickness(20, 5, 20, 10),
                Width = Double.NaN,
                Height = 270,
                ColumnWidth = new DataGridLength(1, DataGridLengthUnitType.Star),
                IsReadOnly = true,
                HeadersVisibility = DataGridHeadersVisibility.Column,
                AreRowDetailsFrozen = true,
                FrozenColumnCount = 1,
                CanUserSortColumns = false,
                CanUserResizeRows = false,
                CanUserReorderColumns = false,
                CanUserResizeColumns = false,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
                SelectionMode = DataGridSelectionMode.Extended
            };
        }

        /// <summary>
        /// Populate the Data Grid with volume data
        /// </summary>
        /// <param name="approach"></param>
        /// <param name="limit">number of records</param>
        /// <param name="offset">starting column</param>
        /// <returns>A DataTable which displays volume data</returns>
        private DataTable GenerateVsTable(Approach approach, int limit, int offset)
        {
            var vsDataTable = new DataTable();

            // Column headings
            for (var i = offset; i <= offset + limit; i++)
                vsDataTable.Columns.Add(i == 0 ? "-" : string.Format("{0} hours", i - 1), typeof(string));

            // List dates
            var dates = new List<DateTime>();
            for (var date = _settings.StartDate; date < _settings.EndDate; date = date.AddMinutes(_settings.Interval))
                dates.Add(date);

            // Get volume store data 12 hours
            var approachVolumes = GetVolumesList(approach);
            for (var rowIndex = 0; rowIndex < 12; rowIndex++)
            {
                var row = vsDataTable.NewRow();
                for (var columnIndex = 0; columnIndex < limit + 1; columnIndex++)
                {
                    if (columnIndex == 0)
                        row[columnIndex] = _settings.Interval * rowIndex + " mins";
                    else
                        row[columnIndex] = approachVolumes[(offset + columnIndex - 1) * 12 + rowIndex];
                }
                vsDataTable.Rows.Add(row);
            }

            var totalsRow = vsDataTable.NewRow();
            totalsRow[0] = "Total";
            for (var j = 0; j < limit; j++)
            {
                totalsRow[j + 1] = CalculateColumnTotal(approachVolumes, j, vsDataTable.Rows.Count);
            }
            vsDataTable.Rows.Add(totalsRow);

            return vsDataTable;
        }

        /// <summary>
        /// Calculates the total for each column in the datagrid
        /// </summary>
        /// <param name="data"></param>
        /// <param name="column"></param>
        /// <param name="numberOfRows"></param>
        /// <returns></returns>
        private static int CalculateColumnTotal(List<int> data, int column, int numberOfRows)
        {
            var total = 0;
            for (var i = 0; i < numberOfRows; i++)
            {
                total += data[i + column * numberOfRows];
            }
            return total;
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
        /// <param name="offset">starting column</param>
        /// <returns>max volume record</returns>
        private int CalculatePeak(Approach approach, int limit, int offset)
        {
            var volumesList = GetVolumesList(approach);
            return volumesList.GetRange(offset * 12, limit * 12).Max();
        }
    }
}