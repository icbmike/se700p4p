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

            }

        }

        private static RecordType checkRecordType(byte[] record)
        {
            //Get the first four bytes and sum them, if the sum is zero, it is a comment record
            byte[] firstFourBytes = record.Take(4).ToArray();
            int sum = firstFourBytes.Sum(x => (int)x);      //Using LINQ, casting individual bytes to ints
            if (sum == 0) return RecordType.COMMENT;

            //If the first two bytes sum to zero and it is not a comment record then it is a datetime record
            byte[] firstTwoBytes = record.Take(2).ToArray();
            sum = firstTwoBytes.Sum(x => (int)x);
            if (sum == 0) return RecordType.DATETIME;

            //Otherwise it is a volume record
            return RecordType.VOLUME;
        }

        private static bool getBit(byte b, int pos)
        {
            return (b & (1 << pos - 1)) != 0;
        }
    }
}
