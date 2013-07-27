using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Models.Configuration;
using ATTrafficAnalayzer.Models.Settings;
using ATTrafficAnalayzer.Models.Volume;
using DataGrid = System.Windows.Controls.DataGrid;
using DataGridCell = System.Windows.Controls.DataGridCell;

namespace ATTrafficAnalayzer.Views.Screens
{
    /// <summary>
    /// Interaction logic for VSSCreen.xaml
    /// </summary>
    public partial class VsTable
    {
        private Measurement _maxTotal = new Measurement();
        private Measurement _maxAm = new Measurement();
        private Measurement _maxPm = new Measurement();
        private Measurement _peakHourAm = new Measurement();
        private Measurement _peakHourPm = new Measurement();

        private readonly SettingsTray _settings;
        private readonly ReportConfiguration _configuration;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="configName"></param>
        public VsTable(SettingsTray settings, string configName)
        {
            var dbHelper = DbHelper.GetDbHelper();
            _configuration = dbHelper.GetConfiguration(configName);
            _settings = settings;

            InitializeComponent();

            ScreenTitle.Content = _configuration.ConfigName;

            foreach (var approach in _configuration.Approaches)
            {
                var approachSummary = new TextBlock
                {
                    TextWrapping = TextWrapping.NoWrap,
                    Background = Brushes.GhostWhite,
                    Margin = new Thickness(20, 15, 20, 5),
                    Padding = new Thickness(5)
                };

                ContainerStackPanel.Children.Add(approachSummary);
                ContainerStackPanel.Children.Add(CreateVsTable(24, 0, approach));

                var amPeak = approach.GetAmPeak();
                var pmPeak = approach.GetPmPeak();
                
                approachSummary.Inlines.Add(new Bold(new Run(string.Format("Approach: {0} - Detectors: {1}\n", approach.Name, string.Join(", ", approach.Detectors)))));
                approachSummary.Inlines.Add(new Run(string.Format("AM Peak: {0} vehicles\n", amPeak)));
                approachSummary.Inlines.Add(new Run(string.Format("PM Peak: {0} vehicles\n", pmPeak)));
                approachSummary.Inlines.Add(new Run(string.Format("Total volume: {0} vehicles\n", approach.GetTotal())));

                _maxAm.CheckIfMax(amPeak, approach.Name);
                _maxPm.CheckIfMax(pmPeak, approach.Name);
                _maxTotal.CheckIfMax(approach.GetTotal(), approach.Name);
            }

            OverallSummaryTextBlock.Inlines.Add(new Bold(new Run(string.Format("{0} Overview\n", _configuration.ConfigName))));
            OverallSummaryTextBlock.Inlines.Add(new Run(string.Format("Busiest approach: {0} @ {1} vehicles\n", string.Join(", ", _maxTotal.GetApproachesAsString()), _maxTotal.GetValue())));
            OverallSummaryTextBlock.Inlines.Add(new Run(string.Format("Busiest AM hour: {0} @ {1} vehicles\n", string.Join(", ", _maxAm.GetApproachesAsString()), _maxAm.GetValue())));
            OverallSummaryTextBlock.Inlines.Add(new Run(string.Format("Busiest PM hour: {0} @ {1} vehicles\n", string.Join(", ", _maxPm.GetApproachesAsString()), _maxPm.GetValue())));
            OverallSummaryTextBlock.Inlines.Add(new Run(string.Format("AM peak period: {0} @ {1} vehicles\n", string.Join(", ", _peakHourAm.GetApproachesAsString()), _peakHourAm.GetValue())));
            OverallSummaryTextBlock.Inlines.Add(new Run(string.Format("PM peak period: {0} @ {1} vehicles", string.Join(", ", _peakHourPm.GetApproachesAsString()), _peakHourPm.GetValue())));

            Logger.Info("constructed view", "VS table");
        }


        /// <summary>
        /// Create a Data Grid to display volume store data
        /// </summary>
        /// <param name="limit">number of records</param>
        /// <param name="offset">starting column</param>
        /// <param name="approach"></param>
        /// <returns>a datagrid to which displays the volume data</returns>
        private DataGrid CreateVsTable(int limit, int offset, Approach approach)
        {
            var cellStyle = new Style(typeof(DataGridCell));
            cellStyle.Setters.Add(new Setter(BackgroundProperty, Brushes.Aqua));

            return new DataGrid
            {
                ItemsSource = approach.GetDataTable(_settings, _configuration.Intersection, limit, offset).AsDataView(),
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
        }
    }
}