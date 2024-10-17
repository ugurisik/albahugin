using System;

namespace albahugin.Hugin.Common {
    public class PortClosedException : GMP3Exception
    {
        private MessageState msgState;

        public MessageState MsgState => msgState;

        public PortClosedException()
            : base("Port closed exception occured")
        {
        }

        public PortClosedException(string message)
            : base(message)
        {
        }

        public PortClosedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public PortClosedException(MessageState msgState)
            : base("Port closed exception occured. Message state : " + msgState)
        {
            this.msgState = msgState;
        }
    }

}

