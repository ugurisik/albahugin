using System;

namespace albahugin.Hugin.Common {
    public class PrinterCorruptDataException : GMP3Exception
    {
        private int command;

        public int Command
        {
            get
            {
                return command;
            }
            set
            {
                command = value;
            }
        }

        public PrinterCorruptDataException()
            : base("Corrupt data exception occured")
        {
        }

        public PrinterCorruptDataException(string message)
            : base(message)
        {
        }

        public PrinterCorruptDataException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}


