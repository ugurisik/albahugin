using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Threading;
using albahugin.Hugin.Common;
using Hugin.GMPCommon;

namespace Hugin.ExDevice { 

public class T300SerialConnection : IConnection
{
	private const int MAX_TRY_COUNT = 3;

	private const int TIMEOUT = 3000;

	private const byte STX = 2;

	private const byte ETX = 3;

	private const byte ACK = 6;

	private const byte NACK = 21;

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

	public T300SerialConnection(SerialPort serialPort)
	{
		sp = serialPort;
	}

	public int Send(byte[] buffer, int offset, int count)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		if (!CheckConn())
		{
			throw new PortClosedException((MessageState)0);
		}
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
					Logger.Log((LogLevel)6, new byte[1] { (byte)num3 }, "Read ACK/NACK Data");
					switch (num3)
					{
					case 6:
						break;
					default:
						throw new OperationCanceledException("T300 Invalid data received(not ACK/NACK): " + num3);
					case 21:
						continue;
					}
				}
				catch (TimeoutException)
				{
					Thread.Sleep(100);
					goto IL_0120;
				}
				catch (Exception ex2)
				{
					Logger.Log(ex2);
					goto IL_0120;
				}
				break;
				IL_0120:
				if (!CheckConn())
				{
					throw new PortClosedException((MessageState)0);
				}
				if (++num2 >= 3)
				{
					throw new OperationCanceledException("T300 Message couldn't sent.");
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
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (!CheckConn())
			{
				throw new PortClosedException((MessageState)0);
			}
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
		int num = 0;
		byte[] array = new byte[0];
		byte[] array2 = new byte[0];
		byte[] array3 = new byte[3];
		try
		{
			array3 = Receive(0, 3);
			if (array3[0] == 2)
			{
				num = array3[1] * 256 + array3[2];
				array = new byte[num + 2];
				array = Receive(0, num + 2);
				if (array[array.Length -1] != 3)
				{
					throw new Exception("Corrupt data");
				}
				short num2 = array[array.Length - 2];
				short num3 = MessageBuilder.CalculateCRC(array, 0, num);
				if (num2 != num3)
				{
					throw new Exception("Corrupt data : LRC Mismatch");
				}
				Logger.Log((LogLevel)6, new byte[1] { 6 }, "Send ACK Data");
				sp.Write(new byte[1] { 6 }, 0, 1);
				sp.BaseStream.Flush();
				Thread.Sleep(50);
			}
			else if (array3[0] == 90 && array3[1] == 66 && array3[2] == 94)
			{
				Logger.Log((LogLevel)6, array3, "!!! Z Box header");
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
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		byte[] array = new byte[length];
		int num = 50;
		int num2 = 0;
		while (sp.BytesToRead < length)
		{
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
		while (true)
		{
			try
			{
				sp.Read(array, 0, array.Length);
			}
			catch (Exception ex)
			{
				Logger.Log(ex);
				if (!CheckConn())
				{
					throw new PortClosedException((MessageState)4);
				}
				if (++num2 >= 3)
				{
					throw ex;
				}
				Thread.Sleep(5000);
				continue;
			}
			break;
		}
		Logger.Log((LogLevel)6, array, "Received Data");
		return array;
	}
}
}