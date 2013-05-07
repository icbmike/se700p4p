using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows.Markup;

namespace ATTrafficAnalayzer
{
    class IntervalConverter : MarkupExtension, IValueConverter 


    {

        private static IntervalConverter _converter = null;
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null)
            {
                _converter = new IntervalConverter();
            }
            return _converter;
        }


        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value + " min";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return int.Parse((value as string).Split(new char[]{' '})[0]);   
        }
    }
}
