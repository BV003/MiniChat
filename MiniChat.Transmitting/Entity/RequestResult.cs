using System;

namespace MiniChat.Transmitting
{
    [Serializable]
    public class RequestResult
    {
        public bool? Success { get; set; }
        public object Object { get; set; }
    }
}