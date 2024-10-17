using System;

namespace albahugin.Hugin.Common {
    public class CertificateNotValidException : CertificateException
    {
        public CertificateNotValidException()
            : base("Certificate is not valid")
        {
        }

        public CertificateNotValidException(string message)
            : base(message)
        {
        }

        public CertificateNotValidException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}


