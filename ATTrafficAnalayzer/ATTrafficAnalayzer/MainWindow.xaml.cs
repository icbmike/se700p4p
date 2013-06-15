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
using Microsoft.Win32;
using Parago.Windows;
using ATTrafficAnalayzer.VolumeModel;
using System.Collections.ObjectModel;

namespace ATTrafficAnalayzer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        enum displays { graph, table };
        displays display;

        private VolumeStore _volumeStore;
        private VolumeDBHelper _dbHelper;
        ObservableCollection<string> _standardReports;

        public ObservableCollection<string> StandardReports
        {
            get { return _standardReports; }
            set { _standardReports = value; }
        }
        ObservableCollection<string> _specialReports;

        public ObservableCollection<string> SpecialReports
        {
            get { return _specialReports; }
            set { _specialReports = value; }
        }

        public MainWindow()
        {
            Logger.Clear();

            InitializeComponent();
            DataContext = this;
            _dbHelper = new VolumeDBHelper();
            _standardReports = new ObservableCollection<string>();
            
            foreach (String config in _dbHelper.getConfigurations())
            {
                _standardReports.Add(config);
            }

            _specialReports = new ObservableCollection<string>();
            _specialReports.Add("WHHO");

            _volumeStore = new VolumeStore();
            

            this.mainContentControl.Content = new WelcomeScreen();
        }

        private void  MenuItemImportClick(object sender, RoutedEventArgs e)
        {
            // Configure open file dialog box 
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.FileName = ""; // Default file name
            dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile); //The initial directory
            dlg.DefaultExt = ".VS"; // Default file extension 
            dlg.Filter = "Volume Store Files (.VS)|*.VS"; // Filter files by extension 

            // Show open file dialog box 
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results 
            if (result == true)
            {
                ProgressDialogResult res = ProgressDialog.Execute(this, "Importing VS File", (bw, we) => {

                    ProgressDialog.Report(bw, "Reading Files");

                    // Open document 
                    string filename = dlg.FileName;
                    _dbHelper.importFile(filename);
                    //_volumeStore.readFile(bw, filename);
                }, ProgressDialogSettings.WithSubLabelAndCancel);
            }
        }

        private void changeScreen(UserControl screen)
        {
            this.mainContentControl.Content = screen;
        }

        private bool getRadioContent(Object sender)
        {
            RadioButton button = sender as RadioButton;
            return (button.IsChecked == true);
        }

        private void checkDisplayValue()
        {
            bool displayValue = getRadioContent(graphradio);

            if (displayValue)
            {
                display = displays.graph;
            }
            else
            {
                display = displays.table;
            }
        }

        private void switchScreen(object sender, RoutedEventArgs e)
        {
            checkDisplayValue();
            SettingsTray settings = SettingsTray.DataContext as SettingsTray;
            
            if (display == displays.table)
            {
                changeScreen(new VSTable(_volumeStore, settings.Interval, settings.StartDate, settings.EndDate));
            }
            else if (display == displays.graph)
            {
                changeScreen(new VSGraph(_volumeStore, settings.Interval, settings.StartDate, settings.EndDate));
            } else {
                throw new Exception();
            }
        }

        private void newReportClick(object sender, RoutedEventArgs e)
        {
            changeScreen(new ReportConfigurationScreen());
        }

        private void renameBtn_Click(object sender, RoutedEventArgs e)
        {
            ListViewItem item = standardReportsListBox.SelectedItems();

        }


    }
}
