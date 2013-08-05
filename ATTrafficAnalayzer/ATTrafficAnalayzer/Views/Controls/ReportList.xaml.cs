using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using ATTrafficAnalayzer.Models.Configuration;
using ATTrafficAnalayzer.Views.Screens;

namespace ATTrafficAnalayzer.Views.Controls
{
    /// <summary>
    /// Interaction logic for ReportList.xaml
    /// </summary>
    public partial class ReportList : UserControl
    {
        private readonly ReportsDataTableHelper _reportsDataTableHelper = ReportsDataTableHelper.GetDataTableHelper();

        public ReportList()
        {
            InitializeComponent();            
            DataContext = this;

            var dv = _reportsDataTableHelper.GetConfigDataView();
            StandardReportsTreeView.ItemsSource = dv;
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
                    _reportsDataTableHelper.RemoveConfig(selectedItem);

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

        public void ConfigurationSavedEventHandler(object sender, ReportConfigurationScreen.ConfigurationSavedEventArgs args)
        {
            
            StandardReportsTreeView.ItemsSource = _reportsDataTableHelper.GetConfigDataView();
            StandardReportsTreeView.DisplayMemberPath = "name";
        }

        private void exportBtn_Click(object sender, RoutedEventArgs e)
        {
            ExportEvent(this, new EditConfigurationEventHandlerArgs(GetSelectedConfiguration()));
        }
    }
}
