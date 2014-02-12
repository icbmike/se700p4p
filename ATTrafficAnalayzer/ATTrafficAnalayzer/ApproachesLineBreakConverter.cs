using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace ATTrafficAnalayzer
{
    public class ApproachesLineBreakConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var longString = value as string;
            return string.Join("\n", longString.Split(new[] {", "}, StringSplitOptions.None));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //Do nothing
            return null;
        }
    }
}