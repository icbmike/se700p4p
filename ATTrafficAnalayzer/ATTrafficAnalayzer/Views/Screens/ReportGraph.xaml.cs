﻿using System;
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

namespace ATTrafficAnalayzer.Views.Screens
{
    /// <summary>
    /// Interaction logic for VSGraph.xaml
    /// </summary>
    public partial class ReportGraph : IView
    {
        private static readonly Brush[] SeriesColours = { Brushes.Red, Brushes.Green, Brushes.Blue, Brushes.BlueViolet, Brushes.Black, Brushes.DarkOrange };

        private string _configName;
        private DateTime _startDate;
        private DateTime _endDate;
        private int _interval;
        private readonly List<LineAndMarker<MarkerPointsGraph>> _series;
        private IDataSource _dataSource;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="settings"> Lets the Graph screen get the start and end date at the time of construction</param>
        /// <param name="configName">The name of the report to display</param>
        public ReportGraph(DateSettings settings, string configName, IDataSource dataSource)
        {
            _startDate = settings.StartDate;
            _endDate = settings.EndDate;
            _interval = settings.Interval;
            _configName = configName;
            _dataSource = dataSource;
            InitializeComponent();

            //Remove mouse and keyboard navigation and hide the legend.
            _series = new List<LineAndMarker<MarkerPointsGraph>>();
            Plotter.Children.Remove(Plotter.KeyboardNavigation);
            Plotter.Children.Remove(Plotter.MouseNavigation);
            Plotter.Legend.Visibility = Visibility.Collapsed;

            Render();
        }

        /// <summary>
        /// Method to display and render the graph.
        /// </summary>
        private void Render()
        {
            var configuation = _dataSource.GetConfiguration(_configName);

            if (!SqliteDataSource.GetDbHelper().VolumesExist(_startDate, _endDate))
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
                for (int i=0; i < approachVolumes.Count(); i++) {
                    if (approachVolumes[i] >= 150 && _interval == 5)
                        approachVolumes[i] = 150;
                }                                           

                //Check that we actually have volumes that we need
                if (approachVolumes.Count / (_interval / 5) != dateList.Count)
                {
                    countsMatch = false;
                    break;
                }
                //Sum volumes based on the interval
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
               _series.Add(Plotter.AddLineGraph(compositeDataSource, new Pen(SeriesColours[brushCounter % SeriesColours.Count()], 1),
                  new CirclePointMarker { Size = 0.0, Fill = SeriesColours[(brushCounter) % SeriesColours.Count()] },
                  new PenDescription(approach.Name)));

               //Add toggle checkboxes
                var stackPanel = new StackPanel {Orientation = Orientation.Horizontal};
                stackPanel.Children.Add( new Label {Content = approach.Name, Margin = new Thickness(0, -5, 0, 0) });
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
                    VolumeDateCountsDontMatch(this);
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

        public void DateRangeChangedHandler(object sender, Toolbar.DateRangeChangedEventHandlerArgs args)
        {

            if (!args.StartDate.Equals(_startDate) || !args.EndDate.Equals(_endDate) || !args.Interval.Equals(_interval))
            {
                //InitializeGraph() is a time consuming operation.
                //We dont want to do it if we don't have to.

                _startDate = args.StartDate;
                _endDate = args.EndDate;
                _interval = args.Interval;

                Render();
            }
        }

        public void ReportChangedHandler(object sender, ReportBrowser.SelectedReportChangeEventHandlerArgs args)
        {
            if (!args.SelectionCleared)
            {
                if (!_configName.Equals(args.ReportName))
                {
                    _configName = args.ReportName;
                    Render();
                }
            }
        }

        public event VolumeAndDateCountsDontMatchHandler VolumeDateCountsDontMatch;
    }
}
