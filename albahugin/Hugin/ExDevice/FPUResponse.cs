using System;
using System.Collections.Generic;
using System.Text;
using Hugin.GMPCommon;

namespace Hugin.ExDevice { 

public class FPUResponse
{
	private string fiscalId;

	private int seqNum;

	private int errCode;

	private State fpuState;

	private byte[] data;

	private GMPField[] detail;

	public string FiscalId => fiscalId;

	public int SequenceNum => seqNum;

	public int ErrorCode => errCode;

	public State FPUState => fpuState;

	public byte[] Data => data;

	public GMPField[] Detail
	{
		get
		{
			return detail;
		}
		set
		{
			detail = value;
		}
	}

	public FPUResponse(byte[] bytesRead)
	{
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		int num2 = 0;
		try
		{
			num = 2;
			List<byte> list = new List<byte>();
			for (int i = 0; i < 12; i++)
			{
				list.Add(bytesRead[num + i]);
			}
			fiscalId = Encoding.ASCII.GetString(list.ToArray());
			num += 12;
			num2 = MessageBuilder.ByteArrayToHex(bytesRead, num, 3);
			num += 3;
			if (num2 != 16748321 && num2 != 16748328)
			{
				throw new InvalidOperationException("Response Message Incorrect");
			}
		}
		catch (InvalidOperationException)
		{
			throw new Exception("Message id error");
		}
		catch (Exception)
		{
			throw new Exception("Invalid data");
		}
		try
		{
			int length = MessageBuilder.GetLength(bytesRead, num, out num);
			int num3 = num;
			int num4 = num;
			while (num < length + num3)
			{
				num4 = num;
				switch (MessageBuilder.GetTag(bytesRead, num, out num))
				{
				case 14647816:
				{
					int length5 = MessageBuilder.GetLength(bytesRead, num, out num);
					int num6 = MessageBuilder.ConvertBcdToInt(bytesRead, num, length5);
					num += length5;
					seqNum = num6;
					continue;
				}
				case 14647817:
					if (MessageBuilder.ConvertBytesToDate(bytesRead, num).Date != DateTime.Now.Date)
					{
						throw new Exception("The terminal date and ECR date are different!");
					}
					continue;
				case 14676002:
				{
					int length2 = MessageBuilder.GetLength(bytesRead, num, out num);
					errCode = MessageBuilder.ConvertBcdToInt(bytesRead, num, length2);
					num += length2;
					continue;
				}
				case 14676003:
				{
					int length3 = MessageBuilder.GetLength(bytesRead, num, out num);
					int num5 = MessageBuilder.ConvertBcdToInt(bytesRead, num, length3);
					num += length3;
					fpuState = (State)num5;
					continue;
				}
				case 14675986:
				{
					int length4 = MessageBuilder.GetLength(bytesRead, num, out num);
					num += length4;
					continue;
				}
				}
				int length6 = MessageBuilder.GetLength(bytesRead, num, out num);
				int j = 0;
				int num7 = 0;
				int num8 = num - num4;
				if (data != null && data.Length != 0)
				{
					j = data.Length;
					int newSize = data.Length + length6 + num8;
					Array.Resize(ref data, newSize);
				}
				else
				{
					data = new byte[length6 + num8];
				}
				for (; j < data.Length; j++)
				{
					data[j] = bytesRead[num7 + num4];
					num7++;
				}
				num += length6;
			}
		}
		catch (Exception)
		{
			throw new Exception("Invalid Data");
		}
		if (errCode == 0 && SequenceNum < 0)
		{
			throw new Exception("Invalid Data");
		}
		data = bytesRead;
	}

	private FPUResponse(bool isBusy)
	{
	}

	internal static FPUResponse CreateBusyMessage(int reqSequenceNr, string reqFiscalId)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		FPUResponse fPUResponse = new FPUResponse(isBusy: true);
		fPUResponse.seqNum = reqSequenceNr;
		fPUResponse.fiscalId = reqFiscalId;
		fPUResponse.errCode = 122;
		fPUResponse.fpuState = (State)15;
		return fPUResponse;
	}
}
}