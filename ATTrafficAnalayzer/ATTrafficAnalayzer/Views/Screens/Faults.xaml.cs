﻿using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Models.Settings;
using System;
using System.Data;
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
                _startDate = args.startDate;
                _endDate = args.endDate;

                FillGrid();
            }
        }

        public void ReportChangedHandler(object sender, ReportBrowser.SelectedReporChangeEventHandlerArgs args)
        {
            throw new NotImplementedException();
        }

        public event VolumeAndDateCountsDontMatchHandler VolumeDateCountsDontMatch;

        void IView.DateRangeChangedHandler(object sender, Toolbar.DateRangeChangedEventHandlerArgs args)
        {
            DateRangeChangedHandler(sender, args);
        }
    }
}
