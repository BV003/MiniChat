using System;
using System.Globalization;
using System.Windows.Data;

namespace MiniComm.Client.Converter
{
    /// <summary>
    /// 布尔值取反转换器
    /// </summary>
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => !(bool)value;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => !(bool)value;
    }
}