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
using System.Data;
using System.ComponentModel;
using Microsoft.Research.DynamicDataDisplay.DataSources;

namespace ATTrafficAnalayzer
{

    /// <summary>
    /// Interaction logic for VSSCreen.xaml
    /// </summary>
    public partial class VSTable : UserControl
    {
        private VolumeStore _volumeStore = null;
        private int _interval;
        private DateTime _startDate;
        private DateTime _endDate;

        public VSTable(VolumeStore _volumeStore, int _interval, DateTime _startDate, DateTime _endDate)
        {
            // TODO Complete member initialization
            this._volumeStore = _volumeStore;
            this._interval = _interval;
            this._startDate = _startDate;
            this._endDate = _endDate;
            InitializeComponent();

            var dateLabel = new Label();
            dateLabel.Content = "Day: xxxx AM     Time range: yyyy";
            dateLabel.Margin = new Thickness(10);
            putStuffHere.Children.Add(dateLabel);

            var dg = new DataGrid();
            dg.ItemsSource = generateVSTable().AsDataView();
            dg.Margin = new Thickness(10);
            dg.Width = Double.NaN; //Translates to Auto
            dg.Height = 280;
            dg.ColumnWidth = new DataGridLength(1, DataGridLengthUnitType.Star);
            dg.IsReadOnly = true;
            putStuffHere.Children.Add(dg);

            Logger.Info("constructed view", "VS table");
        }

        public DataTable generateVSTable()
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
                ds.Add(date.dateTime);
            }
            var dates = ds.ToArray();

            // List intersections
            var intersection = _volumeStore.getIntersections().ToList()[0]; // Use the first intersection for the time being

            // List detectors
            var detector = _volumeStore.getDetectorsAtIntersection(intersection)[0]; // Use the first detector for the time being

            // Get volume store data
            for (var i = 0; i < 12; i++)
            {
                var row = vsDataTable.NewRow();
                for (var j = 0; j < 12; j++)
                {
                    row[j] = _volumeStore.getVolume(intersection, detector, dates[i * 12 + j]);
                }
                vsDataTable.Rows.Add(row);
            }

            return vsDataTable;
        }
    }
}