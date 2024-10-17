using System;
using System.Collections.Generic;
using albahugin.Hugin.DiffieHellman;
using albahugin.Hugin.ExDevice;
using Hugin.GMPCommon;

namespace Hugin.ExDevice { 

internal class T300KeyExchange : T300HSState
{
	private const string PRM_GMP3_PRF_LABEL = "GMP-3 istek";

	private const string COMPUTE_KEYS_LABEL = "GMP-3 anahtarlar";

	private const string CER_KAMU_SM_PRODUCER = "KAMU SM";

	public override T300HSState NextState => new T300Close();

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
			GMPField val3 = val2.FindTag(14675715);
			context.ExDH.GivenPubKey = new BigInteger(val3.Value);
			context.ExDH.GenerateResponse();
			byte[] array = new byte[val3.Value.Length];
			Buffer.BlockCopy(val3.Value, 0, array, 0, val3.Value.Length);
			val3 = val2.FindTag(14675722);
			if (!CertificateManager.Verify(context.EcrCertificate, array, val3.Value))
			{
				Logger.Log((LogLevel)2, "CRYPTOGRAM A DOĞRULAMA HATASI");
				throw new Exception("IMZA DOĞRULAMA\nHATASI");
			}
			val3 = val2.FindTag(14675721);
			context.PosIndex = MessageBuilder.ConvertBcdToInt(val3.Value, 0, val3.Length);
		}
		byte[] array2 = new byte[32];
		Buffer.BlockCopy(context.ExRandom, 0, array2, 0, 16);
		Buffer.BlockCopy(context.EcrRandom, 0, array2, 16, 16);
		AppendLog("DH Created Key", context.ExDH.Key);
		byte[] array3 = HSMessageContext.PRFForTLS1_2(context.ExDH.Key, "GMP-3 istek", array2, 32);
		AppendLog("masterKey", array3);
		context.keyHMAC = HSMessageContext.PRFForTLS1_2(array3, "GMP-3 anahtarlar", array2, 32);
		AppendLog("keyHMAC", context.keyHMAC);
		context.keyIV = HSMessageContext.PRFForTLS1_2(context.keyHMAC, "GMP-3 anahtarlar", array2, 32);
		AppendLog("keyIV", context.keyIV);
		Array.Resize(ref context.keyIV, 16);
		context.keyEnc = HSMessageContext.PRFForTLS1_2(context.keyIV, "GMP-3 anahtarlar", array2, 32);
		AppendLog("Encrypt Key", context.keyEnc);
		context.CreateCryptoTransformer();
		return context;
	}

	private void AppendLog(string remark, byte[] buffer)
	{
		Logger.Log((LogLevel)6, buffer, remark);
	}

	public override GMPMessage GetMessage()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected O, but got Unknown
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Expected O, but got Unknown
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Expected O, but got Unknown
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Expected O, but got Unknown
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Expected O, but got Unknown
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Expected O, but got Unknown
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Expected O, but got Unknown
		GMPMessage val = new GMPMessage(16747106);
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
		val3.Add(new GMPField(14675715, 256, context.ExDH.PubKey));
		val3.Add(new GMPField(14675721, 1, MessageBuilder.ConvertIntToBCD(context.PosIndex, 1)));
		val.AddItem((GMPItem)new GMPField(val3.Tag, val3.Length, val3.Value));
		return val;
	}
}
}