using System.Collections.Generic;

namespace Hugin.POS.CompactPrinter.FP300 { 

internal class DailySummary
{
	public List<SummaryItem> summaries = new List<SummaryItem>();

	public double CummulativeTotal;

	public double CummulativeVat;
}
}