
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
                default:
                    return new SqlceDataSource();
//                case "sqlite":
//                default:
//                    return SqliteDataSource.GetDbHelper();
            }
        }
    }
}