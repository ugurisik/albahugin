using RGiesecke.DllExport;
using System;

namespace albahugin
{
    public class App
    {
        [DllExport]
        public static int Connect(string ipAddress, int port, string fiscal) { 
            Core c = new Core();
            return c.Connect(ipAddress, port, fiscal);
        }

        [DllExport]
        public static int Disconnect()
        {
            Core c = new Core();
            return c.Disconnect();
        }

        [DllExport]
        public static void PrintTest()
        {
            Core c = new Core();
            c.startNF();
            c.printSample();
            c.closeNF();
        }

        [DllExport]
        public static void SaleTest()
        {
            Core c = new Core();
            c.startReceipt();
            c.saleReceipt();
            c.paymentReceipt();
        }

        [DllExport]
        public static int saveProduct(int productId, string productName, double price)
        {
            Core c = new Core();
            decimal pr = (decimal)price;
            return c.saveProduct(productId, productName, pr);
        }

        [DllExport]
        public static int close()
        {
            Core c = new Core();
            if (c.closeDoc() == 0)
            {
                Console.WriteLine("CloseDoc");
                return 0;
            }
            else if (c.voidDoc() == 0)
            {
                Console.WriteLine("VoidDoc");
                return 0;
            }
            else
            {
                return 500;
            }
        }

        [DllExport]
        public static int startPayment()
        {
            Core c = new Core();
            int start = c.startPayment();
            Console.WriteLine("Start:" + start.ToString());
            return start;
        }

        [DllExport]
        public static int saleItem(double total, double itemPrice, double itemQuantity, int itemNo)
        {
            decimal t = (decimal)total;
            Core c = new Core();
            int sale = c.saleItem(itemPrice, itemQuantity, "", itemNo);
            Console.WriteLine("Sale:" + sale.ToString());
            return sale;
        }

        [DllExport]
        public static int endPayment(double total)
        {
            decimal t = (decimal)total;
            Core c = new Core();
            int eft = c.eftPayment(t);
            Console.WriteLine("EFT:" + eft.ToString());
            return eft;
        }

        [DllExport]
        public static int printSysInfo()
        {
            Core c = new Core();
            return c.sysInfo();
        }

        [DllExport]
        public static int printDayReport()
        {
            Core c = new Core();
            return c.periodicDateReport();
        }
    }
}
