using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Models.Configuration;
using ATTrafficAnalayzer.Models.Settings;
using ATTrafficAnalayzer.Views.Controls;

namespace ATTrafficAnalayzer.Views.Screens
{
    /// <summary>
    /// Interaction logic for Summary.xaml
    /// </summary>
    public partial class Summary
    {
        private readonly DbHelper _dbHelper;
        private DateTime _endDate;
        private DateTime _startDate;
        private Report _configuration;

        public Summary(SettingsTray settings, string configName)
        {
            _startDate = settings.StartDate;
            _endDate = settings.EndDate;
            _dbHelper = DbHelper.GetDbHelper();
            _configuration = _dbHelper.GetConfiguration(configName);

            InitializeComponent();

            FillSummary();
        }

        internal void DateRangeChangedHandler(object sender, Toolbar.DateRangeChangedEventHandlerArgs args)
        {
            if (!args.startDate.Equals(_startDate) || !args.endDate.Equals(_endDate))
            {
                _startDate = args.startDate;
                _endDate = args.endDate;
            }
            FillSummary();
        }

        public void ReportChangedHandler(object sender, ReportBrowser.SelectedReporChangeEventHandlerArgs args)
        {
            _configuration = _dbHelper.GetConfiguration(args.ReportName);
            FillSummary();
        }

        internal void ReportChangedHandler(object sender, Toolbar.DateRangeChangedEventHandlerArgs args)
        {
            if (!args.startDate.Equals(_startDate) || !args.endDate.Equals(_endDate))
            {
                _startDate = args.startDate;
                _endDate = args.endDate;
            }
            FillSummary();
        }

        private void FillSummary()
        {
            ApproachesStackPanel.Children.Clear();

            ScreenTitle.Content = _configuration.ConfigName;

            var timeSpan = _endDate - _startDate;
            for (var day = 0; day < timeSpan.TotalDays; day++)
            {
                var dayLabel = new Label
                {
                    Content = "Day of the Week",
                    FontSize = 20.0,
                    FontWeight = new FontWeight()
                };
                ApproachesStackPanel.Children.Add(dayLabel);

                foreach (var approach in _configuration.Approaches)
                {
                    ApproachesStackPanel.Children.Add(CreateApproachSummary(approach));
                }
            }
        }

        private static TextBlock CreateApproachSummary(Approach approach)
        {
            var approachSummary = new TextBlock
            {
                Margin = new Thickness(5),
                Padding = new Thickness(10),
                Background = System.Windows.Media.Brushes.GhostWhite
            };

            approachSummary.Inlines.Add(new Bold(new Run(string.Format("Approach: {0} - Detectors: {1}\n", approach.Name, string.Join(", ", approach.Detectors)))));
            approachSummary.Inlines.Add(new Run(string.Format("AM Peak: {0} vehicles @ {1}\n", approach.AmPeak.GetValue(), approach.AmPeak.GetApproachesAsString())));
            approachSummary.Inlines.Add(new Run(string.Format("PM Peak: {0} vehicles @ {1}\n", approach.PmPeak.GetValue(), approach.PmPeak.GetApproachesAsString())));
            approachSummary.Inlines.Add(new Run(string.Format("Total volume: {0} vehicles", approach.GetTotal())));

            return approachSummary;
        }
    }
}
