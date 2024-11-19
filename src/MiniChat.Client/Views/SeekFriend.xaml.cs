using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using MiniComm.Client.Models;
using MiniComm.Client.ViewModels;

namespace MiniComm.Client.Views
{
    /// <summary>
    /// SeekFriend.xaml 的交互逻辑
    /// </summary>
    public partial class SeekFriend : Window
    {
        public static SeekFriend SeekFriendWindow;

        public SeekFriend()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SeekFriendWindow = this;
            this.DataContext = new SeekFriendViewModel();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            SeekFriendWindow = null;
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