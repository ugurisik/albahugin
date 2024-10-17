using System.Globalization;
using System.Threading;

namespace albahugin.Hugin.Common {
    public class Utils
    {
        public static bool CheckValueLength(string value, int maxLen)
        {
            if (!string.IsNullOrEmpty(value) && value.Length <= maxLen)
            {
                return true;
            }
            return false;
        }

        public static string FixTurkishUpperCase(string text)
        {
            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
            CultureInfo currentCulture2 = new CultureInfo("tr-TR");
            Thread.CurrentThread.CurrentCulture = currentCulture2;
            string result = text.ToUpper();
            Thread.CurrentThread.CurrentCulture = currentCulture;
            return result;
        }
    }
}


