using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ATTrafficAnalayzer
{
    public class RecordFactory
    {
        public static DateTimeRecord createDateTimeRecord(byte[] recordBytes)
        {

            int year = recordBytes[2],
                month = recordBytes[3],
                day = recordBytes[4],
                hour = recordBytes[5];

            bool fiveMinutePeriod = getBit(recordBytes[6], 8);
            int minutes = Convert.ToInt32(getBit(recordBytes[6], 1)) * 1 +
                            Convert.ToInt32(getBit(recordBytes[6], 1)) * 2 +
                            Convert.ToInt32(getBit(recordBytes[6], 1)) * 4 +
                            Convert.ToInt32(getBit(recordBytes[6], 1)) * 8 +
                            Convert.ToInt32(getBit(recordBytes[6], 1)) * 16 +
                            Convert.ToInt32(getBit(recordBytes[6], 1)) * 32;

            return new DateTimeRecord(year, month, day, hour, minutes, fiveMinutePeriod);

        }


        public static VolumeRecord createVolumeRecord(byte[] recordBytes, int recordSize)
        {
            int index = 0;
            int intersectionNumber = recordBytes[index] + recordBytes[index + 1] * 256;
            //recordSize -= 2; // record size includes the intersection number
            index += 2;
            VolumeRecord newRecord = new VolumeRecord(intersectionNumber);
            Console.WriteLine(intersectionNumber);
            while (index < recordSize)
            {
                byte volume0_7 = recordBytes[index];
                byte detector_volume_8_10 = recordBytes[index + 1];
                int volume = (int)volume0_7 + 
                    Convert.ToInt32(getBit(detector_volume_8_10, 1)) * 256 + 
                    Convert.ToInt32(getBit(detector_volume_8_10, 2)) * 512 + 
                    Convert.ToInt32(getBit(detector_volume_8_10, 3)) * 1024;
                
                
                int detectorNumber = Convert.ToInt32(getBit(detector_volume_8_10, 4))  * 1 + 
                    Convert.ToInt32(getBit(detector_volume_8_10, 5)) * 2 + 
                    Convert.ToInt32(getBit(detector_volume_8_10, 6)) * 4 + 
                    Convert.ToInt32(getBit(detector_volume_8_10, 7)) * 8 + 
                    Convert.ToInt32(getBit(detector_volume_8_10, 8)) * 16;
                
                index += 2;
                
                newRecord.SetVolumeForDetector(detectorNumber, volume);
            }

            return newRecord;
        }

        public static RecordType checkRecordType(byte[] recordBytes)
        {
            //Get the first four bytes and sum them, if the sum is zero, it is a comment record
            byte[] firstFourBytes = recordBytes.Take(4).ToArray();
            int sum = firstFourBytes.Sum(x => (int)x); //Using LINQ, casting individual bytes to ints
            if (sum == 0) return RecordType.COMMENT;

            //If the first two bytes sum to zero and it is not a comment record then it is a datetime record
            byte[] firstTwoBytes = recordBytes.Take(2).ToArray();
            sum = firstTwoBytes.Sum(x => (int)x);
            if (sum == 0) return RecordType.DATETIME;

            //Otherwise it is a volume record

            byte b = 128;

            return RecordType.VOLUME;
        }
        public static bool getBit(byte b, int pos)
        {
            return (b & (1 << pos - 1)) != 0;
        }

       
    }
}
