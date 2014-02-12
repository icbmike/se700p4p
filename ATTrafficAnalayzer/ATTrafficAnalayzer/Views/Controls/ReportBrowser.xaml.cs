using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Models.Settings;
using ATTrafficAnalayzer.Modes;

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
            Configurables = new ObservableCollection<BaseConfigurable>();
            InitializeComponent();
            
            _dataSource = DataSourceFactory.GetDataSource();

        }

        public ObservableCollection<BaseConfigurable> Configurables { get; set; }

        #region New Configuration

        public event EventHandler NewConfigurationEvent;

        protected virtual void OnNewConfigwurationEvent()
        {
            var handler = NewConfigurationEvent;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        #endregion

        #region Selected Configuration


        public BaseConfigurable GetSelectedConfiguration()
        {
            return (ConfigurablesListView.SelectedValue as BaseConfigurable);
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
            OnNewConfigwurationEvent();
        }

        private void editBtn_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void removeBtn_Click(object sender, RoutedEventArgs e)
        {
            //Get selection
            var selectedItem = GetSelectedConfiguration();

            //Configure the message box to be displayed 
            string messageBoxText = "Are you sure you wish to delete " + selectedItem.Name + "?";
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
                    backgroundWorker.DoWork += (o, args) => selectedItem.Delete();
                    
                    ProgressBar.Visibility = Visibility.Visible;
                    backgroundWorker.RunWorkerCompleted +=
                        (o, args) =>
                            {
                                ProgressBar.Visibility = Visibility.Collapsed;
                                messageBoxText = selectedItem.Name + " was deleted";
                                caption = "Delete successful";
                                button = MessageBoxButton.OK;
                                icon = MessageBoxImage.Information;
                                MessageBox.Show(messageBoxText, caption, button, icon);
                                //Refresh the view
                                RaiseRefreshRequested();
                            };
                    backgroundWorker.RunWorkerAsync();


                    Logger.Debug(selectedItem.Name + " report deleted", "Reports panel");
                    break;

                case MessageBoxResult.Cancel:
                    Logger.Debug(selectedItem.Name + " report deletion was canceled", "Reports panel");
                    break;
            }
        }

        public event EventHandler RefreshRequested;

        protected virtual void RaiseRefreshRequested()
        {
            var handler = RefreshRequested;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private void exportBtn_Click(object sender, RoutedEventArgs e)
        {
            GetSelectedConfiguration().Export();
        }

        #endregion

        private void HandleDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var configurable = ((ListViewItem)sender).Content as BaseConfigurable;
            configurable.View();
        }
    }
}