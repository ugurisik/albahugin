using System.Collections.Generic;

namespace Hugin.POS.CompactPrinter.FP300 { 

internal class JSONDocument
{
	public List<JSONItem> FiscalItems = new List<JSONItem>();

	public List<Adjustment> Adjustments = new List<Adjustment>();

	public List<PaymentInfo> Payments = new List<PaymentInfo>();

	public List<string> FooterNotes = new List<string>();

	public List<JSONSlipContent> SlipContents = new List<JSONSlipContent>();

	public EndOfReceipt EndOfReceiptInfo = new EndOfReceipt();
}
}