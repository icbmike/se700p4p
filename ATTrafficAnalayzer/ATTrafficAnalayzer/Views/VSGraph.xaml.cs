using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Research.DynamicDataDisplay.PointMarkers;

namespace ATTrafficAnalayzer
{
    /// <summary>
    /// Interaction logic for VSGraph.xaml
    /// </summary>
    public partial class VSGraph : UserControl
    {
        private VolumeStore _volumeStore;
        private int _interval;
        private DateTime _startDate;
        private DateTime _endDate;

        public VSGraph()
        {
            InitializeComponent();

            Logger.Info("constructed view", "VS graph");
        }


        public VSGraph(VolumeStore _volumeStore, int interval, DateTime startDate, DateTime endDate)
        {
            // TODO: Complete member initialization
            this._volumeStore = _volumeStore;
            this._interval = interval;
            this._startDate = startDate;
            this._endDate = endDate;
            InitializeComponent();

            var ds = new List<DateTime>();
            foreach(DateTimeRecord d in _volumeStore.DateTimeRecords){
                ds.Add(d.dateTime);
            }

            DateTime[] dates = ds.ToArray();
            int intersection = _volumeStore.getIntersections().ToList()[0];
            int detector = _volumeStore.getDetectorsAtIntersection(intersection)[0];
            
            var vs = new List<int>();
            foreach (DateTime d in dates)
            {
                vs.Add(_volumeStore.getVolume(intersection, detector, d));
            }
            int[] volumes = vs.ToArray();


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
