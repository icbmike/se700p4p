using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ATTrafficAnalayzer
{
    public class VolumeStoreSingleton
    {
        private Dictionary<DateTimeRecord, List<VolumeRecord>> _volumesDictionary;
        private static VolumeStoreSingleton instance;

        private VolumeStoreSingleton()
        {
            _volumesDictionary = new Dictionary<DateTimeRecord, List<VolumeRecord>>();
        }

        public static VolumeStoreSingleton getInstance()
        {
            if (instance == null)
            {
                instance = new VolumeStoreSingleton();
            }
            return instance;
        }

        public void readFile(string filename)
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
                int recordSize = byteArray[index] + byteArray[index+1] * 256; //The record size is stored in two bytes, little endian
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
                        _volumesDictionary.Add(currentDateTime, new List<VolumeRecord>());
                        break;
                    case RecordType.VOLUME:
                        VolumeRecord volumeRecord = RecordFactory.createVolumeRecord(record, recordSize);
                        _volumesDictionary[currentDateTime].Add(volumeRecord);

                        break;
                }
            }
        }   

        
    }
}
