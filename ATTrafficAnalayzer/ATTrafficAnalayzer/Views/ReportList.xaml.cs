using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ATTrafficAnalayzer.VolumeModel;

namespace ATTrafficAnalayzer.Views
{
    /// <summary>
    /// Interaction logic for ReportList.xaml
    /// </summary>
    public partial class ReportList : UserControl
    {
        private VolumeDbHelper _volumeDbHelper;
        public ReportList()
        {
            _volumeDbHelper =  VolumeDbHelper.GetDbHelper();
            InitializeComponent();
        }

        private void newBtn_Click(object sender, RoutedEventArgs e)
        {
            //ChangeScreen(new ReportConfigurationScreen());
            var reportConfigurationScreen = new ReportConfigurationScreen();
            standardReportsListBox.ItemsSource = _volumeDbHelper.GetConfigs();
            //ChangeScreen(reportConfigurationScreen);
        }

        private void renameBtn_Click(object sender, RoutedEventArgs e)
        {
            var item = standardReportsListBox.SelectedItem.ToString();
            Console.WriteLine("Rename: {0}", item);
        }

        private void deleteBtn_Click(object sender, RoutedEventArgs e)
        {
            //Get selection
            var selectedRow = standardReportsListBox.SelectedItem as DataRowView;
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
                    _volumeDbHelper.RemoveConfig(selectedItem);

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

        private void editBtn_Click(object sender, RoutedEventArgs e)
        {
           // ChangeScreen(new ReportConfigurationScreen());
        }
    }
}
