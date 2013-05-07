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
        public VSTable(VolumeStore dataset)
        {
            InitializeComponent();

            
            DataTable datatable = new DataTable();

            datatable.Columns.Add(new DataColumn("00", typeof(string)));
            datatable.Columns.Add(new DataColumn("01", typeof(string)));
            datatable.Columns.Add(new DataColumn("02", typeof(string)));
            datatable.Columns.Add(new DataColumn("03", typeof(string)));
            datatable.Columns.Add(new DataColumn("04", typeof(string)));

            this.dataGrid.Content = datatable;

            //_testData.Columns.Add(new DataColumn("Name", typeof(string)));
            //_testData.Columns.Add(new DataColumn("Department", typeof(string)));

            //// Temp Code: User should add rows
            //DataRow row = _testData.NewRow();
            //row["Name"] = "John Smith";
            //row["Department"] = "Accounting";
            //_testData.Rows.Add(row);

            //// Initialize combo boxes
            //List departmentComboBoxList = new List() { "Accounting", "Purchasing", "Engineering" };
            //_Departments.ItemsSource = departmentComboBoxList;

            
        }
    }
}
