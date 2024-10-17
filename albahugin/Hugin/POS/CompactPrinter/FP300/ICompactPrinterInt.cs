using System;

namespace Hugin.POS.CompactPrinter.FP300 { 

public interface ICompactPrinterInt
{
	string SetDepartment(int id, string name, int vatId, int price, int weighable);

	string SetDepartment(int id, string name, int vatId, int price, int weighable, int limit);

	string SetCurrencyInfo(int id, string name, int exchangeRate);

	string SetVATRate(int index, int vatRate);

	string SaveProduct(int productId, string productName, int deptId, int price, int weighable, string barcode, int subCatId);

	string PrintDocumentHeader(string tckn_vkn, int amount, int docType);

	string PrintAdvanceDocumentHeader(string tckn, string name, int amount);

	string PrintCollectionDocumentHeader(string invoiceSerial, DateTime invoiceDate, int amount, string subscriberNo, string institutionName, int comissionAmount);

	string PrintCurrentAccountCollectionDocumentHeader(string tcknVkn, string customerName, string docSerial, DateTime docDate, int amount);

	string PrintItem(int PLUNo, int quantity, int amount, string name, string barcode, int deptId, int weighable);

	string PrintDepartment(int deptId, int quantity, int amount, string name, int weighable);

	string PrintAdjustment(int adjustmentType, int amount, int percentage);

	string Void(int PLUNo, int quantity);

	string VoidDepartment(int deptId, string deptName, int quantity);

	string PrintSubtotal(int stoppageAmount);

	string PrintPayment(int paymentType, int index, int paidTotal);

	string GetEFTCardInfo(int amount);

	string GetEFTAuthorisation(int amount, int installment, string cardNumber);

	string RefundEFTPayment(int acquierID, int amount);

	string PrintXReport(int count, int amount, bool isAffectDrawer);

	string PrintZReport(int countReturnDoc, int amountReturnDoc, bool isAffectDrawer);

	string CashIn(int amount);

	string CashOut(int amount);
}
}