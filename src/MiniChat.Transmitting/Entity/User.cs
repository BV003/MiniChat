using System;

namespace MiniChat.Transmitting
{
    [Serializable]
    public class User
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string NickName { get; set; }
        public string Gender { get; set; }
        public int Age { get; set; }
        public byte[] HeadIcon { get; set; }
        public int State { get; set; }
    }
}