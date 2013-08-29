using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Models.Settings;
using ATTrafficAnalayzer.Views.Controls;

namespace ATTrafficAnalayzer.Views.Screens
{
    /// <summary>
    /// Interaction logic for Faults.xaml
    /// </summary>
    public partial class Faults : IView
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
            Render();
        }

        public void Render()
        {
            if (!DbHelper.GetDbHelper().VolumesExist(_startDate, _endDate))
                MessageBox.Show("You haven't imported volume data for the selected date range");

            var dataAdapter = _dbHelper.GetFaultsDataAdapter(_startDate, _endDate);
            var dataTable = new DataTable();
            dataAdapter.Fill(dataTable);

            foreach (var row in dataTable.Rows)
            {
                var dataRow = row as DataRow;
                dataRow[1] = String.Join(", ", (dataRow[1] as String).Split(new[]{","}, StringSplitOptions.None).ToList().Distinct());
            }

            FaultsDataGrid.ItemsSource = dataTable.AsDataView();
        }

        #region Event Handlers

        public event VolumeAndDateCountsDontMatchHandler VolumeDateCountsDontMatch;

        public void DateRangeChangedHandler(object sender, Toolbar.DateRangeChangedEventHandlerArgs args)
        {
            if (!args.StartDate.Equals(_startDate) || !args.EndDate.Equals(_endDate))
            {
                _startDate = args.StartDate;
                _endDate = args.EndDate;

                Render();
            }
        }

        public void ReportChangedHandler(object sender, ReportBrowser.SelectedReportChangeEventHandlerArgs args)
        {
            throw new NotImplementedException();
        }

        #endregion

        private void FaultsDataGrid_OnAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.Column.Header.Equals("Intersection"))
            {
                e.Column.Width = new DataGridLength(1, DataGridLengthUnitType.SizeToHeader);
            }
            
        }
    }
}
