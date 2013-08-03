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
        private Mode _mode;

        public MainWindow()
        {
            Logger.Clear();

            DataContext = this;

            InitializeComponent();
            var homeScreen = new Home();
            homeScreen.ImportRequested += FileImportMenuItem_Click;

            SettingsToolbar.ModeChanged += ReportBrowser.ModeChangedHandler;
            SettingsToolbar.ModeChanged += SettingsToolbar_OnModeChanged;
            ImportCompleted += homeScreen.ImportCompletedHandler;

            ChangeScreen(homeScreen);
            _mode = Mode.Report;
        }

        #region Screen Switching

        private void RemoveHandlers(object screen)
        {
            if (screen as IConfigScreen != null)
                RemoveHandlers(screen as IConfigScreen);
            else if (screen as IView != null)
                RemoveHandlers(screen as IView);
            else
                MessageBox.Show("Somethings has gone horribly wrong");
        }
        private void RemoveHandlers(IView iView)
        {
            ReportBrowser.ReportChanged -= iView.ReportChangedHandler;
            SettingsToolbar.DateRangeChanged -= iView.DateRangeChangedHandler;
            iView.VolumeDateCountsDontMatch -= OnVolumeDateCountsDontMatch;
        }
        private void RemoveHandlers(IConfigScreen iConfigScreen)
        {
            iConfigScreen.ConfigurationSaved -= ReportBrowser.ConfigurationSavedEventHandler;
            iConfigScreen.ConfigurationSaved -= IConfigScreen_ConfigurationSaved;

        }

        private void ChangeScreen(UserControl screen)
        {
            if (ScreenContentControl.Content != null)
                RemoveHandlers(ScreenContentControl.Content);

            ScreenContentControl.Content = screen;
        }

        private void SettingsToolbar_OnModeChanged(object sender, Toolbar.ModeChangedEventHandlerArgs args)
        {
            _mode = args.Mode;

            switch (_mode)
            {
                case Mode.Home:
                    ReportBrowser.Visibility = Visibility.Collapsed;
                    var homeScreen = new Home();
                    homeScreen.ImportRequested += FileImportMenuItem_Click;
                    homeScreen.VolumeDateCountsDontMatch += OnVolumeDateCountsDontMatch;
                    ChangeScreen(homeScreen);
                    break;

                case Mode.Report:
                    ReportBrowser.Visibility = Visibility.Visible;
                    if (ReportBrowser.GetSelectedConfiguration() == null)
                    {
                        ChangeScreen(new ReportConfig());
                        MessageBox.Show("Construct your new report or select a report from the Report Browser");
                    }
                    else if (args.View.Equals(Toolbar.View.Graph))
                    {
                        var graphScreen = new ReportGraph(SettingsToolbar.SettingsTray, ReportBrowser.GetSelectedConfiguration());
                        SettingsToolbar.DateRangeChanged += graphScreen.DateRangeChangedHandler;
                        ReportBrowser.ReportChanged += graphScreen.ReportChangedHandler;
                        graphScreen.VolumeDateCountsDontMatch += OnVolumeDateCountsDontMatch;
                        ChangeScreen(graphScreen);
                    }
                    else if (args.View.Equals(Toolbar.View.Table))
                    {
                        var tableScreen = new ReportTable(SettingsToolbar.SettingsTray, ReportBrowser.GetSelectedConfiguration());
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
                        var summaryConfigScreen = new SummaryConfig();
                        summaryConfigScreen.ConfigurationSaved += IConfigScreen_ConfigurationSaved;
                        ChangeScreen(summaryConfigScreen);
                        MessageBox.Show("Construct your new report or select a report from the Report Browser");
                    }
                    else
                    {
                        var summaryScreen = new SummaryTable(SettingsToolbar.SettingsTray,
                            ReportBrowser.GetSelectedConfiguration());
                        SettingsToolbar.DateRangeChanged += summaryScreen.DateRangeChangedHandler;
                        ReportBrowser.ReportChanged += summaryScreen.ReportChangedHandler;
                        ChangeScreen(summaryScreen);
                    }
                    break;

                case Mode.Faults:
                    ReportBrowser.Visibility = Visibility.Collapsed;
                    var faultsScreen = new Faults(SettingsToolbar.SettingsTray);
                    SettingsToolbar.DateRangeChanged += faultsScreen.DateRangeChangedHandler;
                    faultsScreen.VolumeDateCountsDontMatch += OnVolumeDateCountsDontMatch;
                    ChangeScreen(faultsScreen);
                    break;
            }
        }

        #endregion

        #region File Importing

        public delegate void ImportCompletedHandler(object sender);
        public event ImportCompletedHandler ImportCompleted;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (DbHelper.VolumesTableEmpty())
                BulkImport();
        }

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
                    ImportFile();
                else
                    break;
            }
        }

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
                var settings = new ProgressDialogSettings(true, false, false);
                foreach (var filename in dlg.FileNames)
                {
                    var skipAllOrOne = DbHelper.DuplicatePolicy.Skip;
                    ProgressDialog.Execute(this, "Importing VS File: " + filename.Substring(filename.LastIndexOf("\\") + 1), (b, w) =>
                    {
                        // Open document 
                        skipAllOrOne = DbHelper.ImportFile(b, w, filename,
                                                           progress =>
                                                           ProgressDialog.ReportWithCancellationCheck(b, w, progress, "Reading File"),
                                                           () =>
                                                           {
                                                               var waitForInput = true;
                                                               var policy = DbHelper.DuplicatePolicy.Continue;
                                                               Dispatcher.BeginInvoke(new Action(() =>
                                                                   {
                                                                       var dialog = new DuplicatePolicyDialog { Owner = this };
                                                                       dialog.ShowDialog();
                                                                       policy = dialog.SelectedPolicy;
                                                                       waitForInput = false;
                                                                   }), null);
                                                               while (waitForInput) { }

                                                               return policy;
                                                           });


                        b.RunWorkerCompleted += (sender, args) => { if (ImportCompleted != null) ImportCompleted(this); };

                    }, settings);
                    if (skipAllOrOne.Equals(DbHelper.DuplicatePolicy.SkipAll)) break;
                }
            }
        }

        #endregion

        #region File Exporting

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
                var csvExporter = new CSVExporter(dlg.FileName, SettingsToolbar.SettingsTray, args.ConfigToBeEdited);
                csvExporter.DoExport();
            }
        }

        #endregion

        #region Menu Event Handlers

        private void FileImportMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ImportFile();
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
            const MessageBoxImage icon = MessageBoxImage.Information;

            MessageBox.Show(messageBoxText, caption, button, icon);
        }

        #endregion

        #region Other Event Handlers

        private void OnVolumeDateCountsDontMatch(IView sender)
        {
            MessageBox.Show("You don't have volume data imported for the range you specified");

            var homeScreen = new Home();
            homeScreen.ImportRequested += FileImportMenuItem_Click;
            ChangeScreen(homeScreen);
        }

        private void ReportBrowser_OnEditConfigurationEvent(object sender, ReportBrowser.EditConfigurationEventHandlerArgs args)
        {
            if (args.New)
            {
                if (_mode.Equals(Mode.Report))
                {
                    if (DbHelper.GetDbHelper().VolumesExist(SettingsToolbar.StartDate, SettingsToolbar.EndDate))
                    {
                        var reportConfigurationScreen = new ReportConfig();
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
                else
                {
                    if (DbHelper.GetDbHelper().VolumesExistForMonth(SettingsToolbar.Month))
                    {
                        var monthlySummary = new SummaryConfig();
                        monthlySummary.ConfigurationSaved += ReportBrowser.ConfigurationSavedEventHandler;
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
                var reportConfigurationScreen = new ReportConfig(args.ConfigToBeEdited);

                reportConfigurationScreen.ConfigurationSaved += ReportBrowser.ConfigurationSavedEventHandler;
                reportConfigurationScreen.ConfigurationSaved += IConfigScreen_ConfigurationSaved;

                ImportCompleted += reportConfigurationScreen.ImportCompletedHandler;
                ChangeScreen(reportConfigurationScreen);
            }
        }

        void IConfigScreen_ConfigurationSaved(object sender, ConfigurationSavedEventArgs args)
        {
            if (_mode.Equals(Mode.Report))
            {
                var reportTableScreen = new ReportTable(SettingsToolbar.SettingsTray, args.Name);
                SettingsToolbar.DateRangeChanged += reportTableScreen.DateRangeChangedHandler;
                ReportBrowser.ReportChanged += reportTableScreen.ReportChangedHandler;
                ChangeScreen(reportTableScreen);
            }
            else if (_mode.Equals(Mode.Summary))
            {
                var summaryTableScreen = new SummaryTable(SettingsToolbar.SettingsTray, args.Name);
                SettingsToolbar.DateRangeChanged += summaryTableScreen.DateRangeChangedHandler;
                ReportBrowser.ReportChanged += summaryTableScreen.ReportChangedHandler;
                ChangeScreen(summaryTableScreen);
            }
        }

        #endregion
    }
}
