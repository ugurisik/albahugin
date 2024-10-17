using System;

namespace albahugin.Hugin.Common {
    public class OkcCertificateNotFoundException : CertificateException
    {
        public OkcCertificateNotFoundException()
            : base("OKC Certificate cannot found")
        {
        }

        public OkcCertificateNotFoundException(string message)
            : base(message)
        {
        }

        public OkcCertificateNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}


