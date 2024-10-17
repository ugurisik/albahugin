using System.Security.Cryptography.X509Certificates;
using albahugin.Hugin.DiffieHellman;
using albahugin.Hugin.Common;
using Hugin.GMPCommon;

namespace Hugin.ExDevice { 

internal class Start : HSState
{
	public override HSState NextState => new KeyExchange();

	public Start(DeviceInfo devInfo)
	{
		HSState.DevInfo = devInfo;
	}

	public override void Init(HSMessageContext context)
	{
		base.context = new HSMessageContext();
	}

	public override HSMessageContext Process(byte[] buffer)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		GMPMessage val = GMPMessage.Parse(buffer);
		CheckErrorCode(val);
		context.EcrDevInfo = new DeviceInfo();
		GMPField val2 = val.FindTag(14663946);
		context.EcrDevInfo.Brand = MessageBuilder.DefaultEncoding.GetString(val2.Value);
		val2 = val.FindTag(14663947);
		context.EcrDevInfo.Model = MessageBuilder.DefaultEncoding.GetString(val2.Value);
		val2 = val.FindTag(14663948);
		context.EcrDevInfo.TerminalNo = MessageBuilder.DefaultEncoding.GetString(val2.Value);
		val2 = val.FindTag(14675723);
		context.EcrDevInfo.Version = MessageBuilder.DefaultEncoding.GetString(val2.Value);
		SecureComm.SetVersion(context.EcrDevInfo);
		context.EcrDH = new global::albahugin.Hugin.DiffieHellman.DiffieHellman(256);
		context.ExDH = new global::albahugin.Hugin.DiffieHellman.DiffieHellman(256);
		val2 = val.FindTag(14675713);
		try
		{
			context.EcrCertificate = new X509Certificate2(val2.Value);
		}
		catch
		{
		}
		val2 = val.FindTag(14675724);
		context.ExDH.Prime = new BigInteger(val2.Value);
		val2 = val.FindTag(14675725);
		context.ExDH.G = new BigInteger(val2.Value);
		context.ExDH.GeneratePubKey();
		val2 = val.FindTag(14675718);
		context.ErrorCode = MessageBuilder.ConvertBcdToInt(val2.Value, 0, val2.Length);
		val2 = val.FindTag(14675721);
		context.PosIndex = MessageBuilder.ConvertBcdToInt(val2.Value, 0, val2.Length);
		val2 = val.FindTag(14663950);
		context.EcrRandom = val2.Value;
		return context;
	}

	public override GMPMessage GetMessage()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected O, but got Unknown
		GMPMessage msg = new GMPMessage(16747104);
		AddExDevInfo(ref msg);
		msg.AddItem((GMPItem)new GMPField(14675723, HSState.DevInfo.Version));
		context.ExRandom = MessageBuilder.GetSecureRandomBytes(16);
		msg.AddItem((GMPItem)new GMPField(14663949, 16, context.ExRandom));
		Logger.Log((LogLevel)6, context.ExRandom, "Created Random");
		return msg;
	}
}
}