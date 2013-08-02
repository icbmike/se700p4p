﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Models.Settings;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Research.DynamicDataDisplay.PointMarkers;

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
            this.startDate = settings.StartDate;
            this.endDate = settings.EndDate;
            this.interval = settings.Interval;

            this.configName = configName;
            InitializeComponent();

            ScreenTitle.Content = configName;
            this.series = new List<LineAndMarker<MarkerPointsGraph>>();
            
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

               series.Add(Plotter.AddLineGraph(compositeDataSource, new Pen(SeriesColours[brushCounter % SeriesColours.Count()], 1),
                  new CirclePointMarker { Size = 0.0, Fill = SeriesColours[(brushCounter) % SeriesColours.Count()] },
                  new PenDescription(approach.Name)));
                brushCounter++;
            }
        }

        public void DateRangeChangedHandler(object sender, Controls.Toolbar.DateRangeChangedEventHandlerArgs args)
        {
            Console.WriteLine("CALLED");

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
