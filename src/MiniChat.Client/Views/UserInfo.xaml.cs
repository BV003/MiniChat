using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using MiniComm.Client.Models;
using MiniComm.Client.ViewModels;

namespace MiniComm.Client.Views
{
    /// <summary>
    /// UserInfo.xaml 的交互逻辑
    /// </summary>
    public partial class UserInfo : Window
    {
        public static UserInfo UserInfoWindow;

        public UserInfo(UserModel userModel, Visibility visibility)
        {
            InitializeComponent();
            this.DataContext = new UserInfoViewModel(userModel, visibility);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UserInfoWindow?.Close();
            UserInfoWindow = this;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            UserInfoWindow = null;
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