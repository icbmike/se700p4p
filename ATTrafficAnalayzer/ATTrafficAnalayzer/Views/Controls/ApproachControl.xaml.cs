using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.ObjectModel;

namespace ATTrafficAnalayzer.Views.Controls
{
    /// <summary>
    /// Interaction logic for ApproachControl.xaml
    /// </summary>
    public partial class ApproachControl : Border, INotifyPropertyChanged
    {
        private ObservableCollection<int> _detectors;
        private WrapPanel _container;
        private string _approachName;

        public string ApproachName
        {
            get { return _approachName; }
            set { 
                _approachName = value;
                if (null != this.PropertyChanged)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("ApproachName"));
                }
            }
        }

        public ObservableCollection<int> Detectors
        {
            get { return _detectors; }
            set { _detectors = value; }
        }

        public ApproachControl(WrapPanel container, IEnumerable<int> detectors)
        {
            DataContext = this;
            _container = container;
            _detectors = new ObservableCollection<int>();

            
            if (detectors != null)
            {
                foreach (var d in detectors)
                {
                    Detectors.Add(d);
                }
            }
            
            InitializeComponent();

        }
        public ApproachControl(WrapPanel container, List<int> detectors, String name) : this(container, detectors)
        {
            ApproachName = name;
        }

        public int GetDetectorCount()
        {
            return _detectors.Count;
        }



        private void ListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            var listview = sender as ListView;

            if (listview != null && (listview.SelectedItems.Count == 0 || Keyboard.GetKeyStates(Key.LeftShift).Equals(KeyStates.Down)))
            {
                return;
            }

            var items = listview.SelectedItems.Cast<int>().ToList();


            var data = new DataObject();
            data.SetData("source", listview);
            data.SetData("items", items);
            data.SetData("fromMainList", false);
            data.SetData("approach", this);
            DragDrop.DoDragDrop(listview, data, DragDropEffects.Move);

        }

        private void ApproachDrop(object sender, DragEventArgs e)
        {
            var source = e.Data.GetData("source") as ListView;
            var items = e.Data.GetData("items") as List<int>;
            var fromMainList = (bool)e.Data.GetData("fromMainList");

            if (source == DetectorListView)
            {
                return;
            }

            var dragSourceList = source.ItemsSource as ObservableCollection<int>;
            foreach (var item in items)
            {
                if(!Detectors.Contains(item)) Detectors.Add(item);
                if(!fromMainList) dragSourceList.Remove(item);
            }
            
            var sortedDetectors = Detectors.OrderBy(x => x).ToList();
            Detectors.Clear();
            foreach (int i in sortedDetectors)
            {
                Detectors.Add(i);
            }
            
            if (dragSourceList.Count == 0)
            {
                if (e.Data.GetDataPresent("approach"))
                {
                    _container.Children.Remove(e.Data.GetData("approach") as ApproachControl);
                }
            }

        }

        internal void AddDetector(int detector)
        {
            Detectors.Add(detector);
        }

        public event PropertyChangedEventHandler PropertyChanged;

    }
}
