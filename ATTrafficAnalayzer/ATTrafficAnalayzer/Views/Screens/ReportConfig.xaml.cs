﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.ObjectModel;
using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Models.ReportConfiguration;
using ATTrafficAnalayzer.Modes;
using ATTrafficAnalayzer.Views.Controls;

namespace ATTrafficAnalayzer.Views.Screens
{
    /// <summary>
    /// Interaction logic for Config.xaml
    /// </summary>
    public partial class ReportConfig : INotifyPropertyChanged
    {
        private readonly IDataSource _dataSource;
        private bool _isNewConfig = true;
        private string _oldName;

        /// <summary>
        /// Default constructor
        /// </summary>
        public ReportConfig(IDataSource dataSource)
        {
            DataContext = this;
            _intersectionList = new ObservableCollection<int>();
            _detectorList = new ObservableCollection<int>();

            _dataSource = dataSource;
            foreach (var intersection in _dataSource.GetIntersections())
                _intersectionList.Add(intersection);

            InitializeComponent();

            Logger.Info("constructed view", "report config");
        }

        /// <summary>
        /// Constructor used to edit a configuration.
        /// </summary>
        /// <param name="configToBeEdited">The name of the configuration to be edited</param>
        /// <param name="dataSource">Where config will be retrieved from</param>
        public ReportConfig(string configToBeEdited, IDataSource dataSource) : this(dataSource)
        {
            //Populate config screen
            var config = _dataSource.GetConfiguration(configToBeEdited);
           
            foreach (var approach in config.Approaches)
            {
                var configApproachBox = new ConfigApproachBox(Approaches, approach.Detectors, approach.ApproachName)
                {
                    Margin = new Thickness(20, 20, 0, 0)
                };
                Approaches.Children.Add(configApproachBox);
            }

            SelectedIntersection = config.Intersection;
            _isNewConfig = false;
            _oldName = configToBeEdited;
            ReportNameTextBox.Text = configToBeEdited;
        }

        #region Bindings

        private int _selectedIntersection;
        public int SelectedIntersection
        {
            get { return _selectedIntersection; }
            set
            {
                _selectedIntersection = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("SelectedIntersection"));
            }
        }

        private ObservableCollection<int> _intersectionList;
        public ObservableCollection<int> IntersectionList
        {
            get { return _intersectionList; }
            set { _intersectionList = value; }
        }

        private ObservableCollection<int> _detectorList;
        private ReportConfiguration _configuration;

        public ObservableCollection<int> DetectorList
        {
            get { return _detectorList; }
            set { _detectorList = value; }
        }

        public ReportConfiguration Configuration
        {
            get { return _configuration; }
            set { _configuration = value;
                Render();
            }
        }

        #endregion

        private void Render()
        {
            if (Configuration == null) return;

            ReportNameTextBox.Text = Configuration.Name;

            Approaches.Children.RemoveRange(1, Approaches.Children.Count - 1);
            foreach (var approach in Configuration.Approaches)
            {
                var configApproachBox = new ConfigApproachBox(Approaches, approach.Detectors, approach.ApproachName)
                {
                    Margin = new Thickness(20, 20, 0, 0)
                };
                Approaches.Children.Add(configApproachBox);
            }

            SelectedIntersection = Configuration.Intersection;
            _isNewConfig = false;
            _oldName = Configuration.Name;
        }


        #region Control Event Handlers

        /// <summary>
        /// Drop handler for when detectors are dropped onto the plus sign.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewApproachDrop(object sender, DragEventArgs e)
        {
            //Get the source listview
            var source = e.Data.GetData("source") as ListView;
            //Get the items in that list view
            var items = e.Data.GetData("items") as List<int>;
            Debug.Assert(source != null, "source != null");
            
            var dragSourceList = source.ItemsSource as ObservableCollection<int>;

            //Check the source isnt the detectors list
            if (!Equals(source, DetectorListView))
            {
                Debug.Assert(items != null, "items != null");
                //If its from an ApproachBox remove the items from there
                foreach (var item in items)
                {
                    ((ObservableCollection<int>)source.ItemsSource).Remove(item);
                }
            }

            //Create a new approach box and add it to the screen
            var approach = new ConfigApproachBox(Approaches, items, "New Approach") { Margin = new Thickness(20, 20, 0, 0) };
            Approaches.Children.Add(approach);

            Debug.Assert(dragSourceList != null, "dragSourceList != null");
            
            //If we have emptied an approach box, remove it.
            if (dragSourceList.Count == 0)
            {
                if (e.Data.GetDataPresent("approach"))
                {
                    Approaches.Children.Remove(e.Data.GetData("approach") as ConfigApproachBox);
                }
            }
        }


        /// <summary>
        /// Save button click handler. Replaces configs if they are being edited, creates new entries in the database otherwise.
        /// Fires ConfigurationSaved upon completion.
        /// TODO: put saving in BackgroundWorker with some sort of indeterminate progress dialog.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!_isNewConfig)
            {
                //Delete the previous config before inserting the new one
                try
                {
                    _dataSource.RemoveConfiguration(_oldName);
                }
                catch (Exception exception)
                {
                    Logger.Error(exception, "ReportConfig");
                }
            }

            var configName = ReportNameTextBox.Text;

            var approaches = new List<Approach>();
            for (var i = 1; i < Approaches.Children.Count; i++)
            {
                var appCtrl = Approaches.Children[i] as ConfigApproachBox;
                Debug.Assert(appCtrl != null, "appCtrl != null");
                //Construct Approach objects from the controls. TODO: Move this into a method on ConfigApproachBox
                approaches.Add(new Approach(appCtrl.ApproachName, appCtrl.Detectors.ToList(), _dataSource));
            }

            //Do the database insertion.
            _dataSource.AddConfiguration(new ReportConfiguration(configName, SelectedIntersection, approaches, _dataSource));

            //Fire the saved event
            if (ConfigurationSaved != null)
                ConfigurationSaved(this, new ConfigurationSavedEventArgs(configName, null)); //We don't have a reference to the containing mode right now
        }

        

        /// <summary>
        /// Button handler. Convenience button to distribute detectors into their own approach.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Distribute_Click(object sender, RoutedEventArgs e)
        {
            //Remove all current approaches
            while (Approaches.Children.Count > 1)
                Approaches.Children.RemoveAt(1);

            //Add an approach per detector
            foreach (var detector in _detectorList)
            {
                var newApproach = new ConfigApproachBox(Approaches, null, string.Format("Approach {0}", detector))
                {
                    Margin = new Thickness(20, 20, 0, 0)
                };
                newApproach.AddDetector(detector);
                Approaches.Children.Add(newApproach);
            }
        }

        /// <summary>
        /// Button click handler. Convenience button to group all detectors into one approach.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Group_Click(object sender, RoutedEventArgs e)
        {
            while (Approaches.Children.Count > 1)
                Approaches.Children.RemoveAt(1);

            var newApproach = new ConfigApproachBox(Approaches, null, "All Detectors")
            {
                Margin = new Thickness(20, 20, 0, 0)
            };
            Approaches.Children.Add(newApproach);
            
            foreach (var detector in _detectorList)
                newApproach.AddDetector(detector);
        }

        /// <summary>
        /// Method to automatically name the new report.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReportNameTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isNewConfig)
            {
                var configTextBox = (TextBox)sender;

                for (var count = 1; ; count++)
                {
                    if (!_dataSource.ConfigurationExists("Configuration " + count))
                    {
                        configTextBox.Text = "Configuration " + count;
                        break;
                    }
                }
            }
        }

        #endregion

        #region Other Event Handlers

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

        private void OnIntersectionSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _detectorList.Clear();

            foreach (var detector in _dataSource.GetDetectorsAtIntersection(_selectedIntersection))
            {
                _detectorList.Add(detector);
            }
        }

        internal void ImportCompletedHandler(object sender)
        {
            //Refresh combobox list
            _intersectionList.Clear();
            foreach (var intersection in _dataSource.GetIntersections())
                _intersectionList.Add(intersection);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
        /// <summary>
        /// Event handler to remove ugly red banner message.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
           private void UserControl_GotFocus_1(object sender, RoutedEventArgs e)
        {
            Popup.Visibility = Visibility.Collapsed;
        }


       
        public event ConfigurationSavedEventHandler ConfigurationSaved;
    }
}
