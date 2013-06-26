using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.ObjectModel;
using ATTrafficAnalayzer.VolumeModel;

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
        VolumeDbHelper _dbHelper;

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

        public ReportConfigurationScreen()
        {
            DataContext = this;
            _intersectionList = new List<int>();
            _detectorList = new ObservableCollection<int>();

            _dbHelper = new VolumeDbHelper();
            foreach (var detector in VolumeDbHelper.GetIntersections())
            {
                _intersectionList.Add(detector);
            }

            InitializeComponent();

            Logger.Info("constructed view", "report config");
        }

        private void OnIntersectionSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _detectorList.Clear();
            foreach (var detector in VolumeDbHelper.GetDetectorsAtIntersection(_selectedIntersection))
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
            var data = new DataObject();
            data.SetData("source", listview);
            data.SetData("items", items);
            DragDrop.DoDragDrop(listview, data, DragDropEffects.Move);
        }

        private void NewApproachDrop(object sender, DragEventArgs e)
        {
            var source = e.Data.GetData("source") as ListView;
            var items = e.Data.GetData("items") as List<int>;

            foreach(var item in items)
            {
                (source.ItemsSource as ObservableCollection<int>).Remove(item);
            }
            
            var approach = new ApproachControl(Approaches, items) {Margin = new Thickness(20, 20, 0, 0)};
            Approaches.Children.Add(approach);
        }

        private void saveConfigBtn_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }   
    }
}
