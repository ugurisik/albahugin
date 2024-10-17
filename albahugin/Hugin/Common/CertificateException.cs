using System;

namespace albahugin.Hugin.Common {
    public class CertificateException : Exception
    {
        public int ErrorCode;

        public CertificateException()
            : base("Certificate Exception occured")
        {
        }

        public CertificateException(string message)
            : base(message)
        {
        }

        public CertificateException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

}

