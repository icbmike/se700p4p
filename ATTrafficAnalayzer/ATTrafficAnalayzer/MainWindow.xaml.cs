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
using System.Data;
using System.Data.Common;

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

        public MainWindow()
        {
            Logger.Clear();

            InitializeComponent();
            DataContext = this;
            Console.WriteLine("1");


            _dbHelper = new VolumeDBHelper();

            Console.WriteLine("2");

            standardReportsListBox.ItemsSource = _dbHelper.getConfigurations();
            standardReportsListBox.DisplayMemberPath = "name";

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
            String item = standardReportsListBox.SelectedItem.ToString();
            Console.WriteLine("Rename: " + item);
        }

        private void deleteBtn_Click(object sender, RoutedEventArgs e)
        {
            // Configure the message box to be displayed 
            DataRowView drv = standardReportsListBox.SelectedItem as DataRowView;
            String item = drv.Row["name"] as string;
            string messageBoxText = "Are you sure you wish to delete " + item + "?";
            string caption = "Confirm delete";
            MessageBoxButton button = MessageBoxButton.OKCancel;
            MessageBoxImage icon = MessageBoxImage.Question;

            // Display message box
            MessageBoxResult isConfirmedDeletion = MessageBox.Show(messageBoxText, caption, button, icon);

            // Process message box results 
            switch (isConfirmedDeletion)
            {
                case MessageBoxResult.OK:
                    bool isDeleted = _dbHelper.testRemove();
                    if (isDeleted)
                    {
                        messageBoxText = item + " was deleted";
                        caption = "Delete successful";
                        button = MessageBoxButton.OK;
                        icon = MessageBoxImage.Information;
                        MessageBox.Show(messageBoxText, caption, button, icon);

                        Logger.Debug(item + " report deleted", "Reports panel");
                    }
                    else
                    {
                        messageBoxText = item + " could not be deleted";
                        caption = "Delete failure";
                        button = MessageBoxButton.OK;
                        icon = MessageBoxImage.Error;
                        MessageBox.Show(messageBoxText, caption, button, icon);

                        Logger.Error(item + " report could not be deleted", "Reports panel");
                    }
                    break;
                case MessageBoxResult.Cancel:
                    Logger.Debug(item + " report deletion was canceled", "Reports panel");
                    break;
            }
        }

        private void editBtn_Click(object sender, RoutedEventArgs e)
        {
            changeScreen(new ReportConfigurationScreen());
        }
    }
}
