using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Parago.Windows;
using ATTrafficAnalayzer.VolumeModel;
using System.Data;

namespace ATTrafficAnalayzer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        enum Displays { Graph, Table };
        Displays _display;

        private VolumeStore _volumeStore;
        private VolumeDbHelper _dbHelper;

        public MainWindow()
        {
            Logger.Clear();

            InitializeComponent();
            DataContext = this;

            _dbHelper = new VolumeDbHelper();

            standardReportsListBox.ItemsSource = _dbHelper.GetConfigs();
            standardReportsListBox.DisplayMemberPath = "name";

            mainContentControl.Content = new WelcomeScreen();
        }

        private void fileImportMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Configure open file dialog box 
            var dlg = new OpenFileDialog
                {
                    FileName = "",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    DefaultExt = ".VS",
                    Filter = "Volume Store Files (.VS)|*.VS"
                };

            // Show open file dialog box 
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results 
            if (result == true)
            {
                var res = ProgressDialog.Execute(this, "Importing VS File", (bw, we) => {

                    ProgressDialog.Report(bw, "Reading Files");

                    // Open document 
                    var filename = dlg.FileName;
                    VolumeDbHelper.ImportFile(filename);
                    //_volumeStore.readFile(bw, filename);
                }, ProgressDialogSettings.WithSubLabelAndCancel);
            }
        }

        private void ChangeScreen(UserControl screen)
        {
            mainContentControl.Content = screen;
        }

        private static bool getRadioContent(Object sender)
        {
            var button = sender as RadioButton;
            return (button.IsChecked == true);
        }

        private void checkDisplayValue()
        {
            var displayValue = getRadioContent(graphradio);

            if (displayValue)
            {
                _display = Displays.Graph;
            }
            else
            {
                _display = Displays.Table;
            }
        }

        private void SwitchScreen(object sender, RoutedEventArgs e)
        {
            checkDisplayValue();
            var settings = SettingsTray.DataContext as SettingsTray;
            
            if (_display == Displays.Table)
            {
                ChangeScreen(new VsTable(_volumeStore, settings.Interval, settings.StartDate, settings.EndDate));
            }
            else if (_display == Displays.Graph)
            {
                ChangeScreen(new VsGraph(_volumeStore, settings.Interval, settings.StartDate, settings.EndDate));
            } else {
                throw new Exception();
            }
        }

        private void newBtn_Click(object sender, RoutedEventArgs e)
        {
            ChangeScreen(new ReportConfigurationScreen());
        }

        private void renameBtn_Click(object sender, RoutedEventArgs e)
        {
            var item = standardReportsListBox.SelectedItem.ToString();
            Console.WriteLine("Rename: " + item);
        }

        private void deleteBtn_Click(object sender, RoutedEventArgs e)
        {
            //Get selection
            var selectedRow = standardReportsListBox.SelectedItem as DataRowView;
            var selectedItem = selectedRow.Row["name"] as string;

            //Configure the message box to be displayed 
            var messageBoxText = "Are you sure you wish to delete " + selectedItem + "?";
            var caption = "Confirm delete";
            var button = MessageBoxButton.OKCancel;
            var icon = MessageBoxImage.Question;

            //Display message box
            var isConfirmedDeletion = MessageBox.Show(messageBoxText, caption, button, icon);

            //Process message box results 
            switch (isConfirmedDeletion)
            {
                case MessageBoxResult.OK:
                    _dbHelper.RemoveConfig(selectedItem);
                    
                    messageBoxText = selectedItem + " was deleted";
                    caption = "Delete successful";
                    button = MessageBoxButton.OK;
                    icon = MessageBoxImage.Information;
                    MessageBox.Show(messageBoxText, caption, button, icon);

                    Logger.Debug(selectedItem + " report deleted", "Reports panel");
                    break;

                case MessageBoxResult.Cancel:
                    Logger.Debug(selectedItem + " report deletion was canceled", "Reports panel");
                    break;
            }
        }

        private void editBtn_Click(object sender, RoutedEventArgs e)
        {
            ChangeScreen(new ReportConfigurationScreen());
        }
    }
}
