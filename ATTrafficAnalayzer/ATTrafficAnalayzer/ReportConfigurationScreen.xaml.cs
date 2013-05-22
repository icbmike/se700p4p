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
            DragDrop.DoDragDrop(listview, items, DragDropEffects.Move);

        }

        private void Border_Drop(object sender, DragEventArgs e)
        {
            var items = e.Data.GetData(typeof(List<int>)) as List<int>;

            var b = new Border();
            b.Width = 150;
            b.Height = 150;
            b.Margin = new Thickness(20, 20, 0, 0);
            b.CornerRadius = new CornerRadius(5);
            b.BorderBrush = Brushes.Black;
            b.BorderThickness = new Thickness(2);
            b.AllowDrop = true;
            var newList = new ListView();
            newList.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0xAA, 0xEE));

            foreach(int i in items)
            {
                newList.Items.Add(i);
            }
            b.Child = newList;
            Approaches.Children.Add(b);

        }
    }
}
