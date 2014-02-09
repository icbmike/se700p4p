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
    public partial class ReportGraph : IView
    {
        private static readonly Brush[] SeriesColours = { Brushes.Red, Brushes.Green, Brushes.Blue, Brushes.BlueViolet, Brushes.Black, Brushes.DarkOrange };

        private DateSettings _dateSettings;
        private readonly List<LineAndMarker<MarkerPointsGraph>> _series;
        private readonly IDataSource _dataSource;
        private Configuration _configuration;

        public Configuration Configuration
        {
            get { return _configuration; }
            set { _configuration = value; Render(); }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dateSettings"> Lets the Graph screen get the start and end date at the time of construction</param>
        public ReportGraph(DateSettings dateSettings, IDataSource dataSource)
        {
            _dateSettings = dateSettings;
            _dataSource = dataSource;

            InitializeComponent();

            //Remove mouse and keyboard navigation and hide the legend.
            _series = new List<LineAndMarker<MarkerPointsGraph>>();
            Plotter.Children.Remove(Plotter.KeyboardNavigation);
            Plotter.Children.Remove(Plotter.MouseNavigation);
            Plotter.Children.Remove(Plotter.DefaultContextMenu);
            Plotter.Legend.Visibility = Visibility.Collapsed;

            Render();
        }

       

        /// <summary>
        /// Method to display and render the graph.
        /// </summary>
        private void Render()
        {

            if (!_dataSource.VolumesExistForDateRange(_dateSettings.StartDate, _dateSettings.EndDate))
            {
                MessageBox.Show("You haven't imported volume data for the selected date range");
                return;
            }

            if (_configuration == null)
            {
                MessageBox.Show("Construct your new report or select a report from the list on the left");
                return;
            }

            ScreenTitle.Content = _configuration.Name;

            var intersection = _configuration.Intersection;

            //Clear anything that's already on the graph
            foreach (var graph in _series)
            {
                Plotter.Children.Remove(graph.LineGraph);
                Plotter.Children.Remove(graph.MarkerGraph);
            }
            //Clear the checkboxes
            ToggleContainer.Children.Clear();
            
            //Clear the series
            _series.Clear();

            // List dates
            var dateList = new List<DateTime>();
            for (var date = _dateSettings.StartDate;
                date < _dateSettings.EndDate;
                date = date.AddMinutes(_dateSettings.Interval))
                dateList.Add(date);

            var datesDataSource = new EnumerableDataSource<DateTime>(dateList.ToArray());
            datesDataSource.SetXMapping(x => DateAxis.ConvertToDouble(x));
            
            var brushCounter = 0;
            var countsMatch = true;
            foreach (var approach in _configuration.Approaches)
            {
                //Get volume info from db
                var approachVolumes = approach.GetVolumesList(intersection, _dateSettings.StartDate, _dateSettings.EndDate);
                for (int i=0; i < approachVolumes.Count(); i++) {
                    if (approachVolumes[i] >= 150 && _dateSettings.Interval == 5)
                        approachVolumes[i] = 150;
                }                                           

                //Check that we actually have volumes that we need
                if (approachVolumes.Count / (_dateSettings.Interval / 5) != dateList.Count)
                {
                    countsMatch = false;
                    break;
                }
                //Sum volumes based on the interval
                var compressedVolumes = new int[dateList.Count];
                var valuesPerCell = _dateSettings.Interval / 5;
                for (var j = 0; j < dateList.Count; j++)
                {
                    var cellValue = 0;

                    for (var i = 0; i < _dateSettings.Interval / 5; i++)
                    {
                        cellValue += approachVolumes[i + valuesPerCell * j];
                    }
                    compressedVolumes[j] = cellValue;
                }

                var volumesDataSource = new EnumerableDataSource<int>(compressedVolumes);

                volumesDataSource.SetYMapping(y => y);
                var compositeDataSource = new CompositeDataSource(datesDataSource, volumesDataSource);

                //Add the series to the graph
               _series.Add(Plotter.AddLineGraph(compositeDataSource, new Pen(SeriesColours[brushCounter % SeriesColours.Count()], 1),
                  new CirclePointMarker { Size = 0.0, Fill = SeriesColours[(brushCounter) % SeriesColours.Count()] },
                  new PenDescription(approach.ApproachName)));

               //Add toggle checkboxes
                var stackPanel = new StackPanel {Orientation = Orientation.Horizontal};
                stackPanel.Children.Add( new Label {Content = approach.ApproachName, Margin = new Thickness(0, -5, 0, 0) });
                stackPanel.Children.Add( new Border{Background = SeriesColours[(brushCounter)%SeriesColours.Count()], Height = 15, Width = 15, CornerRadius = new CornerRadius(5)});
                
                var checkbox = new CheckBox
                {
                    Content = stackPanel,
                    IsChecked = true
                };
                brushCounter++;

                checkbox.Checked += checkbox_Checked;
                checkbox.Unchecked += checkbox_Checked;
                ToggleContainer.Children.Add(checkbox);
            }

            //If previously counts didn't match, tell anyone who cares
            if (!countsMatch)
            {
                if (VolumeDateCountsDontMatch != null)
                {
                    VolumeDateCountsDontMatch(this, EventArgs.Empty);
                }
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

        public void DateSettingsChanged(DateSettings newSettings)
        {
            _dateSettings = newSettings;
            Render();
        }

        public void SelectedReportChanged(string newSelection)
        {
            if (newSelection != null && !_configuration.Name.Equals(newSelection))
            {
                _configuration = _dataSource.GetConfiguration(newSelection);
                Render();
            }
        }

        public event EventHandler VolumeDateCountsDontMatch;
    }
}
