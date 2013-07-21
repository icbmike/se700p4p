using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
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
    
    public partial class VsGraph : UserControl
    {
        private readonly SettingsTray _settings;
        private DbHelper _dbHelper;

        private static Brush[] seriesColours = {Brushes.Red, Brushes.Green, Brushes.Blue, Brushes.BlueViolet, Brushes.Black};

        public VsGraph(SettingsTray settings, string configName)
        {
            _settings = settings;
            _dbHelper = DbHelper.GetDbHelper();

            InitializeComponent();

            ScreenTitle.Content = configName;
          
            var ds = new List<DateTime>();
            for(var date = _settings.StartDate; date < _settings.EndDate; date = date.AddMinutes(_settings.Interval)){
                ds.Add(date);
            }

            var dates = ds.ToArray(); 
            var reportConfiguration = _dbHelper.GetConfiguration(configName);
            var intersection = reportConfiguration.Intersection;

            var datesDataSource = new EnumerableDataSource<DateTime>(dates);
            datesDataSource.SetXMapping(x => dateAxis.ConvertToDouble(x));
            int brushCounter = 0;
            foreach (var approach in reportConfiguration.Approaches)
            {
                var approachVolumes = new List<int>();
                foreach (var detector in approach.Detectors)
                {

                    if (approachVolumes.Count == 0)
                    {
                        approachVolumes.AddRange(_dbHelper.GetVolumes(intersection, detector, settings.StartDate,
                                                                      settings.EndDate));
                    }
                    else
                    {
                        List<int> detectorVolumes = _dbHelper.GetVolumes(intersection, detector, settings.StartDate, settings.EndDate);
                        approachVolumes = approachVolumes.Zip(detectorVolumes, (i, i1) => i + i1).ToList();
                    }
                }
                
                var volumesDataSource = new EnumerableDataSource<int>(approachVolumes.ToArray());
                volumesDataSource.SetYMapping(y => y);
                var compositeDataSource = new CompositeDataSource(datesDataSource, volumesDataSource);

                
                Plotter.AddLineGraph(compositeDataSource, new Pen(seriesColours[brushCounter % seriesColours.Count()], 1),
                  new CirclePointMarker { Size = 0.0, Fill = seriesColours[(brushCounter ) % seriesColours.Count()] },
                  new PenDescription(approach.Name));
                brushCounter++;
            }    
        }
    }
}
