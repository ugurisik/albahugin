using System;

namespace albahugin.Hugin.Common {

    public class CertificateVerifyException : CertificateException
    {
        public CertificateVerifyException()
            : base("Certificate cannot be verified")
        {
        }

        public CertificateVerifyException(string message)
            : base(message)
        {
        }

        public CertificateVerifyException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

}


