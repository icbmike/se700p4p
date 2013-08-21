using System;
using System.Data;
using System.Data.SQLite;
using ATTrafficAnalayzer.Models.Settings;

namespace ATTrafficAnalayzer.Models.Configuration
{
    class ReportsDataTableHelper
    {
        private static ReportsDataTableHelper _instance;
        private static readonly object SyncLock = new object();
        public static ReportsDataTableHelper GetDataTableHelper()
        {
            lock (SyncLock)
            {
                return _instance ?? (_instance = new ReportsDataTableHelper());
            }
        }

        private readonly DbHelper _dbHelper = DbHelper.GetDbHelper();

        private ReportsDataTableHelper()
        {
            CreateConfigDataSet();
        }

        #region Config Related Methods

        private SQLiteDataAdapter _regularReportsDataAdapter;
        private SQLiteDataAdapter _monthlySummaryDataAdapter;

        private DataTable _regularReportsDataTable = new DataTable();
        private DataTable _monthlySummaryDataTable = new DataTable();

        private void CreateConfigDataSet()
        {
            //Regular reports
            _regularReportsDataAdapter = _dbHelper.GetConfigsDataAdapter();
            _regularReportsDataAdapter.Fill(_regularReportsDataTable);
            new SQLiteCommandBuilder(_regularReportsDataAdapter);

            _regularReportsDataTable.PrimaryKey = new[] { _regularReportsDataTable.Columns["name"] };

            //Monthly Summaries
            _monthlySummaryDataAdapter = _dbHelper.GetMonthlySummariesDataAdapter();
            _monthlySummaryDataAdapter.Fill(_monthlySummaryDataTable);
            new SQLiteCommandBuilder(_monthlySummaryDataAdapter);
            _monthlySummaryDataTable.PrimaryKey = new[] { _monthlySummaryDataTable.Columns["name"] };
        }

        public DataView GetRegularReportDataView()
        {
            return _regularReportsDataTable.DefaultView;
        }

        public void RemoveConfig(string configToDelete, Mode selectedMode)
        {
            RemoveConfigFromDataSet(configToDelete, selectedMode);
            SyncConfigs();
        }

        private void RemoveConfigFromDataSet(String configToDelete, Mode selectedMode)
        {
            var configs = selectedMode.Equals(Mode.Report) ? _regularReportsDataTable.Rows : _monthlySummaryDataTable.Rows;
            var rowToDelete = configs.Find(configToDelete);
            rowToDelete.Delete();
        }

        public void SyncConfigs()
        {
            _regularReportsDataAdapter.Update(_regularReportsDataTable);
            _monthlySummaryDataAdapter.Update(_monthlySummaryDataTable);
            _regularReportsDataAdapter.Fill(_regularReportsDataTable);
            _monthlySummaryDataAdapter.Fill(_monthlySummaryDataTable);
        }

        #endregion

        public DataView GetMonthlySummaryDataView()
        {
            return _monthlySummaryDataTable.DefaultView;
        }
    }
}
