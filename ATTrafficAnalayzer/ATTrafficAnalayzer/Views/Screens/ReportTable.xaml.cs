using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
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
     

        readonly IDataSource _dataSource;
        private Configuration _configuration;


        public ObservableCollection<Approach> Approaches { get; set; }

        public Configuration Configuration
        {
            get { return _configuration; }
            set {
                _configuration = value;
                Render();
            }
        }

        /// <summary>
        /// Constructor create a component displaying the specified config
        /// </summary>
        /// <param name="dateSettings">Lets the graph view get the date range at the time of construction</param>
        /// <param name="dataSource">The dataSource to get volume information from</param>
        /// <param name="configuration"></param>
        /// <param name="configName">The config to be displayed</param>
        public ReportTable(DateSettings dateSettings, IDataSource dataSource)
        {
            _dataSource = dataSource;
            DateSettings = dateSettings;

            Approaches = new ObservableCollection<Approach>();
           
            DataContext = this;
            InitializeComponent();

        }

        /// <summary>
        /// Displays the grid
        /// </summary>
        private void Render()
        {
            OverallSummaryBorder.Visibility = Visibility.Hidden;
            
            ScreenTitle.Content = _configuration.Name;

            //Load the data for approaches using Task magic
            var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
            var tasks = new List<Task>();
            foreach (var approach in _configuration.Approaches)
            {
                tasks.Add(Task.Factory.StartNew(() => approach.LoadDataTable(DateSettings, Intersection, 0)));
            }
            Task.Factory.ContinueWhenAll(tasks.ToArray(), completedTasks =>
            {
                //We don't care, this is just a synchronization point that lets us add the approaches in order
            }).ContinueWith(task =>
            {
                foreach (var approach in _configuration.Approaches)
                {
                    if (approach.HasDataForDate) Approaches.Add(approach);
                    else
                    {
                        if (VolumeDateCountsDontMatch != null) VolumeDateCountsDontMatch(this, EventArgs.Empty);
                        break;
                    }
                } 

                //Overall deets
                var stringBuilder = new StringBuilder();
                stringBuilder.Append("Busiest Approach: ");
                stringBuilder.AppendLine("[b]" + _configuration.GetBusiestApproach(DateSettings).ApproachName + "[/b]");

                stringBuilder.Append("Busiest AM Hour: ");
                stringBuilder.Append("[b]" + _configuration.GetAMPeakPeriod(DateSettings).ToShortTimeString() + "[/b]");
                stringBuilder.Append(" with volume: ");
                stringBuilder.AppendLine("[b]" + _configuration.GetAMPeakVolume(DateSettings) + "[/b]");

                stringBuilder.Append("Busiest PM Hour: ");
                stringBuilder.Append("[b]" + _configuration.GetPMPeakPeriod(DateSettings).ToShortTimeString() + "[/b]");
                stringBuilder.Append(" with volume: ");
                stringBuilder.AppendLine("[b]" + _configuration.GetPMPeakVolume(DateSettings) + "[/b]");
                stringBuilder.Append("Total volume: [b]" + _configuration.GetTotalVolume() +"[/b]");

                OverallSummaryTextBlock.Html = stringBuilder.ToString();
                OverallSummaryBorder.Visibility = Visibility.Visible;

            }, scheduler);
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

        public event EventHandler VolumeDateCountsDontMatch;
    }

}