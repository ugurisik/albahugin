using System;

namespace albahugin.Hugin.Common
{
    public class OutOfRangeException : Exception
    {
        public OutOfRangeException()
            : base("Value is out of range")
        {
        }

        public OutOfRangeException(int value, int min, int max)
            : base("Value is out of range. Value: " + value + ", Min : " + min + ", Max : " + max)
        {
        }

        public OutOfRangeException(string message)
            : base(message)
        {
        }

        public OutOfRangeException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }


}

