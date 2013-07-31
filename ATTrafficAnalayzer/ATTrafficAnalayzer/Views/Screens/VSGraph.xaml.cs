using System;
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
        private static readonly Brush[] SeriesColours = {Brushes.Red, Brushes.Green, Brushes.Blue, Brushes.BlueViolet, Brushes.Black};

        public VsGraph(SettingsTray settings, string configName)
        {
            var settingsTray = settings;
            var dbHelper = DbHelper.GetDbHelper();
            var reportConfiguration = dbHelper.GetConfiguration(configName);
            var intersection = reportConfiguration.Intersection;

            InitializeComponent();

            ScreenTitle.Content = configName;             
          
            // List dates
            var dateList = new List<DateTime>();
            for (var date = settingsTray.StartDate;
                date < settingsTray.EndDate;
                date = date.AddMinutes(settingsTray.Interval))
            {
                dateList.Add(date);
            }

            var datesDataSource = new EnumerableDataSource<DateTime>(dateList.ToArray());
            datesDataSource.SetXMapping(x => DateAxis.ConvertToDouble(x));

            var brushCounter = 0;
            foreach (var approach in reportConfiguration.Approaches)
            {
                var approachVolumes = approach.GetVolumesList(intersection, settingsTray.StartDate, settingsTray.EndDate);
                
                var volumesDataSource = new EnumerableDataSource<int>(approachVolumes.ToArray());
                volumesDataSource.SetYMapping(y => y);
                var compositeDataSource = new CompositeDataSource(datesDataSource, volumesDataSource);
                
                Plotter.AddLineGraph(compositeDataSource, new Pen(SeriesColours[brushCounter % SeriesColours.Count()], 1),
                  new CirclePointMarker { Size = 0.0, Fill = SeriesColours[(brushCounter ) % SeriesColours.Count()] },
                  new PenDescription(approach.Name));
                brushCounter++;
            }    
        }
    }
}
