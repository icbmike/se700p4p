using System.Collections.Generic;
using System.Windows;
using ATTrafficAnalayzer.Models;
using System.ComponentModel;
using System;
using ATTrafficAnalayzer.Views.Controls;

namespace ATTrafficAnalayzer.Views.Screens
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home
    {

        IDataSource _helper;

        #region events

        public delegate void ImportRequestEventHandler(object sender, RoutedEventArgs e);

        public event ImportRequestEventHandler ImportRequested;

        #endregion

        public Home()
        {
            InitializeComponent();
            _helper = DbHelper.GetDbHelper();
            Render();
            Logger.Info("constructed view", "homescreen");
        }

        private void Render()
        {
            var bw = new BackgroundWorker();
            bw.DoWork += DoWorkHandler;
            bw.RunWorkerCompleted += WorkerCompletedHandler;
            bw.RunWorkerAsync();
        }

        private void WorkerCompletedHandler(object sender, RunWorkerCompletedEventArgs e)
        {
            ImportedDatesList.ItemsSource = e.Result as List<DateTime>;
            ProgressBar.Visibility = Visibility.Collapsed;
            ImportedDatesList.Visibility = Visibility.Visible;
        }

        private void DoWorkHandler(object sender, DoWorkEventArgs e)
        {
            var importedDates = _helper.GetImportedDates();
            e.Result = importedDates;
        }

        private void ImportButtonClick(object sender, RoutedEventArgs e)
        {
            ImportRequested(this, e);
        }

        internal void ImportCompletedHandler(object sender)
        {
            var bw = new BackgroundWorker();
            ProgressBar.Visibility = Visibility.Visible;
            ImportedDatesList.Visibility = Visibility.Collapsed;

            bw.DoWork += DoWorkHandler;
            bw.RunWorkerCompleted += WorkerCompletedHandler;
            bw.RunWorkerAsync();
        }

        public event VolumeAndDateCountsDontMatchHandler VolumeDateCountsDontMatch;

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (DateTime date in ImportedDatesList.SelectedItems)
                _helper.RemoveVolumes(date);
            Render(); 
        }
    }
}
