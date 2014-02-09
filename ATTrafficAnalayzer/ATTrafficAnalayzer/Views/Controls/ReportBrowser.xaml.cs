using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Models.ReportConfiguration;
using ATTrafficAnalayzer.Models.Settings;
using ATTrafficAnalayzer.Modes;
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
            DataContext = this;
            Configurables = new ObservableCollection<Configurable>();
            InitializeComponent();
            
            _dataSource = DataSourceFactory.GetDataSource();

        }

        public ObservableCollection<Configurable> Configurables { get; set; }

        #region New/Edit Configuration

        public delegate void EditConfigurationEventHandler(object sender, EditConfigurationEventHandlerArgs args);

        public event EditConfigurationEventHandler EditConfigurationEvent;

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


        public Configurable GetSelectedConfiguration()
        {
            return (ConfigurablesListView.SelectedValue as Configurable);
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
            EditConfigurationEvent(this, new EditConfigurationEventHandlerArgs(GetSelectedConfiguration().Name));
        }

        private void removeBtn_Click(object sender, RoutedEventArgs e)
        {
            //Get selection
            var selectedItem = ConfigurablesListView.SelectedItem as string;

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
            ExportEvent(this, new EditConfigurationEventHandlerArgs(GetSelectedConfiguration().Name));
        }

        #endregion

        private void ConfigurablesListViewOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 1)
            {
                //The regular case
                GetSelectedConfiguration().View();
            }
        }
    }
}