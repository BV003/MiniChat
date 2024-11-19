using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using MiniComm.Client.ViewModels;

namespace MiniComm.Client.Views
{
    /// <summary>
    /// Login.xaml 的交互逻辑
    /// </summary>
    public partial class Login : Window
    {
        public static Login LoginWindow;

        public Login()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoginWindow = this;
            this.DataContext = new LoginViewModel();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            LoginWindow = null;
            Signup.SignupWindow?.Close();
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
    }
}