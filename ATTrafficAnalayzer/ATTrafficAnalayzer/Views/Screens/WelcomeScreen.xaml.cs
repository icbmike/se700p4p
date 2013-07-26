using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using ATTrafficAnalayzer.Models;

namespace ATTrafficAnalayzer.Views.Screens
{
    /// <summary>
    /// Interaction logic for WelcomeScreen.xaml
    /// </summary>
    public partial class WelcomeScreen : UserControl
    {

        #region events

        public delegate void ImportRequestEventHandler(object sender, RoutedEventArgs e);

        public event ImportRequestEventHandler ImportRequested;

        #endregion


        public WelcomeScreen(ImportRequestEventHandler handler)
        {
            InitializeComponent();
            ImportRequested += handler;
            DbHelper helper = DbHelper.GetDbHelper();

            var importedDates = helper.GetImportedDates();
            ImportedDatesList.ItemsSource = importedDates;


            Logger.Info("constructed view", "homescreen");
        }

        private void ImportButtonClick(object sender, RoutedEventArgs e)
        {
            ImportRequested(this, e);
        }

    }
}
