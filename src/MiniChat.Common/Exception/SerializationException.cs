using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MiniChat.Common
{
    [Serializable]
    public class SerializationException : Exception
    {
        public SerializationException()
        {
        }

        public SerializationException(string message)
            : base(message)
        {
        }

        public SerializationException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected SerializationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
