using MiniChat.Transmitting;
using MiniComm.Client.Models;
using MiniSocket.Transmitting;

namespace MiniComm.Client.Services
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