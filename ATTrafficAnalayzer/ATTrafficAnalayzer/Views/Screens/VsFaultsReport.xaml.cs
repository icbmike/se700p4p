using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Models.Settings;
using System;
using System.Data;
using System.Windows.Controls;
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
