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
    /// Interaction logic for ApproachControl.xaml
    /// </summary>
    public partial class ApproachControl : Border
    {


        private String _approachName;
        private ObservableCollection<int> _detectors;
        private WrapPanel _container;

        public String ApproachName
        {
            get { return _approachName; }
            set { _approachName = value; }
        }

        public ObservableCollection<int> Detectors
        {
            get { return _detectors; }
            set { _detectors = value; }
        }

        public ApproachControl(WrapPanel container, List<int> detectors)
        {
            DataContext = this;
            _container = container;
            _detectors = new ObservableCollection<int>();
            
            if (detectors != null)
            {
                foreach (int d in detectors)
                {
                    Detectors.Add(d);
                }
            }
            InitializeComponent();

        }

        public int getDetectorCount()
        {
            return _detectors.Count;
        }



        private void ListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            var listview = sender as ListView;
            var items = new List<int>();

            if (listview.SelectedItems.Count == 0 || Keyboard.GetKeyStates(Key.LeftShift).Equals(KeyStates.Down))
            {
                return;
            }

            foreach (int x in listview.SelectedItems)
            {
                items.Add(x);
            }
            DataObject data = new DataObject();
            data.SetData("source", listview);
            data.SetData("items", items);
            data.SetData("approach", this);
            DragDrop.DoDragDrop(listview, data, DragDropEffects.Move);

        }

        private void ApproachDrop(object sender, DragEventArgs e)
        {
            var source = e.Data.GetData("source") as ListView;
            var items = e.Data.GetData("items") as List<int>;
            

            var dragSourceList = source.ItemsSource as ObservableCollection<int>;
            foreach (int item in items)
            {
                Detectors.Add(item);
                dragSourceList.Remove(item);
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
    }
}
