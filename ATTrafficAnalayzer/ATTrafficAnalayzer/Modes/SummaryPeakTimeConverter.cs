using System;
using System.Globalization;
using System.Windows.Data;

namespace ATTrafficAnalayzer.Modes
{
    public class SummaryPeakTimeConverter : IValueConverter
    {
        //What the view sees
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value + " " + parameter;
        }

        //What the model sees
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return int.Parse(((string)value).Split(new[] { ' ' })[0]);
        }
    }
}