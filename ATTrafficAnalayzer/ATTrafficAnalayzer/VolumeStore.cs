using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ComponentModel;

namespace ATTrafficAnalayzer
{
    public class VolumeStore
    {
        private Dictionary<DateTime, List<VolumeRecord>> _volumesDictionary; //Dictionary of a list of volumeRecords (intersection is the index) for a date.
        private List<int> _intersections; //List of intersections
        private List<DateTimeRecord> _dateTimeRecords;
        private Dictionary<int, List<int>> _detectors; //Dictionary of detectors at an intersection

        public VolumeStore()
        {
            _volumesDictionary = new Dictionary<DateTime, List<VolumeRecord>>();
            _intersections = new List<int>();
            _detectors = new Dictionary<int, List<int>>();
            _dateTimeRecords = new List<DateTimeRecord>();
        }

        public void readFile(BackgroundWorker bw, string filename)
        {
            //Load the file into memory
            FileStream fs = new FileStream(filename, FileMode.Open);
            int sizeInBytes = (int)fs.Length;
            byte[] byteArray = new byte[sizeInBytes];
            fs.Read(byteArray, 0, sizeInBytes);

            //Now decrypt it
            int index = 0;
            DateTimeRecord currentDateTime = null;
            while (index < sizeInBytes) //seek through the byte array untill we reach the end
            {
                bw.ReportProgress(index / sizeInBytes * 100);
                int recordSize = byteArray[index] + byteArray[index + 1] * 256; //The record size is stored in two bytes, little endian
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
                RecordType recordType = RecordFactory.checkRecordType(record);

                //Construct the appropriate record type
                switch (recordType)
                {
                    case RecordType.DATETIME:
                        currentDateTime = RecordFactory.createDateTimeRecord(record);
                        _dateTimeRecords.Add(currentDateTime);
                        _volumesDictionary.Add(currentDateTime.dateTime, new List<VolumeRecord>());
                        break;
                    case RecordType.VOLUME:
                        VolumeRecord volumeRecord = RecordFactory.createVolumeRecord(record, recordSize);
                        _volumesDictionary[currentDateTime.dateTime].Add(volumeRecord);
                        break;
                }
            }
            
        }

        public List<int> getIntersections()
        {
            return _intersections;
        }

        public List<int> getDetectorsAtIntersection(int intersection){
            return _detectors[intersection];
        }

        public int getVolume(int intersection, int detector, DateTime date)
        {
            return _volumesDictionary[date][intersection].GetVolumeForDetector(detector);
        }
        
    }
}
