using System;

namespace albahugin.Hugin.Common {

    public class CertificateTypeException : CertificateException
    {
        public CertificateTypeException()
            : base("Certificate type mismatch")
        {
        }

        public CertificateTypeException(string message)
            : base(message)
        {
        }

        public CertificateTypeException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

}


