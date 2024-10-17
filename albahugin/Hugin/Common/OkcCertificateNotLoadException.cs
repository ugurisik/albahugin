using System;

namespace albahugin.Hugin.Common {
    public class OkcCertificateNotLoadException : CertificateException
    {
        public OkcCertificateNotLoadException()
            : base("OKC Certificate cannot load")
        {
        }

        public OkcCertificateNotLoadException(string message)
            : base(message)
        {
        }

        public OkcCertificateNotLoadException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}


