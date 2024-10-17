using System.Collections.Generic;

namespace albahugin.Hugin.Common {

    public class Customer
    {
        public const int MAX_LEN_TCKN_VKN = 12;

        public const int MAX_LEN_NAME = 32;

        public const int MAX_LEN_LABEL = 32;

        public const int MAX_LEN_TAXOFFICE = 32;

        public const int MAX_LEN_ADDRESS = 32;

        private string tckn_vkn;

        private string name;

        private string label;

        private string taxOffice;

        private List<string> addressList;

        public string TCKN_VKN
        {
            get
            {
                return tckn_vkn;
            }
            set
            {
                if (Utils.CheckValueLength(value, 12))
                {
                    tckn_vkn = value;
                    return;
                }
                throw new FieldLengthExceededException(value, 12);
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                if (Utils.CheckValueLength(value, 32))
                {
                    name = value;
                    return;
                }
                throw new FieldLengthExceededException(value, 32);
            }
        }

        public string Label
        {
            get
            {
                return label;
            }
            set
            {
                label = value;
            }
        }

        public string TaxOffice
        {
            get
            {
                return taxOffice;
            }
            set
            {
                if (Utils.CheckValueLength(value, 32))
                {
                    taxOffice = value;
                    return;
                }
                throw new FieldLengthExceededException(value, 32);
            }
        }

        public List<string> AddressList
        {
            get
            {
                return addressList;
            }
            set
            {
                if (value != null)
                {
                    foreach (string item in value)
                    {
                        if (!Utils.CheckValueLength(item, 32))
                        {
                            throw new FieldLengthExceededException(item, 32);
                        }
                    }
                }
                addressList = value;
            }
        }
    }

}


