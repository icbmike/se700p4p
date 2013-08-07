using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Markup;

namespace ATTrafficAnalayzer.Views.Screens
{
    public class DetectorsListToStringConverter : MarkupExtension, IValueConverter
    {
        private static DetectorsListToStringConverter _converter;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _converter ?? (_converter = new DetectorsListToStringConverter());
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //What the GUI sees
            var list = value as List<int>;
            var sb = new StringBuilder();
            for (var i = 0; i < list.Count; i++)
            {
                sb.Append(list[i]);
                if (i < list.Count - 1)
                {
                    sb.Append(", ");
                }
            }

            return sb.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //What the model sees
            var str = value as String;
            return str.Split(new[] { ", " }, StringSplitOptions.None).Select(int.Parse).ToList();
        }

    }
}