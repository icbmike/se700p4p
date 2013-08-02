using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using ATTrafficAnalayzer.Models;
using System.ComponentModel;
using System;

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
           
            var bw = new BackgroundWorker();

            bw.DoWork += bw_DoWork;
            bw.RunWorkerCompleted += bw_RunWorkerCompleted;
            bw.RunWorkerAsync();

            Logger.Info("constructed view", "homescreen");
        }

        void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ImportedDatesList.ItemsSource = e.Result as List<DateTime>;
            ProgressBar.Visibility = System.Windows.Visibility.Collapsed;
            ImportedDatesList.Visibility = System.Windows.Visibility.Visible;
        }

        void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            DbHelper helper = DbHelper.GetDbHelper();
            var importedDates = helper.GetImportedDates();
            e.Result = importedDates;
        }

        private void ImportButtonClick(object sender, RoutedEventArgs e)
        {
            ImportRequested(this, e);
        }

    }
}
