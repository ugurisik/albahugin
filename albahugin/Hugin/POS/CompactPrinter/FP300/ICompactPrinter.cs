using System;
using System.Drawing;
using System.Runtime.InteropServices;
using albahugin.Hugin.Common;

namespace Hugin.POS.CompactPrinter.FP300 { 

[Guid("C5F5E903-8428-47A9-84C1-3306FF076ABE")]
public interface ICompactPrinter
{
	string FiscalRegisterNo { get; set; }

	bool IsVx675 { get; }

	int LogerLevel { get; set; }

	string LogDirectory { get; set; }

	string LibraryVersion { get; }

	int PrinterBufferSize { get; }

	event OnReportLineHandler OnReportLine;

	event OnFileSendingProgressHandler OnFileSendingProgress;

	string SetDepartment(int id, string name, int vatId, decimal price, int weighable);

	string SetDepartment(int id, string name, int vatId, decimal price, int weighable, decimal limit);

	string GetDepartment(int deptId);

	string SetCreditInfo(int id, string name);

	string GetCreditInfo(int id);

	string SetCurrencyInfo(int id, string name, decimal exchangeRate);

	string GetCurrencyInfo(int id);

	string SetMainCategory(int id, string name);

	string GetMainCategory(int mainCatId);

	string SetSubCategory(int id, string name, int mainCatId);

	string GetSubCategory(int subCatId);

	string SaveCashier(int id, string name, string password);

	string GetCashier(int cashierId);

	string SignInCashier(int id, string password);

	string CheckCashierIsValid(int id, string password);

	string GetLogo(int index);

	string SetLogo(int index, string line);

	DateTime GetDateTime();

	string SetDateTime(DateTime date, DateTime time);

	string GetVATRate(int index);

	string SetVATRate(int index, decimal vatRate);

	string SaveProduct(int productId, string productName, int deptId, decimal price, int weighable, string barcode, int subCatId);

	string GetProduct(int pluNo);

	string SaveGMPConnectionInfo(string ip, int port);

	string LoadGraphicLogo(System.Drawing.Image imgObj, int index = 0);

	string GetProgramOptions(int progName);

	string SaveProgramOptions(int progEnum, string progValue);

	string SendMultipleProduct(string[] productLines);

	string SetEndOfReceiptNote(int index, string line);

	string GetEndOfReceiptNote(int index);

	string PrintDocumentHeader();

	string PrintDocumentHeader(string tckn_vkn, decimal amount, int docType);

	string PrintDocumentHeader(int docType, string tckn_vkn, string docSerial, DateTime docDateTime);

	string PrintAdvanceDocumentHeader(string tckn, string name, decimal amount);

	string PrintCollectionDocumentHeader(string invoiceSerial, DateTime invoiceDate, decimal amount, string subscriberNo, string institutionName, decimal comissionAmount);

	string PrintCurrentAccountCollectionDocumentHeader(string tcknVkn, string customerName, string docSerial, DateTime docDate, decimal amount);

	string PrintFoodDocumentHeader();

	string PrintParkDocument(string plate, DateTime entrenceDate);

	string PrintInvoiceHeader(DateTime invoiceDT, string serial, string orderNo, Customer customerInfo);

	string PrintReturnDocumentHeader(DateTime invoiceDT, string serial, string orderNo, Customer customerInfo);

	string PrintSelfEmployementHeader(Customer customer, Service[] services);

	string PrintItem(int PLUNo, decimal quantity, decimal amount, string name, string barcode, int deptId, int weighable);

	string PrintDepartment(int deptId, decimal quantity, decimal amount, string name, int weighable);

	string PrintAdjustment(int adjustmentType, decimal amount, int percentage);

	string Correct();

	string Void(int PLUNo, decimal quantity);

	string VoidDepartment(int deptId, string deptName, decimal quantity);

	string PrintSubtotal(bool hardcopy);

	string PrintSubtotal(decimal stoppageAmount);

	string PrintPayment(int paymentType, int index, decimal paidTotal);

	string CloseReceipt(bool slipCopy);

	string VoidReceipt();

	string PrintRemarkLine(string[] lines);

	string PrintReceiptBarcode(string barcode);

	string PrintReceiptBarcode(int barcodeType, string barcode);

	string PrintJSONDocument(string jsonStr);

	string PrintJSONDocumentDeptOnly(string jsonStr);

	string VoidPayment(int paymentSequenceNo);

	string GetEFTCardInfo(decimal amount);

	string GetEFTAuthorisation(decimal amount, int installment, string cardNumber);

	string SaveCardInfoList(string[] cardList);

	string VoidEFTPayment(int acquierID, int batchNo, int stanNo);

	string RefundEFTPayment(int acquierID);

	string RefundEFTPayment(int acquierID, decimal amount);

	string GetEFTSlipCopy(int acquierID, int batchNo, int stanNo, int zNo, int receiptNo);

	string GetBankListOnEFT();

	string GetSalesInfo();

	string PrintSalesDocument(string jsonStr);

	string PrintEDocumentCopy(int docType, string[] lines);

	string PrintSlip(int type, string[] lines);

	string GetReportContent();

	string PrintXReport(int copy);

	string PrintXReport(int count, decimal amount, bool isAffectDrawer);

	string PrintXPluReport(int firstPlu, int lastPlu, int copy);

	string PrintSystemInfoReport(int copy);

	string PrintReceiptTotalReport(int copy);

	string PrintZReport();

	string PrintZReport(int copy);

	string PrintZReport(int countReturnDoc, decimal amountReturnDoc, bool isAffectDrawer);

	string PrintPeriodicZZReport(int firstZ, int lastZ, int copy, bool detail);

	string PrintPeriodicDateReport(DateTime firstDay, DateTime lastDay, int copy, bool detail);

	string PrintEJPeriodic(DateTime day, int copy);

	string PrintEJPeriodic(DateTime startTime, DateTime endTime, int copy);

	string PrintODocPeriodic(DateTime firstDate, DateTime lastDate, int oDocType);

	string PrintODocPeriodic(int oDocType);

	string PrintEJPeriodic(int ZStartId, int docStartId, int ZEndId, int docEndId, int copy);

	string PrintEJDetail(int copy);

	string PrintEndDayReport();

	string SendTestData(byte[] data);

	string EnterServiceMode(string password);

	string ExitServiceMode(string password);

	string ClearDailyMemory();

	string FactorySettings();

	string CloseFM();

	string SetExternalDevAddress(string ip, int port);

	string UpdateFirmware(string ip, int port);

	string PrintLogs(DateTime date);

	string CreateDB();

	bool Connect(object commObj, DeviceInfo serverInfo);

	void SetCommObject(object commObj);

	string CheckPrinterStatus();

	string GetLastResponse();

	string CashIn(decimal amount);

	string CashOut(decimal amount);

	string ChangeKeyLockStatus(bool isLock);

	string CloseNFReceipt();

	string Fiscalize(string password);

	string StartFMTest();

	string GetDrawerInfo();

	string GetLastDocumentInfo(bool lastZ);

	string GetServiceCode();

	string InterruptReport();

	string ClearError();

	string OpenDrawer();

	string SaveNetworkSettings(string ip, string subnet, string gateway);

	string SetEJLimit(int index);

	string StartEJ();

	string StartFM(int fiscalNo);

	string StartNFReceipt();

	string StartNFDocument(int documentType);

	string TransferFile(string fileName);

	string WriteNFLine(string[] lines);

	string GetLastLog();

	string GetDailySummary();

	string GetECRVersion();
}
}