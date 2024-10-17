using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Threading;
using albahugin.Hugin.Common;
using Hugin.GMPCommon;

namespace Hugin.ExDevice { 



public class SerialConnection : IConnection
{
	private const int MAX_TRY_COUNT = 3;

	private const int TIMEOUT = 3000;

	private const byte STX = 2;

	private const byte ETX = 3;

	private const byte ACK = 6;

	private const byte NACK = 21;

	private bool iReadSTX = false;

	private static int CHECK_CONN_PERIOD_TIME = 1000;

	private static SerialPort sp;

	public int Timeout
	{
		get
		{
			return sp.ReadTimeout;
		}
		set
		{
			sp.ReadTimeout = value;
		}
	}

	public event OnMessageHandler OnReportLine;

	public SerialConnection(SerialPort serialPort)
	{
		sp = serialPort;
	}

	public int Send(byte[] buffer, int offset, int count)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		if (!CheckConn())
		{
			throw new PortClosedException((MessageState)0);
		}
		iReadSTX = false;
		ClearBuffer();
		int num = 0;
		int readTimeout = sp.ReadTimeout;
		byte[] array = new byte[count + 2];
		array[num++] = 2;
		Array.Copy(buffer, offset, array, num, count);
		num += count;
		array[num++] = 3;
		byte[] array2 = new byte[0];
		int num2 = 0;
		sp.ReadTimeout = 4500;
		try
		{
			while (true)
			{
				try
				{
					sp.Write(array, 0, num);
					Logger.Log((LogLevel)6, array, "Serial Sent Data");
					sp.BaseStream.Flush();
					Thread.Sleep(10);
					int num3 = sp.ReadByte();
					Logger.Log((LogLevel)6, new byte[1] { (byte)num3 }, "Read ACK Data");
					switch (num3)
					{
					case 2:
						iReadSTX = true;
						break;
					case 6:
						break;
					default:
						throw new OperationCanceledException("Invalid data received: " + num3);
					}
				}
				catch (TimeoutException)
				{
					Thread.Sleep(100);
					goto IL_012d;
				}
				catch (Exception ex2)
				{
					Logger.Log(ex2);
					goto IL_012d;
				}
				break;
				IL_012d:
				if (!CheckConn())
				{
					throw new PortClosedException((MessageState)0);
				}
				if (++num2 >= 3)
				{
					throw new OperationCanceledException("Message couldn't sent.");
				}
				ClearBuffer();
			}
		}
		finally
		{
			sp.ReadTimeout = readTimeout;
		}
		return count;
	}

	private void ClearBuffer()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (!CheckConn())
			{
				throw new PortClosedException((MessageState)0);
			}
			sp.ReadExisting();
			sp.DiscardInBuffer();
		}
		catch (IOException)
		{
			throw new PortClosedException((MessageState)0);
		}
	}

	private bool CheckConn()
	{
		if (sp == null)
		{
			return false;
		}
		if (!sp.IsOpen)
		{
			return false;
		}
		List<string> list = new List<string>(SerialPort.GetPortNames());
		if (!list.Exists((string comName) => comName.ToUpper() == sp.PortName.ToUpper()))
		{
			return false;
		}
		return true;
	}

	public byte[] Read()
	{
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		byte[] array = new byte[0];
		byte[] array2 = new byte[3];
		try
		{
			if (iReadSTX)
			{
				byte[] array3 = Receive(0, 2);
				array2[0] = 2;
				array2[1] = array3[0];
				array2[2] = array3[1];
			}
			else
			{
				array2 = Receive(0, 3);
			}
			if (array2[0] == 2)
			{
				num = array2[1] * 256 + array2[2];
				array = new byte[num + 3];
				array = Receive(0, num + 3);
				if (array[array.Length -1] != 3)
				{
					throw new PrinterCorruptDataException();
				}
				short num2 = (short)((array[array.Length - 3] << 8) + array[array.Length - 2]);
				ushort num3 = SecureComm.ComputeChecksum(array, 0, num);
				if (num2 != num3)
				{
				}
				Logger.Log((LogLevel)6, new byte[1] { 6 }, "Send ACK Data");
				sp.Write(new byte[1] { 6 }, 0, 1);
				sp.BaseStream.Flush();
				Thread.Sleep(50);
			}
			else if (array2[0] == 90 && array2[1] == 66 && array2[2] == 94)
			{
				Logger.Log((LogLevel)6, array2, "!!! Z Box header");
				byte[] array4 = Receive(0, 7);
				Logger.Log((LogLevel)6, array4, "!!! Z Box request header");
				array4 = Receive(0, array4[5] * 256 + array4[6]);
				Logger.Log((LogLevel)6, array4, "!!! Z Box request");
				return Read();
			}
		}
		catch (Exception ex)
		{
			if (CheckConn())
			{
				sp.Write(new byte[1] { 21 }, 0, 1);
				Logger.Log((LogLevel)6, new byte[1] { 21 }, "Send Nack Data");
			}
			throw ex;
		}
		return array;
	}

	private byte[] Receive(int offset, int length)
	{
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		byte[] array = new byte[length];
		int num = 50;
		int num2 = 0;
		int num3 = length;
		while (true)
		{
			num3 = ((length <= 4000) ? length : 4000);
			if (sp.BytesToRead >= num3)
			{
				num = 0;
				length -= num3;
				sp.Read(array, num2, num3);
				num2 += num3;
				if (length <= 0)
				{
					break;
				}
				byte[] array2 = new byte[1] { 6 };
				sp.Write(array2, 0, 1);
			}
			Thread.Sleep(50);
			num += 50;
			if (num >= sp.ReadTimeout)
			{
				Logger.Log((LogLevel)6, "Timeout :" + sp.ReadTimeout);
				throw new TimeoutException();
			}
			if (num % CHECK_CONN_PERIOD_TIME == 0 && !CheckConn())
			{
				throw new PortClosedException((MessageState)3);
			}
		}
		Logger.Log((LogLevel)6, array, "Received Data");
		return array;
	}
}
}