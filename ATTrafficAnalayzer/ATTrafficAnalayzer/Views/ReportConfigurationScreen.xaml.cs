using System;
using System.Collections.Generic;
using System.Linq;
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
        private String _configName;

        public String ConfigName
        {
            get { return _configName; }
            set { _configName = value; }
        }
        VolumeDBHelper _dbHelper;

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

            _dbHelper = new VolumeDBHelper();
            foreach (int detector in _dbHelper.getIntersections())
            {
                _intersectionList.Add(detector);
            }

            InitializeComponent();

            Logger.Info("constructed view", "report config");
        }

        private void onIntersectionSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _detectorList.Clear();
            foreach (int detector in _dbHelper.getDetectorsAtIntersection(_selectedIntersection))
            {
                _detectorList.Add(detector);
            }

        }

        private void ListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            var listview = sender as ListView;
            var items = new List<int>();
            
            if (listview.SelectedItems.Count == 0)
            {
                return;
            }
            
            foreach (int x in listview.SelectedItems)
            {
                items.Add(x);
            }
            DataObject data = new DataObject();
            data.SetData("source", listview);
            data.SetData("fromMainList", true);
            data.SetData("items", items);
            DragDrop.DoDragDrop(listview, data, DragDropEffects.Move);

        }

        private void NewApproachDrop(object sender, DragEventArgs e)
        {
            var source = e.Data.GetData("source") as ListView;
            var items = e.Data.GetData("items") as List<int>;

            var dragSourceList = source.ItemsSource as ObservableCollection<int>;
            
            
            if (source != DetectorListView)
            {
                foreach (int item in items)
                {
                    (source.ItemsSource as ObservableCollection<int>).Remove(item);
                }
            }

            ApproachControl approach = new ApproachControl(Approaches, items) { Margin = new Thickness(20, 20, 0, 0) };
            Approaches.Children.Add(approach);

            if (dragSourceList.Count == 0)
            {
                if (e.Data.GetDataPresent("approach"))
                {
                    Approaches.Children.Remove(e.Data.GetData("approach") as ApproachControl);
                }
            }

        }

        private void Distribute_Click(object sender, RoutedEventArgs e)
        {
            while (Approaches.Children.Count > 1)
            {
                Approaches.Children.RemoveAt(1);
            }
           
            foreach (int detector in _detectorList)
            {
                var newApproach = new ApproachControl(Approaches, null, "Approach " + detector) { Margin = new Thickness(20, 20, 0, 0) };
                newApproach.AddDetector(detector);
                Approaches.Children.Add(newApproach);
                
            }
        }
        
        private void Group_Click(object sender, RoutedEventArgs e)
        {
            while (Approaches.Children.Count > 1)
            {
                Approaches.Children.RemoveAt(1);
            }
            var newApproach = new ApproachControl(Approaches, null, "All Detectors") { Margin = new Thickness(20, 20, 0, 0) };
            Approaches.Children.Add(newApproach);
            foreach (int detector in _detectorList)
            {
                newApproach.AddDetector(detector);
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
           
            List<Approach> approaches = new List<Approach>();
            for(int i = 1; i < Approaches.Children.Count; i ++){

                ApproachControl appCtrl = Approaches.Children[i] as ApproachControl;
                approaches.Add(new Approach(appCtrl.ApproachName, appCtrl.Detectors.ToList()));
            }
            _dbHelper.addConfiguration(new ReportConfiguration(ConfigName,  _selectedIntersection, approaches));
        } 
    }
}
