using System;
using System.Collections.Generic;
using Hugin.GMPCommon;

namespace Hugin.ExDevice { 

internal class T300Close : T300HSState
{
	public override T300HSState NextState => null;

	public override HSMessageContext Process(byte[] buffer)
	{
		List<byte> list = new List<byte>();
		list.AddRange(MessageBuilder.AddLength(buffer.Length));
		list.AddRange(buffer);
		GMPMessage val = GMPMessage.Parse(list.ToArray());
		CheckErrorCode(val);
		GMPGroup val2 = val.FindGroup(57199);
		if (val2 != null)
		{
			GMPField val3 = val2.FindTag(14675721);
			context.PosIndex = MessageBuilder.ConvertBcdToInt(val3.Value, 0, val3.Length);
			val3 = val2.FindTag(14675714);
			context.KeyCancelCounter = MessageBuilder.ConvertBcdToInt(val3.Value, 0, val3.Length);
		}
		for (int i = 0; i < context.keyIV.Length; i++)
		{
			context.keyIV[i] = (byte)(context.ExRandom[i] ^ context.EcrRandom[i]);
		}
		Logger.Log((LogLevel)6, context.keyIV, "context.keyIV");
		context.CreateCryptoTransformer();
		return context;
	}

	public override GMPMessage GetMessage()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Expected O, but got Unknown
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Expected O, but got Unknown
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Expected O, but got Unknown
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Expected O, but got Unknown
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Expected O, but got Unknown
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Expected O, but got Unknown
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Expected O, but got Unknown
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Expected O, but got Unknown
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Expected O, but got Unknown
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Expected O, but got Unknown
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Expected O, but got Unknown
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Expected O, but got Unknown
		GMPMessage val = new GMPMessage(16747107);
		byte[] array = null;
		if (T300SecureComm.VERSION == 1)
		{
			int num = 32;
			array = new byte[num + 32];
			for (int i = 0; i < num; i++)
			{
				array[i] = 32;
			}
			for (int j = num; j < array.Length; j++)
			{
				array[j] = byte.MaxValue;
			}
		}
		else
		{
			array = new byte[32];
			for (int k = 0; k < array.Length; k++)
			{
				array[k] = byte.MaxValue;
			}
		}
		GMPGroup val2 = new GMPGroup(57090);
		val2.Add(new GMPField(14647816, 3, MessageBuilder.ConvertIntToBCD(ExtDevCommon.SequenceNum, 3)));
		val2.Add(new GMPField(14647817, 3, MessageBuilder.Date2Bytes(DateTime.Now)));
		val2.Add(new GMPField(14647818, 3, MessageBuilder.Time2Bytes(DateTime.Now)));
		val.AddItem((GMPItem)new GMPField(val2.Tag, val2.Length, val2.Value));
		GMPGroup msg = new GMPGroup(57153);
		AddExDevInfo(ref msg);
		AddEcrDevInfo(ref msg);
		val.AddItem((GMPItem)new GMPField(msg.Tag, msg.Length, msg.Value));
		GMPGroup val3 = new GMPGroup(57199);
		Logger.Log((LogLevel)6, array, "KCV");
		val3.Add(new GMPField(14675726, 32, context.TransformData(array, 0, array.Length)));
		val3.Add(new GMPField(14675721, 1, MessageBuilder.ConvertIntToBCD(context.PosIndex, 1)));
		byte[] array2 = T300SecureComm.ComputeDLLHashData();
		byte[] array3 = context.TransformData(array2, 0, array2.Length);
		val3.Add(new GMPField(14675856, 32, array3));
		Logger.Log((LogLevel)6, array3, "UNIQUE ID");
		Logger.Log((LogLevel)6, array2, "DLL HASH DATA");
		val.AddItem((GMPItem)new GMPField(val3.Tag, val3.Length, val3.Value));
		return val;
	}
}
}