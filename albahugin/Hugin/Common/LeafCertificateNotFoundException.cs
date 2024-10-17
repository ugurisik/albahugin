using System;

namespace albahugin.Hugin.Common {
    public class LeafCertificateNotFoundException : CertificateException
    {
        public LeafCertificateNotFoundException()
            : base("Leaf Certificate cannot found")
        {
        }

        public LeafCertificateNotFoundException(string message)
            : base(message)
        {
        }

        public LeafCertificateNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

}

