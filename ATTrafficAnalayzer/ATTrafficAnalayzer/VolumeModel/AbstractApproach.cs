using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ATTrafficAnalayzer.VolumeModel
{
    abstract class AbstractApproach
    {
        private String _name;

        public String Name {
            get { return _name; }
            set { _name = value; }
        }

      public abstract int getVolume();
    }
}
