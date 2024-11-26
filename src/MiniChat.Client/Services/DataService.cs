using MiniChat.Transmitting;
using MiniChat.Client.Models;

namespace MiniChat.Client.Services
{
    public class DataService : IDataService
    {
        public UserModel GetUserModel(User user)
        {
            UserModel userModel = new UserModel()
            {
                UserID = user.UserID,
                UserName = user.UserName,
                Password = user.Password,
                NickName = user.NickName,
                Gender = user.Gender,
                Age = user.Age,
                HeadIcon = user.HeadIcon,
                State = user.State
            };
            return userModel;
        }
    }
}