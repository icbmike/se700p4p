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
            
            DataContext = this;

            InitializeComponent();
            startDatePicker.SelectedDate = new DateTime(2013, 3, 11);
            endDatePicker.SelectedDate = new DateTime(2013, 3, 12);

            ChangeScreen(new WelcomeScreen(fileImportMenuItem_Click));
        }
        
        private void BulkImport()
        {
            var messageBoxText = "There is currently no volume data in the database. Would you like to import this data now?";
            const string caption = "Import Volume Store files";
            const MessageBoxButton button = MessageBoxButton.YesNo;
            const MessageBoxImage icon = MessageBoxImage.Question;

            MessageBoxResult result;
            while (true)
            {
                result = MessageBox.Show(messageBoxText, caption, button, icon);
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
            Nullable<bool> result = dlg.ShowDialog();

            
            // Process open file dialog box results 
            if (result == true)
            {
                var settings = new ProgressDialogSettings(true, false, false);

                var res = ProgressDialog.Execute(this, "Importing VS File", (b, w) =>
                    {
                    
                    // Open document 
                    var filename = dlg.FileName;
                    DbHelper.ImportFile(b, w, filename, (progress) => {
                        ProgressDialog.ReportWithCancellationCheck(b, w, progress, "Reading File");
                    });
                  
                    }, settings);

               
            }
            
        }

        private void ChangeScreen(UserControl screen)
        {
           // if (screen.GetType() != MainContentControl.Content.GetType())
                MainContentControl.Content = screen;
        }

        private void SwitchScreen(object sender, RoutedEventArgs e)
        {
            var settings = SettingsTray.DataContext as SettingsTray;

            if (DbHelper.GetDbHelper().VolumesExistForDateRange(settings.StartDate, settings.EndDate))
            {
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
            else
            {
                MessageBox.Show("You haven't imported volume data for the selected date range");
            }
        }

        private void HomeImageMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ChangeScreen(new WelcomeScreen(fileImportMenuItem_Click));
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
                var settings = SettingsTray.DataContext as SettingsTray;
                if (DbHelper.GetDbHelper().VolumesExistForDateRange(settings.StartDate, settings.EndDate))
                {

                    var reportConfigurationScreen = new ReportConfigurationScreen(SettingsTray.DataContext as SettingsTray);
                    reportConfigurationScreen.ConfigurationSaved += ReportList.ConfigurationSavedEventHandler;
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
              //  BulkImport();
        }

        private void ReportList_OnExportEvent(object sender, ReportList.EditConfigurationEventHandlerArgs args)
        {
            var dlg = new SaveFileDialog()
            {
                FileName = "",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                DefaultExt = ".csv",
                Filter = "CSV Files (.csv)|*.csv"
            };

            if (dlg.ShowDialog() == true)
            {
                var csvExporter = new CSVExporter(dlg.FileName, SettingsTray.DataContext as SettingsTray, args.ConfigToBeEdited);
                csvExporter.DoExport();    
            }          
        }
    }
}
