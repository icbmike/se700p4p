using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.ComponentModel;

namespace ATTrafficAnalayzer
{
    public class VolumeStore
    {
        private Dictionary<DateTime, Dictionary<int, VolumeRecord>> _volumesDictionary; //Dictionary of a list of volumeRecords (intersection is the index) for a date.
        private HashSet<int> _intersections; //List of intersections
        private List<DateTimeRecord> _dateTimeRecords;

        public List<DateTimeRecord> DateTimeRecords
        {
            get { return _dateTimeRecords; }
        }
        private Dictionary<int, List<int>> _detectors; //Dictionary of detectors at an intersection

        public VolumeStore()
        {
            _volumesDictionary = new Dictionary<DateTime,Dictionary<int, VolumeRecord>>();
            _intersections = new HashSet<int>();
            _detectors = new Dictionary<int, List<int>>();
            _dateTimeRecords = new List<DateTimeRecord>();
        }

        public void ReadFile(BackgroundWorker bw, string filename)
        {
            //Load the file into memory
            var fs = new FileStream(filename, FileMode.Open);
            var sizeInBytes = (int)fs.Length;
            var byteArray = new byte[sizeInBytes];
            fs.Read(byteArray, 0, sizeInBytes);

            //Now decrypt it
            var index = 0;
            DateTimeRecord currentDateTime = null;
            while (index < sizeInBytes) //seek through the byte array untill we reach the end
            {
                bw.ReportProgress(index / sizeInBytes * 100);
                var recordSize = byteArray[index] + byteArray[index + 1] * 256; //The record size is stored in two bytes, little endian

                index += 2;

                byte[] record;
                if (recordSize % 2 == 0) //Records with odd record length have a trailing null byte.
                {
                    record = byteArray.Skip(index).Take(recordSize).ToArray();
                    index += recordSize;
                }
                else
                {
                    record = byteArray.Skip(index).Take(recordSize + 1).ToArray();
                    index += recordSize + 1;
                }

                //Find out what kind of data we have
                var recordType = RecordFactory.CheckRecordType(record);

                //Construct the appropriate record type
                switch (recordType)
                {
                    case RecordType.Datetime:
                        currentDateTime = RecordFactory.CreateDateTimeRecord(record);
                        _dateTimeRecords.Add(currentDateTime);
                        _volumesDictionary.Add(currentDateTime.DateTime, new Dictionary<int, VolumeRecord>());
                        break;
                    case RecordType.Volume:

                        var volumeRecord = RecordFactory.CreateVolumeRecord(record, recordSize);
                        _volumesDictionary[currentDateTime.DateTime].Add(volumeRecord.IntersectionNumber, volumeRecord);
                        _intersections.Add(volumeRecord.IntersectionNumber);
                        
                        if(!_detectors.ContainsKey(volumeRecord.IntersectionNumber)){
                            _detectors.Add(volumeRecord.IntersectionNumber, volumeRecord.GetDetectors());
                        }
                        break;
                }
            }
        }

        public HashSet<int> GetIntersections()
        {
            return _intersections;
        }

        public List<int> GetDetectorsAtIntersection(int intersection){
            return _detectors[intersection];
        }

        public int GetVolume(int intersection, int detector, DateTime date)
        {
            var x = _volumesDictionary[date];
            return x[intersection].GetVolumeForDetector(detector);
        }
    }
}
