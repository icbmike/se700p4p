﻿using System.Collections.Generic;
using System.Diagnostics;
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
    /// Interaction logic for Config.xaml
    /// </summary>
    public partial class Config : IConfigScreen
    {
        private ObservableCollection<int> _detectorList;
        private ObservableCollection<int> _intersectionList;
        private int _selectedIntersection;

        private readonly DbHelper _dbHelper;
        private readonly ReportsDataTableHelper _reportsDataTableHelper = ReportsDataTableHelper.GetDataTableHelper();

        #region events

       

        #endregion

        public int SelectedIntersection
        {
            get { return _selectedIntersection; }
            set { _selectedIntersection = value; }
        }

        public ObservableCollection<int> IntersectionList
        {
            get { return _intersectionList; }
            set { _intersectionList = value; }
        }

        public ObservableCollection<int> DetectorList
        {
            get { return _detectorList; }
            set { _detectorList = value; }
        }

        public Config()
        {
            DataContext = this;
            _intersectionList = new ObservableCollection<int>();
            _detectorList = new ObservableCollection<int>();

            _dbHelper = DbHelper.GetDbHelper();
            foreach (var intersection in DbHelper.GetIntersections())
                _intersectionList.Add(intersection);

            InitializeComponent();

            Logger.Info("constructed view", "report config");
        }

        private void OnIntersectionSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _detectorList.Clear();
            
            foreach (var detector in _dbHelper.GetDetectorsAtIntersection(_selectedIntersection))
            {
                _detectorList.Add(detector);
            }
        }

        private void ListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var listview = sender as ListView;

            Debug.Assert(listview != null, "listview != null");
            if (listview.SelectedItems.Count == 0)
            {
                return;
            }

            var items = listview.SelectedItems.Cast<int>().ToList();

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
            Debug.Assert(source != null, "source != null");
            var dragSourceList = source.ItemsSource as ObservableCollection<int>;

            if (!Equals(source, DetectorListView))
            {
                Debug.Assert(items != null, "items != null");
                foreach (var item in items)
                {
                    ((ObservableCollection<int>) source.ItemsSource).Remove(item);
                }
            }

            var approach = new ConfigApproachBox(Approaches, items) { Margin = new Thickness(20, 20, 0, 0) };

            Approaches.Children.Add(approach);


            Debug.Assert(dragSourceList != null, "dragSourceList != null");
            if (dragSourceList.Count == 0)
            {
                if (e.Data.GetDataPresent("approach"))
                {
                    Approaches.Children.Remove(e.Data.GetData("approach") as ConfigApproachBox);
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
                var newApproach = new ConfigApproachBox(Approaches, null, string.Format("Group {0}", detector)) { Margin = new Thickness(20, 20, 0, 0) };
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
            var newApproach = new ConfigApproachBox(Approaches, null, "All Detectors") { Margin = new Thickness(20, 20, 0, 0) };
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
                var appCtrl = Approaches.Children[i] as ConfigApproachBox;
                Debug.Assert(appCtrl != null, "appCtrl != null");
                approaches.Add(new Approach(appCtrl.ApproachName, appCtrl.Detectors.ToList()));
            }

            _dbHelper.addConfiguration(new Report(configName, _selectedIntersection, approaches));
            _reportsDataTableHelper.SyncConfigs();
            ConfigurationSaved(this, new ConfigurationSavedEventArgs(configName)); 
        }

        private void ConfigNameTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            var configTextBox = (TextBox) sender;

            for (var count=1; ; count++)                             
            {
                if (!_dbHelper.ConfigExists("Report " + count))
                {
                    configTextBox.Text = "Report " + count;
                    break;
                }
            }
        }

        internal void ImportCompletedHandler(object sender)
        {
            //Refresh combobox list
            _intersectionList.Clear();
            foreach (var intersection in DbHelper.GetIntersections())
                _intersectionList.Add(intersection);
        }

        public event ConfigurationSavedEventHander ConfigurationSaved;
    }
}
