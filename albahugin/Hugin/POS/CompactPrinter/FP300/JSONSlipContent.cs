using System.Collections.Generic;

namespace Hugin.POS.CompactPrinter.FP300 { 

internal class JSONSlipContent
{
	public int Type = 1;

	public List<JSONSlipLine> Lines = new List<JSONSlipLine>();
}
}