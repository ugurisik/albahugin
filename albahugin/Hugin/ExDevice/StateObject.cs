using System.Collections.Generic;
using System.Net.Sockets;

namespace Hugin.ExDevice { 

public class StateObject
{
	public Socket workSocket = null;

	public const int BufferSize = 4096;

	public byte[] buffer = new byte[4096];

	public List<byte> sb = new List<byte>();
}
}