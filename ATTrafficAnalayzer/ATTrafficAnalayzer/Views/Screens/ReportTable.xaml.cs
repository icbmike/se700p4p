using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Models.ReportConfiguration;
using ATTrafficAnalayzer.Models.Settings;

namespace ATTrafficAnalayzer.Views.Screens
{
    /// <summary>
    /// Interaction logic for VSSCreen.xaml
    /// </summary>
    public partial class ReportTable : IView
    {
        public DateSettings DateSettings { get; set; }
        public int Intersection { get { return _configuration.Intersection; }  }
     
        private Configuration _configuration;

        readonly IDataSource _dataSource;


        public ObservableCollection<Approach> Approaches { get; set; }
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
            DateSettings = dateSettings;

            Approaches = new ObservableCollection<Approach>();
           
            DataContext = this;
            Loaded += ReportTable_Loaded;
            InitializeComponent();

        }

        private void ReportTable_Loaded(object sender, RoutedEventArgs e)
        {
            var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
            var tasks = new List<Task>();
            foreach (var approach in _configuration.Approaches)
            {
                tasks.Add(Task.Factory.StartNew(() => approach.LoadDataTable(DateSettings, Intersection, 0)));
            }

            Task.Factory.ContinueWhenAll(tasks.ToArray(), completedTasks =>
            {
               //
            }).ContinueWith(task => _configuration.Approaches.ForEach(Approaches.Add), scheduler);

            Render();
        }

        /// <summary>
        /// Displays the grid
        /// </summary>
        private void Render()
        {
            ScreenTitle.Content = _configuration.Name;

            //Remove all exisitng approaches!
            OverallSummaryTextBlock.Inlines.Clear();

//            var stringBuilder = new StringBuilder();
//            stringBuilder.Append("Busiest Approach: ");
//            stringBuilder.AppendLine("[b]" + _configuration.GetBusiestApproach(DateSettings).ApproachName + "[/b]");
//
//            stringBuilder.Append("Busiest AM Hour: ");
//            stringBuilder.Append("[b]" + _configuration.GetAMPeakPeriod(DateSettings).ToShortTimeString() + "[/b]");
//            stringBuilder.Append(" with volume: ");
//            stringBuilder.AppendLine("[b]" + _configuration.GetAMPeakVolume(DateSettings) + "[/b]");
//
//            stringBuilder.Append("Busiest PM Hour: ");
//            stringBuilder.Append("[b]" + _configuration.GetPMPeakPeriod(DateSettings).ToShortTimeString() + "[/b]");
//            stringBuilder.Append(" with volume: ");
//            stringBuilder.AppendLine("[b]" + _configuration.GetPMPeakVolume(DateSettings) + "[/b]");
//
//            OverallSummaryTextBlock.Html = stringBuilder.ToString();

        }

        /// <summary>
        /// Handler for DateRangeChanged event from Toolbar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void DateSettingsChanged(DateSettings newSettings)
        {
            if (!newSettings.StartDate.Equals(DateSettings.StartDate) || !newSettings.EndDate.Equals(DateSettings.EndDate) || !newSettings.Interval.Equals(DateSettings.Interval))
            {
                DateSettings = newSettings;
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