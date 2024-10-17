using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using albahugin.Hugin.DiffieHellman;
using albahugin.Hugin.Common;
using Hugin.GMPCommon;

namespace Hugin.ExDevice { 



internal class T300Start : T300HSState
{
	public override T300HSState NextState => new T300KeyExchange();

	public T300Start(DeviceInfo devInfo)
	{
		T300HSState.DevInfo = devInfo;
	}

	public override void Init(HSMessageContext context)
	{
		base.context = new HSMessageContext();
	}

	public override HSMessageContext Process(byte[] buffer)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		List<byte> list = new List<byte>();
		list.AddRange(MessageBuilder.AddLength(buffer.Length));
		list.AddRange(buffer);
		GMPMessage val = GMPMessage.Parse(list.ToArray());
		CheckErrorCode(val);
		context.EcrDevInfo = new DeviceInfo();
		GMPGroup val2 = val.FindGroup(57153);
		if (val2 != null)
		{
			GMPField val3 = val2.FindTag(14663946);
			context.EcrDevInfo.Brand = MessageBuilder.DefaultEncoding.GetString(val3.Value);
			val3 = val2.FindTag(14663947);
			context.EcrDevInfo.Model = MessageBuilder.DefaultEncoding.GetString(val3.Value);
			val3 = val2.FindTag(14663948);
			context.EcrDevInfo.TerminalNo = MessageBuilder.DefaultEncoding.GetString(val3.Value);
			val3 = val2.FindTag(14663950);
			context.EcrRandom = val3.Value;
		}
		val2 = val.FindGroup(57199);
		if (val2 != null)
		{
			GMPField val3 = val2.FindTag(14675723);
			context.EcrDevInfo.Version = MessageBuilder.DefaultEncoding.GetString(val3.Value);
			T300SecureComm.SetVersion(context.EcrDevInfo);
			context.EcrDH = new global::albahugin.Hugin.DiffieHellman.DiffieHellman(256);
			context.ExDH = new global::albahugin.Hugin.DiffieHellman.DiffieHellman(256);
			val3 = val2.FindTag(14675713);
			try
			{
				context.EcrCertificate = new X509Certificate2(val3.Value);
			}
			catch (Exception)
			{
			}
			val3 = val2.FindTag(14675724);
			context.ExDH.Prime = new BigInteger(val3.Value);
			val3 = val2.FindTag(14675725);
			context.ExDH.G = new BigInteger(val3.Value);
			context.ExDH.GeneratePubKey();
			val3 = val2.FindTag(14675721);
			context.PosIndex = MessageBuilder.ConvertBcdToInt(val3.Value, 0, val3.Length);
		}
		return context;
	}

	public override GMPMessage GetMessage()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Invalid comparison between Unknown and I4
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected O, but got Unknown
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Expected O, but got Unknown
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Expected O, but got Unknown
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Expected O, but got Unknown
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Expected O, but got Unknown
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Expected O, but got Unknown
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Expected O, but got Unknown
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Expected O, but got Unknown
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Expected O, but got Unknown
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Expected O, but got Unknown
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Expected O, but got Unknown
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		GMPMessage val = new GMPMessage(16747104);
		if ((int)T300SecureComm.TestCase == 1)
		{
			val = new GMPMessage(14675969);
		}
		GMPGroup val2 = new GMPGroup(57090);
		val2.Add(new GMPField(14647816, 3, MessageBuilder.ConvertIntToBCD(ExtDevCommon.SequenceNum, 3)));
		val2.Add(new GMPField(14647817, 3, MessageBuilder.Date2Bytes(DateTime.Now)));
		val2.Add(new GMPField(14647818, 3, MessageBuilder.Time2Bytes(DateTime.Now)));
		val.AddItem((GMPItem)new GMPField(val2.Tag, val2.Length, val2.Value));
		GMPGroup msg = new GMPGroup(57153);
		AddExDevInfo(ref msg);
		context.ExRandom = MessageBuilder.GetSecureRandomBytes(16);
		msg.Add(new GMPField(14663949, 16, context.ExRandom));
		Logger.Log((LogLevel)6, context.ExRandom, "Created Random");
		val.AddItem((GMPItem)new GMPField(msg.Tag, msg.Length, msg.Value));
		GMPGroup val3 = new GMPGroup(57199);
		val3.Add(new GMPField(14675723, T300HSState.DevInfo.Version.PadRight(32, '\0')));
		val.AddItem((GMPItem)new GMPField(val3.Tag, val3.Length, val3.Value));
		return val;
	}
}
}