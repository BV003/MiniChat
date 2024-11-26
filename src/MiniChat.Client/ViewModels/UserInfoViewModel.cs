using System.IO;
using System.Windows;
using Microsoft.Win32;
using MiniChat.Client.Commands;
using MiniChat.Client.Views;
using MiniChat.Client.Models;
using MiniChat.Client.Helper;
using MiniChat.Transmitting;

namespace MiniChat.Client.ViewModels
{
    public class UserInfoViewModel : NotificationObject
    {
        public DelegateCommand UpdateHeadIconCommand { get; }
        public DelegateCommand UpdateInfoCommand { get; }
        public DelegateCommand AddFriendCommand { get; }

        private UserModel _userModel;
        public UserModel UserModel
        {
            get { return _userModel; }
            set
            {
                _userModel = value;
                base.OnPropertyChanged(nameof(UserModel));
            }
        }

        private Visibility _visibility;
        public Visibility Visibility
        {
            get { return _visibility; }
            set
            {
                _visibility = value;
                base.OnPropertyChanged(nameof(Visibility));
            }
        }

        public UserInfoViewModel(UserModel userModel, Visibility visibility)
        {
            UpdateHeadIconCommand = new DelegateCommand(UpdateHeadIcon);
            UpdateInfoCommand = new DelegateCommand(UpdateInfo);
            AddFriendCommand = new DelegateCommand(AddFriend);
            UserModel = userModel;
            Visibility = visibility;
        }

        /// <summary>
        /// 上传头像
        /// </summary>
        private void UpdateHeadIcon()
        {
            if (Visibility == Visibility.Visible)
            {
                OpenFileDialog openImage = new OpenFileDialog();
                openImage.Title = "上传头像";
                openImage.Filter = "图片|*.jpg;*.png";
                if (openImage.ShowDialog() == true)
                {
                    string imagePath = $"{Config.GetUserDirectory(UserModel.UserName, Config.CacheDirectoryName)}\\head{Path.GetExtension(openImage.SafeFileName)}";
                    if (ClientHelper.CutPicture(openImage.FileName, imagePath, 200, 200))
                    {
                        if (HomeViewModel.RequestResultAction == null)
                        {
                            User user = new User()
                            {
                                UserID = UserModel.UserID,
                                UserName = UserModel.UserName,
                                Password = UserModel.Password,
                                NickName = UserModel.NickName,
                                Gender = UserModel.Gender,
                                Age = UserModel.Age,
                                HeadIcon = ClientHelper.GetBytes(imagePath),
                                State = UserModel.State
                            };

                            HomeViewModel.RequestResultAction = (RequestResult result) =>
                            {
                                if (result.Success == true)
                                {
                                    UserInfo.UserInfoWindow.Dispatcher.Invoke(() =>
                                    {
                                        (Home.HomeWindow?.DataContext as HomeViewModel)?.LoadUserCommand?.Execute(null);
                                        UserInfo.UserInfoWindow.Close();
                                    });
                                    MessageBox.Show("头像上传成功！", Config.Name, MessageBoxButton.OK, MessageBoxImage.Asterisk);
                                    return;
                                }
                                MessageBox.Show("头像上传失败！", Config.Name, MessageBoxButton.OK, MessageBoxImage.Error);
                            };

                            if (!Config.MiniClient.SendDatabaseRequest(user, "AlterInfo", null))
                            {
                                HomeViewModel.RequestResultAction = null;
                                MessageBox.Show("上传头像时发生错误！", Config.Name, MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 修改资料
        /// </summary>
        private void UpdateInfo()
        {
            if (EditInfo.EditInfoWindow == null)
            {
                if (UserModel != null)
                {
                    new EditInfo(UserModel).Show();
                    UserInfo.UserInfoWindow?.Close();
                }
            }
        }

        /// <summary>
        /// 添加好友
        /// </summary>
        private void AddFriend()
        {
            if (SeekFriend.SeekFriendWindow == null)
            {
                new SeekFriend().Show();
                UserInfo.UserInfoWindow?.Close();
            }
        }
    }
}