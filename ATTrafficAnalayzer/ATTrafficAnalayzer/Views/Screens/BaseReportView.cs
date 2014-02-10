using System;
using System.Windows.Controls;
using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Models.ReportConfiguration;
using ATTrafficAnalayzer.Models.Settings;
using ATTrafficAnalayzer.Views.Controls;

namespace ATTrafficAnalayzer.Views.Screens
{
    /// <summary>
    /// Interface that lets implementors respond to changes in the ReportBrowser and Toolbar components. 
    /// Also lets other componenets know when volume and date counts don't match.
    /// </summary>
    public abstract class BaseReportView : UserControl
    {
        protected DateSettings DateSettings;

        public virtual void DateSettingsChanged()
        {
            Render();
        }
        
        public event EventHandler VolumeDateCountsDontMatch;

        protected virtual void OnVolumeDateCountsDontMatch()
        {
            var handler = VolumeDateCountsDontMatch;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        protected readonly IDataSource DataSource;
        private Configuration _configuration;
        private int _interval;

        protected BaseReportView(DateSettings dateSettings, IDataSource dataSource)
        {
            DateSettings = dateSettings;
            DataSource = dataSource;
        }

        public Configuration Configuration
        {
            get { return _configuration; }
            set
            {
                _configuration = value;
                Render();
            }
        }

        protected abstract void Render();

        public int Interval
        {
            get { return _interval; }
            set
            {
                _interval = value;
                if (_configuration != null) Render();
            }
        }

    }
}
