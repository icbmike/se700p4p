using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Models.Settings;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
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
using ATTrafficAnalayzer.Views.Controls;

namespace ATTrafficAnalayzer.Views.Screens
{
    /// <summary>
    /// Interaction logic for VsFaultsReport.xaml
    /// </summary>
    public partial class VsFaultsReport : UserControl
    {
        private DbHelper dbHelper;
        private DateTime endDate;
        private DateTime startDate;

        public VsFaultsReport(SettingsTray settings)
        {
            startDate = settings.StartDate;
            endDate = settings.EndDate;

            dbHelper = DbHelper.GetDbHelper();
            InitializeComponent();
            
            FillGrid();
        
        }

        private void FillGrid()
        {
            var dataAdapter = dbHelper.GetFaultsDataAdapter(startDate, endDate);
            var dataTable = new DataTable();
            dataAdapter.Fill(dataTable);
            FaultsDataGrid.ItemsSource = dataTable.AsDataView();
        }

        internal void DateRangeChangedHandler(object sender, Toolbar.DateRangeChangedEventHandlerArgs args)
        {

            if (!args.startDate.Equals(startDate) || !args.endDate.Equals(endDate))
            {
                //InitializeGraph() is a time consuming operation.
                //We dont want to do it if we don't have to.

                startDate = args.startDate;
                endDate = args.endDate;

                FillGrid();
            }
        }
    }
}
