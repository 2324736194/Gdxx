using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace ModbusDemo.Views
{
    class StringJoinConverter : IValueConverter
    {       
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IList list)
            {
                return string.Join(",", list.OfType<object>());
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}