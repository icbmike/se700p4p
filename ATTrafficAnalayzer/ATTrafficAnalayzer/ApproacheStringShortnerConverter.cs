using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ATTrafficAnalayzer
{
    public class ApproacheStringShortnerConverter : IValueConverter
    {
        //What the view sees
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var longString = value as string;
            if (longString.Length > 80)
            {
                longString = longString.Substring(0, 80) + "...";
            }
            return longString;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //Do nothing, its one way;
            return value;
        }
    }
}