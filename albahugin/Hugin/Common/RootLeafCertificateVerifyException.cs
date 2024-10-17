using System;

namespace albahugin.Hugin.Common {
    public class RootLeafCertificateVerifyException : CertificateException
    {
        public RootLeafCertificateVerifyException()
            : base("Root-Leaf Certificate cannot verify")
        {
        }

        public RootLeafCertificateVerifyException(string message)
            : base(message)
        {
        }

        public RootLeafCertificateVerifyException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}


