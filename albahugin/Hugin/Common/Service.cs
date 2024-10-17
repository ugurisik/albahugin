namespace albahugin.Hugin.Common {

    public class Service
    {
        private const int MAX_LEN_DEFINITION = 32;

        private const decimal MAX_VALUE_GROSSWAGES = 999999.99m;

        private const int MAX_VALUE_STOPPAGE_RATE = 99;

        private const int MIN_VALUE_STOPPAGE_RATE = 0;

        private const int MAX_VALUE_VAT_RATE = 99;

        private const int MIN_VALUE_VAT_RATE = 0;

        private const int MAX_VALUE_STOPPAGE_OTHER = 99;

        private const int MIN_VALUE_STOPPAGE_OTHER = 0;

        private string definiton;

        private decimal brutAmount;

        private int stoppageRate;

        private int vatRate;

        private int wageRate;

        public string Definition
        {
            get
            {
                return definiton;
            }
            set
            {
                if (Utils.CheckValueLength(value, 32))
                {
                    definiton = value;
                    return;
                }
                throw new FieldLengthExceededException(value, 32);
            }
        }

        public decimal BrutAmount
        {
            get
            {
                return brutAmount;
            }
            set
            {
                if (value < 999999.99m)
                {
                    brutAmount = value;
                    return;
                }
                throw new ValueExceededMaxException(value, 999999.99m);
            }
        }

        public int StoppageRate
        {
            get
            {
                return stoppageRate;
            }
            set
            {
                if (value >= 0 && value <= 99)
                {
                    stoppageRate = value;
                    return;
                }
                throw new OutOfRangeException(value, 0, 99);
            }
        }

        public int VATRate
        {
            get
            {
                return vatRate;
            }
            set
            {
                if (value >= 0 && value <= 99)
                {
                    vatRate = value;
                    return;
                }
                throw new OutOfRangeException(value, 0, 99);
            }
        }

        public int WageRate
        {
            get
            {
                return wageRate;
            }
            set
            {
                if (value >= 0 && value <= 99)
                {
                    wageRate = value;
                    return;
                }
                throw new OutOfRangeException(value, 0, 99);
            }
        }
    }


}

