using System;

namespace albahugin.Hugin.Common {
    public class RootCertificateNotFoundException : CertificateException
    {
        public RootCertificateNotFoundException()
            : base(" Root Certificate cannot found")
        {
        }

        public RootCertificateNotFoundException(string message)
            : base(message)
        {
        }

        public RootCertificateNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}


