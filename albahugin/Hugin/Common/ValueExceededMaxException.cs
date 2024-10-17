using System;

namespace albahugin.Hugin.Common {
    public class ValueExceededMaxException : Exception
    {
        public ValueExceededMaxException()
            : base("Value exceeded maximum")
        {
        }

        public ValueExceededMaxException(decimal field, decimal max)
            : base("Value exceeded maximum. Value: " + field + ", Max : " + max)
        {
        }

        public ValueExceededMaxException(string message)
            : base(message)
        {
        }

        public ValueExceededMaxException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}


