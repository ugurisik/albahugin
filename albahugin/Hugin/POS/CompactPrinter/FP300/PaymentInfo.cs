namespace Hugin.POS.CompactPrinter.FP300 { 

internal class PaymentInfo
{
	public PaymentType Type = PaymentType.CASH;

	public int Index = 0;

	public decimal PaidTotal = 0.00m;

	public bool viaByEFT = false;
}
}