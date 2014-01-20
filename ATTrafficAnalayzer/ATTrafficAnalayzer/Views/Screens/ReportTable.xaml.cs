using System.Text;
using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Models.ReportConfiguration;
using ATTrafficAnalayzer.Models.Settings;
using ATTrafficAnalayzer.Views.Controls;
using System;

namespace ATTrafficAnalayzer.Views.Screens
{
    /// <summary>
    /// Interaction logic for VSSCreen.xaml
    /// </summary>
    public partial class ReportTable : IView
    {
        private DateSettings _dateSettings;
     
        private Configuration _configuration;

        readonly IDataSource _dataSource;

        /// <summary>
        /// Constructor create a component displaying the specified config
        /// </summary>
        /// <param name="dateSettings">Lets the graph view get the date range at the time of construction</param>
        /// <param name="configName">The config to be displayed</param>
        /// <param name="dataSource">The dataSource to get volume information from</param>
        public ReportTable(DateSettings dateSettings, string configName, IDataSource dataSource)
        {
            _dataSource = dataSource;
            _configuration = _dataSource.GetConfiguration(configName);
            _dateSettings = dateSettings;
           
            InitializeComponent();

            Render();
        }

        /// <summary>
        /// Displays the grid
        /// </summary>
        private void Render()
        {
            ScreenTitle.Content = _configuration.Name;

            //Remove all exisitng approaches!
            ApproachesStackPanel.Children.Clear();
            OverallSummaryTextBlock.Inlines.Clear();

            var stringBuilder = new StringBuilder();
            stringBuilder.Append("Busiest Approach: ");
            stringBuilder.AppendLine("[b]" + _configuration.GetBusiestApproach(_dateSettings).Name + "[/b]");

            stringBuilder.Append("Busiest AM Hour: ");
            stringBuilder.Append("[b]" + _configuration.GetAMPeakPeriod(_dateSettings).ToShortTimeString() + "[/b]");
            stringBuilder.Append(" with volume: ");
            stringBuilder.AppendLine("[b]" + _configuration.GetAMPeakVolume(_dateSettings) + "[/b]");

            stringBuilder.Append("Busiest PM Hour: ");
            stringBuilder.Append("[b]" + _configuration.GetPMPeakPeriod(_dateSettings).ToShortTimeString() + "[/b]");
            stringBuilder.Append(" with volume: ");
            stringBuilder.AppendLine("[b]" + _configuration.GetPMPeakVolume(_dateSettings) + "[/b]");

            OverallSummaryTextBlock.Html = stringBuilder.ToString();
            _configuration.Approaches.ForEach(approach => ApproachesStackPanel.Children.Add(new ApproachTable(approach, _configuration.Intersection, _dateSettings)));

        }                                                                                                                                                   

        /// <summary>
        /// Handler for DateRangeChanged event from Toolbar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void DateSettingsChanged(DateSettings newSettings)
        {
            if (!newSettings.StartDate.Equals(_dateSettings.StartDate) || !newSettings.EndDate.Equals(_dateSettings.EndDate) || !newSettings.Interval.Equals(_dateSettings.Interval))
            {
                _dateSettings = newSettings;
                Render();
            }
        }

        /// <summary>
        /// Event Handler for ReportChanged event from Configuration Browser
        /// </summary>
        /// <param name="newSelection"></param>
        public void SelectedReportChanged(string newSelection)
        {
            if (newSelection != null && !_configuration.Name.Equals(newSelection))
            {
                _configuration = _dataSource.GetConfiguration(newSelection);
                Render();
            }
        }

        public event VolumeAndDateCountsDontMatchHandler VolumeDateCountsDontMatch;
    }

}