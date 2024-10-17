using System;
using System.ComponentModel;
using System.Reflection;
using albahugin.Hugin.Common;
using Hugin.GMPCommon;

namespace Hugin.ExDevice { 

internal abstract class HSState
{
	public static int MAX_BUFFER_LEN = 2048;

	protected HSMessageContext context;

	private static DeviceInfo devInfo;

	public abstract HSState NextState { get; }

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

	public void AddExDevInfo(ref GMPMessage msg)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		msg.AddItem((GMPItem)new GMPField(14663943, DevInfo.Brand));
		msg.AddItem((GMPItem)new GMPField(14663944, DevInfo.Model));
		msg.AddItem((GMPItem)new GMPField(14663945, DevInfo.SerialNum));
	}

	public void AddEcrDevInfo(ref GMPMessage msg)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Expected O, but got Unknown
		msg.AddItem((GMPItem)new GMPField(14663946, context.EcrDevInfo.Brand));
		msg.AddItem((GMPItem)new GMPField(14663947, context.EcrDevInfo.Model));
		msg.AddItem((GMPItem)new GMPField(14663948, context.EcrDevInfo.SerialNum));
	}

	public void CheckErrorCode(GMPMessage msg)
	{
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		GMPField val = msg.FindTag(14675718);
		if (val != null && (val.Value[0] != 0 || val.Value[1] != 0))
		{
			char c = (char)val.Value[0];
			int num = int.Parse(c.ToString());
			c = (char)val.Value[1];
			int num2 = int.Parse(c.ToString());
			int num3 = num * 10 + num2;
			context.ErrorCode = num3;
			HSMErrorCode source = (HSMErrorCode)num3;
			string text = DescriptionAttr<HSMErrorCode>(source);
			Logger.Log((LogLevel)4, text);
			throw new Exception(text);
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