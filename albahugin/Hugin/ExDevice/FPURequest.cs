using System;
using System.Collections.Generic;
using System.Text;
using Hugin.GMPCommon;

namespace Hugin.ExDevice { 

public class FPURequest
{
	public const int MAX_PRCSS_SEC_NUM = 999999;

	public static readonly Command[] INFO_MESSAGES_COMMANDS = new Command[1] { Command.FILE_TRANSFER };

	private Command command;

	private byte[] data;

	private byte[] request;

	private int sequence;

	public Command Command => command;

	public byte[] Data => data;

	public byte[] Request => request;

	public int Sequence => sequence;

	public FPURequest(Command command, byte[] data)
	{
		int dataLen = 0;
		if (data != null)
		{
			dataLen = data.Length;
		}
		if (IsInfoMessage(command))
		{
			request = CreateRequest(ExtDevCommon.FiscalId, 16747304, command, data, dataLen);
		}
		else
		{
			request = CreateRequest(ExtDevCommon.FiscalId, 16747297, command, data, dataLen);
		}
	}

	private byte[] CreateRequest(string terminalNo, int messageType, Command cmd, byte[] data, int dataLen)
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Expected O, but got Unknown
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Expected O, but got Unknown
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Expected O, but got Unknown
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Expected O, but got Unknown
		Encoding encoding = Encoding.GetEncoding(1254);
		List<byte> list = new List<byte>();
		List<byte> list2 = new List<byte>();
		command = cmd;
		this.data = data;
		if (ExtDevCommon.IsT300)
		{
			sequence = ExtDevCommon.SequenceNum % 999999;
			if (messageType == 16747297)
			{
				GMPGroup val = new GMPGroup(57090);
				val.Add(new GMPField(14647816, 3, MessageBuilder.ConvertIntToBCD(ExtDevCommon.SequenceNum, 3)));
				val.Add(new GMPField(14647817, 3, MessageBuilder.Date2Bytes(DateTime.Now)));
				val.Add(new GMPField(14647818, 3, MessageBuilder.Time2Bytes(DateTime.Now)));
				list.AddRange(MessageBuilder.HexToByteArray(val.Tag));
				list.Add((byte)val.Length);
				list.AddRange(val.Value);
			}
		}
		else
		{
			sequence = ExtDevCommon.SequenceNum++ % 999999;
			if (messageType == 16747297)
			{
				list.AddRange(MessageBuilder.HexToByteArray(14647816));
				list.Add(3);
				byte[] collection = MessageBuilder.ConvertIntToBCD(Sequence, 3);
				list.AddRange(collection);
				list.AddRange(MessageBuilder.GetDateTimeInBytes(DateTime.Now));
			}
		}
		if (cmd != 0)
		{
			list.AddRange(MessageBuilder.HexToByteArray(14676001));
			list.Add(1);
			list.Add((byte)cmd);
		}
		if (dataLen > 0)
		{
			list.AddRange(data);
		}
		return list.ToArray();
	}

	public static bool IsInfoMessage(Command cmd)
	{
		Command[] iNFO_MESSAGES_COMMANDS = INFO_MESSAGES_COMMANDS;
		foreach (Command command in iNFO_MESSAGES_COMMANDS)
		{
			if (command == cmd)
			{
				return true;
			}
		}
		return false;
	}
}
}