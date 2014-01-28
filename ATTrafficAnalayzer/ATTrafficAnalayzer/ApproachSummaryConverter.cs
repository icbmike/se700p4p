using System;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace ATTrafficAnalayzer
{
    public class ApproachSummaryConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("[b]" + values[0] + "[/b]");
            stringBuilder.AppendLine("AM Peak Volume: [b]" + values[1] + "[/b] at [b]" + ((DateTime)values[2]).ToShortTimeString() + "[/b]");
            stringBuilder.AppendLine("PM Peak Volume: [b]" + values[3] + "[/b] at [b]" + ((DateTime)values[4]).ToShortTimeString() + "[/b]");
            stringBuilder.Append("Total volume: [b]" + values[5] + "[/b]");

            return stringBuilder.ToString();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}