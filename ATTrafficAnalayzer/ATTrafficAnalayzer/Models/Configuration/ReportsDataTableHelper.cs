using System;
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
            //TODO fault here
            CreateConfigDataSet();
        }

        #region Configuration Related Methods

        private SQLiteDataAdapter _configsDataAdapter;
        private DataTable _configsDataTable = new DataTable();

        private void CreateConfigDataSet()
        {
            _configsDataAdapter = _dbHelper.GetConfigsDataAdapter();
            _configsDataAdapter.Fill(_configsDataTable);

            new SQLiteCommandBuilder(_configsDataAdapter);
            var configsPrimaryKeys = new DataColumn[1];
            configsPrimaryKeys[0] = _configsDataTable.Columns["name"];
            _configsDataTable.PrimaryKey = configsPrimaryKeys;
        }

        public DataView GetConfigDataView()
        {
            return _configsDataTable.DefaultView;
        }

        public void RemoveConfig(string configToDelete)
        {
            RemoveConfigFromDataSet(configToDelete);
            SyncConfigs();
        }

        public void RemoveConfigFromDataSet(String configToDelete)
        {
            var configs = _configsDataTable.Rows;
            var rowToDelete = configs.Find(configToDelete);
            Console.WriteLine("Deleting: " + rowToDelete["name"]);
            rowToDelete.Delete();
        }

        public void SyncConfigs()
        {
            _configsDataAdapter.Update(_configsDataTable);
            _configsDataAdapter.Fill(_configsDataTable);
        }

        #endregion
    }
}
