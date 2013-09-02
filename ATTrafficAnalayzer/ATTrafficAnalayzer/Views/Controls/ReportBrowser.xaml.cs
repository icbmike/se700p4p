using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using ATTrafficAnalayzer.Models.Configuration;
using ATTrafficAnalayzer.Models.Settings;
using ATTrafficAnalayzer.Views.Screens;

namespace ATTrafficAnalayzer.Views.Controls
{
    /// <summary>
    /// Interaction logic for ReportBrowser.xaml
    /// </summary>
    public partial class ReportBrowser
    {
        private readonly DataTableHelper _dataTableHelper = DataTableHelper.GetDataTableHelper();
        private Mode _mode;
        private bool _hasModeChanged;
        private bool _selectionCleared;

        public ReportBrowser()
        {
            InitializeComponent();
            DataContext = this;
            _mode = Mode.Report;

            Render();
        }

        private void Render()
        {
            StandardReportsTreeView.ItemsSource = _mode.Equals(Mode.Report) ? _dataTableHelper.GetReportDataView() : _dataTableHelper.GetSummaryDataView();
            StandardReportsTreeView.DisplayMemberPath = "name";
        }

        #region New/Edit Configuration

        public delegate void EditConfigurationEventHandler(object sender, EditConfigurationEventHandlerArgs args);

        public event EditConfigurationEventHandler EditConfigurationEvent;
        public class EditConfigurationEventHandlerArgs
        {
            public Boolean New { get; set; }
            public string ConfigToBeEdited { get; set; }

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
        }

        public void ConfigurationSavedEventHandler(object sender, ConfigurationSavedEventArgs args)
        {
            _dataTableHelper.SyncConfigs(_mode);
            
            var treeViewItem = StandardReportsTreeView.ItemContainerGenerator.ContainerFromIndex(StandardReportsTreeView.Items.Count - 1) as TreeViewItem;
            if (treeViewItem != null)
                treeViewItem.IsSelected = true;
            
            Render();
        }

        #endregion

        #region Export Configuration

        public delegate void ExportConfigurationEventHandler(object sender, ExportConfigurationEventHandlerArgs args);

        public event EditConfigurationEventHandler ExportEvent;
        public class ExportConfigurationEventHandlerArgs
        {
            public string ConfigToBeExported { get; set; }

            public ExportConfigurationEventHandlerArgs(string configToBeExported)
            {
                ConfigToBeExported = configToBeExported;
            }
        }

        #endregion

        #region Selected Configuration

        public delegate void SelectedReportChangeEventHandler(object sender, SelectedReportChangeEventHandlerArgs args);
        public event SelectedReportChangeEventHandler ReportChanged;
        public class SelectedReportChangeEventHandlerArgs
        {
            public string ReportName { get; set; }
            public bool SelectionCleared { get; set; }

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
        }

        public string GetSelectedConfiguration()
        {
            var selectedRow = StandardReportsTreeView.SelectedItem as DataRowView;
            return selectedRow == null ? null : selectedRow.Row["name"] as string;
        }

        private void StandardReportsTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (!_hasModeChanged)
                if (_selectionCleared)
                {
                    ReportChanged(this, new SelectedReportChangeEventHandlerArgs(_selectionCleared));
                    _selectionCleared = false;
                }
                else
                {
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
            var selectedRow = StandardReportsTreeView.SelectedItem as DataRowView;
            var selectedItem = selectedRow.Row["name"] as string;

            //Configure the message box to be displayed 
            var messageBoxText = "Are you sure you wish to delete " + selectedItem + "?";
            var caption = "Confirm delete";
            var button = MessageBoxButton.OKCancel;
            var icon = MessageBoxImage.Question;

            //Display message box
            var isConfirmedDeletion = MessageBox.Show(messageBoxText, caption, button, icon);

            //Process message box results 
            switch (isConfirmedDeletion)
            {
                case MessageBoxResult.OK:
                    _dataTableHelper.RemoveReport(selectedItem, _mode);

                    messageBoxText = selectedItem + " was deleted";
                    caption = "Delete successful";
                    button = MessageBoxButton.OK;
                    icon = MessageBoxImage.Information;
                    MessageBox.Show(messageBoxText, caption, button, icon);

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
            _mode = args.Mode;
            if (GetSelectedConfiguration() != null)
            {
                _hasModeChanged = true;    
            }
            
            Render();
        }

        #endregion
    }
}
