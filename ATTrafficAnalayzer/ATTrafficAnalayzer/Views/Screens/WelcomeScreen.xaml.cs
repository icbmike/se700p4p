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

            bw.DoWork += DoWorkHandler;
            bw.RunWorkerCompleted += WorkerCompletedHandler;
            bw.RunWorkerAsync();

            Logger.Info("constructed view", "homescreen");
        }

        private void WorkerCompletedHandler(object sender, RunWorkerCompletedEventArgs e)
        {
            ImportedDatesList.ItemsSource = e.Result as List<DateTime>;
            ProgressBar.Visibility = System.Windows.Visibility.Collapsed;
            ImportedDatesList.Visibility = System.Windows.Visibility.Visible;
        }

        private void DoWorkHandler(object sender, DoWorkEventArgs e)
        {
            DbHelper helper = DbHelper.GetDbHelper();
            var importedDates = helper.GetImportedDates();
            e.Result = importedDates;
        }

        private void ImportButtonClick(object sender, RoutedEventArgs e)
        {
            ImportRequested(this, e);
        }


        internal void ImportCompletedHandler(object sender)
        {
            var bw = new BackgroundWorker();
            ProgressBar.Visibility = System.Windows.Visibility.Visible;
            ImportedDatesList.Visibility = System.Windows.Visibility.Collapsed;

            bw.DoWork += DoWorkHandler;
            bw.RunWorkerCompleted += WorkerCompletedHandler;
            bw.RunWorkerAsync();
        }
    }
}
