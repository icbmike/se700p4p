﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Models.ReportConfiguration;
using ATTrafficAnalayzer.Models.Settings;
using ATTrafficAnalayzer.Modes;
using ATTrafficAnalayzer.Views.Screens;

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
            Configurables = new ObservableCollection<Configurable>();
            InitializeComponent();
            
            _dataSource = DataSourceFactory.GetDataSource();

        }

        public ObservableCollection<Configurable> Configurables { get; set; }

        #region New Configuration

        public event EventHandler NewConfigurationEvent;

        protected virtual void OnNewConfigwurationEvent()
        {
            var handler = NewConfigurationEvent;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        #endregion

        #region Export Configuration

        public delegate void ExportConfigurationEventHandler(object sender, ExportConfigurationEventHandlerArgs args);

        public event ExportConfigurationEventHandler ExportEvent;

        public class ExportConfigurationEventHandlerArgs
        {
            public ExportConfigurationEventHandlerArgs(string configToBeExported)
            {
                ConfigToBeExported = configToBeExported;
            }

            public string ConfigToBeExported { get; set; }
        }

        #endregion

        #region Selected Configuration


        public Configurable GetSelectedConfiguration()
        {
            return (ConfigurablesListView.SelectedValue as Configurable);
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
            string messageBoxText = "Are you sure you wish to delete " + selectedItem + "?";
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
                            };
                    backgroundWorker.RunWorkerAsync();


                    Logger.Debug(selectedItem.Name + " report deleted", "Reports panel");
                    break;

                case MessageBoxResult.Cancel:
                    Logger.Debug(selectedItem.Name + " report deletion was canceled", "Reports panel");
                    break;
            }
        }

        private void exportBtn_Click(object sender, RoutedEventArgs e)
        {
            ExportEvent(this, new ExportConfigurationEventHandlerArgs(GetSelectedConfiguration().Name));
        }

        #endregion

        private void ConfigurablesListViewOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 1)
            {
                //The regular case
                GetSelectedConfiguration().View();
            }
        }
    }
}