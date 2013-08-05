using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Models.Settings;
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
    public partial class VsGraph
    {
        private static readonly Brush[] SeriesColours = { Brushes.Red, Brushes.Green, Brushes.Blue, Brushes.BlueViolet, Brushes.Black };
        
        private string configName;
        private DateTime startDate;
        private DateTime endDate;
        private int interval;
        private List<LineAndMarker<MarkerPointsGraph>> series;


        public VsGraph(SettingsTray settings, string configName)
        {
            startDate = settings.StartDate;
            endDate = settings.EndDate;
            interval = settings.Interval;

            this.configName = configName;
            InitializeComponent();

            ScreenTitle.Content = configName;
            series = new List<LineAndMarker<MarkerPointsGraph>>();

            Plotter.Children.Remove(Plotter.KeyboardNavigation);
            Plotter.Children.Remove(Plotter.MouseNavigation);

            //Display the graph
            RenderGraph();

        }

        private void RenderGraph()
        {
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

            var dbHelper = DbHelper.GetDbHelper();
            var reportConfiguration = dbHelper.GetConfiguration(configName);
            var intersection = reportConfiguration.Intersection;

            // List dates
            var dateList = new List<DateTime>();
            for (var date = startDate;
                date < endDate;
                date = date.AddMinutes(interval))
                dateList.Add(date);

            var datesDataSource = new EnumerableDataSource<DateTime>(dateList.ToArray());
            datesDataSource.SetXMapping(x => DateAxis.ConvertToDouble(x));
            
            var brushCounter = 0;
            foreach (var approach in reportConfiguration.Approaches)
            {
                //Get volume info from db
                var approachVolumes = approach.GetVolumesList(intersection, startDate, endDate);
                
                var compressedVolumes = new int[dateList.Count];
                var valuesPerCell = interval / 5;
                for (var j = 0; j < dateList.Count; j++)
                {
                    var cellValue = 0;

                    for (var i = 0; i < interval / 5; i++)
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

            if (!args.startDate.Equals(startDate) || !args.endDate.Equals(endDate) || !args.interval.Equals(interval))
            {
                //InitializeGraph() is a time consuming operation.
                //We dont want to do it if we don't have to.

                startDate = args.startDate;
                endDate = args.endDate;
                interval = args.interval;

                RenderGraph();
            }
        }
    }
}
