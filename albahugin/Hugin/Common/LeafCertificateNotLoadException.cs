using System;

namespace albahugin.Hugin.Common {
    public class LeafCertificateNotLoadException : CertificateException
    {
        public LeafCertificateNotLoadException()
            : base("Leaf Certificate cannot load")
        {
        }

        public LeafCertificateNotLoadException(string message)
            : base(message)
        {
        }

        public LeafCertificateNotLoadException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}


