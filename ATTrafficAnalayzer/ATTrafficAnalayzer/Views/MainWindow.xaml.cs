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
        private Mode _selectedMode;

        public MainWindow()
        {
            Logger.Clear();

            DataContext = this;

            InitializeComponent();
            var homeScreen = new Home();
            homeScreen.ImportRequested += fileImportMenuItem_Click;

            SettingsToolbar.ModeChanged += ReportList.ModeChangedHandler;
            SettingsToolbar.ModeChanged += SettingsToolbarOnModeChanged;
            ImportCompleted += homeScreen.ImportCompletedHandler;

            ChangeScreen(homeScreen);
            _selectedMode = Mode.RegularReports;
        }

        private void SettingsToolbarOnModeChanged(object sender, Toolbar.ModeChangedEventHandlerArgs args)
        {
            _selectedMode = args.SelectedMode;

            if (_selectedMode.Equals(Mode.RegularReports) || _selectedMode.Equals((Mode.MonthlySummary)))
            {
                ReportList.Visibility = Visibility.Visible;
            }
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
            if (MainContentControl.Content != null ) RemoveHandlers(MainContentControl.Content);
            MainContentControl.Content = screen;
        }

        private void RemoveHandlers(object screen)
        {

            if (screen as IConfigScreen != null)
            {
                RemoveHandlers(screen as IConfigScreen);
            }else if (screen as IView != null)
            {
                RemoveHandlers(screen as IView);
            }
            else
            {
                MessageBox.Show("Somethings has gone horribly wrong");
            }
        }


        private void RemoveHandlers(IView iView)
        {
            ReportList.ReportChanged -= iView.ReportChangedHandler;
            SettingsToolbar.DateRangeChanged -= iView.DateRangeChangedHandler;
        }
        private void RemoveHandlers(IConfigScreen iConfigScreen)
        {
            iConfigScreen.ConfigurationSaved -= ReportList.ConfigurationSavedEventHandler;
            iConfigScreen.ConfigurationSaved -= reportConfigurationScreen_ConfigurationSaved;

        }

        private void SwitchScreen(object sender, Toolbar.ScreenChangeEventHandlerArgs args)
        {
            if (DbHelper.GetDbHelper().VolumesExistForDateRange(SettingsToolbar.StartDate, SettingsToolbar.EndDate))
            {

                if (args.Button.Equals(Toolbar.ScreenButton.Faults))
                {
                    var faultsScreen = new Faults(SettingsToolbar.SettingsTray);
                    SettingsToolbar.DateRangeChanged += faultsScreen.DateRangeChangedHandler;
                    ChangeScreen(faultsScreen);
                    ReportList.Visibility = Visibility.Collapsed;
                }
                else if (args.Button.Equals(Toolbar.ScreenButton.Home))
                {
                    var homeScreen = new Home();
                    homeScreen.ImportRequested += fileImportMenuItem_Click;
                    ChangeScreen(homeScreen);

                    ReportList.Visibility = Visibility.Collapsed;
                }
                else
                {
                    //Get selected Config
                    var selectedItem = ReportList.GetSelectedConfiguration();
                    if (selectedItem != null)
                    {
                        if (args.Button.Equals(Toolbar.ScreenButton.Graph))
                        {
                            var graphScreen = new Graph(SettingsToolbar.SettingsTray, selectedItem);
                            SettingsToolbar.DateRangeChanged += graphScreen.DateRangeChangedHandler;
                            ReportList.ReportChanged += graphScreen.ReportChangedHandler;
                            graphScreen.VolumeDateCountsDontMatch += OnVolumeDateCountsDontMatch;
                            ChangeScreen(graphScreen);
                        }
                        else if (args.Button.Equals(Toolbar.ScreenButton.Table))
                        {
                            if (_selectedMode.Equals(Mode.RegularReports))
                            {
                                var tableScreen = new Table(SettingsToolbar.SettingsTray, selectedItem);
                                SettingsToolbar.DateRangeChanged += tableScreen.DateRangeChangedHandler;
                                ReportList.ReportChanged += tableScreen.ReportChangedHandler;
                                tableScreen.VolumeDateCountsDontMatch += OnVolumeDateCountsDontMatch;
                                ChangeScreen(tableScreen);
                            }
                            else
                            {
                                var tableScreen = new Summary();
                                ChangeScreen(tableScreen);
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Select a report from the list on the left");
                    }
                }
            }
            else
                MessageBox.Show("You haven't imported volume data for the selected date range");
        }

        private void OnVolumeDateCountsDontMatch(IView sender)
        {
            MessageBox.Show("You don't have volume data imported for the range you specified");

            var homeScreen = new Home();
            homeScreen.ImportRequested += fileImportMenuItem_Click;
            ChangeScreen(homeScreen);

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

        private void ReportList_OnEditConfigurationEvent(object sender, ReportBrowser.EditConfigurationEventHandlerArgs args)
        {

            if (args.New)
            {
                if (_selectedMode.Equals(Mode.RegularReports))
                {
                    if (DbHelper.GetDbHelper()
                                .VolumesExistForDateRange(SettingsToolbar.StartDate, SettingsToolbar.EndDate))
                    {
                        var reportConfigurationScreen = new Config();
                        reportConfigurationScreen.ConfigurationSaved += ReportList.ConfigurationSavedEventHandler;
                        reportConfigurationScreen.ConfigurationSaved += reportConfigurationScreen_ConfigurationSaved;

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
                    if (DbHelper.GetDbHelper().VolumesExistForMonth(SettingsToolbar.Month))
                    {
                        var monthlySummary = new SummaryConfig();
                        monthlySummary.ConfigurationSaved += ReportList.ConfigurationSavedEventHandler;
                        ChangeScreen(monthlySummary);
                    }
                    else
                    {
                        MessageBox.Show("You haven't imported volume data for the selected month");
                    }

                }
            }
            else
            {
                //Open relevant config screen
                var reportConfigurationScreen = new Config(args.ConfigToBeEdited);

                reportConfigurationScreen.ConfigurationSaved += ReportList.ConfigurationSavedEventHandler;
                reportConfigurationScreen.ConfigurationSaved += reportConfigurationScreen_ConfigurationSaved;

                ImportCompleted += reportConfigurationScreen.ImportCompletedHandler;
                ChangeScreen(reportConfigurationScreen);
            }

        }

        void reportConfigurationScreen_ConfigurationSaved(object sender, ConfigurationSavedEventArgs args)
        {
            var tableScreen = new Table(SettingsToolbar.SettingsTray, args.Name);
            SettingsToolbar.DateRangeChanged += tableScreen.DateRangeChangedHandler;
            ChangeScreen(tableScreen);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (DbHelper.VolumesTableEmpty())
                BulkImport();
        }

        private void ReportList_OnExportEvent(object sender, ReportBrowser.EditConfigurationEventHandlerArgs args)
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
