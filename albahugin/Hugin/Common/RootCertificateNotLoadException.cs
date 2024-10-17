using System;

namespace albahugin.Hugin.Common {
    public class RootCertificateNotLoadException : CertificateException
    {
        public RootCertificateNotLoadException()
            : base("Root Certificate cannot load")
        {
        }

        public RootCertificateNotLoadException(string message)
            : base(message)
        {
        }

        public RootCertificateNotLoadException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}


