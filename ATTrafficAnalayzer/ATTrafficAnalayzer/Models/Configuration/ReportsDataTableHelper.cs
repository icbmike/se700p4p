﻿using System;
using System.Data;
using System.Data.SQLite;
using ATTrafficAnalayzer.Models.Settings;

namespace ATTrafficAnalayzer.Models.Configuration
{
    class DataTableHelper
    {
        private static DataTableHelper _instance;
        private static readonly object SyncLock = new object();
        public static DataTableHelper GetDataTableHelper()
        {
            lock (SyncLock)
                return _instance ?? (_instance = new DataTableHelper());
        }

        private readonly DbHelper _dbHelper = DbHelper.GetDbHelper();

        private DataTableHelper()
        {
            CreateConfigDataSet();
        }

        #region Report Related Methods

        private SQLiteDataAdapter _reportDataAdapter;
        private SQLiteDataAdapter _summaryDataAdapter;

        private readonly DataTable _reportsDataTable = new DataTable();
        private readonly DataTable _summaryDataTable = new DataTable();

        private void CreateConfigDataSet()
        {
            //Regular reports
            _reportDataAdapter = _dbHelper.GetConfigsDataAdapter();
            _reportDataAdapter.Fill(_reportsDataTable);
            new SQLiteCommandBuilder(_reportDataAdapter);
            _reportsDataTable.PrimaryKey = new[] { _reportsDataTable.Columns["name"] };

            //Monthly Summaries
            _summaryDataAdapter = _dbHelper.GetMonthlySummariesDataAdapter();
            _summaryDataAdapter.Fill(_summaryDataTable);
            new SQLiteCommandBuilder(_summaryDataAdapter);
            _summaryDataTable.PrimaryKey = new[] { _summaryDataTable.Columns["name"] };
        }

        public DataView GetReportDataView()
        {
            return _reportsDataTable.DefaultView;
        }

        public void RemoveReport(string configToDelete, Mode selectedMode)
        {
            RemoveConfig(configToDelete, selectedMode);
            SyncConfigs();
        }

        #endregion

        private void RemoveConfig(String configToDelete, Mode selectedMode)
        {
            var configs = selectedMode.Equals(Mode.Report) ? _reportsDataTable.Rows : _summaryDataTable.Rows;
            var rowToDelete = configs.Find(configToDelete);
            rowToDelete.Delete();
        }

        public void SyncConfigs()
        {
            
                _reportDataAdapter.Update(_reportsDataTable);
                _reportDataAdapter.Fill(_reportsDataTable);
           
                _summaryDataAdapter.Update(_summaryDataTable);
                _summaryDataAdapter.Fill(_summaryDataTable);
            
        }

        public DataView GetSummaryDataView()
        {
            return _summaryDataTable.DefaultView;
        }
    }
}
