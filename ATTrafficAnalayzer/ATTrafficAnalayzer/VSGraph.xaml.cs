using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ATTrafficAnalayzer
{
    /// <summary>
    /// Interaction logic for VSGraph.xaml
    /// </summary>
    public partial class VSGraph : UserControl
    {
        private VolumeStore _volumeStore;
        private int _interval;
        private DateTime _startDate;
        private DateTime _endDate;

        public VSGraph()
        {
            InitializeComponent();
        }


        public VSGraph(VolumeStore _volumeStore, int interval, DateTime startDate, DateTime endDate)
        {
            // TODO: Complete member initialization
            this._volumeStore = _volumeStore;
            this._interval = interval;
            this._startDate = startDate;
            this._endDate = endDate;
        }
    }
}
