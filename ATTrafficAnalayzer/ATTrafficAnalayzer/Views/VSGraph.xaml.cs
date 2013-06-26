using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Research.DynamicDataDisplay.PointMarkers;

namespace ATTrafficAnalayzer
{
    /// <summary>
    /// Interaction logic for VSGraph.xaml
    /// </summary>
    public partial class VsGraph : UserControl
    {
        private VolumeStore _volumeStore;
        private int _interval;
        private DateTime _startDate;
        private DateTime _endDate;

        public VsGraph()
        {
            InitializeComponent();

            Logger.Info("constructed view", "VS graph");
        }


        public VsGraph(VolumeStore volumeStore, int interval, DateTime startDate, DateTime endDate)
        {
            // TODO: Complete member initialization
            _volumeStore = volumeStore;
            _interval = interval;
            _startDate = startDate;
            _endDate = endDate;
            InitializeComponent();

            var ds = new List<DateTime>();
            foreach(var d in volumeStore.DateTimeRecords){
                ds.Add(d.DateTime);
            }

            var dates = ds.ToArray();
            var intersection = volumeStore.GetIntersections().ToList()[0];
            var detector = volumeStore.GetDetectorsAtIntersection(intersection)[0];
            
            var vs = new List<int>();
            foreach (var d in dates)
            {
                vs.Add(volumeStore.GetVolume(intersection, detector, d));
            }
            var volumes = vs.ToArray();


            var datesDataSource = new EnumerableDataSource<DateTime>(dates);
            datesDataSource.SetXMapping(x => dateAxis.ConvertToDouble(x));

            var volumesDataSource = new EnumerableDataSource<int>(volumes);
            volumesDataSource.SetYMapping(y => y);

            var compositeDataSource = new CompositeDataSource(datesDataSource, volumesDataSource);
            plotter.AddLineGraph(compositeDataSource, new Pen(Brushes.Blue, 2),
              new CirclePointMarker { Size = 10.0, Fill = Brushes.Red },
              new PenDescription("Volumes"));
        }
    }
}
