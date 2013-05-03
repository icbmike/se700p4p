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

            int year = recordBytes[2] + 1900,
                month = recordBytes[3],
                day = recordBytes[4],
                hour = recordBytes[5];

            if (hour == 24)
            {
                hour = 0;
                day += 1;
            }

            bool fiveMinutePeriod = getBit(recordBytes[6], 8);
            int minutes = Convert.ToInt32(getBit(recordBytes[6], 1)) * 1 +
                            Convert.ToInt32(getBit(recordBytes[6], 2)) * 2 +
                            Convert.ToInt32(getBit(recordBytes[6], 3)) * 4 +
                            Convert.ToInt32(getBit(recordBytes[6], 4)) * 8 +
                            Convert.ToInt32(getBit(recordBytes[6], 5)) * 16 +
                            Convert.ToInt32(getBit(recordBytes[6], 6)) * 32
                            - 2; //Minutes are encoded as minutes + 2

            return new DateTimeRecord(year, month, day, hour, minutes, fiveMinutePeriod);

        }

        public static VolumeRecord createVolumeRecord(byte[] recordBytes, int recordSize)
        {
            int index = 0;
            int intersectionNumber = recordBytes[index] + recordBytes[index + 1] * 256;
            //recordSize -= 2; // record size includes the intersection number
            index += 2;
            VolumeRecord newRecord = new VolumeRecord(intersectionNumber);
           
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
            return RecordType.VOLUME;
        }
        public static bool getBit(byte b, int pos)
        {
            return (b & (1 << pos - 1)) != 0;
        }

       
    }
}
