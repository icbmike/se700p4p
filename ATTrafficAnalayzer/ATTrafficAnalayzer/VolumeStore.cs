using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ATTrafficAnalayzer
{
    public class VolumeStoreSingleton
    {
        private Dictionary<DateTime, List<List<int>>> _volumesDictionary;
        private static VolumeStoreSingleton instance;

        private VolumeStoreSingleton()
        {
            _volumesDictionary = new Dictionary<DateTime, List<List<int>>>();
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
                Console.WriteLine(recordSize);
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

                //TODO: Do stuff with the record

            }

        }

        private static bool getBit(byte b, int pos)
        {
            return (b & (1 << pos - 1)) != 0;
        }
    }
}
