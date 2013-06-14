using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.Data;

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
                                        [dateTime] TIME DEFAULT CURRENT_TIMESTAMP NULL, 
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
            SQLiteConnection conn = new SQLiteConnection(dbFile);
            conn.Open();
            SQLiteCommand query = new SQLiteCommand(conn);
            query.CommandText = "SELECT DISTINCT intersection FROM volumes;";
            SQLiteDataReader reader = query.ExecuteReader();

            var intersections = new List<int>();
            while (reader.Read())
            {
                intersections.Add(reader.GetInt32(0));
            }
            conn.Close();
            return intersections;
            
        }

        public List<int> getDetectorsAtIntersection(int intersection)
        {
            SQLiteConnection conn = new SQLiteConnection(dbFile);
            conn.Open();

            SQLiteCommand query = new SQLiteCommand(conn);
            query.CommandText = "SELECT DISTINCT detector FROM volumes WHERE intersection = '@intersection';";
            query.Parameters.AddWithValue("@intersection", intersection);
            SQLiteDataReader reader = query.ExecuteReader();

            List<int> detectors = new List<int>();
            while (reader.Read())
            {
                detectors.Add(reader.GetInt32(0);
            }

            conn.Close();
            return detectors;
        }

        public int getVolume(int intersection, int detector, DateTime dateTime)
        {
            SQLiteConnection conn = new SQLiteConnection(dbFile);
            conn.Open();

            SQLiteCommand query = new SQLiteCommand(conn);

            query.CommandText = "SELECT volume from volumes WHERE intersection = '@intersection' AND detector = '@detector' AND dateTime = '@dateTime';";
            
            query.Parameters.AddWithValue("@intersection", intersection);
            query.Parameters.AddWithValue("@detector", intersection);
            query.Parameters.AddWithValue("@dateTime", dateTime);

            SQLiteDataReader reader = query.ExecuteReader();
            if (reader.RecordsAffected != 1)
            {
                throw new Exception("WHOAH");
            }
            int volume = reader.GetInt32(0);
            conn.Close();
            return volume;
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
