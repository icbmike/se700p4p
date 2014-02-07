using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Models.Settings;
using ATTrafficAnalayzer.Modes;
using ATTrafficAnalayzer.Views.Controls;
using ATTrafficAnalayzer.Views.Controls.Parago.ProgressDialog;
using ATTrafficAnalayzer.Views.Screens;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Win32;

namespace ATTrafficAnalayzer.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private Mode _mode;
        private DefaultDupicatePolicy _defaultDupicatePolicy;
        private readonly IDataSource _dataSource;
        private DuplicatePolicy _skipAllOrOne;
        private int _amPeakIndex;
        private int _pmPeakIndex;

        /// <summary>
        /// Default constructor used by App
        /// </summary>
        public MainWindow()
        {
            Logger.Clear();
            _dataSource = DataSourceFactory.GetDataSource();
            DataContext = this;

            InitializeComponent();

            //Set the content screen to be a Home
            var homeScreen = CreateHomeScreen();
            ImportCompleted += homeScreen.ImportCompletedHandler;

            ChangeScreen(homeScreen);
            _mode = Mode.Report;
        }

        #region Screen Switching

        /// <summary>
        /// Chang the content screen to be the supplied UserControl
        /// </summary>
        /// <param configName="screen"></param>
        private void ChangeScreen(UserControl screen)
        {
            ContentScreen.Content = screen;
        }

        private ReportConfig CreateReportConfigurationScreen(String configToBeEdited)
        {
            ReportConfig reportConfigurationScreen;
            if (configToBeEdited == null)
            {
                //Create new configuartion screen if we switched from a different mode
                reportConfigurationScreen = new ReportConfig(_dataSource)
                {
                    Popup = {Visibility = Visibility.Visible},
                    Visibility = Visibility.Visible
                };
            }
            else
            {
                reportConfigurationScreen = new ReportConfig(configToBeEdited, _dataSource);
            }

            reportConfigurationScreen.ConfigurationSaved += ReportBrowser.ConfigurationSavedEventHandler;
            reportConfigurationScreen.ConfigurationSaved += IConfigScreen_ConfigurationSaved;
            return reportConfigurationScreen;
        }

        private Home CreateHomeScreen()
        {
            var homeScreen = new Home(_dataSource);
            homeScreen.ImportRequested += FileImportMenuItem_Click;
            return homeScreen;
        }

        /// <summary>
        /// Handler for when the report browser says that the user wants to create or edit a configuration.
        /// </summary>
        /// <param name="sender">Configuration Browser</param>
        /// <param name="args"></param>
        private void ReportBrowser_OnEditConfigurationEvent(object sender, ReportBrowser.EditConfigurationEventHandlerArgs args)
        {
            //Either a new Config or Summary
            if (_mode.Equals(Mode.Report))
            {
                //Check if volumes exist for the selected range
                if (_dataSource.VolumesExistForDateRange(SettingsToolbar.DateSettings.StartDate, SettingsToolbar.DateSettings.EndDate))
                {
                    //Create and display a new Configuration config screen
                    var reportConfigurationScreen = CreateReportConfigurationScreen(args.New ? null : args.ConfigToBeEdited);
                    ChangeScreen(reportConfigurationScreen);
                }
                else
                {
                    MessageBox.Show("You haven't imported volume data for the selected date range");
                }
            }
            else
            {
                //Check volumes exist for specified date range
                if (_dataSource.VolumesExistForMonth(SettingsToolbar.Month))
                {
                    var summaryConfigScreen = CreateSummaryConfigScreen(args.New ? null : args.ConfigToBeEdited);
                    ChangeScreen(summaryConfigScreen);
                }
                else
                {
                    MessageBox.Show("You haven't imported volume data for the selected month");
                }
            }
        }

        private SummaryConfig CreateSummaryConfigScreen(string s)
        {
            //Create and display a new summary config screen
            var summaryConfigScreen = new SummaryConfig(_dataSource);
            summaryConfigScreen.ConfigurationSaved += ReportBrowser.ConfigurationSavedEventHandler;
            summaryConfigScreen.ConfigurationSaved += IConfigScreen_ConfigurationSaved;
            return summaryConfigScreen;
        }

        /// <summary>
        /// Handler for when the selected report changes
        /// </summary>
        /// <param configName="sender"></param>
        /// <param configName="args"></param>
        /// <param name="sender"></param>
        public void ReportChangedHandler(object sender, ReportBrowser.SelectedReportChangeEventHandlerArgs args)
        {
            //Run same code as when a configuration has been saved
            if (args.SelectionCleared || args.ReportName == null) return;

            ShowReportScreen(args.ReportName);

            var view = (ContentScreen.Content as IView);
                if (view != null) view.SelectedReportChanged(args.ReportName);
        }

        /// <summary>
        /// Handler for when a configuration has been saved
        /// </summary>
        /// <param configName="sender"></param>
        /// <param configName="args"></param>
        void IConfigScreen_ConfigurationSaved(object sender, ConfigurationSavedEventArgs args)
        {
            ShowReportScreen(args.Name);
        }

        private void ShowReportScreen(string configName)
        {
            
            //Display the appropriate Table view
            if (_mode.Equals(Mode.Report))
            {
                var screen = new ReportTable(SettingsToolbar.DateSettings, configName, _dataSource);
                screen.VolumeDateCountsDontMatch += OnVolumeDateCountsDontMatch;
                ChangeScreen(screen);
            }
            else if (_mode.Equals(Mode.Summary))
            {
                var screen = new SummaryTable(SettingsToolbar.DateSettings, configName, _dataSource);
                ChangeScreen(screen);

            }
        }

        #endregion

        #region File Importing

        /// <summary>
        /// Delegate and event for when importing a file has completed
        /// </summary>
        /// <param configName="sender"></param>
        public delegate void ImportCompletedHandler(object sender);
        public event ImportCompletedHandler ImportCompleted;

        /// <summary>
        /// Event handler for when the window finished loading.
        /// Prompts the user to populate the volume source if it is empty
        /// </summary>
        /// <param configName="sender"></param>
        /// <param configName="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AddModes();

            if (_dataSource.VolumesExist())
                BulkImport();
        }

        private void AddModes()
        {
            var modes = new List<BaseMode>
            {
                new HomeMode(ModeChange, _dataSource)
            };

            SettingsToolbar.Modes.AddMany(modes.Select(mode => mode.ModeButton));
        }

        private void ModeChange(BaseMode mode)
        {
            ContentScreen = mode.GetView();
            var reportBrowserItems = mode.PopulateReportBrowser();
            if (reportBrowserItems == null)
            {
                ReportBrowser.Visibility = Visibility.Collapsed;
            }
            else
            {
                ReportBrowser.ItemsSource = reportBrowserItems;
                ReportBrowser.Visibility = Visibility.Visible;
            }
            SettingsToolbar.CustomizableToolBar.Items.Clear();
            mode.PopulateToolbar(SettingsToolbar.CustomizableToolBar);
        }

        /// <summary>
        /// Asks the user to repeatedly import files
        /// </summary>
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
                    //Actually import files
                    ImportFile();
                else
                    break;
            }
        }

        /// <summary>
        /// Method to select and import multiple volume files
        /// </summary>
        private void ImportFile()
        {
            // Configure open file dialog box 
            var dlg = new OpenFileDialog
                {
                    FileName = "",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    DefaultExt = ".VS",
                    Filter = "Volume Store Files (.VS)|*.VS",
                    Multiselect = true

                };

            // Show open file dialog box 
            var result = dlg.ShowDialog();

            // Process open file dialog box results 
            if (result == true)
            {
                //Use the progress dialog library.
                var settings = new ProgressDialogSettings(true, false, false);
                foreach (var filename in dlg.FileNames)
                {
                    //Default duplicate policy should maybe be a field
                    _skipAllOrOne = DuplicatePolicy.Skip;

                    //Execute the background task of importing a file
                    ProgressDialog.Execute(this, "Importing VS File: " + filename.Substring(filename.LastIndexOf("\\") + 1), (b, w) =>
                    {
                        // Open document 
                        _dataSource.ImportFile(filename,
                                            progress => ProgressDialog.ReportWithCancellationCheck(b, w, progress, "Reading File"), //Function to run on progress update
                                            GetDuplicatePolicy); //Function to determine a duplicate policy

                        //Worker completed handler, emit ImportCompleted event.
                        b.RunWorkerCompleted += (sender, args) => { if (ImportCompleted != null) ImportCompleted(this); };

                    }, settings);
                    if (_skipAllOrOne.Equals(DuplicatePolicy.SkipAll)) break;
                }
            }
        }

        /// <summary>
        /// Method to get a duplicate policy.
        /// Will return either the duplicate policy in the settings or ask the user on each duplicate conflict.
        /// </summary>
        /// <returns>The selected duplicate policy.</returns>
        private DuplicatePolicy GetDuplicatePolicy()
        {
            DuplicatePolicy policy;
            //If the setting is to ask...
            if (_defaultDupicatePolicy.Equals(DefaultDupicatePolicy.Ask))
            {
                var waitForInput = true;

                //Use a dispatcher to display a dialog to get a choice from the user, because were proably on a background thread.
                policy = DuplicatePolicy.Continue;
                Dispatcher.BeginInvoke(new Action(() =>
                    {
                        var dialog = new DuplicatePolicyDialog { Owner = this };
                        dialog.ShowDialog();
                        policy = dialog.SelectedPolicy;
                        waitForInput = false;
                    }), null);
                while (waitForInput)
                {
                }
            }
            //Else use the setting
            else if (_defaultDupicatePolicy.Equals(DefaultDupicatePolicy.Skip))
            {
                policy = DuplicatePolicy.Skip;
            }
            else //_defaultDupicatePolicy is DefaultDupicatePolicy.Continue 
            {
                policy = DuplicatePolicy.Continue;
            }
            _skipAllOrOne = policy;
            return policy;
        }

        #endregion

        #region File Exporting

        /// <summary>
        /// Handler for when the report browser asks to export a config
        /// Creates a dialog to ask the user for a filename and then uses a CSVExporter to do the deed.
        /// </summary>
        /// <param configName="sender"></param>
        /// <param configName="args"></param>
        private void ReportBrowser_OnExportEvent(object sender, ReportBrowser.EditConfigurationEventHandlerArgs args)
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
                var csvExporter = new CSVExporter(dlg.FileName, SettingsToolbar.DateSettings, args.ConfigToBeEdited, _amPeakIndex, _pmPeakIndex, _dataSource);

                if (_mode.Equals(Mode.Report))
                    csvExporter.ExportReport();
                else if (_mode.Equals(Mode.Summary))
                    csvExporter.ExportSummary();
            }
        }

        #endregion

        #region Menu Event Handlers

        /// <summary>
        /// Click handler for menu item
        /// </summary>
        /// <param configName="sender"></param>
        /// <param configName="e"></param>
        private void FileImportMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ImportFile();
        }

        /// <summary>
        /// Click handler for menu item
        /// </summary>
        /// <param configName="sender"></param>
        /// <param configName="e"></param>
        private void FileExitMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        /// <summary>
        /// Click handler for menu item
        /// </summary>
        /// <param configName="sender"></param>
        /// <param configName="e"></param>
        private void HelpAboutUsMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            // Configure the message box to be displayed 
            const string messageBoxText = "Auckland Transport Traffic Configuration Viewer\n\n" +
                                          "Created by Michael Little and Andrew Luey";
            const string caption = "About Us";
            const MessageBoxButton button = MessageBoxButton.OK;
            const MessageBoxImage icon = MessageBoxImage.None;

            MessageBox.Show(messageBoxText, caption, button, icon);
        }

        #endregion

        /// <summary>
        /// Event handler for when an IView syas that volume and date cpunts dont match
        /// </summary>
        /// <param configName="sender"></param>
        private void OnVolumeDateCountsDontMatch(IView sender)
        {
            MessageBox.Show("You don't have volume data imported for the range you specified");

            var homeScreen = CreateHomeScreen();
            ChangeScreen(homeScreen);
        }

        private void PreferencesMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var preferenceDialog = new DuplicatePolicyPreferenceDialog();
            preferenceDialog.ShowDialog();
            _defaultDupicatePolicy = preferenceDialog.DefaultDuplicatePolicy;
        }

        private void SettingsToolbar_OnDateRangeChanged(object sender, DateRangeChangedEventArgs args)
        {
            throw new NotImplementedException();
        }
    }
}
