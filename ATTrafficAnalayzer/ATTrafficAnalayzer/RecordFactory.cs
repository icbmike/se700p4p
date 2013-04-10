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
            return null;
        }

        public static VolumeRecord createVolumeRecord(byte[] recordBytes)
        {
            return null;
        }

        public static RecordType checkRecordType(byte[] recordBytes)
        {
            //Get the first four bytes and sum them, if the sum is zero, it is a comment record
            byte[] firstFourBytes = recordBytes.Take(4).ToArray();
            int sum = firstFourBytes.Sum(x => (int)x);      //Using LINQ, casting individual bytes to ints
            if (sum == 0) return RecordType.COMMENT;

            //If the first two bytes sum to zero and it is not a comment record then it is a datetime record
            byte[] firstTwoBytes = recordBytes.Take(2).ToArray();
            sum = firstTwoBytes.Sum(x => (int)x);
            if (sum == 0) return RecordType.DATETIME;

            //Otherwise it is a volume record
            return RecordType.VOLUME;
        }

    }
}
