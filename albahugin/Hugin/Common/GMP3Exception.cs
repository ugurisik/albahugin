using System;
using System.IO;

namespace albahugin.Hugin.Common {

    public class GMP3Exception : IOException
    {
        public GMP3Exception()
            : base("Corrupt data exception occured")
        {
        }

        public GMP3Exception(string message)
            : base(message)
        {
        }

        public GMP3Exception(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }


}

