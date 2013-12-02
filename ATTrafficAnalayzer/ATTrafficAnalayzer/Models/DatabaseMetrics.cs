using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ATTrafficAnalayzer.Models
{
    class DatabaseMetrics
    {

        private IDataSource dataSource;
        private List<long> times;
        private List<int> volumes;
        private int count;

        public DatabaseMetrics()
        {
            dataSource = DbHelper.GetDbHelper();
            times = new List<long>();
            volumes = new List<int>();
            count = 0;
        }

        public void Run()
        {
            for (int i = 0; i < 10; i++)
            {
                var sw = new Stopwatch();
                volumes.Clear();
                sw.Start();
                foreach (var detector in dataSource.GetDetectorsAtIntersection(4902))
                {
                    volumes.AddRange(dataSource.GetVolumes(4902, detector, new DateTime(2013, 3, 11), new DateTime(2013, 3, 12)));    
                }
                count = volumes.Count;
                sw.Stop();
                times.Add(sw.ElapsedMilliseconds);
            }
        }

        public void OutputResults()
        {
            Console.WriteLine("----------------------\n\n\n\n");
            Console.WriteLine("Count: " + count);
            Console.WriteLine("Average Elapsed Time: " + times.Average());
            Console.WriteLine("\n\n\n-----------------");
        }

        static void Main(string[] args)
        {
            var dbm = new DatabaseMetrics();
            dbm.Run();
            dbm.OutputResults();
        }
    }
}
