using System.Windows;

namespace MiniChat.Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Config.GetServerIPEndPoint();
        }
    }
}