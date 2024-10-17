using System;

namespace Hugin.ExDevice { 

public class OnMessageEventArgs : EventArgs
{
	private byte[] buffer;

	private bool isEncrypted = false;

	public bool IsEncrypted
	{
		get
		{
			return isEncrypted;
		}
		set
		{
			isEncrypted = value;
		}
	}

	public byte[] Buffer => buffer;

	public OnMessageEventArgs(byte[] buffer)
	{
		this.buffer = buffer;
	}

	public OnMessageEventArgs(byte[] buffer, bool isEncrypted)
		: this(buffer)
	{
		this.isEncrypted = isEncrypted;
	}
}
}