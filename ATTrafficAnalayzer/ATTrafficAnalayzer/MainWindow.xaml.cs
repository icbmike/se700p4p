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

namespace ATTrafficAnalayzer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        enum modes { vs, sm };
        enum displays { graph, table };

        modes mode;
        displays display;

        private VolumeStore _volumeStore;

        public MainWindow()
        {
            InitializeComponent();

            List<string> StandardReports = new List<string>();
            StandardReports.Add("Standard Report 1");
            StandardReports.Add("Standard Report 2");
            StandardReports.Add("Standard Report 3");
            StandardReports.Add("Standard Report 4");
            StandardReports.Add("Standard Report 5");
            standardReportsListBox.ItemsSource = StandardReports;

            List<string> SpecialReports = new List<string>();
            SpecialReports.Add("Special Report 1");
            SpecialReports.Add("Special Report 2");
            SpecialReports.Add("Special Report 3");
            SpecialReports.Add("Special Report 4");
            SpecialReports.Add("Special Report 5");
            specialReportsListBox.ItemsSource = SpecialReports;

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
                    _volumeStore.readFile(bw, filename);
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

        private void checkModeValue()
        {
            bool modeValue = getRadioContent(smradio);

            if (modeValue)
            {
                mode = modes.sm;
            }
            else
            {
                mode = modes.vs;
            }
        }

        private void switchScreen(object sender, RoutedEventArgs e)
        {
            checkDisplayValue();
            checkModeValue();
            SettingsTray settings = SettingsTray.DataContext as SettingsTray;
            
            if (display == displays.table && mode == modes.vs)
            {
                changeScreen(new VSTable(_volumeStore, settings.Interval, settings.StartDate, settings.EndDate));
            }
            else if (display == displays.table && mode == modes.sm)
            {
                changeScreen(new SMTable());
            }
            else if (display == displays.graph && mode == modes.vs)
            {
                changeScreen(new VSGraph(_volumeStore, settings.Interval, settings.StartDate, settings.EndDate));
            }
            else if (display == displays.graph && mode == modes.sm)
            {
                changeScreen(new SMGraph());

            }
        }

        private void newReportClick(object sender, RoutedEventArgs e)
        {
            changeScreen(new ReportConfigurationScreen(_volumeStore));
        }

    }
}
