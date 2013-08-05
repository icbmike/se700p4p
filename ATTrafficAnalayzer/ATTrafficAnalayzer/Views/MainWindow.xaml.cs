using System;
using System.Windows;
using System.Windows.Controls;
using ATTrafficAnalayzer.Models;
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

            DataContext = this;

            InitializeComponent();
            var welcomeScreen = new WelcomeScreen(fileImportMenuItem_Click);
            ImportCompleted += welcomeScreen.ImportCompletedHandler;
            ChangeScreen(welcomeScreen);
        }
                                                             
        public delegate void ImportCompletedHandler(object sender);
        public event ImportCompletedHandler ImportCompleted;

        private void BulkImport()
        {
            var messageBoxText = "There is currently no volume data in the database. Would you like to import this data now?";
            const string caption = "Import Volume Store files";
            const MessageBoxButton button = MessageBoxButton.YesNo;
            const MessageBoxImage icon = MessageBoxImage.Question;

            while (true)
            {
                var result = MessageBox.Show(messageBoxText, caption, button, icon);
                messageBoxText = "Would you like to import another file?";

                if (result.Equals(MessageBoxResult.Yes))
                {
                    ImportFile();
                }
                else
                {
                    break;
                }

            }
        }

        private void fileImportMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ImportFile();
        }

        private void ImportFile()
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
            var result = dlg.ShowDialog();


            // Process open file dialog box results 
            if (result == true)
            {
                var settings = new ProgressDialogSettings(true, false, false);

                ProgressDialog.Execute(this, "Importing VS File", (b, w) =>
                {

                    // Open document 
                    var filename = dlg.FileName;
                    DbHelper.ImportFile(b, w, filename, progress => ProgressDialog.ReportWithCancellationCheck(b, w, progress, "Reading File"));

                    b.RunWorkerCompleted += (sender, args) => { if (ImportCompleted != null) ImportCompleted(this); };

                }, settings);
            }
        }

        private void ChangeScreen(UserControl screen)
        {
            MainContentControl.Content = screen;
        }

        private void SwitchScreen(object sender, Toolbar.ScreenChangeEventHandlerArgs args)
        {
            if (DbHelper.GetDbHelper().VolumesExistForDateRange(SettingsToolbar.StartDate, SettingsToolbar.EndDate))
            {

                if (args.Button.Equals(Toolbar.ScreenChangeEventHandlerArgs.ScreenButton.Faults))
                {
                    var faultsScreen = new VsFaultsReport(SettingsToolbar.SettingsTray);
                    SettingsToolbar.DateRangeChanged += faultsScreen.DateRangeChangedHandler;
                    ChangeScreen(faultsScreen);

                }
                else
                {
                    //Get selected Configuration
                    var selectedItem = ReportList.GetSelectedConfiguration();
                    if (selectedItem != null)
                    {
                        if (args.Button.Equals(Toolbar.ScreenChangeEventHandlerArgs.ScreenButton.Graph))
                        {
                            var graphScreen = new VsGraph(SettingsToolbar.SettingsTray, selectedItem);
                            SettingsToolbar.DateRangeChanged += graphScreen.DateRangeChangedHandler;
                            ChangeScreen(graphScreen);
                        }
                        else if (args.Button.Equals(Toolbar.ScreenChangeEventHandlerArgs.ScreenButton.Table))
                        {
                            var tableScreen = new VsTable(SettingsToolbar.SettingsTray, selectedItem);
                            SettingsToolbar.DateRangeChanged += tableScreen.DateRangeChangedHandler;
                            ChangeScreen(tableScreen);
                        }
                        else if (args.Button.Equals(Toolbar.ScreenChangeEventHandlerArgs.ScreenButton.Home))
                        {
                            ChangeScreen(new WelcomeScreen(fileImportMenuItem_Click));
                        }
                    }
                    else
                    {
                        MessageBox.Show("Select a report from the list on the left");
                    }
                }


            }
            else
            {
                MessageBox.Show("You haven't imported volume data for the selected date range");
            }
        }

        private void FileExitMenuItem_OnClick(object sender, RoutedEventArgs e)
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

        private void ReportList_OnEditConfigurationEvent(object sender, ReportList.EditConfigurationEventHandlerArgs args)
        {

            if (args.New)
            {

                if (DbHelper.GetDbHelper().VolumesExistForDateRange(SettingsToolbar.StartDate, SettingsToolbar.EndDate))
                {
                    var reportConfigurationScreen = new ReportConfigurationScreen();
                    reportConfigurationScreen.ConfigurationSaved += ReportList.ConfigurationSavedEventHandler;
                    ImportCompleted += reportConfigurationScreen.ImportCompletedHandler;
                    ChangeScreen(reportConfigurationScreen);
                }
                else
                {
                    MessageBox.Show("You haven't imported volume data for the selected date range");
                }
            }
            else
            {
                throw new NotImplementedException();
            }

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (DbHelper.VolumesTableEmpty())
                BulkImport();
        }

        private void ReportList_OnExportEvent(object sender, ReportList.EditConfigurationEventHandlerArgs args)
        {

            var dlg = new SaveFileDialog
            {
                FileName = "",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                DefaultExt = ".csv",
                Filter = "CSV Files (.csv)|*.csv"
            };

            if (dlg.ShowDialog() == true)
            {
                var csvExporter = new CSVExporter(dlg.FileName, SettingsToolbar.SettingsTray, args.ConfigToBeEdited);
                csvExporter.DoExport();
            }

        }

    }
}
