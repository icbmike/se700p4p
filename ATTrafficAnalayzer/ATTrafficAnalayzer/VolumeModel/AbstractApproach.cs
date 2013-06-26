using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ATTrafficAnalayzer.VolumeModel
{
    abstract class AbstractApproach
    {
        public string Name { get; set; }
        public abstract int GetVolume();
    }
}
