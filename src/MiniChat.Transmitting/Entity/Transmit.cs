using System;

namespace MiniChat.Transmitting
{
    [Serializable]
    public class Transmit
    {
        public string SourceID { get; set; }
        public string TargetID { get; set; }
        public DataType DataType { get; set; }
        public string Parameter { get; set; }
        public object Object { get; set; }
    }
}