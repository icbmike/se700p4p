using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;

namespace ATTrafficAnalayzer.VolumeModel
{
    class VolumeDBHelper
    {
        string dbFile = "Data Source=TAdb.db3";
        
        public VolumeDBHelper()
        {
            
            SQLiteConnection conn = new SQLiteConnection(dbFile);
            conn.Open();
            
            //Check if tables exist in database file create if they don't

            createVolumesTableIfNotExists(conn);
            createApproachesTableIfNotExists(conn);
            createConfigsTableIfNotExists(conn);

            conn.Close();
        }

        private void createConfigsTableIfNotExists(SQLiteConnection conn)
        {
            var volumesExists = "SELECT name FROM sqlite_master WHERE type='table' AND name='configs';";
            SQLiteCommand exists = new SQLiteCommand(conn);
            exists.CommandText = volumesExists;
            SQLiteDataReader reader = exists.ExecuteReader();
            if (!reader.HasRows)
            {
                var createTable = @"CREATE TABLE [configs] ( 
                                        [name] TEXT  NULL,
                                        [config] TEXT  NULL
                                   )";
                SQLiteCommand create = new SQLiteCommand(conn);
                create.CommandText = createTable;
                create.ExecuteNonQuery();
            }
        }

        private void createApproachesTableIfNotExists(SQLiteConnection conn)
        {
            var volumesExists = "SELECT name FROM sqlite_master WHERE type='table' AND name='approaches';";
            SQLiteCommand exists = new SQLiteCommand(conn);
            exists.CommandText = volumesExists;
            SQLiteDataReader reader = exists.ExecuteReader();
            if (!reader.HasRows)
            {
                var createTable = @"CREATE TABLE [approaches] ( 
                                        [name] TEXT  NULL,
                                        [approach] TEXT  NULL
                                    )";
                SQLiteCommand create = new SQLiteCommand(conn);
                create.CommandText = createTable;
                create.ExecuteNonQuery();
            }
        }

        private void createVolumesTableIfNotExists(SQLiteConnection conn)
        {
            var volumesExists = "SELECT name FROM sqlite_master WHERE type='table' AND name='volumes';";
            SQLiteCommand exists = new SQLiteCommand(conn);
            exists.CommandText = volumesExists;
            SQLiteDataReader reader = exists.ExecuteReader();
            if (!reader.HasRows)
            {
                var createTable = @"CREATE TABLE [volumes] ( 
                                        [DateTime] TIME DEFAULT CURRENT_TIMESTAMP NULL, 
                                        [intersection] INTEGER  NULL,
                                        [detector] INTEGER  NULL,
                                        [volume] INTEGER  NULL
                                    )";
                SQLiteCommand create = new SQLiteCommand(conn);
                create.CommandText = createTable;
                create.ExecuteNonQuery();
            }
        }

        #region Volume Related Methods
        public void importFile(string filename)
        {
        }

        public List<int> getIntersections()
        {
            throw new NotImplementedException();
        }

        public List<int> getDetectorsAtIntersection(int intersection)
        {
            throw new NotImplementedException();
        }

        public int getVolume(int intersection, int detector, DateTime dateTime)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Configuration Related Methods

        public List<String> getConfigurations()
        {
            throw new NotImplementedException();
        }

        public List<Approach> getApproaches(String configName)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
