using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Models.Configuration;
using ATTrafficAnalayzer.Models.Settings;

namespace ATTrafficAnalayzer.Views
{
    public class CSVExporter
    {
        private readonly string _outputFilename;
        private readonly SettingsTray _settings;
        private DbHelper _dbHelper;
        private ReportConfiguration _configuration;

        public CSVExporter(String outputFilename, SettingsTray settings, string configName)
        {
            _outputFilename = outputFilename;
            _settings = settings;
            _dbHelper = DbHelper.GetDbHelper();

            //Retrieve the config for the supplied name
            _configuration = _dbHelper.GetConfiguration(configName);
        }

        public void DoExport()
        {
            using (var file = new StreamWriter(_outputFilename))
            {
                foreach (var approach in _configuration.Approaches)
                {
                    //The approach name and its detectors
                    file.Write(approach.Name + " - Detectors: ");
                    foreach (var detector in approach.Detectors)
                    {
                        file.Write(detector + " ");
                    }
                    file.Write("\n");
                    
                    //Retrieve the volume information from the database
                    // List dates
                    var dates = new List<DateTime>();
                    for (var date = _settings.StartDate; date < _settings.EndDate; date = date.AddMinutes(_settings.Interval))
                    {
                        dates.Add(date);
                    }
                    var approachVolumes = new List<int>();
                    foreach (var detector in approach.Detectors)
                    {
                        if (approachVolumes.Count == 0)
                        {
                            approachVolumes.AddRange(_dbHelper.GetVolumes(_configuration.Intersection, detector, _settings.StartDate,
                                                                          _settings.EndDate));
                        }
                        else
                        {
                            List<int> detectorVolumes = _dbHelper.GetVolumes(_configuration.Intersection, detector, _settings.StartDate,
                                                                          _settings.EndDate);
                            approachVolumes = approachVolumes.Zip(detectorVolumes, (i, i1) => i + i1).ToList();
                        }

                    }

                    //The row headings
                    for (int i = 0; i <= 12; i++)
                    {
                        if (i == 0)
                        {
                            file.Write(",");
                        }
                        else
                        {
                            file.Write((i - 1) + ",");
                        }
                    }
                    file.Write("\n");
                    //Each row
                    // Get volume store data //12 hours
                    for (var i = 0; i < 12; i++)
                    {
                        
                        for (var j = 0; j < 13; j++)
                        {
                            if (j == 0)
                                file.Write(": " + _settings.Interval * i + ",");
                            else
                                file.Write(approachVolumes[i * 12 + j] + ",");
                        }
                        file.Write("\n");            
                    }
                }
            }
        }
    }
}
