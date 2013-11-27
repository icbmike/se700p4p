using System;
using System.ComponentModel;
using ATTrafficAnalayzer.Annotations;

namespace ATTrafficAnalayzer.Models.Settings
{
    public class SettingsTray : INotifyPropertyChanged
    {
        private DateTime _startDate;
        private DateTime _endDate;

        /// <summary>
        ///     Date at the start of the period
        /// </summary>
        public DateTime StartDate
        {
            get { return _startDate; }
            set { _startDate = value; OnPropertyChanged("StartDate"); }
        }

        /// <summary>
        ///     Date at the end of the period
        /// </summary>
        public DateTime EndDate
        {
            get { return _endDate; }
            set { _endDate = value; OnPropertyChanged("EndDate"); }
        }

        /// <summary>
        ///     Time duration of each interval
        /// </summary>
        public int Interval { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Event handler for when a property changes
        /// </summary>
        /// <param name="propertyName">Property name</param>
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
