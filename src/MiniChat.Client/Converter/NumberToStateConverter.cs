using System;
using System.Globalization;
using System.Windows.Data;

namespace MiniComm.Client.Converter
{
    /// <summary>
    /// 状态信息转换器
    /// </summary>
    public class NumberToStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int)
            {
                switch ((int)value)
                {
                    case 0:
                        return "离线";
                    case 1:
                        return "在线";
                }
            }
            return "离线";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string)
            {
                switch (value.ToString())
                {
                    case "离线":
                        return 0;
                    case "在线":
                        return 1;
                }
            }
            return 0;
        }
    }
}