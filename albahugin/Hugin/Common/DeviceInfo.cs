using System.Net;

namespace albahugin.Hugin.Common {

    public class DeviceInfo
    {
        private IPProtocol devIPProtocol = (IPProtocol)0;

        private IPAddress devIP = null;

        private int devPort = 0;

        private string devModel = "";

        private string devBrand = "";

        private string terminalNo = "";

        private string version = "";

        private string serial = "";

        public IPProtocol IPProtocol
        {
            get
            {
                return devIPProtocol;
            }
            set
            {
                devIPProtocol = value;
            }
        }

        public IPAddress IP
        {
            get
            {
                return devIP;
            }
            set
            {
                devIP = value;
            }
        }

        public int Port
        {
            get
            {
                return devPort;
            }
            set
            {
                devPort = value;
            }
        }

        public string TerminalNo
        {
            get
            {
                return terminalNo;
            }
            set
            {
                terminalNo = value;
            }
        }

        public string SerialNum
        {
            get
            {
                return serial;
            }
            set
            {
                serial = value;
            }
        }

        public string Model
        {
            get
            {
                return devModel;
            }
            set
            {
                devModel = value;
            }
        }

        public string Brand
        {
            get
            {
                return devBrand;
            }
            set
            {
                devBrand = value;
            }
        }

        public string Version
        {
            get
            {
                return version;
            }
            set
            {
                version = value;
            }
        }

        public DeviceInfo()
        {
            devIPProtocol = IPProtocol.IPV4;
            devIP = IPAddress.Any;
        }
    }


}

