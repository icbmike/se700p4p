using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Models.Settings;
using System;
using System.Data;
using ATTrafficAnalayzer.Views.Controls;

namespace ATTrafficAnalayzer.Views.Screens
{
    /// <summary>
    /// Interaction logic for Faults.xaml
    /// </summary>
    public partial class Faults
    {
        private readonly DbHelper _dbHelper;
        private DateTime _endDate;
        private DateTime _startDate;

        public Faults(SettingsTray settings)
        {
            _startDate = settings.StartDate;
            _endDate = settings.EndDate;

            _dbHelper = DbHelper.GetDbHelper();
            InitializeComponent();
            
            FillGrid();
        
        }

        private void FillGrid()
        {
            var dataAdapter = _dbHelper.GetFaultsDataAdapter(_startDate, _endDate);
            var dataTable = new DataTable();
            dataAdapter.Fill(dataTable);
            FaultsDataGrid.ItemsSource = dataTable.AsDataView();
        }

        internal void DateRangeChangedHandler(object sender, Toolbar.DateRangeChangedEventHandlerArgs args)
        {

            if (!args.startDate.Equals(_startDate) || !args.endDate.Equals(_endDate))
            {
                //InitializeGraph() is a time consuming operation.
                //We dont want to do it if we don't have to.

                _startDate = args.startDate;
                _endDate = args.endDate;

                FillGrid();
            }
        }
    }
}
