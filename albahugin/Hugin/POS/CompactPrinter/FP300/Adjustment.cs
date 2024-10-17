namespace Hugin.POS.CompactPrinter.FP300 { 

internal class Adjustment
{
	public AdjustmentType Type = AdjustmentType.Discount;

	public decimal Amount = default(decimal);

	public int Percentage = 0;

	public string NoteLine1 = "";

	public string NoteLine2 = "";
}
}