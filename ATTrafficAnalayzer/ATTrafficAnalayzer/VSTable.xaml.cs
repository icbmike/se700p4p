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
    public partial class VSTable : UserControl
    {
        private VolumeStore _volumeStore;
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

            // Create a DataGrid
            DataTable vsDataTable = new DataTable();

            // Set column headings
            for (int i = 0; i < 12; i++)
            {
                vsDataTable.Columns.Add(i.ToString(), typeof(string));
            }

            // List dates
            List<DateTime> ds = new List<DateTime>();
            foreach (DateTimeRecord date in _volumeStore.DateTimeRecords)
            {
                ds.Add(date.dateTime);
            }
            DateTime[] dates = ds.ToArray();

            // List intersections
            int intersection = _volumeStore.getIntersections().ToList()[0]; // Use the first intersection for the time being

            // List detectors
            int detector = _volumeStore.getDetectorsAtIntersection(intersection)[0]; // Use the first detector for the time being

            // Get volume store data
            int[][] vsData = { 
                                 new int[12],
                                 new int[12],
                                 new int[12],
                                 new int[12],
                                 new int[12],
                                 new int[12],
                                 new int[12],
                                 new int[12],
                                 new int[12],
                                 new int[12],
                                 new int[12],
                                 new int[12]
                             };
            for (int i = 0; i < 12; i++)
            {
                for (int j = 0; j < 12; j++)
                {
                    vsData[i][j] = _volumeStore.getVolume(intersection, detector, dates[i * 12 + j]);
                }
            }

            // Set DataGrid rows from the data
            for (int i = 0; i < 12; i++)
            {
                DataRow row = vsDataTable.NewRow();
                for (int j = 0; j < 12; j++)
                {
                    row[j] = vsData[i][j];
                }
                vsDataTable.Rows.Add(row);
            }

            table.ItemsSource = vsDataTable.AsDataView();
        }

        //private DataTable _volumeStoreTable;

        //public DataTable VolumeStoreTable
        //{
        //    get { return _volumeStoreTable; }
        //    set
        //        {
        //            _volumeStoreTable = value;
        //            OnPropertyChanged("VolumeStoreTable");
        //        }
        //}
    }
}