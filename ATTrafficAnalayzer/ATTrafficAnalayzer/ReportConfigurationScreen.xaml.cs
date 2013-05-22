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
using System.Collections.ObjectModel;

namespace ATTrafficAnalayzer
{
    /// <summary>
    /// Interaction logic for ReportConfigurationScreen.xaml
    /// </summary>
    /// 

    public partial class ReportConfigurationScreen : UserControl
    {
        private ObservableCollection<int> _detectorList;
        private List<int> _intersectionList;
        private int _selectedIntersection;
        private VolumeStore _vs;

        public int SelectedIntersection
        {
            get { return _selectedIntersection; }
            set { _selectedIntersection = value; }
        }

        public List<int> IntersectionList
        {
            get { return _intersectionList; }
            set { _intersectionList = value; }
        }

        public ObservableCollection<int> DetectorList
        {
            get { return _detectorList; }
            set { _detectorList = value; }
        }

        public ReportConfigurationScreen(VolumeStore vs)
        {
            DataContext = this;
            _vs = vs;
            _intersectionList = vs.getIntersections().ToList();
            _detectorList = new ObservableCollection<int>();
            _detectorList.Add(123);
            InitializeComponent();
        }

        private void onIntersectionSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _detectorList.Clear();
            foreach (int detector in _vs.getDetectorsAtIntersection(_selectedIntersection))
            {
                _detectorList.Add(detector);
            }

        }
    }
}
