using System.Collections.Generic;
using System.Windows;
using ATTrafficAnalayzer.Models;
using System.ComponentModel;
using System;

namespace ATTrafficAnalayzer.Views.Screens
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home
    {
        readonly IDataSource _dataSource;

        #region events

        public event EventHandler ImportRequested;

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public Home(IDataSource dataSource)
        {
            InitializeComponent();
            _dataSource = dataSource;
            Render();
            Logger.Info("constructed view", "homescreen");
        }

        /// <summary>
        /// Uses a background worker to fetch dates for imported data
        /// </summary>
        private void Render()
        {
            var bw = new BackgroundWorker();
            bw.DoWork += DoWorkHandler;
            bw.RunWorkerCompleted += WorkerCompletedHandler;
            bw.RunWorkerAsync();
        }

        /// <summary>
        /// Background wordker WorkerCompletedHandler, displays the fetched data in a listview
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WorkerCompletedHandler(object sender, RunWorkerCompletedEventArgs e)
        {
            ImportedDatesList.ItemsSource = e.Result as List<DateTime>;
            ProgressBar.Visibility = Visibility.Collapsed;
            ImportedDatesList.Visibility = Visibility.Visible;
        }
        
        /// <summary>
        /// Does long running call to get imported dates
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DoWorkHandler(object sender, DoWorkEventArgs e)
        {
            e.Result = _dataSource.GetImportedDates();
        }

        /// <summary>
        /// Click handler for import button, forwards the import request to listeners of this screen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImportButtonClick(object sender, RoutedEventArgs e)
        {
            if (ImportRequested != null) ImportRequested(this, e);
        }

        /// <summary>
        /// Event handler for when an import has completed. Refreshes imported dates
        /// </summary>
        /// <param name="sender"></param>
        internal void ImportCompletedHandler(object sender)
        {
            var bw = new BackgroundWorker();
            ProgressBar.Visibility = Visibility.Visible;
            ImportedDatesList.Visibility = Visibility.Collapsed;

            bw.DoWork += DoWorkHandler;
            bw.RunWorkerCompleted += WorkerCompletedHandler;
            bw.RunWorkerAsync();
        }

        /// <summary>
        /// Delete button click handler, does long running call and then rerenders list. TODO: do the deletion in BackgroundWorker
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (DateTime date in ImportedDatesList.SelectedItems)
                _dataSource.RemoveVolumes(date);
            Render(); 
        }
    }
}
