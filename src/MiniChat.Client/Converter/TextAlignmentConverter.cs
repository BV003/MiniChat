using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;

namespace MiniChat.Client.Converter
{
    public class TextAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string text)
            {
                // 这里是简单示例判断逻辑，比如如果文本以 "M" 开头就右对齐，你可以替换为实际需要的判断条件
                if (text.StartsWith("M"))
                {
                    return TextAlignment.Right;
                }
                return TextAlignment.Left;
            }
            return TextAlignment.Left;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
