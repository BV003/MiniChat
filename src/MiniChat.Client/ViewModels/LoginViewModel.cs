using System.Windows;
using System.Windows.Controls;
using MiniChat.Client.Commands;
using MiniChat.Client.Services;
using MiniChat.Client.Views;
using MiniChat.Client.Helper;
using MiniSocket.Client;
using MiniChat.Transmitting;

namespace MiniChat.Client.ViewModels
{
    public class LoginViewModel : NotificationObject
    {
        private bool loginCanExecute = true;

        public DelegateCommand<PasswordBox> LoginCommand { get; }
        public DelegateCommand SignupCommand { get; }

        private string _userName;
        public string UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                base.OnPropertyChanged(nameof(UserName));
            }
        }

        public LoginViewModel()
        {
            LoginCommand = new DelegateCommand<PasswordBox>(UserLogin, UserLoginCanExecute);
            SignupCommand = new DelegateCommand(UserSignup);
        }

        /// <summary>
        /// 检查用户登录功能是否可用
        /// </summary>
        private bool UserLoginCanExecute(PasswordBox passwordBox) => loginCanExecute;

        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="passwordBox">用户密码框</param>
        private async void UserLogin(PasswordBox passwordBox)
        {
            if (UserName != null && passwordBox != null)
            {
                if (passwordBox.Password != null)
                {
                    string userName = UserName.Trim();
                    string userPassword = passwordBox.Password.Trim();
                    if (!userName.Equals(string.Empty) && !userPassword.Equals(string.Empty))
                    {
                        loginCanExecute = false;
                        Config.MiniClient = new MiniClient(userName, Config.ClientAddressFamily, Config.ClientAgreement, Config.GetServerIPEndPoint());
                        Config.MiniClient.ClientRequestResult += MiniClient_ClientRequestResult;
                        Config.MiniClient.OpenClient();
                        if (await Config.MiniClient.ConnectionServerAsync())
                        {
                            if (Config.MiniClient.SendDatabaseRequest(new User() { UserName = userName, Password = ClientHelper.Encryption(userPassword) }, "Login", null))
                            {
                                if (!await ClientHelper.WaitAsync(() => loginCanExecute, 20))
                                {
                                    loginCanExecute = true;
                                    Config.MiniClient?.CloseClient();
                                    Config.MiniClient = null;
                                    MessageBox.Show("服务器连接超时！", Config.Name, MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                                return;
                            }
                        }
                        loginCanExecute = true;
                        Config.MiniClient?.CloseClient();
                        Config.MiniClient = null;
                        MessageBox.Show("无法连接到服务器！", Config.Name, MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
            }
            MessageBox.Show("账户或密码不能为空！", Config.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        /// <summary>
        /// 用户注册
        /// </summary>
        private void UserSignup()
        {
            if (Signup.SignupWindow == null)
            {
                new Signup().Show();
            }
        }

        private void MiniClient_ClientRequestResult(object sender, RequestResultEventArgs e)
        {
            lock (ClientHelper.LockObject)
            {
                loginCanExecute = true;
                switch (e.Result.Success)
                {
                    case true:
                        Config.MiniClient.ClientRequestResult -= MiniClient_ClientRequestResult;
                        Login.LoginWindow.Dispatcher.Invoke(() =>
                        {
                            new Home(new DataService()).Show();
                            Login.LoginWindow.Close();
                        });
                        break;
                    case false:
                        Config.MiniClient?.CloseClient();
                        Config.MiniClient = null;
                        MessageBox.Show("账户或密码有误，请重新输入！", Config.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
                        break;
                    default:
                        Config.MiniClient?.CloseClient();
                        Config.MiniClient = null;
                        MessageBox.Show("无法连接到服务器！", Config.Name, MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                }
            }
        }
    }
}