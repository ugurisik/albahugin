using Hugin.ExDevice;
using Hugin.GMPCommon;

namespace albahugin.Hugin.ExDevice { 

internal class Close : HSState
{
	public override HSState NextState => null;

	public override HSMessageContext Process(byte[] buffer)
	{
		GMPMessage msg = GMPMessage.Parse(buffer);
		CheckErrorCode(msg);
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
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Expected O, but got Unknown
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Expected O, but got Unknown
		GMPMessage msg = new GMPMessage(16747107);
		byte[] array = null;
		if (SecureComm.VERSION == 1)
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
		AddExDevInfo(ref msg);
		AddEcrDevInfo(ref msg);
		Logger.Log((LogLevel)6, array, "KCV");
		msg.AddItem((GMPItem)new GMPField(14675726, context.TransformData(array, 0, array.Length)));
		msg.AddItem((GMPItem)new GMPField(14675721, MessageBuilder.ConvertIntToBCD(context.PosIndex, 1)));
		return msg;
	}
}
}