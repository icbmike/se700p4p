using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.ObjectModel;
using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Models.Configuration;
using ATTrafficAnalayzer.Views.Controls;

namespace ATTrafficAnalayzer.Views.Screens
{
    /// <summary>
    /// Interaction logic for ReportConfigurationScreen.xaml
    /// </summary>
    public partial class ReportConfigurationScreen : UserControl
    {
        private ObservableCollection<int> _detectorList;
        private List<int> _intersectionList;
        private int _selectedIntersection;

        private readonly DbHelper _dbHelper;
        private readonly DataTableHelper _dataTableHelper = DataTableHelper.GetDataTableHelper();

        #region events

        public delegate void ConfigurationSavedEventHander(object sender, ConfigurationSavedEventArgs args);

        public event ConfigurationSavedEventHander ConfigurationSaved;
        public class ConfigurationSavedEventArgs
        {
            public string Name { get; set; }

            public ConfigurationSavedEventArgs(string name)
            {
                Name = name;
            }
        } 

        #endregion

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

            _dbHelper = DbHelper.GetDbHelper();
            foreach (var detector in DbHelper.GetIntersections())
            {
                _intersectionList.Add(detector);
            }

            InitializeComponent();

            Logger.Info("constructed view", "report config");
        }

        private void OnIntersectionSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _detectorList.Clear();
            foreach (var detector in DbHelper.GetDetectorsAtIntersection(_selectedIntersection))
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
            var configName = ConfigNameTextBox.Text;

            var approaches = new List<Approach>();
            for (var i = 1; i < Approaches.Children.Count; i++)
            {
                var appCtrl = Approaches.Children[i] as ApproachControl;
                approaches.Add(new Approach(appCtrl.ApproachName, appCtrl.Detectors.ToList()));
            }

            _dbHelper.addConfiguration(new ReportConfiguration(configName, _selectedIntersection, approaches));
            _dataTableHelper.SyncConfigs();
            ConfigurationSaved(this, new ConfigurationSavedEventArgs(configName));
        }

        private void ConfigNameTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            var configTextBox = (TextBox) sender;

            for (var count=1; ; count++)
            {
                if (!_dbHelper.ConfigExists("Report" + count))
                {
                    configTextBox.Text = "Report" + count;
                    break;
                }
            }
        }
    }
}
