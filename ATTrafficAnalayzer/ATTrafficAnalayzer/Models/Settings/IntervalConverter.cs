﻿using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace ATTrafficAnalayzer.Models.Settings
{
    class IntervalConverter : MarkupExtension, IValueConverter 

    {

        private static IntervalConverter _converter;
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null)
            {
                _converter = new IntervalConverter();
            }
            return _converter;
        }


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value + " min";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return int.Parse((value as string).Split(new[]{' '})[0]);   
        }
    }
}
