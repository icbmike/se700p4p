using System;
using System.ComponentModel;
using ATTrafficAnalayzer.Annotations;

namespace ATTrafficAnalayzer.Models.Settings
{
    public class DateSettings : INotifyPropertyChanged
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

        protected bool Equals(DateSettings other)
        {
            return _startDate.Equals(other._startDate) && _endDate.Equals(other._endDate) && Interval == other.Interval;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DateSettings) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _startDate.GetHashCode();
                hashCode = (hashCode*397) ^ _endDate.GetHashCode();
                hashCode = (hashCode*397) ^ Interval;
                return hashCode;
            }
        }
    }
}
