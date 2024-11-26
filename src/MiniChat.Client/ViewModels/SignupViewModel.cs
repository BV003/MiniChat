using System;
using System.Windows;
using MiniChat.Client.Commands;
using MiniChat.Client.Views;
using MiniChat.Client.Helper;
using MiniSocket.Client;
using MiniChat.Transmitting;

namespace MiniChat.Client.ViewModels
{
    public class SignupViewModel : NotificationObject
    {
        private MiniClient miniClient;
        private bool signupCanExecute = true;

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

        private string _userPassword;
        public string UserPassword
        {
            get { return _userPassword; }
            set
            {
                _userPassword = value;
                base.OnPropertyChanged(nameof(UserPassword));
                if (_userPassword.Length < 6)
                {
                    MessageBox.Show("密码的长度不能小于6位！", Config.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private string _confirmPassword;
        public string ConfirmPassword
        {
            get { return _confirmPassword; }
            set
            {
                _confirmPassword = value;
                base.OnPropertyChanged(nameof(ConfirmPassword));
                if (!_confirmPassword.Equals(_userPassword))
                {
                    MessageBox.Show("两次输入的密码不相同！", Config.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private string _nickName;
        public string NickName
        {
            get { return _nickName; }
            set
            {
                _nickName = value;
                base.OnPropertyChanged(nameof(NickName));
            }
        }

        private string _age;
        public string Age
        {
            get { return _age; }
            set
            {
                if (int.TryParse(value.Trim(), out int result))
                {
                    _age = result.ToString();
                    if (result < 0 || result > 120)
                    {
                        MessageBox.Show("年龄不能小于0或大于120！", Config.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
                        _age = "0";
                    }
                    base.OnPropertyChanged(nameof(Age));
                    return;
                }
                Age = "0";
            }
        }

        private bool _gender;
        public bool Gender
        {
            get { return _gender; }
            set
            {
                _gender = value;
                base.OnPropertyChanged(nameof(Gender));
            }
        }

        public SignupViewModel()
        {
            SignupCommand = new DelegateCommand(UserSignup, UserSignupCanExecute);
            _gender = true;
        }

        /// <summary>
        /// 检查用户注册功能是否可用
        /// </summary>
        private bool UserSignupCanExecute() => signupCanExecute;

        /// <summary>
        /// 用户注册
        /// </summary>
        private async void UserSignup()
        {
            if (UserName != null && UserPassword != null && ConfirmPassword != null && NickName != null && Age != null)
            {
                string userName = UserName.Trim();
                string userPassword = UserPassword.Trim();
                string confirmPassword = ConfirmPassword.Trim();
                string nickName = NickName.Trim();
                int age = int.Parse(Age.Trim());
                string gender = Gender == true ? "男" : "女";
                if (!userName.Equals(string.Empty) && !userPassword.Equals(string.Empty) && !confirmPassword.Equals(string.Empty) && !nickName.Equals(string.Empty))
                {
                    if (userPassword.Length >= 6 && userPassword.Equals(confirmPassword))
                    {
                        signupCanExecute = false;
                        miniClient = new MiniClient(userName, Config.ClientAddressFamily, Config.ClientAgreement, Config.GetServerIPEndPoint());
                        miniClient.ClientRequestResult += MiniClient_ClientRequestResult;
                        miniClient.OpenClient();
                        if (await miniClient.ConnectionServerAsync())
                        {
                            User user = new User()
                            {
                                UserName = userName,
                                Password = ClientHelper.Encryption(userPassword),
                                NickName = nickName,
                                Gender = gender,
                                Age = age,
                                HeadIcon = ClientHelper.GetBytes(gender == "男" ? new Uri("Resources/Images/boy.png", UriKind.Relative) : new Uri("Resources/Images/girl.png", UriKind.Relative))
                            };
                            if (miniClient.SendDatabaseRequest(user, "Signup", null))
                            {
                                if (!await ClientHelper.WaitAsync(() => signupCanExecute, 600))
                                {
                                    signupCanExecute = true;
                                    miniClient?.CloseClient();
                                    miniClient = null;
                                    MessageBox.Show("服务器连接超时！", Config.Name, MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                                return;
                            }
                        }
                        signupCanExecute = true;
                        miniClient?.CloseClient();
                        miniClient = null;
                        MessageBox.Show("无法连接到服务器！", Config.Name, MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    MessageBox.Show("密码不符合要求！", Config.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            MessageBox.Show("用户信息不能为空！", Config.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void MiniClient_ClientRequestResult(object sender, RequestResultEventArgs e)
        {
            lock (ClientHelper.LockObject)
            {
                signupCanExecute = true;
                miniClient?.CloseClient();
                miniClient = null;
                switch (e.Result.Success)
                {
                    case true:
                        Signup.SignupWindow.Dispatcher.Invoke(() =>
                        {
                            Signup.SignupWindow.Close();
                            MessageBox.Show("注册成功，欢迎使用！", Config.Name, MessageBoxButton.OK, MessageBoxImage.Asterisk);
                        });
                        break;
                    case false:
                        MessageBox.Show("账户已经被注册！", Config.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
                        break;
                    default:
                        MessageBox.Show("无法连接到服务器！", Config.Name, MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                }
            }
        }
    }
}