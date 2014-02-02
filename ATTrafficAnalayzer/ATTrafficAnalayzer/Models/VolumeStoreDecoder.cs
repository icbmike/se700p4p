using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                if (recordSize%2 != 0)
                {
                    //If record size is odd then an extra byte is added to make it even
                    recordSize += 1;
                }

                //Find out what kind of data we have
                var recordType = VolumeRecordFactory.CheckRecordType(byteArray, index);

                //Construct the appropriate record type
                switch (recordType)
                {
                    case VolumeRecordType.Datetime:
                        currentDateTime = VolumeRecordFactory.CreateDateTimeRecord(byteArray, index);
                        volumeStore.Add(currentDateTime);
                        break;
                    case VolumeRecordType.Volume:
                        currentDateTime.VolumeRecords.Add(VolumeRecordFactory.CreateVolumeRecord(byteArray, index, recordSize));
                        break;
                }

                index += recordSize;
            }
            fs.Close();
            return volumeStore;
        }
    }
}
