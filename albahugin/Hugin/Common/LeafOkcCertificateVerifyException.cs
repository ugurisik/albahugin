using System;

namespace albahugin.Hugin.Common
{

    public class LeafOkcCertificateVerifyException : CertificateException
    {
        public LeafOkcCertificateVerifyException()
            : base("Leaf-Okc Certificate cannot verify")
        {
        }

        public LeafOkcCertificateVerifyException(string message)
            : base(message)
        {
        }

        public LeafOkcCertificateVerifyException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }


}

