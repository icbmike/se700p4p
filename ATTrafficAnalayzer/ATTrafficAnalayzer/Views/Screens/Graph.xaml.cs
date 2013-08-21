using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Models.Settings;
using ATTrafficAnalayzer.Properties;
using ATTrafficAnalayzer.Views.Controls;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Research.DynamicDataDisplay.PointMarkers;
using System.Windows.Controls;
using System.Windows;
using System.Globalization;

namespace ATTrafficAnalayzer.Views.Screens
{
    /// <summary>
    /// Interaction logic for VSGraph.xaml
    /// </summary>
    public partial class Graph : IView
    {
        private static readonly Brush[] SeriesColours = { Brushes.Red, Brushes.Green, Brushes.Blue, Brushes.BlueViolet, Brushes.Black };
        
        private string _configName;
        private DateTime _startDate;
        private DateTime _endDate;
        private int _interval;
        private readonly List<LineAndMarker<MarkerPointsGraph>> series;


        public Graph(SettingsTray settings, string configName)
        {
            _startDate = settings.StartDate;
            _endDate = settings.EndDate;
            _interval = settings.Interval;
            _configName = configName;

            InitializeComponent();

            series = new List<LineAndMarker<MarkerPointsGraph>>();
            Plotter.Children.Remove(Plotter.KeyboardNavigation);
            Plotter.Children.Remove(Plotter.MouseNavigation);

            RenderGraph();
        }

        private void RenderGraph()
        {
            var dbHelper = DbHelper.GetDbHelper();
            var configuation = dbHelper.GetConfiguration(_configName);

            if (!DbHelper.GetDbHelper().VolumesExist(_startDate, _endDate))
            {
                MessageBox.Show("You haven't imported volume data for the selected date range");
                return;
            }

            if (configuation == null)
            {
                MessageBox.Show("Construct your new report or select a report from the list on the left");
                return;
            }

            ScreenTitle.Content = _configName;

            var intersection = configuation.Intersection;

            //Clear anything that's already on the graph
            foreach (var graph in series)
            {
                Plotter.Children.Remove(graph.LineGraph);
                Plotter.Children.Remove(graph.MarkerGraph);
            }
            //Clear the checkboxes
            ToggleContainer.Children.Clear();
            
            //Clear the series
            series.Clear();

            // List dates
            var dateList = new List<DateTime>();
            for (var date = _startDate;
                date < _endDate;
                date = date.AddMinutes(_interval))
                dateList.Add(date);

            var datesDataSource = new EnumerableDataSource<DateTime>(dateList.ToArray());
            datesDataSource.SetXMapping(x => DateAxis.ConvertToDouble(x));
            
            var brushCounter = 0;
            var countsMatch = true;
            foreach (var approach in configuation.Approaches)
            {
                //Get volume info from db
                var approachVolumes = approach.GetVolumesList(intersection, _startDate, _endDate);

                //Check that we actually have volumes that we need
                if (approachVolumes.Count / (_interval / 5) != dateList.Count)
                {
                    countsMatch = false;
                    break;
                }

                var compressedVolumes = new int[dateList.Count];
                var valuesPerCell = _interval / 5;
                for (var j = 0; j < dateList.Count; j++)
                {
                    var cellValue = 0;

                    for (var i = 0; i < _interval / 5; i++)
                    {
                        cellValue += approachVolumes[i + valuesPerCell * j];
                    }
                    compressedVolumes[j] = cellValue;
                }

                var volumesDataSource = new EnumerableDataSource<int>(compressedVolumes);

                volumesDataSource.SetYMapping(y => y);
                var compositeDataSource = new CompositeDataSource(datesDataSource, volumesDataSource);

                //Add the series to the graph
               series.Add(Plotter.AddLineGraph(compositeDataSource, new Pen(SeriesColours[brushCounter % SeriesColours.Count()], 1),
                  new CirclePointMarker { Size = 0.0, Fill = SeriesColours[(brushCounter) % SeriesColours.Count()] },
                  new PenDescription(approach.Name)));
                brushCounter++;

                //Add toggle checkboxes
                var checkbox = new CheckBox();
                checkbox.Content = new Label() { Content = approach.Name, Margin = new Thickness(0,-5,0,0) };
                checkbox.IsChecked = true;
                checkbox.Checked += checkbox_Checked;
                checkbox.Unchecked += checkbox_Checked;
                ToggleContainer.Children.Add(checkbox);
            }

            if (!countsMatch)
            {
                if (VolumeDateCountsDontMatch != null)
                {
                    VolumeDateCountsDontMatch(this);
                }
            }
        }

        void checkbox_Checked(object sender, RoutedEventArgs e)
        {
            var checkbox = sender as CheckBox;

            //Get the index of the checkbox that has been checked
            var index = ToggleContainer.Children.IndexOf(checkbox);
                
            //See if it's checked
            if (checkbox.IsChecked.Value)
            {
                Plotter.Children.Add(series[index].LineGraph);
                Plotter.Children.Add(series[index].MarkerGraph);
            }
            else          
            {
                Plotter.Children.Remove(series[index].LineGraph);
                Plotter.Children.Remove(series[index].MarkerGraph);
            }
        }

        public void DateRangeChangedHandler(object sender, Toolbar.DateRangeChangedEventHandlerArgs args)
        {

            if (!args.StartDate.Equals(_startDate) || !args.EndDate.Equals(_endDate) || !args.Interval.Equals(_interval))
            {
                //InitializeGraph() is a time consuming operation.
                //We dont want to do it if we don't have to.

                _startDate = args.StartDate;
                _endDate = args.EndDate;
                _interval = args.Interval;

                RenderGraph();
            }
        }

        public void ReportChangedHandler(object sender, ReportBrowser.SelectedReporChangeEventHandlerArgs args)
        {
            _configName = args.ReportName;
            RenderGraph();
        }

        public event VolumeAndDateCountsDontMatchHandler VolumeDateCountsDontMatch;
    }
}
