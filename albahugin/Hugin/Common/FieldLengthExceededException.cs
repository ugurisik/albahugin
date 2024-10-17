using System;

namespace albahugin.Hugin.Common {

    public class FieldLengthExceededException : Exception
    {
        public FieldLengthExceededException()
            : base("Field length exceeded")
        {
        }

        public FieldLengthExceededException(string field, int len)
            : base("Field length exceeded. Field: " + field + ", Max length: " + len)
        {
        }

        public FieldLengthExceededException(string message)
            : base(message)
        {
        }

        public FieldLengthExceededException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

}


