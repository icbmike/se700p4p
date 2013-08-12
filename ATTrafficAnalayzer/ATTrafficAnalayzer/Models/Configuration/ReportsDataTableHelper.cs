using System;
using System.Collections;
using System.Data;
using System.Data.SQLite;

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
            var configsPrimaryKeys = new DataColumn[1];
            configsPrimaryKeys[0] = _regularReportsDataTable.Columns["name"];
            _regularReportsDataTable.PrimaryKey = configsPrimaryKeys;

            //MonthlySummary
        }

        public DataView GetRegularReportDataView()
        {
            return _regularReportsDataTable.DefaultView;
        }

        public void RemoveConfig(string configToDelete)
        {
            RemoveConfigFromDataSet(configToDelete);
            SyncConfigs();
        }

        public void RemoveConfigFromDataSet(String configToDelete)
        {
            var configs = _regularReportsDataTable.Rows;
            var rowToDelete = configs.Find(configToDelete);
            Console.WriteLine("Deleting: " + rowToDelete["name"]);
            rowToDelete.Delete();
        }

        public void SyncConfigs()
        {
            _regularReportsDataAdapter.Update(_regularReportsDataTable);
            _regularReportsDataAdapter.Fill(_regularReportsDataTable);
        }

        #endregion

        public DataView GetMonthlySummaryDataView()
        {
            throw new NotImplementedException();
        }
    }
}
