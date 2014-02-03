using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Models.ReportConfiguration;
using ATTrafficAnalayzer.Models.Settings;
using ATTrafficAnalayzer.Views.Screens;

namespace ATTrafficAnalayzer.Views.Controls
{
    /// <summary>
    ///     Interaction logic for ReportBrowser.xaml
    /// </summary>
    public partial class ReportBrowser
    {
        private readonly IDataSource _dataSource;
        private bool _hasModeChanged;
        private Mode _mode;
        private bool _selectionCleared;
        
        public ReportBrowser()
        {
            InitializeComponent();
            DataContext = this;
            _mode = Mode.Report;
            _dataSource = DataSourceFactory.GetDataSource();
            Render();
        }

        private void Render()
        {
            StandardReportsTreeView.ItemsSource = _mode.Equals(Mode.Report)
                                                      ? _dataSource.GetConfigurationNames()
                                                      : _dataSource.GetSummaryNames();
        }

        #region New/Edit Configuration

        public delegate void EditConfigurationEventHandler(object sender, EditConfigurationEventHandlerArgs args);

        public event EditConfigurationEventHandler EditConfigurationEvent;

        public void ConfigurationSavedEventHandler(object sender, ConfigurationSavedEventArgs args)
        {
            var treeViewItem =
                StandardReportsTreeView.ItemContainerGenerator.ContainerFromIndex(StandardReportsTreeView.Items.Count -
                                                                                  1) as TreeViewItem;
            if (treeViewItem != null)
                treeViewItem.IsSelected = true;

            Render();
        }

        public class EditConfigurationEventHandlerArgs
        {
            public EditConfigurationEventHandlerArgs()
            {
                New = true;
                ConfigToBeEdited = null;
            }

            public EditConfigurationEventHandlerArgs(string configToBeEdited)
            {
                New = false;
                ConfigToBeEdited = configToBeEdited;
            }

            public Boolean New { get; set; }
            public string ConfigToBeEdited { get; set; }
        }

        #endregion

        #region Export Configuration

        public delegate void ExportConfigurationEventHandler(object sender, ExportConfigurationEventHandlerArgs args);

        public event EditConfigurationEventHandler ExportEvent;

        public class ExportConfigurationEventHandlerArgs
        {
            public ExportConfigurationEventHandlerArgs(string configToBeExported)
            {
                ConfigToBeExported = configToBeExported;
            }

            public string ConfigToBeExported { get; set; }
        }

        #endregion

        #region Selected Configuration

        public delegate void SelectedReportChangeEventHandler(object sender, SelectedReportChangeEventHandlerArgs args);

        public event SelectedReportChangeEventHandler ReportChanged;

        public string GetSelectedConfiguration()
        {
            return StandardReportsTreeView.SelectedItem as string;
        }

        private void StandardReportsTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (!_hasModeChanged)
                if (_selectionCleared)
                {
                    if (ReportChanged != null)
                        ReportChanged(this, new SelectedReportChangeEventHandlerArgs(_selectionCleared));
                    _selectionCleared = false;
                }
                else
                {
                    if (ReportChanged != null)
                        ReportChanged(this, new SelectedReportChangeEventHandlerArgs(GetSelectedConfiguration()));
                }

            _hasModeChanged = false;
        }

        public void ClearSelectedConfig()
        {
            _selectionCleared = true;
            (StandardReportsTreeView.ItemContainerGenerator.ContainerFromItem(StandardReportsTreeView.SelectedItem) as
             TreeViewItem).IsSelected = false;
        }

        public class SelectedReportChangeEventHandlerArgs
        {
            public SelectedReportChangeEventHandlerArgs(string reportName)
            {
                ReportName = reportName;
                SelectionCleared = false;
            }

            public SelectedReportChangeEventHandlerArgs(bool selectionCleared)
            {
                SelectionCleared = selectionCleared;
                ReportName = null;
            }

            public string ReportName { get; set; }
            public bool SelectionCleared { get; set; }
        }

        #endregion

        #region Menu Event Handlers

        private void newBtn_Click(object sender, RoutedEventArgs e)
        {
            EditConfigurationEvent(this, new EditConfigurationEventHandlerArgs());
        }

        private void editBtn_Click(object sender, RoutedEventArgs e)
        {
            EditConfigurationEvent(this, new EditConfigurationEventHandlerArgs(GetSelectedConfiguration()));
        }

        private void removeBtn_Click(object sender, RoutedEventArgs e)
        {
            //Get selection
            var selectedItem = StandardReportsTreeView.SelectedItem as string;

            //Configure the message box to be displayed 
            string messageBoxText = "Are you sure you wish to delete " + selectedItem + "?";
            string caption = "Confirm delete";
            var button = MessageBoxButton.OKCancel;
            var icon = MessageBoxImage.Question;

            //Display message box
            MessageBoxResult isConfirmedDeletion = MessageBox.Show(messageBoxText, caption, button, icon);

            //Process message box results 
            switch (isConfirmedDeletion)
            {
                case MessageBoxResult.OK:
                    var backgroundWorker = new BackgroundWorker();
                    if (_mode.Equals(Mode.Report))
                    {
                        backgroundWorker.DoWork += (o, args) =>
                        {
                            _dataSource.RemoveConfiguration(selectedItem);
                        };

                    }
                    else
                    {
                        backgroundWorker.DoWork += (o, args) => _dataSource.RemoveSummary(selectedItem);
                    }
                    ProgressBar.Visibility = Visibility.Visible;
                    backgroundWorker.RunWorkerCompleted +=
                        (o, args) =>
                            {
                                ProgressBar.Visibility = Visibility.Collapsed;
                                messageBoxText = selectedItem + " was deleted";
                                caption = "Delete successful";
                                button = MessageBoxButton.OK;
                                icon = MessageBoxImage.Information;
                                MessageBox.Show(messageBoxText, caption, button, icon);
                                //Refresh the view
                                Render();
                            };
                    backgroundWorker.RunWorkerAsync();


                    Logger.Debug(selectedItem + " report deleted", "Reports panel");
                    break;

                case MessageBoxResult.Cancel:
                    Logger.Debug(selectedItem + " report deletion was canceled", "Reports panel");
                    break;
            }
        }

        private void exportBtn_Click(object sender, RoutedEventArgs e)
        {
            ExportEvent(this, new EditConfigurationEventHandlerArgs(GetSelectedConfiguration()));
        }

        #endregion

        #region Other Event Handlers

        public void ModeChangedHandler(object sender, Toolbar.ModeChangedEventHandlerArgs args)
        {
            if (GetSelectedConfiguration() != null)
            {
                _hasModeChanged = true;
            }

            if (_mode.Equals(args.Mode)) return;

            _mode = args.Mode;
            Render();
        }

        #endregion
    }
}