using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Data;

namespace ATTrafficAnalayzer
{

    /// <summary>
    /// Interaction logic for VSSCreen.xaml
    /// </summary>
    public partial class VsTable : UserControl
    {
        private VolumeStore _volumeStore = null;
        private int _interval;
        private DateTime _startDate;
        private DateTime _endDate;

        public VsTable(VolumeStore volumeStore, int interval, DateTime startDate, DateTime endDate)
        {
            // TODO Complete member initialization
            _volumeStore = volumeStore;
            _interval = interval;
            _startDate = startDate;
            _endDate = endDate;
            InitializeComponent();

            var dateLabel = new Label {Content = string.Format("Day: {0} Time range: {1}", "", ""), Margin = new Thickness(10)};
            putStuffHere.Children.Add(dateLabel);

            var dg = new DataGrid
                {
                    ItemsSource = GenerateVsTable().AsDataView(),
                    Margin = new Thickness(10),
                    Width = Double.NaN,
                    Height = 280,
                    ColumnWidth = new DataGridLength(1, DataGridLengthUnitType.Star),
                    IsReadOnly = true
                };
            putStuffHere.Children.Add(dg);

            Logger.Info("constructed view", "VS table");
        }

        public DataTable GenerateVsTable()
        {
            // Create a DataGrid
            var vsDataTable = new DataTable();

            // Set column headings
            for (var i = 1; i <= 12; i++)
            {
                vsDataTable.Columns.Add(i.ToString(), typeof(string));
            }

            // List dates
            var ds = new List<DateTime>();
            foreach (var date in _volumeStore.DateTimeRecords)
            {
                ds.Add(date.DateTime);
            }
            var dates = ds.ToArray();

            // List intersections
            var intersection = _volumeStore.GetIntersections().ToList()[0]; // Use the first intersection for the time being

            // List detectors
            var detector = _volumeStore.GetDetectorsAtIntersection(intersection)[0]; // Use the first detector for the time being

            // Get volume store data
            for (var i = 0; i < 12; i++)
            {
                var row = vsDataTable.NewRow();
                for (var j = 0; j < 12; j++)
                {
                    row[j] = _volumeStore.GetVolume(intersection, detector, dates[i * 12 + j]);
                }
                vsDataTable.Rows.Add(row);
            }

            return vsDataTable;
        }
    }
}