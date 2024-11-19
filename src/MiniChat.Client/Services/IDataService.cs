using MiniComm.Client.Models;
using MiniSocket.Transmitting;

namespace MiniComm.Client.Services
{
    public interface IDataService
    {
        UserModel GetUserModel(User user);
    }
}