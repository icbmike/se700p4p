using System;
using System.Windows;
using System.Windows.Controls;
using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Models.Settings;
using ATTrafficAnalayzer.Views.Controls;
using ATTrafficAnalayzer.Views.Controls.Parago.ProgressDialog;
using ATTrafficAnalayzer.Views.Screens;
using Microsoft.Win32;
using System.Diagnostics;

namespace ATTrafficAnalayzer.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private Mode _mode;
        private DefaultDupicatePolicy _defaultDupicatePolicy;
        private int _amPeakIndex = 8;
        private int _pmPeakIndex = 4;
        private IDataSource dataSource;

        /// <summary>
        /// Default constructor used by App
        /// </summary>
        public MainWindow()
        {
            Logger.Clear();
            dataSource = DataSourceFactory.GetDataSource();
            DataContext = this;

            InitializeComponent();

            //Set the content screen to be a Home
            var homeScreen = new Home(dataSource);
            homeScreen.ImportRequested += FileImportMenuItem_Click;

            SettingsToolbar.ModeChanged += ReportBrowser.ModeChangedHandler;
            SettingsToolbar.ModeChanged += SettingsToolbar_OnModeChanged;
            ImportCompleted += homeScreen.ImportCompletedHandler;

            ChangeScreen(homeScreen);
            _mode = Mode.Report;
        }

        #region Screen Switching

        /// <summary>
        /// Removes event handlers from a screen, 
        /// determines what kind of screen and then calls that specific RemoveHandlers method
        /// </summary>
        /// <param name="screen"></param>
        private void RemoveHandlers(object screen)
        {
            if (screen as IConfigScreen != null)
                RemoveHandlers(screen as IConfigScreen);
            else if (screen as IView != null)
                RemoveHandlers(screen as IView);
        }

        /// <summary>
        /// Removes handlers from an IView
        /// </summary>
        /// <param name="iView"></param>
        private void RemoveHandlers(IView iView)
        {
            ReportBrowser.ReportChanged -= iView.ReportChangedHandler;
            SettingsToolbar.DateRangeChanged -= iView.DateRangeChangedHandler;
            iView.VolumeDateCountsDontMatch -= OnVolumeDateCountsDontMatch;
        }
        /// <summary>
        /// Removes handlers from an IConfigScreen
        /// </summary>
        /// <param name="iConfigScreen"></param>
        private void RemoveHandlers(IConfigScreen iConfigScreen)
        {
            iConfigScreen.ConfigurationSaved -= ReportBrowser.ConfigurationSavedEventHandler;
            iConfigScreen.ConfigurationSaved -= IConfigScreen_ConfigurationSaved;
        }

        /// <summary>
        /// Chang the content screen to be the supplied UserControl
        /// </summary>
        /// <param name="screen"></param>
        private void ChangeScreen(UserControl screen)
        {
            var oldScreen = ScreenContentControl.Content;

            ScreenContentControl.Content = screen;

            if (oldScreen != null)
                RemoveHandlers(oldScreen);
        }

        /// <summary>
        /// Mode changed handler
        /// </summary>
        /// <param name="sender">Toolbar</param>
        /// <param name="args"></param>
        private void SettingsToolbar_OnModeChanged(object sender, Toolbar.ModeChangedEventHandlerArgs args)
        {

            var prevMode = _mode;
            _mode = args.Mode;

            //Tell the Configuration browser to clear its selected item if the mode has changed
            if (!prevMode.Equals(_mode) && ReportBrowser.GetSelectedConfiguration() != null)
                ReportBrowser.ClearSelectedConfig();

            //Get the selected configuration
            var selectedConfiguration = ReportBrowser.GetSelectedConfiguration();

            //Detemine what mode we have changed to.
            switch (_mode)
            {
                case Mode.Home:
                    //Display the Home screen
                    ReportBrowser.Visibility = Visibility.Collapsed;
                    var homeScreen = new Home(dataSource);
                    homeScreen.ImportRequested += FileImportMenuItem_Click;
                    homeScreen.VolumeDateCountsDontMatch += OnVolumeDateCountsDontMatch;
                    ChangeScreen(homeScreen);
                    break;

                case Mode.Report:
                    //Display either a config screen, table or graph.
                    ReportBrowser.Visibility = Visibility.Visible;
                    if (ReportBrowser.GetSelectedConfiguration() == null)
                    {
                        //Create new configuartion screen if we switched from a different mode
                        var reportConfigurationScreen = new ReportConfig(dataSource);
                        reportConfigurationScreen.Popup.Visibility = Visibility.Visible;
                        reportConfigurationScreen.ConfigurationSaved += ReportBrowser.ConfigurationSavedEventHandler;
                        reportConfigurationScreen.ConfigurationSaved += IConfigScreen_ConfigurationSaved;
                        ReportBrowser.ReportChanged += ReportChangedHandler;
                        ImportCompleted += reportConfigurationScreen.ImportCompletedHandler;
                        reportConfigurationScreen.Visibility = Visibility.Visible;
                        ChangeScreen(reportConfigurationScreen);
                    }
                    //We havent changed mode, have only changed view
                    else if (args.View.Equals(Toolbar.View.Graph))
                    {
                        //Create and display a new Graph
                        var graphScreen = new ReportGraph(SettingsToolbar.SettingsTray, ReportBrowser.GetSelectedConfiguration(), dataSource);
                        SettingsToolbar.DateRangeChanged += graphScreen.DateRangeChangedHandler;
                        ReportBrowser.ReportChanged += graphScreen.ReportChangedHandler;
                        graphScreen.VolumeDateCountsDontMatch += OnVolumeDateCountsDontMatch;
                        ChangeScreen(graphScreen);
                    }
                    else if (args.View.Equals(Toolbar.View.Table))
                    {
                        //Create and display a new Table
                        var tableScreen = new ReportTable(SettingsToolbar.SettingsTray, ReportBrowser.GetSelectedConfiguration(), dataSource);
                        SettingsToolbar.DateRangeChanged += tableScreen.DateRangeChangedHandler;
                        ReportBrowser.ReportChanged += tableScreen.ReportChangedHandler;
                        tableScreen.VolumeDateCountsDontMatch += OnVolumeDateCountsDontMatch;
                        ChangeScreen(tableScreen);
                    }
                    break;

                case Mode.Summary:
                    ReportBrowser.Visibility = Visibility.Visible;
                    if (ReportBrowser.GetSelectedConfiguration() == null)
                    {
                        //If we are switching from a different mode, display a config screen
                        var summaryConfigScreen = new SummaryConfig(dataSource) { Popup = { Visibility = Visibility.Visible } };
                        summaryConfigScreen.ConfigurationSaved += ReportBrowser.ConfigurationSavedEventHandler;
                        summaryConfigScreen.ConfigurationSaved += IConfigScreen_ConfigurationSaved;
                        ReportBrowser.ReportChanged += ReportChangedHandler;
                        summaryConfigScreen.Popup.Visibility = Visibility.Visible;
                        ChangeScreen(summaryConfigScreen);
                    }
                    else
                    {
                        //Else display a new Table
                        var summaryScreen = new SummaryTable(SettingsToolbar.SettingsTray,
                        ReportBrowser.GetSelectedConfiguration(), dataSource);
                        SettingsToolbar.DateRangeChanged += summaryScreen.DateRangeChangedHandler;
                        ReportBrowser.ReportChanged += summaryScreen.ReportChangedHandler;
                        ChangeScreen(summaryScreen);
                    }
                    break;

                case Mode.Faults:
                    //Display suspected faults
                    ReportBrowser.Visibility = Visibility.Collapsed;
                    var faultsScreen = new Faults(SettingsToolbar.SettingsTray, dataSource);
                    SettingsToolbar.DateRangeChanged += faultsScreen.DateRangeChangedHandler;
                    faultsScreen.VolumeDateCountsDontMatch += OnVolumeDateCountsDontMatch;
                    ChangeScreen(faultsScreen);
                    break;
            }
        }

        /// <summary>
        /// Handler for when the report browser says that the user wants to create or edit a configuration.
        /// </summary>
        /// <param name="sender">Configuration Browser</param>
        /// <param name="args"></param>
        private void ReportBrowser_OnEditConfigurationEvent(object sender, ReportBrowser.EditConfigurationEventHandlerArgs args)
        {
            //Creating a new config
            if (args.New)
            {
                //Either a new Config or Summary
                if (_mode.Equals(Mode.Report))
                {
                    if (SettingsToolbar.StartDatePicker.SelectedDate != null && SettingsToolbar.EndDatePicker.SelectedDate != null)
                    {
                        //Check if volumes exist for the selected range
                        if (dataSource.VolumesExist((DateTime) SettingsToolbar.StartDatePicker.SelectedDate, (DateTime) SettingsToolbar.EndDatePicker.SelectedDate))
                        {
                            //Create and display a new Configuration config screen
                            var reportConfigurationScreen = new ReportConfig(dataSource);
                            reportConfigurationScreen.ConfigurationSaved += ReportBrowser.ConfigurationSavedEventHandler;
                            reportConfigurationScreen.ConfigurationSaved += IConfigScreen_ConfigurationSaved;
                            ImportCompleted += reportConfigurationScreen.ImportCompletedHandler;
                            ChangeScreen(reportConfigurationScreen);
                        }
                        else
                        {
                            MessageBox.Show("You haven't imported volume data for the selected date range");
                        }
                    }
                }
                else
                {
                    //Check volumes exist for specified date range
                    if (dataSource.VolumesExistForMonth(SettingsToolbar.Month))
                    {
                        //Create and display a new summary config screen
                        var summaryConfigScreen = new SummaryConfig(dataSource);
                        summaryConfigScreen.ConfigurationSaved += ReportBrowser.ConfigurationSavedEventHandler;
                        summaryConfigScreen.ConfigurationSaved += IConfigScreen_ConfigurationSaved;

                        //TODO Import completed event
                        ChangeScreen(summaryConfigScreen);
                    }
                    else
                    {
                        MessageBox.Show("You haven't imported volume data for the selected month");
                    }
                }
            }
            //Editing an existing config
            else
            {
                //Open relevant config screen
                if (_mode.Equals(Mode.Report))
                {
                    var reportConfigurationScreen = new ReportConfig(args.ConfigToBeEdited, dataSource);
                    reportConfigurationScreen.ConfigurationSaved += ReportBrowser.ConfigurationSavedEventHandler;
                    reportConfigurationScreen.ConfigurationSaved += IConfigScreen_ConfigurationSaved;
                    ImportCompleted += reportConfigurationScreen.ImportCompletedHandler;
                    ChangeScreen(reportConfigurationScreen);
                }else if (_mode.Equals(Mode.Summary))
                {
                    var summaryConfigScreen = new SummaryConfig(args.ConfigToBeEdited, dataSource);
                    summaryConfigScreen.ConfigurationSaved += ReportBrowser.ConfigurationSavedEventHandler;
                    summaryConfigScreen.ConfigurationSaved += IConfigScreen_ConfigurationSaved;

                    //TODO Import completed event
                    ChangeScreen(summaryConfigScreen);
                }
            }
        }

        /// <summary>
        /// Handler for when the selected report changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void ReportChangedHandler(object sender, ReportBrowser.SelectedReportChangeEventHandlerArgs args)
        {
            //Run same code as when a configuration has been saved
            if (!args.SelectionCleared && args.ReportName != null)
                IConfigScreen_ConfigurationSaved(this, new ConfigurationSavedEventArgs(args.ReportName));
        }

        /// <summary>
        /// Handler for when a configuration has been saved
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void IConfigScreen_ConfigurationSaved(object sender, ConfigurationSavedEventArgs args)
        {
            //Display the appropriate Table view
            if (_mode.Equals(Mode.Report))
            {
                var reportTableScreen = new ReportTable(SettingsToolbar.SettingsTray, args.Name, dataSource);
                SettingsToolbar.DateRangeChanged += reportTableScreen.DateRangeChangedHandler;
                ReportBrowser.ReportChanged += reportTableScreen.ReportChangedHandler;
                ChangeScreen(reportTableScreen);
                
            }
            else if (_mode.Equals(Mode.Summary))
            {
                var summaryTableScreen = new SummaryTable(SettingsToolbar.SettingsTray, args.Name, dataSource);
                SettingsToolbar.DateRangeChanged += summaryTableScreen.DateRangeChangedHandler;
                ReportBrowser.ReportChanged += summaryTableScreen.ReportChangedHandler;
                ChangeScreen(summaryTableScreen);
            }
        }

        #endregion

        #region File Importing

        /// <summary>
        /// Delegate and event for when importing a file has completed
        /// </summary>
        /// <param name="sender"></param>
        public delegate void ImportCompletedHandler(object sender);
        public event ImportCompletedHandler ImportCompleted;

        /// <summary>
        /// Event handler for when the window finished loading.
        /// Prompts the user to populate the volume source if it is empty
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (dataSource.VolumesTableEmpty())
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
                    //Default duplicate policy
                    var skipAllOrOne = DuplicatePolicy.Skip;

                    //Execture the background task of importing a file
                    ProgressDialog.Execute(this, "Importing VS File: " + filename.Substring(filename.LastIndexOf("\\") + 1), (b, w) =>
                    {
                        // Open document 
                        skipAllOrOne = dataSource.ImportFile(b, w, filename,
                                                           progress =>
                                                           ProgressDialog.ReportWithCancellationCheck(b, w, progress, "Reading File"),
                                                           GetDuplicatePolicy); //Function to determine a duplicate policy

                        //Worker completed handler, emit ImportCompleted event.
                        b.RunWorkerCompleted += (sender, args) => { if (ImportCompleted != null) ImportCompleted(this); };

                    }, settings);
                    if (skipAllOrOne.Equals(DuplicatePolicy.SkipAll)) break;
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

            return policy;
        }

        #endregion

        #region File Exporting

        /// <summary>
        /// DateRangeChanged handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void SettingsToolbar_DateRangeChanged(object sender, Toolbar.DateRangeChangedEventHandlerArgs args)
        {
            _amPeakIndex = args.AmPeakHour;
            _pmPeakIndex = args.PmPeakHour;
        }

        /// <summary>
        /// Handler for when the report browser asks to export a config
        /// Creates a dialog to ask the user for a filename and then uses a CSVExporter to do the deed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
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
                var csvExporter = new CSVExporter(dlg.FileName, SettingsToolbar.SettingsTray, args.ConfigToBeEdited, _amPeakIndex, _pmPeakIndex, dataSource);

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
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileImportMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ImportFile();
        }

        /// <summary>
        /// Click handler for menu item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileExitMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        /// <summary>
        /// Click handler for menu item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        /// <param name="sender"></param>
        private void OnVolumeDateCountsDontMatch(IView sender)
        {
            MessageBox.Show("You don't have volume data imported for the range you specified");

            var homeScreen = new Home(dataSource);
            homeScreen.ImportRequested += FileImportMenuItem_Click;
            ChangeScreen(homeScreen);
        }

        private void PreferencesMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var preferenceDialog = new DuplicatePolicyPreferenceDialog();
            preferenceDialog.ShowDialog();
            _defaultDupicatePolicy = preferenceDialog.DefaultDuplicatePolicy;
        }
    }
}
