using System;
using System.Windows;
using System.Windows.Controls;
using ATTrafficAnalayzer.Views;
using Microsoft.Win32;
using Parago.Windows;
using ATTrafficAnalayzer.VolumeModel;
using System.Data;

namespace ATTrafficAnalayzer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        enum Displays { Graph, Table };
        Displays _display;

        private VolumeDbHelper _dbHelper;

        public MainWindow ()
        {
            Logger.Clear ();

            InitializeComponent ();
            DataContext = this;
            
            InitializeComponent();
            startDatePicker.SelectedDate = new DateTime(2013, 3, 11);
            endDatePicker.SelectedDate = new DateTime(2013, 3, 12);

            _dbHelper = VolumeDbHelper.GetDbHelper();

            standardReportsListBox.ItemsSource = _dbHelper.GetConfigs ();
            standardReportsListBox.DisplayMemberPath = "name";

            mainContentControl.Content = new WelcomeScreen ();
        }

        private void fileImportMenuItem_Click (object sender, RoutedEventArgs e)
        {
            // Configure open file dialog box 
            var dlg = new OpenFileDialog
                {
                    FileName = "",
                    InitialDirectory = Environment.GetFolderPath (Environment.SpecialFolder.UserProfile),
                    DefaultExt = ".VS",
                    Filter = "Volume Store Files (.VS)|*.VS"
                };

            // Show open file dialog box 
            Nullable<bool> result = dlg.ShowDialog ();

            // Process open file dialog box results 
            if (result == true)
            {
                var res = ProgressDialog.Execute (this, "Importing VS File", (bw, we) =>
                {

                    ProgressDialog.Report (bw, "Reading Files");

                    // Open document 
                    var filename = dlg.FileName;
                    VolumeDbHelper.ImportFile (filename);
                    //_volumeStore.readFile(bw, filename);
                }, ProgressDialogSettings.WithSubLabelAndCancel);
            }
        }

        //Changes the screen in the content part of the main windows
        //Potentially could check if the new screen is an instance of one already being displayed??
        private void ChangeScreen (UserControl screen)
        {
            if (screen.GetType () != mainContentControl.Content.GetType ())
                mainContentControl.Content = screen;
        }


        private void SwitchScreen (object sender, RoutedEventArgs e)
        {
            var settings = SettingsTray.DataContext as SettingsTray;
            //Get selection
            var selectedRow = standardReportsListBox.SelectedItem as DataRowView;
            var selectedItem = selectedRow.Row["name"] as string;
            if (sender.Equals(GraphButton))
            {
                ChangeScreen(new VsGraph(settings, selectedItem));
            }
            else if (sender.Equals(TableButton))
            {
                ChangeScreen(new VsTable(settings, selectedItem));
            }        
        }

        private void newBtn_Click (object sender, RoutedEventArgs e)
        {
            ChangeScreen (new ReportConfigurationScreen ());
            var reportConfigurationScreen = new ReportConfigurationScreen();
            standardReportsListBox.ItemsSource = _dbHelper.GetConfigs();
            ChangeScreen(reportConfigurationScreen);
        }

        private void renameBtn_Click (object sender, RoutedEventArgs e)
        {
            var item = standardReportsListBox.SelectedItem.ToString ();
            Console.WriteLine ("Rename: {0}", item);
        }

        private void deleteBtn_Click (object sender, RoutedEventArgs e)
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
            var isConfirmedDeletion = MessageBox.Show (messageBoxText, caption, button, icon);

            //Process message box results 
            switch (isConfirmedDeletion)
            {
                case MessageBoxResult.OK:
                    _dbHelper.RemoveConfig (selectedItem);

                    messageBoxText = selectedItem + " was deleted";
                    caption = "Delete successful";
                    button = MessageBoxButton.OK;
                    icon = MessageBoxImage.Information;
                    MessageBox.Show (messageBoxText, caption, button, icon);

                    Logger.Debug (selectedItem + " report deleted", "Reports panel");
                    break;

                case MessageBoxResult.Cancel:
                    Logger.Debug (selectedItem + " report deletion was canceled", "Reports panel");
                    break;
            }
        }

        private void editBtn_Click (object sender, RoutedEventArgs e)
        {
            ChangeScreen (new ReportConfigurationScreen ());
        }

        private void Image_MouseLeftButtonDown (object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ChangeScreen (new WelcomeScreen ());
        }

        private void FileQuitMenuItem_OnClick (object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown ();
        }

        private void HelpAboutUsMenuItem_OnClick (object sender, RoutedEventArgs e)
        {
            // Configure the message box to be displayed 
            const string messageBoxText = "Auckland Transport Traffic Report Viewer\n\n" +
                                          "Created by Michael Little and Andrew Luey";
            const string caption = "About Us";
            const MessageBoxButton button = MessageBoxButton.OK;
            const MessageBoxImage icon = MessageBoxImage.None;

            MessageBox.Show (messageBoxText, caption, button, icon);
        }

        private void MainToolbar_OnLoaded (object sender, RoutedEventArgs e)
        {
            var toolBar = sender as ToolBar;
            var overflowGrid = toolBar.Template.FindName ("OverflowGrid", toolBar) as FrameworkElement;
            if (overflowGrid != null)
            {
                overflowGrid.Visibility = Visibility.Collapsed;
            }
        }
    }
}
