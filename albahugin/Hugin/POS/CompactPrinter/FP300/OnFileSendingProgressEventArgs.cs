using System;

namespace Hugin.POS.CompactPrinter.FP300 { 

public class OnFileSendingProgressEventArgs : EventArgs
{
	private string data;

	public string Data => data;

	public OnFileSendingProgressEventArgs(string data)
	{
		this.data = data;
	}
}
}