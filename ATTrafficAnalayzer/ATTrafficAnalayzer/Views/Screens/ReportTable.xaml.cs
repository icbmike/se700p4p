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
    public partial class ReportTable
    {
        public int Intersection { get { return Configuration.Intersection; }  }
        public ObservableCollection<Approach> Approaches { get; set; }

        /// <summary>
        /// Constructor create a component displaying the specified config
        /// </summary>
        /// <param name="dateSettings">Lets the graph view get the date range at the time of construction</param>
        /// <param name="dataSource">The dataSource to get volume information from</param>
        public ReportTable(DateSettings dateSettings, IDataSource dataSource): base(dateSettings, dataSource)
        {
            Approaches = new ObservableCollection<Approach>();
           
            DataContext = this;
            InitializeComponent();

        }

        /// <summary>
        /// Displays the grid
        /// </summary>
        protected override void Render()
        {
            OverallSummaryBorder.Visibility = Visibility.Hidden;
            Approaches.Clear();
            ScreenTitle.Content = Configuration.Name;

            //Load the data for approaches using Task magic
            var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
            var tasks = new List<Task>();
            foreach (var approach in Configuration.Approaches)
            {
                tasks.Add(Task.Factory.StartNew(() => approach.LoadDataTable(DateSettings, Interval, Intersection, 0)));
            }
            Task.Factory.ContinueWhenAll(tasks.ToArray(), completedTasks =>
            {
                //We don't care, this is just a synchronization point that lets us add the approaches in order
            }).ContinueWith(task =>
            {
                foreach (var approach in Configuration.Approaches)
                {
                    if (approach.HasDataForDate) Approaches.Add(approach);
                    else
                    {
                        OnVolumeDateCountsDontMatch();
                        break;
                    }
                } 

                //Overall deets
                var stringBuilder = new StringBuilder();
                stringBuilder.Append("Busiest Approach: ");
                stringBuilder.AppendLine("[b]" + Configuration.GetBusiestApproach(DateSettings).ApproachName + "[/b]");

                stringBuilder.Append("Busiest AM Hour: ");
                stringBuilder.Append("[b]" + Configuration.GetAMPeakPeriod(DateSettings).ToShortTimeString() + "[/b]");
                stringBuilder.Append(" with volume: ");
                stringBuilder.AppendLine("[b]" + Configuration.GetAMPeakVolume(DateSettings) + "[/b]");

                stringBuilder.Append("Busiest PM Hour: ");
                stringBuilder.Append("[b]" + Configuration.GetPMPeakPeriod(DateSettings).ToShortTimeString() + "[/b]");
                stringBuilder.Append(" with volume: ");
                stringBuilder.AppendLine("[b]" + Configuration.GetPMPeakVolume(DateSettings) + "[/b]");
                stringBuilder.Append("Total volume: [b]" + Configuration.GetTotalVolume() +"[/b]");

                OverallSummaryTextBlock.Html = stringBuilder.ToString();
                OverallSummaryBorder.Visibility = Visibility.Visible;

            }, scheduler);
        }

    }

}