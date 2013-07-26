﻿using System;
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
using DataGrid = System.Windows.Controls.DataGrid;
using DataGridCell = System.Windows.Controls.DataGridCell;

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

        private int _approachTotal;

        public struct Measurement
        {
            public int Value;
            public List<string> Approaches;
        }

        private Measurement _maxTotal;
        private readonly Measurement _maxAm;
        private readonly Measurement _maxPm;
        private readonly Measurement _peakHourAm;
        private readonly Measurement _peakHourPm;

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

            ScreenTitle.Content = _configuration.ConfigName;

            _maxTotal.Value = 0;
            _maxTotal.Approaches = new List<string>();
            _maxAm.Value = 0;
            _maxAm.Approaches = new List<string>();
            _maxPm.Value = 0;
            _maxPm.Approaches = new List<string>();
            _peakHourAm.Value = 0;
            _peakHourAm.Approaches = new List<string>();
            _peakHourPm.Value = 0;
            _peakHourPm.Approaches = new List<string>();

            foreach (var approach in _configuration.Approaches)
            {
                _approachTotal = 0;

                var approachSummary = new TextBlock
                {
                    TextWrapping = TextWrapping.NoWrap,
                    Background = Brushes.GhostWhite,
                    Margin = new Thickness(20, 15, 20, 5),
                    Padding = new Thickness(5)
                };
                
                ContainerStackPanel.Children.Add(approachSummary);
                ContainerStackPanel.Children.Add(CreateVsTable(approach, 24, 0));

                var amPeak = CalculatePeak(approach, 12, 0);
                _maxAm = CheckIfMax(_maxAm, amPeak, approach.Name);

                var pmPeak = CalculatePeak(approach, 12, 12);
                _maxPm = CheckIfMax(_maxPm, pmPeak, approach.Name);

                approachSummary.Inlines.Add(new Bold(new Run(string.Format("Approach: {0} - Detectors: {1}\n", approach.Name, string.Join(", ", approach.Detectors)))));
                approachSummary.Inlines.Add(new Run(string.Format("AM Peak: {0} vehicles\n", amPeak)));
                approachSummary.Inlines.Add(new Run(string.Format("PM Peak: {0} vehicles\n", pmPeak)));
                approachSummary.Inlines.Add(new Run(string.Format("Total volume: {0} vehicles\n", _approachTotal)));
            }

            OverallSummaryTextBlock.Inlines.Add(new Bold(new Run(string.Format("{0} Overview\n", _configuration.ConfigName))));
            OverallSummaryTextBlock.Inlines.Add(new Run(string.Format("Busiest approach: {0} @ {1} vehicles\n", string.Join(", ", _maxTotal.Approaches), _maxTotal.Value)));
            OverallSummaryTextBlock.Inlines.Add(new Run(string.Format("Busiest AM hour: {0} @ {1} vehicles\n", string.Join(", ", _maxAm.Approaches), _maxAm.Value)));
            OverallSummaryTextBlock.Inlines.Add(new Run(string.Format("Busiest PM hour: {0} @ {1} vehicles\n", string.Join(", ", _maxPm.Approaches), _maxPm.Value)));
            OverallSummaryTextBlock.Inlines.Add(new Run(string.Format("AM peak period: {0} @ {1} vehicles\n", string.Join(", ", _peakHourAm.Approaches), _peakHourAm.Value)));
            OverallSummaryTextBlock.Inlines.Add(new Run(string.Format("PM peak period: {0} @ {1} vehicles", string.Join(", ", _peakHourPm.Approaches), _peakHourPm.Value)));

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
            // CREATE DATA TABLE

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
            var maxRowValue = 0;
            var maxRowIndex = new List<int>();
            for (var j = 0; j < limit; j++)
            {
                var total = CalculateColumnTotal(approachVolumes, j, vsDataTable.Rows.Count);
                totalsRow[j + 1] = total;
                _approachTotal += total;
                if (total > maxRowValue)
                {
                    maxRowValue = total;
                    maxRowIndex.Clear();
                    maxRowIndex.Add(j + 1);
                }
                else if (maxRowValue == total)
                {
                    maxRowIndex.Add(j + 1);
                }
            }
            vsDataTable.Rows.Add(totalsRow);

            _maxTotal = CheckIfMax(_maxTotal, _approachTotal, approach.Name);

            var cellStyle = new Style(typeof(DataGridCell));
            cellStyle.Setters.Add(new Setter(BackgroundProperty, Brushes.Aqua));

            var dataGrid = new DataGrid
            {
                ItemsSource = vsDataTable.AsDataView(),
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
                SelectionMode = DataGridSelectionMode.Extended,
                CellStyle = cellStyle,
                FontSize = 11
            };

            return dataGrid;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="measurement"></param>
        /// <param name="value"></param>
        /// <param name="approachName"></param>
        /// <returns></returns>
        private static Measurement CheckIfMax(Measurement measurement, int value, string approachName)
        {
            if (value > measurement.Value)
            {
                measurement.Value = value;
                measurement.Approaches.Clear();
                measurement.Approaches.Add(approachName);
            }
            else if (value == measurement.Value)
            {
                measurement.Approaches.Add(approachName);
            }
            return measurement;
        }
    }
}