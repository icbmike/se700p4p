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

namespace ATTrafficAnalayzer
{
    /// <summary>
    /// Interaction logic for VSSCreen.xaml
    /// </summary>
    public partial class VSTable : UserControl
    {
        private VolumeStore _volumeStore;
        private int _interval;
        private DateTime _startDate;
        private DateTime _endDate;
        private List<int> _intersections;

        public VSTable(VolumeStore volumeStore, int interval, DateTime startDate, DateTime endDate)
        {
            // TODO: Complete member initialization
            this._volumeStore = volumeStore;
            this._interval = interval;
            this._startDate = startDate;
            this._endDate = endDate;
            InitializeComponent();

            this._intersections = this._volumeStore.getIntersections();


            //DataTable datatable = new DataTable();

            //datatable.Columns.Add(new DataColumn("00", typeof(string)));
            //datatable.Columns.Add(new DataColumn("01", typeof(string)));
            //datatable.Columns.Add(new DataColumn("02", typeof(string)));
            //datatable.Columns.Add(new DataColumn("03", typeof(string)));
            //datatable.Columns.Add(new DataColumn("04", typeof(string)));

            //DataRow row = datatable.NewRow();
            //row["00"] = "hello";
            //row["01"] = "hello";
            //row["02"] = "hello";
            //row["03"] = "hello";
            //row["04"] = "hello";
            //datatable.Rows.Add(row);

            //this.dataGrid.Content = datatable;
        }
    }
}
