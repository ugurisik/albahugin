using albahugin.Hugin.Common;
using Hugin.POS.CompactPrinter.FP300;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace albahugin
{
    public class Core : IBridge
    {
        private static IConnection conn;
        private static ICompactPrinter printer = null;
        public static Encoding DefaultEncoding = Encoding.GetEncoding(1254);
        public bool isMatchedBefore = false;
        public IConnection Connection { get { return conn; } set { conn = value; } }
        public ICompactPrinter Printer { get { return printer; } set { printer = value; } }
        public static string logFilePath = "";
        public static string documentNo = "";
        public string docId = "";
        public void Log(string log)
        {
            Console.WriteLine(Utils.ClearTurkishCharacter(log));
            LogToFile(log);
        }
        public void Log()
        {
            if (printer == null)
            {
                String lastlog = printer.GetLastLog();
                if (String.IsNullOrEmpty(lastlog))
                {
                    if (!lastlog.Contains("|"))
                    {
                        Log(lastlog);
                        return;
                    }

                    string[] parsedLog = lastlog.Split('|');
                    if (parsedLog.Length == 5)
                    {
                        string command = parsedLog[0];
                        string sequnce = parsedLog[1];
                        string state = parsedLog[2];
                        string errorCode = parsedLog[3];
                        string errorMsg = parsedLog[4];

                        if (command != "NULL")
                        {

                            if (sequnce.Length == 1)
                                Console.WriteLine(String.Format("{0} {1}:", sequnce, ("KOMUT").PadRight(12, ' ')));
                            else if (sequnce.Length == 2)
                                Console.WriteLine(String.Format("{0} {1}:", sequnce, ("KOMUT").PadRight(11, ' ')));
                            else
                                Console.WriteLine(String.Format("{0} {1}:", sequnce, ("KOMUT").PadRight(10, ' ')));


                            Console.WriteLine(command);
                            Console.Write("  " + ("FPU STATE").PadRight(12, ' ') + ":");
                            Console.WriteLine(state);
                        }

                        Console.Write("  " + ("YANIT").PadRight(12, ' ') + ":");
                        Console.WriteLine(errorMsg);

                    }
                }
            }
        }

        public static void LogToFile(string message)
        {
            using (StreamWriter writer = new StreamWriter("C:\\ALBAPOS\\log\\"+logFilePath, true))
            {
                writer.WriteLine(message);
            }
        }

        private static string fiscalId = "";

        public void SetFiscalId(string strId)
        {
            int id = int.Parse(strId.Substring(2));

            if (id == 0 || id > 99999999)
            {
                throw new Exception("Geçersiz mali numara.");
            }
            fiscalId = strId;

            if (printer != null)
                printer.FiscalRegisterNo = fiscalId;
        }

        public int Connect(string ipAddress, int port, string fiscal)
        {
            try
            {
                if (Connection == null) {
                    Log(FormMessage.CONNECTING + "... (" + FormMessage.PLEASE_WAIT + ")");
                    Connection = new TCPConnection(ipAddress, port);
                    Connection.Open();
                    MatchDevice(port,fiscal);
                    Log(FormMessage.CONNECTED);
                }
                else
                {
                    Connection.Close();
                    Connection = null;
                    Log(FormMessage.DISCONNECTED);
                    this.Connect(ipAddress,port,fiscal);
                }
                return 0;
            }
            catch (Exception ex) {
                Log(FormMessage.OPERATION_FAILS + "--> " + FormMessage.CONNECTION_ERROR + ": " + ex.Message);
                return 500;
            }
        }

        public int checkConnection()
        {
            try {
                if (this.Connection != null)
                {
                    Log(FormMessage.CONNECTED);
                    return 200;
                }
                else {
                    Log(FormMessage.CONNECTIONNOTFOUND);
                    return 404;
                }
            }
            catch (Exception e) {
                Log(FormMessage.CONNECTION_ERROR);
                return 500;
            }
        }

        public int Disconnect() {
            try
            {
                if (this.Connection != null)
                {
                    this.Connection.Close();
                    this.Connection = null;
                    Log(FormMessage.DISCONNECTED);
                    return 0;
                }
                else
                {
                    Log(FormMessage.CONNECTIONNOTFOUND);
                    return 500;
                }
            }
            catch (Exception ex)
            {
                Log(FormMessage.OPERATION_FAILS + "--> " + FormMessage.CONNECTION_ERROR + ": " + ex.Message);
                return 500;
            }
        }

        public void MatchDevice(int port, string fiscal) {
            SetFiscalId(fiscal);
            Utils.createDir(@"C:\ALBAPOS\log");
            Utils.createDir(@"C:\ALBAPOS\Certificates");


            DeviceInfo di = new DeviceInfo();
            di.IP = System.Net.IPAddress.Parse(Utils.GetIPAddress());
            di.IPProtocol = IPProtocol.IPV4;
            di.Port = port;
            di.TerminalNo = fiscal.PadLeft(8, '0');
            di.Brand = "HUGIN";
            di.Model = "HUGIN COMPACT";
            di.Version = new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).LastWriteTime.ToShortDateString();

            try { 
                string guid = Utils.GetMachineGuid();
                if (guid.Length <= 0) { di.SerialNum = Utils.CreateMD5("123"); }
                Log(FormMessage.HARDWAREID+": " + guid);
            }catch (Exception e) { di.SerialNum = Utils.CreateMD5("0123"); }

            if (conn.IsOpen) {
                if (isMatchedBefore) { 
                    printer.SetCommObject(conn);
                    return;
                }
                try {
                    printer = new CompactPrinter();
                    printer.FiscalRegisterNo = fiscalId;
                    printer.LogDirectory = "C:\\ALBAPOS\\log";
                    printer.LogerLevel = 6;
                    if (!printer.Connect(conn.ToObject(), di))
                    {
                        throw new OperationCanceledException(FormMessage.UNABLE_TO_MATCH);
                    }
                    if (printer.PrinterBufferSize != conn.BufferSize)
                    {
                        conn.BufferSize = printer.PrinterBufferSize;
                    }
                    printer.SetCommObject(conn.ToObject());
                    isMatchedBefore = true;


                } catch (Exception e) {
                    throw e;
                }
                CPResponse.Bridge = this;
            }

        }

        public void startNF()
        {
            try
            {
                ParseResponse(new CPResponse(this.Printer.StartNFReceipt()));
            }
            catch (Exception ex)
            {
                Log(FormMessage.OPERATION_FAILS + ": " + ex.Message);
            }
        }

        public void printSample()
        {
            List<string> lines = new List<string>();
            for (int i = 0; i < 25; i++)
            {
                lines.Add(String.Format("{1} {0}", i, FormMessage.SAMPLE_LINE));
            }

            try
            {
                CPResponse response = new CPResponse(Printer.WriteNFLine(lines.ToArray()));
            }
            catch (Exception ex)
            {
                Log(FormMessage.OPERATION_FAILS + ": " + ex.Message);
            }
        }

        public void closeNF()
        {
            try
            {
                ParseResponse(new CPResponse(this.Printer.CloseNFReceipt()));
            }
            catch (Exception ex)
            {
                Log(FormMessage.OPERATION_FAILS + ": " + ex.Message);
            }
        }

        public void startReceipt()
        {
            try
            {
                CPResponse response = new CPResponse(Printer.PrintDocumentHeader());
                if (response.ErrorCode == 0)
                {
                    Log(FormMessage.DOCUMENT_ID.PadRight(12, ' ') + ":" + response.GetNextParam());
                }
            }
            catch (Exception ex)
            {
                Log(FormMessage.OPERATION_FAILS + ": " + ex.Message);
            }
        }

        public void saleReceipt()
        {
            try
            {
                CPResponse response = new CPResponse(Printer.PrintItem(1, 1, 11, null, null, -1, -1));

                if (response.ErrorCode == 0)
                {
                    Log(String.Format(FormMessage.SUBTOTAL.PadRight(12, ' ') + ":{0}", response.GetNextParam()));
                }
            }
            catch (Exception ex)
            {
                Log(FormMessage.OPERATION_FAILS + ": " + ex.Message);
            }
        }

        public void paymentReceipt()
        {
            try
            {
                int index = -1;
                int paymentType = 0;
                decimal amount = 155;
                CPResponse response = new CPResponse(Printer.PrintPayment(paymentType, index, amount));

                if (response.ErrorCode == 0)
                {
                    Log(String.Format(FormMessage.SUBTOTAL.PadRight(12, ' ') + ":{0}", response.GetNextParam()));

                    Log(String.Format(FormMessage.PAID_TOTAL.PadRight(12, ' ') + ":{0}", response.GetNextParam()));

                }

            }
            catch (Exception ex)
            {
                Log(FormMessage.OPERATION_FAILS + ": " + ex.Message);
            }
        }

        public int saveProduct(int productId, string productName, decimal price)
        {
            try
            {
                CPResponse response = new CPResponse(this.Printer.SaveProduct(productId, productName, 1, price, -1, null, -1));
                if (response.ErrorCode == 0)
                {
                    Log(String.Format(FormMessage.OPERATION_SUCCESSFULL.PadRight(12, ' ') + ":{0}", response.GetNextParam()));
                }
                else
                {
                    Log(FormMessage.PRODUCT_COULD_NOT_BE_SAVED + ": " + response.StatusMessage + " | " + response.ErrorMessage);
                }
                return response.ErrorCode;
            }
            catch (Exception e)
            {
                Log(FormMessage.PRODUCT_COULD_NOT_BE_SAVED + ": " + e.Message);
                return 500;
            }
        }

        public int closeDoc(bool slipCopy)
        {
            try
            {
                CPResponse response = new CPResponse(this.Printer.CloseReceipt(false));
                if (response.ErrorCode == 0)
                {
                    Log(FormMessage.DOCUMENT_ID.PadRight(12, ' ') + ":" + response.GetNextParam());
                }
                return response.ErrorCode;
            }
            catch (Exception ex)
            {
                Log(FormMessage.OPERATION_FAILS + ": " + ex.Message);
                return 500;
            }
        }

        public int voidDoc()
        {
            try
            {
                CPResponse response = new CPResponse(this.Printer.VoidReceipt());

                if (response.ErrorCode == 0)
                {
                    Log(FormMessage.VOIDED_DOC_ID.PadRight(12, ' ') + ":" + response.GetNextParam());
                }
                return response.ErrorCode;
            }
            catch (Exception ex)
            {
                Log(FormMessage.OPERATION_FAILS + ": " + ex.Message);
                return 500;
            }
        }

        public int startPayment()
        {
            try
            {
                docId = "";
                CPResponse response = new CPResponse(Printer.PrintDocumentHeader());
                //CPResponse response = new CPResponse(Printer.PrintDocumentHeader(1, "11111111111", "1234FISNO", new DateTime()));
                if (response.ErrorCode == 0)
                {
                    docId = response.GetNextParam();
                    Log(FormMessage.DOCUMENT_ID.PadRight(12, ' ') + ":" + docId);
                }
                else {
                    Log("startPaymentErr:"+response.ErrorMessage + " StatusMessage:"+response.StatusMessage);
                    docId = "-1";
                }
                return response.ErrorCode;
            }
            catch (Exception ex)
            {
                Log(FormMessage.OPERATION_FAILS + ": " + ex.Message);
                docId = "-1";
                return 500;
            }
        }

        public int startPaymentWithHeader(string tc, string slipno , int type)
        {
            try
            {
                docId = "";
                //CPResponse response = new CPResponse(Printer.PrintDocumentHeader());
                CPResponse response = new CPResponse(Printer.PrintDocumentHeader(type, tc, slipno, new DateTime()));
                if (response.ErrorCode == 0)
                {
                    docId = response.GetNextParam();
                    Log(FormMessage.DOCUMENT_ID.PadRight(12, ' ') + ":" + docId);
                }
                else
                {
                    Log("startPaymentErr:" + response.ErrorMessage + " StatusMessage:" + response.StatusMessage);
                    docId = "-1";
                }
                return response.ErrorCode;
            }
            catch (Exception ex)
            {
                Log(FormMessage.OPERATION_FAILS + ": " + ex.Message);
                docId = "-1";
                return 500;
            }
        }

        public int saleItem(double itemPrice, double itemQuantity, string itemName, int itemNo)
        {
            try
            {
                decimal q = (decimal)itemQuantity;
                decimal p = (decimal)itemPrice;
                CPResponse response = new CPResponse(Printer.PrintItem(itemNo, q, p, itemName, null, -1, -1));
                if (response.ErrorCode == 0)
                {
                    Log(String.Format(FormMessage.SUBTOTAL.PadRight(12, ' ') + ":{0}", response.GetNextParam()));
                }
                return response.ErrorCode;
            }
            catch (Exception ex)
            {
                Log(FormMessage.OPERATION_FAILS + ": " + ex.Message);
                return 500;
            }
        }

        public int eftPayment(decimal total)
        {
            try
            {
                CPResponse response = new CPResponse(Printer.GetEFTAuthorisation(total, 1, ""));
                if (response.ErrorCode == 0)
                {
                    string totalAmount = response.GetNextParam();
                    string provisionCode = response.GetNextParam();
                    string paidAmount = response.GetNextParam();
                    string installmentCount = response.GetNextParam();
                    string acquirerId = response.GetNextParam();
                    string bin = response.GetNextParam();
                    string issuerId = response.GetNextParam();
                    string subOprtType = response.GetNextParam();
                    string batch = response.GetNextParam();
                    string stan = response.GetNextParam();
                    string totalPaidAmount = response.GetNextParam();

                    for (int i = 0; i < response.ParamList.Count; i++) {
                        Log(response.ParamList[i].ToString());                    
                    }


                    Log(String.Format("İşlem Tutarı   :{0}", paidAmount));
                    Log(String.Format("Ödeme Toplamı  :{0}", totalPaidAmount));
                    Log(String.Format("Belge Tutarı   :{0}", totalAmount));
                    Log(String.Format("Taksit sayısı  :{0}", installmentCount));
                    Log(String.Format("Provizyon kodu :{0}", provisionCode));
                    Log(String.Format("ACQUIRER ID    :{0}", acquirerId));
                    Log(String.Format("BIN            :{0}", bin));
                    Log(String.Format("ISSUERER ID    :{0}", issuerId));
                    if (!String.IsNullOrEmpty(batch))
                        Log(String.Format("BATCH NO       :{0}", batch));
                    if (!String.IsNullOrEmpty(stan))
                        Log(String.Format("STAN NO        :{0}", stan));

                    if (subOprtType == null)
                    {
                        subOprtType = Utils.SubOperationType.SATIS.ToString();
                    }
                    else
                    {
                        subOprtType = Enum.GetName(typeof(Utils.SubOperationType), int.Parse(subOprtType));
                    }
                    Log(String.Format("Alt İşlem Tipi :{0}", subOprtType));
                }
                else
                {
                    Log(FormMessage.OPERATION_FAILS + ": " + response.ErrorMessage);
                }
                return response.ErrorCode;
            }
            catch (Exception e)
            {
                Log(FormMessage.OPERATION_FAILS + ": " + e.Message);
                return 500;
            }
        }
        private void ParseResponse(CPResponse response)
        {
            try
            {
                if (response.ErrorCode == 0)
                {
                    string retVal = response.GetNextParam();
                    if (!String.IsNullOrEmpty(retVal))
                    {
                        Log(String.Format(FormMessage.DATE.PadRight(12, ' ') + ":{0}", retVal));
                    }

                    retVal = response.GetNextParam();
                    if (!String.IsNullOrEmpty(retVal))
                    {
                        Log(String.Format(FormMessage.TIME.PadRight(12, ' ') + ":{0}", retVal));
                    }
                    retVal = response.GetNextParam();
                    if (!String.IsNullOrEmpty(retVal))
                    {
                        Log(String.Format("NOTE".PadRight(12, ' ') + ":{0}", retVal));
                    }
                    retVal = response.GetNextParam();
                    if (!String.IsNullOrEmpty(retVal))
                    {
                        Log(String.Format(FormMessage.AMOUNT.PadRight(12, ' ') + ":{0}", retVal));
                    }
                    retVal = response.GetNextParam();
                    if (!String.IsNullOrEmpty(retVal))
                    {
                        Log(FormMessage.DOCUMENT_ID.PadRight(12, ' ') + ":" + retVal);
                    }

                    retVal = response.GetNextParam();
                    if (!String.IsNullOrEmpty(retVal))
                    {
                    }

                    retVal = response.GetNextParam();
                    if (!String.IsNullOrEmpty(retVal))
                    {
                        String authNote = "";
                        try
                        {
                            switch (int.Parse(retVal))
                            {
                                case 0:
                                    authNote = FormMessage.SALE;
                                    break;
                                case 1:
                                    authNote = "PROGRAM";
                                    break;
                                case 2:
                                    authNote = FormMessage.SALE + " & Z";
                                    break;
                                case 3:
                                    authNote = FormMessage.ALL;
                                    break;
                                default:
                                    authNote = "";
                                    break;
                            }

                            Log(FormMessage.AUTHORIZATION.PadRight(12, ' ') + ":" + authNote);
                        }
                        catch { }
                    }
                }

            }
            catch (Exception ex)
            {
                Log(FormMessage.OPERATION_FAILS + ": " + ex.Message);
            }
        }

        public int sysInfo()
        {
            try
            {   
                // TODO:: Not Working!
                CPResponse response = new CPResponse(this.Printer.PrintSystemInfoReport(2));
                if (response.ErrorCode == 0)
                {
                    Log(FormMessage.OPERATION_SUCCESSFULL + ":" + response.GetNextParam());
                }
                return response.ErrorCode;
            }
            catch (Exception e)
            {
                Log(FormMessage.OPERATION_FAILS + ": " + e.Message);
                return 500;
            }
        }

        public int periodicDateReport()
        {
            try
            {
                DateTime lastDay = DateTime.Now;
                DateTime firstDay = lastDay.AddDays(-1);
                CPResponse response = new CPResponse(this.Printer.PrintPeriodicDateReport(firstDay,lastDay,0,true));
                if (response.ErrorCode == 0)
                {
                    Log(FormMessage.OPERATION_SUCCESSFULL + ":" + response.GetNextParam());
                }
                return response.ErrorCode;
            }
            catch (Exception e)
            {
                Log(FormMessage.OPERATION_FAILS + ": " + e.Message);
                return 500;
            }
        }

        public int PrintRemarkLine(string[] lines)
        {
            try
            {
                CPResponse response = new CPResponse(this.Printer.PrintRemarkLine(lines));
                if (response.ErrorCode == 0)
                {
                    Log(FormMessage.OPERATION_SUCCESSFULL + ":" + response.GetNextParam());
                }
                Log("lines:" + response.ErrorCode + " | "+ response.ErrorMessage + " | "+response.StatusCode + " | " + response.StatusMessage);
                return response.ErrorCode;
            }
            catch (Exception e)
            {
                Log(FormMessage.OPERATION_FAILS + ": " + e.Message);
                return 500;
            }
        }

        public int PrintBarcode(int type, string barcodeQr)
        {
            try
            {
                CPResponse response = new CPResponse(this.Printer.PrintReceiptBarcode(type, barcodeQr));
                if (response.ErrorCode == 0)
                {
                    Log(FormMessage.OPERATION_SUCCESSFULL + ":" + response.GetNextParam());
                }
                Log("lines:" + response.ErrorCode + " | " + response.ErrorMessage + " | " + response.StatusCode + " | " + response.StatusMessage);
                return response.ErrorCode;
            }
            catch (Exception e)
            {
                Log(FormMessage.OPERATION_FAILS + ": " + e.Message);
                return 500;
            }
        }

    }
}
