using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;
using MiniComm.Client.Models;
using MiniComm.Client.ViewModels;

namespace MiniComm.Client.Views
{
    /// <summary>
    /// UserInfo.xaml 的交互逻辑
    /// </summary>
    public partial class EditInfo : Window
    {
        public static EditInfo EditInfoWindow;

        public EditInfo(UserModel userModel)
        {
            InitializeComponent();
            this.DataContext = new EditInfoViewModel(userModel);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            EditInfoWindow = this;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            EditInfoWindow = null;
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