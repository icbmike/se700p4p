using System;
using System.Collections.Generic;
using System.Windows;
using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Modes;
using ATTrafficAnalayzer.Views.Controls;
using ATTrafficAnalayzer.Views.Controls.Parago.ProgressDialog;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Win32;

namespace ATTrafficAnalayzer.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private DefaultDupicatePolicy _defaultDupicatePolicy;
        private readonly IDataSource _dataSource;
        private DuplicatePolicy _skipAllOrOne;
        private BaseMode _currentMode;
        private HomeMode _homeMode;

        /// <summary>
        /// Default constructor used by App
        /// </summary>
        public MainWindow()
        {
            Logger.Clear();
            _dataSource = DataSourceFactory.GetDataSource();
            DataContext = this;

            InitializeComponent();

        }

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
            if (_dataSource.VolumesExist())
                BulkImport();
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
        private void ReportBrowser_OnExportEvent(object sender, ReportBrowser.ExportConfigurationEventHandlerArgs args)
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
//                var csvExporter = new CSVExporter(dlg.FileName, SettingsToolbar.DateSettings, args.ConfigToBeEdited, _amPeakIndex, _pmPeakIndex, _dataSource);
//
//                if (_mode.Equals(Mode.Report))
//                    csvExporter.ExportReport();
//                else if (_mode.Equals(Mode.Summary))
//                    csvExporter.ExportSummary();
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

        private void PreferencesMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var preferenceDialog = new DuplicatePolicyPreferenceDialog();
            preferenceDialog.ShowDialog();
            _defaultDupicatePolicy = preferenceDialog.DefaultDuplicatePolicy;
        }

        private void SettingsToolbar_OnDateRangeChanged(object sender, DateRangeChangedEventArgs args)
        {
            _currentMode.DateRangeChangedEventHandler(sender, args);
        }

        private void SettingsToolbar_OnLoaded(object sender, RoutedEventArgs e)
        {
            CreateModes();
        }

        private void CreateModes()
        {
            //Construct the modes
            _homeMode = new HomeMode(ModeChange, _dataSource);
            var reportMode = new ReportMode(ModeChange, _dataSource, SettingsToolbar.DateSettings);
            var faultsMode = new FaultsMode(ModeChange, _dataSource, SettingsToolbar.DateSettings);
            var summaryMode = new SummaryMode(ModeChange, _dataSource, SettingsToolbar.DateSettings);
            var redLightRunningMode = new RedLightRunningMode(ModeChange, _dataSource, SettingsToolbar.DateSettings);

            //Add them to the toolbar
            var modes = new List<BaseMode>
            {
                _homeMode,
                reportMode,
                faultsMode,
                summaryMode,
                redLightRunningMode
            };
            SettingsToolbar.Modes.AddMany(modes);

            //Do specifics for each mode
            _currentMode = _homeMode; //Make homeMode the starting mode
            _homeMode.ImportRequested += (sender, args) => ImportFile();
            ContentScreen.Content = _homeMode.GetView();

            //Setup ReportMode
            reportMode.DateVolumeCountsDontMatch += OnDateVolumeCountsDontMatch;
            reportMode.ConfigurationSaved += OnConfigurationCreated;

            //Setup SummaryMode
            summaryMode.ConfigurationSaved += OnConfigurationCreated;

            //Setup Red Light Running Mode
            reportMode.DateVolumeCountsDontMatch += OnDateVolumeCountsDontMatch;
            redLightRunningMode.ConfigurationSaved += OnConfigurationCreated;
        }

        private void OnConfigurationCreated(object sender, ConfigurationSavedEventArgs eventArgs)
        {
            ReportBrowser.Configurables.Clear();
            ReportBrowser.Configurables.AddMany(eventArgs.Mode.PopulateReportBrowser());
        }

        private void OnDateVolumeCountsDontMatch(object sender, EventArgs eventArgs)
        {
            ModeChange(_homeMode);
        }

        private void ModeChange(BaseMode mode)
        {
            _currentMode = mode;
            ContentScreen.Content = mode.GetView();
            var reportBrowserItems = mode.PopulateReportBrowser();
            if (reportBrowserItems == null)
            {
                ReportBrowser.Visibility = Visibility.Collapsed;
            }
            else
            {
                ReportBrowser.Configurables.Clear();
                ReportBrowser.Configurables.AddMany(reportBrowserItems);
                ReportBrowser.Visibility = Visibility.Visible;
            }
            SettingsToolbar.CustomizableToolBar.Items.Clear();
            mode.PopulateToolbar(SettingsToolbar.CustomizableToolBar);
        }

        private void ReportBrowserOnNewConfigurationEvent(object sender, EventArgs e)
        {
            _currentMode.ShowConfigurationView();
        }
    }
}
