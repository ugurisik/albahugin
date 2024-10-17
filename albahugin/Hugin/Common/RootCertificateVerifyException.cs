using System;

namespace albahugin.Hugin.Common {
    public class RootCertificateVerifyException : CertificateException
    {
        public RootCertificateVerifyException()
            : base("Root Certificate cannot verify")
        {
        }

        public RootCertificateVerifyException(string message)
            : base(message)
        {
        }

        public RootCertificateVerifyException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}


