using System;
using System.Windows;
using MiniChat.Client.Commands;
using MiniChat.Client.Models;
using MiniChat.Client.Views;
using MiniChat.Client.Helper;
using MiniChat.Transmitting;

namespace MiniChat.Client.ViewModels
{
    public class EditInfoViewModel : NotificationObject
    {
        private UserModel _userModel;

        public DelegateCommand SaveInfoCommand { get; }
        public DelegateCommand SavePasswordCommand { get; }

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

        private string _oldPassword;
        public string OldPassword
        {
            get { return _oldPassword; }
            set
            {
                _oldPassword = value;
                base.OnPropertyChanged(nameof(OldPassword));
            }
        }

        private string _newPassword;
        public string NewPassword
        {
            get { return _newPassword; }
            set
            {
                _newPassword = value;
                base.OnPropertyChanged(nameof(NewPassword));
                if (_newPassword.Length < 6)
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
                if (!_confirmPassword.Equals(_newPassword))
                {
                    MessageBox.Show("两次输入的密码不相同！", Config.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        public EditInfoViewModel(UserModel userModel)
        {
            _userModel = userModel;
            SaveInfoCommand = new DelegateCommand(SaveInfo);
            SavePasswordCommand = new DelegateCommand(SavePassword);
            NickName = userModel.NickName;
            Age = userModel.Age.ToString();
            Gender = userModel.Gender == "男" ? true : false;
        }

        /// <summary>
        /// 保存资料
        /// </summary>
        private void SaveInfo()
        {
            if (NickName != null && Age != null)
            {
                string nickName = NickName.Trim();
                int age = int.Parse(Age.Trim());
                string gender = Gender == true ? "男" : "女";
                if (!nickName.Equals(string.Empty))
                {
                    if (HomeViewModel.RequestResultAction == null)
                    {
                        User user = new User()
                        {
                            UserID = _userModel.UserID,
                            UserName = _userModel.UserName,
                            Password = _userModel.Password,
                            NickName = nickName,
                            Gender = gender,
                            Age = age,
                            HeadIcon = _userModel.HeadIcon,
                            State = _userModel.State
                        };

                        HomeViewModel.RequestResultAction = new Action<RequestResult>((RequestResult result) =>
                        {
                            if (result.Success == true)
                            {
                                EditInfo.EditInfoWindow.Dispatcher.Invoke(() =>
                                {
                                    (Home.HomeWindow?.DataContext as HomeViewModel)?.LoadUserCommand?.Execute(null);
                                    EditInfo.EditInfoWindow?.Close();
                                });
                                MessageBox.Show("资料保存成功！", Config.Name, MessageBoxButton.OK, MessageBoxImage.Asterisk);
                                return;
                            }
                            MessageBox.Show("资料保存失败！", Config.Name, MessageBoxButton.OK, MessageBoxImage.Error);
                        });

                        if (!Config.MiniClient.SendDatabaseRequest(user, "AlterInfo", null))
                        {
                            HomeViewModel.RequestResultAction = null;
                            MessageBox.Show("保存资料时发生错误！", Config.Name, MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    return;
                }
            }
            MessageBox.Show("资料不能为空！", Config.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        /// <summary>
        /// 保存密码
        /// </summary>
        private void SavePassword()
        {
            if (OldPassword != null && NewPassword != null && ConfirmPassword != null)
            {
                string oldPassword = ClientHelper.Encryption(OldPassword.Trim());
                string newPassword = ClientHelper.Encryption(NewPassword.Trim());
                string confirmPassword = ClientHelper.Encryption(ConfirmPassword.Trim());
                if (newPassword.Length >= 6 && newPassword.Equals(confirmPassword))
                {
                    if (HomeViewModel.RequestResultAction == null)
                    {
                        HomeViewModel.RequestResultAction = (RequestResult result) =>
                        {
                            switch (result.Success)
                            {
                                case true:
                                    EditInfo.EditInfoWindow.Dispatcher.Invoke(() =>
                                    {
                                        EditInfo.EditInfoWindow?.Close();
                                    });
                                    MessageBox.Show("密码保存成功！", Config.Name, MessageBoxButton.OK, MessageBoxImage.Asterisk);
                                    break;
                                case false:
                                    MessageBox.Show("输入的旧密码与原密码不一致！", Config.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
                                    break;
                                default:
                                    MessageBox.Show("密码保存失败！", Config.Name, MessageBoxButton.OK, MessageBoxImage.Error);
                                    break;
                            }
                        };

                        if (!Config.MiniClient.SendDatabaseRequest(new User() { UserName = _userModel.UserName, Password = oldPassword }, "AlterPassword", newPassword))
                        {
                            HomeViewModel.RequestResultAction = null;
                            MessageBox.Show("保存密码时发生错误！", Config.Name, MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    return;
                }
                MessageBox.Show("密码不符合要求！", Config.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            MessageBox.Show("密码不能为空！", Config.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}