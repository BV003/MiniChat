using System.Windows;

namespace MiniChat.Client.ViewModels
{
    /// <summary>
    /// 密码框绑定帮助类
    /// </summary>
    public static class PasswordBoxBindingHelper
    {
        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.RegisterAttached("Password", typeof(string), typeof(PasswordBoxBindingHelper), new PropertyMetadata(string.Empty));

        public static string GetPassword(DependencyObject dependencyObject) => dependencyObject.GetValue(PasswordProperty).ToString();
        public static void SetPassword(DependencyObject dependencyObject, string value) => dependencyObject.SetValue(PasswordProperty, value);
    }
}