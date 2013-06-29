using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.ObjectModel;
using ATTrafficAnalayzer.VolumeModel;

namespace ATTrafficAnalayzer.Views
{
    /// <summary>
    /// Interaction logic for ReportConfigurationScreen.xaml
    /// </summary>
    public partial class ReportConfigurationScreen : UserControl
    {
        private ObservableCollection<int> _detectorList;
        private List<int> _intersectionList;
        private int _selectedIntersection;
        readonly VolumeDbHelper _dbHelper;

        public string ConfigName { get; set; }

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

            if (listview.SelectedItems.Count == 0)
            {
                return;
            }

            foreach (int x in listview.SelectedItems)
            {
                items.Add(x);
            }

            var data = new DataObject();
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
                foreach (var item in items)
                {
                    (source.ItemsSource as ObservableCollection<int>).Remove(item);
                }
            }

            var approach = new ApproachControl(Approaches, items) { Margin = new Thickness(20, 20, 0, 0) };

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

            foreach (var detector in _detectorList)
            {
                var newApproach = new ApproachControl(Approaches, null, string.Format("Group {0}", detector)) { Margin = new Thickness(20, 20, 0, 0) };
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
            foreach (var detector in _detectorList)
            {
                newApproach.AddDetector(detector);
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var approaches = new List<Approach>();
            for (var i = 1; i < Approaches.Children.Count; i++)
            {
                var appCtrl = Approaches.Children[i] as ApproachControl;
                approaches.Add(new Approach(appCtrl.ApproachName, appCtrl.Detectors.ToList()));
            }

            _dbHelper.addConfiguration(new ReportConfiguration(ConfigName, _selectedIntersection, approaches));

            if (ConfigurationSaved != null)
                ConfigurationSaved(this, new ConfigurationSavedEventHandlerArgs(ConfigName));
        }

        public delegate void ConfigurationSavedEventHandler(object sender, ConfigurationSavedEventHandlerArgs args);

        public event ConfigurationSavedEventHandler ConfigurationSaved;

        public class ConfigurationSavedEventHandlerArgs
        {
            public string ConfigName { get; set; }

            public ConfigurationSavedEventHandlerArgs(string configName)
            {
                ConfigName = configName;
            }
        }
    }
}
