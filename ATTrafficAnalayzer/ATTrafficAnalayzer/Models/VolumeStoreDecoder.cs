using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ATTrafficAnalayzer.Models.Volume;

namespace ATTrafficAnalayzer.Models
{
    public class VolumeStoreDecoder
    {
        public static List<DateTimeRecord> DecodeFile(string filename)
        {
            //Load the file into memory
            var fs = new FileStream(filename, FileMode.Open);
            var sizeInBytes = (int) fs.Length;
            var byteArray = new byte[sizeInBytes];
            fs.Read(byteArray, 0, sizeInBytes);

            //Now decrypt it
            var index = 0;
            DateTimeRecord currentDateTime = null;

            var volumeStore = new List<DateTimeRecord>();
            while (index < sizeInBytes) //seek through the byte array untill we reach the end
            {
                var recordSize = byteArray[index] + byteArray[index + 1]*256;
                //The record size is stored in two bytes, little endian

                index += 2;
                byte[] record;
                if (recordSize%2 == 0) //Records with odd record length have a trailing null byte.
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
                var recordType = VolumeRecordFactory.CheckRecordType(record);

                //Construct the appropriate record type
                switch (recordType)
                {
                    case VolumeRecordType.Datetime:
                        currentDateTime = VolumeRecordFactory.CreateDateTimeRecord(record);
                        volumeStore.Add(currentDateTime);
                        break;
                    case VolumeRecordType.Volume:
                        currentDateTime.VolumeRecords.Add(VolumeRecordFactory.CreateVolumeRecord(record, recordSize));
                        break;
                }
            }
            fs.Close();
            return volumeStore;
        }
    }
}
