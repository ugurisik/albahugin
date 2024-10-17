namespace Hugin.POS.CompactPrinter.FP300 { 

internal class JSONItem
{
	public int Id = 0;

	public decimal Quantity = 0.00m;

	public decimal Price = 0.00m;

	public string Name = "";

	public string Barcode = "";

	public int DeptId = 0;

	public int Status = 0;

	public Adjustment Adj = null;

	public string NoteLine1 = "";

	public string NoteLine2 = "";

	public bool isVoided = false;
}
}