using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using albahugin.Hugin.Common;
using Hugin.GMPCommon;

namespace Hugin.ExDevice { 

public class TCPConnection : IConnection
{
	private const int MAX_TRY_COUNT = 3;

	private const int TIMEOUT = 3000;

	private const byte STX = 2;

	private const byte ETX = 3;

	private const byte ACK = 6;

	private const byte NACK = 21;

	private string ipAddress = string.Empty;

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

	public TCPConnection(Socket socket)
	{
		client = socket;
	}

	public int Send(byte[] buffer, int offset, int count)
	{
		string text = ReadExisting();
		int num = 0;
		byte[] array = new byte[count + 2];
		array[num++] = 2;
		Array.Copy(buffer, offset, array, num, count);
		num += count;
		array[num++] = 3;
		byte[] array2 = new byte[0];
		int num2 = 0;
		Logger.Enter((object)this, MethodBase.GetCurrentMethod().Name);
		while (true)
		{
			Write(array, 0, num);
			try
			{
				switch (ReadByte())
				{
				case 2:
					text = ReadExisting();
					continue;
				case 6:
					break;
				default:
					throw new OperationCanceledException();
				}
			}
			catch (TimeoutException)
			{
				goto IL_0092;
			}
			break;
			IL_0092:
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
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		byte[] array = new byte[0];
		byte[] array2 = new byte[3];
		try
		{
			Logger.DebugLine((object)this, "Read", 246);
			array2 = Receive(0, 3);
			if (array2[0] == 2)
			{
				num = array2[1] * 256 + array2[2];
				array = new byte[num + 3];
				Logger.DebugLine((object)this, "Read", 256);
				array = Receive(0, num + 3);
				Logger.DebugLine((object)this, "Read", 259);
				if (array[array.Length - 1] != 3)
				{
					throw new PrinterCorruptDataException();
				}
				short num2 = (short)((array[array.Length - 3] << 8) + array[array.Length - 2]);
				ushort num3 = SecureComm.ComputeChecksum(array, 0, num);
				if (num2 != num3)
				{
				}
				Write(new byte[1] { 6 }, 0, 1);
			}
		}
		catch (Exception ex)
		{
			Write(new byte[1] { 21 }, 0, 1);
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