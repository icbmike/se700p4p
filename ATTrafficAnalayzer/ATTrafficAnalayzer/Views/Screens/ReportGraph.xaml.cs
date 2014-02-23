using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Models.ReportConfiguration;
using ATTrafficAnalayzer.Models.Settings;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Research.DynamicDataDisplay.PointMarkers;
using System.Windows.Controls;
using System.Windows;

namespace ATTrafficAnalayzer.Views.Screens
{
    /// <summary>
    /// Interaction logic for VSGraph.xaml
    /// </summary>
    public partial class ReportGraph 
    {
        private static readonly Brush[] SeriesColours = { Brushes.Red, Brushes.Green, Brushes.Blue, Brushes.BlueViolet, Brushes.Black, Brushes.DarkOrange };

        private readonly List<LineAndMarker<MarkerPointsGraph>> _series;
       
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dateSettings"> Lets the Graph screen get the start and end date at the time of construction</param>
        public ReportGraph(DateSettings dateSettings, IDataSource dataSource) : base(dateSettings, dataSource)
        {

            InitializeComponent();

            //Remove mouse and keyboard navigation and hide the legend.
            _series = new List<LineAndMarker<MarkerPointsGraph>>();
            Plotter.Children.Remove(Plotter.KeyboardNavigation);
            Plotter.Children.Remove(Plotter.MouseNavigation);
            Plotter.Children.Remove(Plotter.DefaultContextMenu);
            Plotter.Legend.Visibility = Visibility.Collapsed;

        }

        /// <summary>
        /// Method to display and render the graph.
        /// </summary>
        protected override void Render(bool reloadConfiguration = true)
        {

            if (!DataSource.VolumesExistForDateRange(DateSettings.StartDate, DateSettings.EndDate))
            {
                MessageBox.Show("You haven't imported volume data for the selected date range");
                return;
            }

            if (Configuration == null)
            {
                MessageBox.Show("Construct your new report or select a report from the list on the left");
                return;
            }

            ScreenTitle.Content = Configuration.Name;

            var intersection = Configuration.Intersection;

            //Clear anything that's already on the graph
            foreach (var graph in _series)
            {
                Plotter.Children.Remove(graph.LineGraph);
                Plotter.Children.Remove(graph.MarkerGraph);
            }
            //Clear the checkboxes
            var checkboxMask = new bool[ToggleContainer.Children.Count];
            
            if (!reloadConfiguration)
            {
                for (var index = 0; index < ToggleContainer.Children.Count; index++)
                {
                    checkboxMask[index] = (ToggleContainer.Children[index] as CheckBox).IsChecked.Value;
                }
            }

            ToggleContainer.Children.Clear();
            
            //Clear the series
            _series.Clear();

            // List dates
            var dateList = new List<DateTime>();
            for (var date = DateSettings.StartDate;
                date < DateSettings.EndDate;
                date = date.AddMinutes(Interval))
                dateList.Add(date);

            var datesDataSource = new EnumerableDataSource<DateTime>(dateList.ToArray());
            datesDataSource.SetXMapping(x => DateAxis.ConvertToDouble(x));
            
            var brushCounter = 0;
            var countsMatch = true;
            for (var index = 0; index < Configuration.Approaches.Count; index++)
            {
                var approach = Configuration.Approaches[index];
//Get volume info from db
                var approachVolumes = approach.GetVolumesList(intersection, DateSettings.StartDate, DateSettings.EndDate);
                for (var i = 0; i < approachVolumes.Count(); i++)
                {
                    if (approachVolumes[i] >= 150 && Interval == 5)
                        approachVolumes[i] = 150;
                }

                //Check that we actually have volumes that we need
                if (approachVolumes.Count/(Interval/5) != dateList.Count)
                {
                    countsMatch = false;
                    break;
                }
                //Sum volumes based on the interval
                var compressedVolumes = new int[dateList.Count];
                var valuesPerCell = Interval/5;
                for (var j = 0; j < dateList.Count; j++)
                {
                    var cellValue = 0;

                    for (var i = 0; i < Interval/5; i++)
                    {
                        cellValue += approachVolumes[i + valuesPerCell*j];
                    }
                    compressedVolumes[j] = cellValue;
                }

                var volumesDataSource = new EnumerableDataSource<int>(compressedVolumes);

                volumesDataSource.SetYMapping(y => y);
                var compositeDataSource = new CompositeDataSource(datesDataSource, volumesDataSource);

                //Add the series to the graph
                _series.Add(Plotter.AddLineGraph(compositeDataSource,
                    new Pen(SeriesColours[brushCounter%SeriesColours.Count()], 1),
                    new CirclePointMarker {Size = 0.0, Fill = SeriesColours[(brushCounter)%SeriesColours.Count()]},
                    new PenDescription(approach.ApproachName)));


                if (!reloadConfiguration && !checkboxMask[index])
                {
                    //If we aren't reloading the configuration, immediately remove the line and markers
                    Plotter.Children.Remove(_series[index].LineGraph);
                    Plotter.Children.Remove(_series[index].MarkerGraph);
                }

                //Add toggle checkboxes
                var stackPanel = new StackPanel {Orientation = Orientation.Horizontal};
                stackPanel.Children.Add(new Label {Content = approach.ApproachName, Margin = new Thickness(0, -5, 0, 0)});
                stackPanel.Children.Add(new Border
                {
                    Background = SeriesColours[(brushCounter)%SeriesColours.Count()],
                    Height = 15,
                    Width = 15,
                    CornerRadius = new CornerRadius(5)
                });

                var checkbox = new CheckBox
                {
                    Content = stackPanel,
                    IsChecked = reloadConfiguration || checkboxMask[index]
                };
                brushCounter++;

                checkbox.Checked += checkbox_Checked;
                checkbox.Unchecked += checkbox_Checked;
                ToggleContainer.Children.Add(checkbox);
            }

            //If previously counts didn't match, tell anyone who cares
            if (!countsMatch)
            {
               OnVolumeDateCountsDontMatch();
            }
        }


        /// <summary>
        /// Event handler to toggle series on the graph corresponding to the checked checkbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void checkbox_Checked(object sender, RoutedEventArgs e)
        {
            var checkbox = sender as CheckBox;

            //Get the index of the checkbox that has been checked
            var index = ToggleContainer.Children.IndexOf(checkbox);

            //See if it's checked
            if (checkbox.IsChecked.Value)
            {
                Plotter.Children.Add(_series[index].LineGraph);
                Plotter.Children.Add(_series[index].MarkerGraph);
            }
            else
            {
                Plotter.Children.Remove(_series[index].LineGraph);
                Plotter.Children.Remove(_series[index].MarkerGraph);
            }

            Plotter.Legend.Visibility = Visibility.Collapsed;
        }

    }
}
