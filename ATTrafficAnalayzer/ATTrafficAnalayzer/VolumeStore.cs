using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ATTrafficAnalayzer
{
    public class VolumeStore
    {
        private byte[] _file;

        public VolumeStore(String filename)
        {
            _file = File.ReadAllBytes(filename);
        }
    }
}
