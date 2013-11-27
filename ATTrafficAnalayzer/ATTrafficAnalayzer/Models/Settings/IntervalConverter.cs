using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace ATTrafficAnalayzer.Models.Settings
{
    class IntervalConverter : MarkupExtension, IValueConverter 

    {
        private static IntervalConverter _converter;

        /// <summary>     
        ///     
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _converter ?? (_converter = new IntervalConverter());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //What the GUI sees
            return value + " min";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //What the model sees
            return int.Parse((value as string).Split(new[]{' '})[0]);   
        }
    }
}
