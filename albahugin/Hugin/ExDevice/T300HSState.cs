using System;
using System.ComponentModel;
using System.Reflection;
using albahugin.Hugin.Common;
using Hugin.GMPCommon;

namespace Hugin.ExDevice { 



internal abstract class T300HSState
{
	public static int MAX_BUFFER_LEN = 2048;

	protected HSMessageContext context;

	private static DeviceInfo devInfo;

	public abstract T300HSState NextState { get; }

	public static DeviceInfo DevInfo
	{
		get
		{
			return devInfo;
		}
		set
		{
			devInfo = value;
		}
	}

	public abstract GMPMessage GetMessage();

	public abstract HSMessageContext Process(byte[] buffer);

	public virtual void Init(HSMessageContext context)
	{
		this.context = context;
	}

	public void AddExDevInfo(ref GMPGroup msg)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Expected O, but got Unknown
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Expected O, but got Unknown
		if ((int)T300SecureComm.TestCase == 3)
		{
			msg.Add(new GMPField(14663943, 32, MessageBuilder.DefaultEncoding.GetBytes(DevInfo.Brand.PadRight(20, ' '))));
		}
		else
		{
			msg.Add(new GMPField(14663943, 20, MessageBuilder.DefaultEncoding.GetBytes(DevInfo.Brand.PadRight(20, ' '))));
		}
		msg.Add(new GMPField(14663944, 20, MessageBuilder.DefaultEncoding.GetBytes(DevInfo.Model.PadRight(20, ' '))));
		msg.Add(new GMPField(14663945, 16, MessageBuilder.DefaultEncoding.GetBytes(DevInfo.SerialNum.PadRight(16, ' '))));
	}

	public void AddEcrDevInfo(ref GMPGroup msg)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Expected O, but got Unknown
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Expected O, but got Unknown
		msg.Add(new GMPField(14663946, 20, MessageBuilder.DefaultEncoding.GetBytes(context.EcrDevInfo.Brand.PadRight(20, ' '))));
		msg.Add(new GMPField(14663947, 20, MessageBuilder.DefaultEncoding.GetBytes(context.EcrDevInfo.Model.PadRight(20, ' '))));
		msg.Add(new GMPField(14663948, 16, MessageBuilder.DefaultEncoding.GetBytes(context.EcrDevInfo.TerminalNo.PadRight(16, ' '))));
	}

	public void CheckErrorCode(GMPMessage msg)
	{
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Invalid comparison between Unknown and I4
		GMPGroup val = msg.FindGroup(57199);
		if (val == null)
		{
			return;
		}
		GMPField val2 = val.FindTag(14675718);
		if (val2 != null)
		{
			char c = (char)val2.Value[0];
			int num = int.Parse(c.ToString());
			c = (char)val2.Value[1];
			int num2 = int.Parse(c.ToString());
			int num3 = num * 10 + num2;
			context.ErrorCode = num3;
			HSMErrorCode val3 = (HSMErrorCode)num3;
			string text = DescriptionAttr<HSMErrorCode>(val3);
			if ((int)val3 > 0)
			{
				Logger.Log((LogLevel)4, text);
				throw new Exception(text);
			}
		}
	}

	private static string DescriptionAttr<T>(T source)
	{
		string result = string.Empty;
		FieldInfo field = source.GetType().GetField(source.ToString());
		DescriptionAttribute[] array = (DescriptionAttribute[])field.GetCustomAttributes(typeof(DescriptionAttribute), inherit: false);
		if (array != null && array.Length != 0)
		{
			result = array[0].Description;
		}
		return result;
	}
}
}