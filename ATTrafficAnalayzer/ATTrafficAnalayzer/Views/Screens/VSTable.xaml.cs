using System.Data;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Models.Configuration;
using ATTrafficAnalayzer.Models.Settings;
using ATTrafficAnalayzer.Models.Volume;
using ATTrafficAnalayzer.Views.Controls;
using DataGridCell = System.Windows.Controls.DataGridCell;
using System;

namespace ATTrafficAnalayzer.Views.Screens
{
    /// <summary>
    /// Interaction logic for VSSCreen.xaml
    /// </summary>
    public partial class VsTable
    {
        private readonly Measurement _maxTotal = new Measurement();
        private readonly Measurement _maxAm = new Measurement();
        private readonly Measurement _maxPm = new Measurement();
        private readonly Measurement _peakHourAm = new Measurement();
        private readonly Measurement _peakHourPm = new Measurement();

        private SettingsTray _settings;
        private DateTime startDate;
        private DateTime endDate;
        private int interval;

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

            this._settings = settings;
            this.startDate = settings.StartDate;
            this.endDate = settings.EndDate;
            this.interval = settings.Interval;

            InitializeComponent();

            ScreenTitle.Content = _configuration.ConfigName;

            RenderTable();
            
        }


        private void RenderTable()
        {
            //Clear all the things!
            ApproachesStackPanel.Children.Clear();

            _maxAm.ClearApproaches();
            _maxPm.ClearApproaches();
            _maxTotal.ClearApproaches();
            _peakHourAm.ClearApproaches();
            _peakHourPm.ClearApproaches(); 

            OverallSummaryTextBlock.Inlines.Clear();
            
            //Add all the things!

            var timeSpan = endDate - startDate;
            for (var day = 0; day < timeSpan.TotalDays; day++)
            {
                foreach (var approach in _configuration.Approaches)
                {

                    ApproachesStackPanel.Children.Add(CreateApproachDisplay(approach, day));

                    _maxTotal.CheckIfMax(approach.GetTotal(), approach.Name);
                    _maxAm.CheckIfMax(approach.AmPeak.GetValue(), approach.Name);
                    _maxPm.CheckIfMax(approach.PmPeak.GetValue(), approach.Name);
                    _peakHourAm.CheckIfMax(approach.AmPeak.GetValue(),
                        string.Format("{0} ({1})", approach.Name, approach.AmPeak.GetApproachesAsString()));
                    _peakHourPm.CheckIfMax(approach.PmPeak.GetValue(),
                        string.Format("{0} ({1})", approach.Name, approach.PmPeak.GetApproachesAsString()));
                }
            }

            OverallSummaryTextBlock.Inlines.Add(new Bold(new Run(string.Format("{0} Overview\n", _configuration.ConfigName))));
            OverallSummaryTextBlock.Inlines.Add(new Run(string.Format("Busiest approach: {0} with {1} vehicles\n", string.Join(", ", _maxTotal.GetApproachesAsString()), _maxTotal.GetValue())));
            OverallSummaryTextBlock.Inlines.Add(new Run(string.Format("Busiest AM hour: {0} with {1} vehicles\n", string.Join(", ", _maxAm.GetApproachesAsString()), _maxAm.GetValue())));
            OverallSummaryTextBlock.Inlines.Add(new Run(string.Format("Busiest PM hour: {0} with {1} vehicles\n", string.Join(", ", _maxPm.GetApproachesAsString()), _maxPm.GetValue())));
            OverallSummaryTextBlock.Inlines.Add(new Run(string.Format("AM peak period: {0} with {1} vehicles\n", string.Join(", ", _peakHourAm.GetApproachesAsString()), _peakHourAm.GetValue())));
            OverallSummaryTextBlock.Inlines.Add(new Run(string.Format("PM peak period: {0} with {1} vehicles", string.Join(", ", _peakHourPm.GetApproachesAsString()), _peakHourPm.GetValue())));

            Logger.Info("constructed view", "VS table");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="approach"></param>
        /// <param name="day"></param>
        /// <returns></returns>
        private ApproachTableDisplay CreateApproachDisplay(Approach approach, int day)
        {
            var approachDisplay = new ApproachTableDisplay();

            var cellStyle = new Style(typeof(DataGridCell));
            cellStyle.Setters.Add(new Setter(BackgroundProperty, Brushes.Aqua));
            approachDisplay.ApproachDataGrid.ItemsSource = approach.GetDataTable(_settings, _configuration.Intersection, 24, 0, day).AsDataView();
            approachDisplay.ApproachDataGrid.CellStyle = cellStyle;

            approachDisplay.ApproachSummary.Inlines.Add(new Bold(new Run(string.Format("Approach: {0} - Detectors: {1}\n", approach.Name, string.Join(", ", approach.Detectors)))));
            approachDisplay.ApproachSummary.Inlines.Add(new Run(string.Format("AM Peak: {0} vehicles @ {1}\n", approach.AmPeak.GetValue(), approach.AmPeak.GetApproachesAsString())));
            approachDisplay.ApproachSummary.Inlines.Add(new Run(string.Format("PM Peak: {0} vehicles @ {1}\n", approach.PmPeak.GetValue(), approach.PmPeak.GetApproachesAsString())));
            approachDisplay.ApproachSummary.Inlines.Add(new Run(string.Format("Total volume: {0} vehicles", approach.GetTotal())));

            return approachDisplay;
        }

        internal void DateRangeChangedHandler(object sender, Toolbar.DateRangeChangedEventHandlerArgs args)
        {

            if (!args.startDate.Equals(startDate) || !args.endDate.Equals(endDate) || !args.interval.Equals(interval))
            {
                //RenderTable() is a time consuming operation.
                //We dont want to do it if we don't have to.

                startDate = args.startDate;
                endDate = args.endDate;
                interval = args.interval;

                RenderTable();
            }
        }
    }
}