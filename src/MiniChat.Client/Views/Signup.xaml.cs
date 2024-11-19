using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.ComponentModel;
using MiniComm.Client.ViewModels;

namespace MiniComm.Client.Views
{
    /// <summary>
    /// Signup.xaml 的交互逻辑
    /// </summary>
    public partial class Signup : Window
    {
        public static Signup SignupWindow;

        public Signup()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SignupWindow = this;
            this.DataContext = new SignupViewModel();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            SignupWindow = null;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void WindowMinimized(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void WindowClose(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox passwordBox = sender as PasswordBox;
            if (passwordBox != null)
            {
                PasswordBoxBindingHelper.SetPassword(passwordBox, passwordBox.Password);
            }
        }
    }
}