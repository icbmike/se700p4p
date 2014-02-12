
using System.Configuration;

namespace ATTrafficAnalayzer.Models
{
    public class DataSourceFactory
    {
        public static IDataSource GetDataSource()
        {
            switch (ConfigurationManager.AppSettings["dataSource"])
            {
                default:
                    return new SqliteDataSource();
            }
        }
    }
}