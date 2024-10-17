using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using Hugin.GMPCommon;

namespace Hugin.ExDevice { 

public class T300TCPConnection : IConnection
{
	private const int MAX_TRY_COUNT = 3;

	private const int TIMEOUT = 3000;

	private const byte STX = 2;

	private const byte ETX = 3;

	private const byte ACK = 6;

	private const byte NACK = 21;

	private string ipAddress = string.Empty;

	private int port = 0;

	private Socket client = null;

	public int Timeout
	{
		get
		{
			return client.ReceiveTimeout;
		}
		set
		{
			client.ReceiveTimeout = value;
		}
	}

	public event OnMessageHandler OnReportLine;

	public T300TCPConnection(Socket socket)
	{
		client = socket;
	}

	public int Send(byte[] buffer, int offset, int count)
	{
		string text = ReadExisting();
		int num = 0;
		byte[] array = new byte[0];
		int num2 = 0;
		Logger.Enter((object)this, MethodBase.GetCurrentMethod().Name);
		while (true)
		{
			Write(buffer, 0, buffer.Length);
			try
			{
			}
			catch (TimeoutException)
			{
				goto IL_003b;
			}
			break;
			IL_003b:
			num2++;
			if (num2 > 3)
			{
				count = 0;
				break;
			}
		}
		Logger.Exit((object)this, MethodBase.GetCurrentMethod().Name);
		return count;
	}

	public byte[] Read()
	{
		int num = 0;
		byte[] array = new byte[0];
		byte[] array2 = new byte[3];
		try
		{
			Logger.DebugLine((object)this, "Read", 246);
			array2 = Receive(0, 2);
			num = array2[0] * 256 + array2[1];
			array = new byte[num + 1];
			Logger.DebugLine((object)this, "Read", 256);
			array = Receive(0, num + 1);
			Logger.DebugLine((object)this, "Read", 259);
			short num2 = array[array.Length - 1];
			byte[] array3 = new byte[array.Length - 1];
			Array.Copy(array, 0, array3, 0, array.Length - 1);
			ushort num3 = MessageBuilder.CalculateLRC(array3);
			if (num2 != num3)
			{
				throw new Exception("Corrupt data: LRC mismatch");
			}
		}
		catch (Exception ex)
		{
			throw ex;
		}
		return array;
	}

	private byte[] Receive(int offset, int length)
	{
		byte[] array = new byte[length];
		int num = 0;
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		do
		{
			if (client.Available >= length)
			{
				num = client.Receive(array, 0, array.Length, SocketFlags.None);
				break;
			}
		}
		while (stopwatch.ElapsedMilliseconds < client.ReceiveTimeout);
		if (num < length)
		{
			throw new TimeoutException();
		}
		Logger.Log((LogLevel)6, array, "TCP Received Data");
		return array;
	}

	private void Write(byte[] data, int offset, int length)
	{
		Logger.Enter((object)this, MethodBase.GetCurrentMethod().Name);
		try
		{
			client.Send(data, offset, length, SocketFlags.None);
			Logger.Log((LogLevel)6, data, "TCP Sent Data");
		}
		catch (Exception ex)
		{
			throw ex;
		}
		Logger.Exit((object)this, MethodBase.GetCurrentMethod().Name);
	}

	private int ReadByte()
	{
		int num = 0;
		int num2 = 1;
		byte[] array = new byte[num2];
		Logger.Enter((object)this, MethodBase.GetCurrentMethod().Name);
		do
		{
			if (client.Poll(20000, SelectMode.SelectRead))
			{
				client.Receive(array, 0, array.Length, SocketFlags.None);
				Logger.Exit((object)this, MethodBase.GetCurrentMethod().Name);
				Logger.Log((LogLevel)6, array, "TCP Received Data");
				return array[0];
			}
			num += 20;
		}
		while (num < 3000);
		throw new TimeoutException();
	}

	private string ReadExisting()
	{
		string text = string.Empty;
		Logger.Enter((object)this, MethodBase.GetCurrentMethod().Name);
		while (client.Available > 0)
		{
			byte[] array = new byte[client.Available];
			array = Receive(0, array.Length);
			text += Encoding.ASCII.GetString(array);
		}
		Logger.Exit((object)this, MethodBase.GetCurrentMethod().Name);
		return text;
	}
}
}