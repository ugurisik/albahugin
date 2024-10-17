using System;

namespace Hugin.POS.CompactPrinter.FP300 { 

public class OnReportLineEventArgs : EventArgs
{
	private string line;

	public string Line => line;

	public OnReportLineEventArgs(string line)
	{
		this.line = line;
	}
}
}