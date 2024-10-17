using System.Collections.Generic;

namespace Hugin.POS.CompactPrinter.FP300 { 

internal class SFResponseObject
{
	public int errorCode = -1;

	public int statusCode = -1;

	public List<string> paramList = new List<string>();
}
}