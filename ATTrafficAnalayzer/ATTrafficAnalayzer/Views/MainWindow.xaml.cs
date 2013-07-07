using System;
using System.Windows;
using System.Windows.Controls;
using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Models.Settings;
using ATTrafficAnalayzer.Views.Controls;
using ATTrafficAnalayzer.Views.Controls.Parago.ProgressDialog;
using ATTrafficAnalayzer.Views.Screens;
using Microsoft.Win32;

namespace ATTrafficAnalayzer.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            Logger.Clear();

            InitializeComponent();
            DataContext = this;

            InitializeComponent();
            startDatePicker.SelectedDate = new DateTime(2013, 3, 11);
            endDatePicker.SelectedDate = new DateTime(2013, 3, 12);

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
                var res = ProgressDialog.Execute(this, "Importing VS File", (bw, we) =>
                {

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
            if (screen.GetType() != mainContentControl.Content.GetType())
                mainContentControl.Content = screen;
        }

        private void SwitchScreen(object sender, RoutedEventArgs e)
        {
            var settings = SettingsTray.DataContext as SettingsTray;
            //Get selected Configuration
            var selectedItem = ReportList.GetSelectedConfiguration();
            if (selectedItem != null)
            {
                if (sender.Equals(GraphButton))
                {
                    ChangeScreen(new VsGraph(settings, selectedItem));
                }
                else if (sender.Equals(TableButton))
                {
                    ChangeScreen(new VsTable(settings, selectedItem));
                }
            }
            else
            {
                MessageBox.Show("Select a report from the list on the left");
            }
        }



        private void HomeImageMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ChangeScreen(new WelcomeScreen());
        }

        private void FileQuitMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void HelpAboutUsMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            // Configure the message box to be displayed 
            const string messageBoxText = "Auckland Transport Traffic Report Viewer\n\n" +
                                          "Created by Michael Little and Andrew Luey";
            const string caption = "About Us";
            const MessageBoxButton button = MessageBoxButton.OK;
            const MessageBoxImage icon = MessageBoxImage.None;

            MessageBox.Show(messageBoxText, caption, button, icon);
        }

        private void MainToolbar_OnLoaded(object sender, RoutedEventArgs e)
        {
            var toolBar = sender as ToolBar;
            var overflowGrid = toolBar.Template.FindName("OverflowGrid", toolBar) as FrameworkElement;
            if (overflowGrid != null)
            {
                overflowGrid.Visibility = Visibility.Collapsed;
            }
        }

        private void ReportList_OnEditConfigurationEvent(object sender, ReportList.EditConfigurationEventHandlerArgs args)
        {
            if (args.New)
            {
                var reportConfigurationScreen = new ReportConfigurationScreen();
                reportConfigurationScreen.ConfigurationSaved += ReportList.ConfigurationSavedEventHandler;
                ChangeScreen(reportConfigurationScreen);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
