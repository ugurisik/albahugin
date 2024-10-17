using System.Collections.Generic;

namespace Hugin.POS.CompactPrinter.FP300 { 

internal class SummaryItem
{
	public int DocumentType;

	public int ValidCount;

	public int CancelledCount;

	public double ValidAmount;

	public double CancelledAmount;

	public double VatTotal;

	public List<VatGroupSale> VatGroupSales = new List<VatGroupSale>();

	public double Cash;

	public double Credit;

	public double Other;
}

}