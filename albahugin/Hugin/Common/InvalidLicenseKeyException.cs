using System;

namespace albahugin.Hugin.Common {

    public class InvalidLicenseKeyException : Exception
    {
        public InvalidLicenseKeyException()
            : base("Invalid License Key")
        {
        }

        public InvalidLicenseKeyException(string message)
            : base(message)
        {
        }

        public InvalidLicenseKeyException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

}


