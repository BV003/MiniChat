using System;

namespace MiniChat.Transmitting
{
    [Serializable]
    public enum DataType : byte
    {
        Text = 1,
        Image = 2,
        Audio = 3,
        Video = 4,
        File = 5,
        Request = 6
    }
}