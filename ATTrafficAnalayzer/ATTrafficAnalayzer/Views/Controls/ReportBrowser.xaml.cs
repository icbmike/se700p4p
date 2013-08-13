using System;
using System.Data;
using System.Windows;
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
        private readonly ReportsDataTableHelper _reportsDataTableHelper = ReportsDataTableHelper.GetDataTableHelper();
        private Mode _selectedMode;
        private bool _modeChanged;

        public ReportBrowser()
        {
            InitializeComponent();
            DataContext = this;
            _selectedMode = Mode.RegularReports;

            PopulateListView();
        }

        private void PopulateListView()
        {
            StandardReportsTreeView.ItemsSource = 
                _selectedMode.Equals(Mode.RegularReports)

                    ? _reportsDataTableHelper.GetRegularReportDataView()
                    : _reportsDataTableHelper.GetMonthlySummaryDataView();

            
            StandardReportsTreeView.DisplayMemberPath = "name";
        }

        #region events

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

        private void newBtn_Click(object sender, RoutedEventArgs e)
        {
            EditConfigurationEvent(this, new EditConfigurationEventHandlerArgs());
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
                    _reportsDataTableHelper.RemoveConfig(selectedItem, _selectedMode);

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

        public string GetSelectedConfiguration()
        {
            var selectedRow = StandardReportsTreeView.SelectedItem as DataRowView;
            return selectedRow == null ? null : selectedRow.Row["name"] as string;
        }

        private void editBtn_Click(object sender, RoutedEventArgs e)
        {
            EditConfigurationEvent(this, new EditConfigurationEventHandlerArgs(GetSelectedConfiguration()));
        }

        public void ConfigurationSavedEventHandler(object sender, ConfigurationSavedEventArgs args)
        {
            _reportsDataTableHelper.SyncConfigs();
            PopulateListView();
        }

        private void exportBtn_Click(object sender, RoutedEventArgs e)
        {
            ExportEvent(this, new EditConfigurationEventHandlerArgs(GetSelectedConfiguration()));
        }

        public delegate void SelectedReportChangeEventHandler(object sender, SelectedReporChangeEventHandlerArgs args);
        public event SelectedReportChangeEventHandler ReportChanged;

        public class SelectedReporChangeEventHandlerArgs
        {
            public string ReportName { get; set; }

            public SelectedReporChangeEventHandlerArgs(string reportName)
            {
                ReportName = reportName;
            }
        }

        private void StandardReportsTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (ReportChanged != null && _modeChanged)
            {
                ReportChanged(this, new SelectedReporChangeEventHandlerArgs(GetSelectedConfiguration()));
                _modeChanged = false;
            }
        }

        public void ModeChangedHandler(object sender, Toolbar.ModeChangedEventHandlerArgs args)
        {
            _selectedMode = args.SelectedMode;
            _modeChanged = true;
            PopulateListView();

        }
    }
}
