using System;

namespace albahugin.Hugin.Common
{
    public class CertificateMatchException : CertificateException
    {
        public CertificateMatchException()
            : base("Certificates Pubkey mismatch")
        {
        }

        public CertificateMatchException(string message)
            : base(message)
        {
        }

        public CertificateMatchException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}


