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
            _detectorList.Add(1);
            _detectorList.Add(2);
            _detectorList.Add(3);
            InitializeComponent();

            Logger.Info("constructed view", "report config");
        }

        private void onIntersectionSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _detectorList.Clear();
            foreach (int detector in _vs.getDetectorsAtIntersection(_selectedIntersection))
            {
                _detectorList.Add(detector);
            }

        }

        private void ListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            var listview = sender as ListView;
            var items = new List<int>();

            foreach (int x in listview.SelectedItems)
            {
                items.Add(x);
            }
            DataObject data = new DataObject();
            data.SetData("source", listview);
            data.SetData("items", items);
            DragDrop.DoDragDrop(listview, data, DragDropEffects.Move);

        }

        private void NewApproachDrop(object sender, DragEventArgs e)
        {
            var source = e.Data.GetData("source") as ListView;
            var items = e.Data.GetData("items") as List<int>;


            foreach(int item in items)
            {
                (source.ItemsSource as ObservableCollection<int>).Remove(item);
            }
            
            ApproachControl approach = new ApproachControl(Approaches, items);
            approach.Margin = new Thickness(20, 20, 0, 0);
            Approaches.Children.Add(approach);

        }        
    }
}
