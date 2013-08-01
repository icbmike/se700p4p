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

namespace ATTrafficAnalayzer.Views.Screens
{
    /// <summary>
    /// Interaction logic for VsFaultsReport.xaml
    /// </summary>
    public partial class VsFaultsReport : UserControl
    {
        private SettingsTray settings;
        private DbHelper dbHelper;

        public VsFaultsReport(SettingsTray settings)
        {
            this.settings = settings;
            this.dbHelper = DbHelper.GetDbHelper();
            InitializeComponent();
            FillGrid();
        
        }

        private void FillGrid()
        {

            var dataAdapter = dbHelper.GetFaultsDataAdapter(settings.StartDate, settings.EndDate);
            var dataTable = new DataTable();
            dataAdapter.Fill(dataTable);
            FaultsDataGrid.ItemsSource = dataTable.AsDataView();
        }


        internal void DateRangeChangedHandler(object sender, Controls.Toolbar.DateRangeChangedEventHandlerArgs args)
        {
            throw new NotImplementedException();
        }
    }
}
