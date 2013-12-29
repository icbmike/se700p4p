
using System.Configuration;

namespace ATTrafficAnalayzer.Models
{
    public class DataSourceFactory
    {
        public static IDataSource GetDataSource()
        {
            switch (ConfigurationManager.AppSettings["dataSource"])
            {
                case "sqlce":
                    return new SqlceDataSource();
                case "sqlite":
                default:
                    return new SqliteDataSource();
            }
        }
    }
}