using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using albahugin.Hugin.Common;
using Hugin.ExDevice;
using Hugin.GMPCommon;
using Newtonsoft.Json;

namespace Hugin.POS.CompactPrinter.FP300 { 

[Guid("DDA8C063-FA5F-4424-84DF-68AABADFC11C")]
[ClassInterface(ClassInterfaceType.None)]
[ComSourceInterfaces(typeof(ICompactPrinter))]
public class CompactPrinter : ICompactPrinter
    {
	private static ICompactPrinter printer = null;

	private static ISecureComm secureComm = null;

	private static DeviceInfo serverInfo = null;

	private static string fiscalId = "";

	private static string currentLog = "";

	private static int supportedBufferSize = 2048;

	private static bool onReporting = false;

	protected const int PRINTER_LINE_LENGTH = 48;

	private const byte SLIP_LINE_STYLE = 3;

	private const byte SLIP_LINE_ALIGN = 1;

	private static JSONDocument JSONDocAfterNoPaper = null;

	private static bool NoPaperFlag = false;

	private static int lastStatus = 1;

	public static ICompactPrinter Printer
	{
		get
		{
			if (printer == null)
			{
				printer = new CompactPrinter();
			}
			return printer;
		}
	}

	public string FiscalRegisterNo
	{
		get
		{
			return fiscalId;
		}
		set
		{
			fiscalId = value;
		}
	}

	public bool IsVx675 => secureComm.IsVx675;

	public int LogerLevel
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Expected I4, but got Unknown
			return (int)Logger.Level;
		}
		set
		{
			Logger.Level = (LogLevel)value;
		}
	}

	public string LogDirectory
	{
		get
		{
			return Logger.LogFileDirectory;
		}
		set
		{
			Logger.LogFileDirectory = value;
		}
	}

	public string LibraryVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();

	public int PrinterBufferSize => supportedBufferSize;

	protected bool isSecurecomVx675 => secureComm.IsVx675;

	protected int SecurecomConnTimeout
	{
		get
		{
			return secureComm.ConnTimeout;
		}
		set
		{
			secureComm.ConnTimeout = value;
		}
	}

	public event OnReportLineHandler OnReportLine = null;

	public event OnFileSendingProgressHandler OnFileSendingProgress = null;

	public string SetDepartment(int id, string name, int vatId, decimal price, int weighable)
	{
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Expected O, but got Unknown
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>
		{
			id.ToString(),
			name,
			vatId.ToString(),
			price.ToString(),
			weighable.ToString()
		});
		List<byte> list = new List<byte>();
		SFResponse sFResponse = null;
		list.AddRange(MessageBuilder.HexToByteArray(14675969));
		list.AddRange(MessageBuilder.AddLength(1));
		list.AddRange(MessageBuilder.ConvertIntToBCD(id, 1));
		list.AddRange(MessageBuilder.HexToByteArray(14675992));
		list.AddRange(MessageBuilder.AddLength(name.Length));
		list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(Utils.FixTurkishUpperCase(name)));
		list.AddRange(MessageBuilder.HexToByteArray(14675988));
		list.AddRange(MessageBuilder.AddLength(1));
		list.AddRange(MessageBuilder.ConvertIntToBCD(vatId, 1));
		price = TruncateDigitsAfterComma(price, 2);
		byte[] array = MessageBuilder.ConvertDecimalToBCD(price, 3);
		list.AddRange(MessageBuilder.HexToByteArray(14675972));
		list.AddRange(MessageBuilder.AddLength(array.Length));
		list.AddRange(array);
		list.AddRange(MessageBuilder.HexToByteArray(14675999));
		list.AddRange(MessageBuilder.AddLength(1));
		list.AddRange(MessageBuilder.ConvertIntToBCD(weighable, 1));
		FPUResponse val = Send(new FPURequest((Command)19, list.ToArray()));
		sFResponse = new SFResponse();
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		GMPMessage val2 = GMPMessage.Parse(val.Data);
		GMPGroup val3 = val2.FindGroup(57152);
		if (val3 != null)
		{
			GMPField val4 = val3.FindTag(14675992);
			if (val4 != null)
			{
				sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.DefaultEncoding.GetString(val4.Value));
			}
			val4 = val3.FindTag(14675988);
			if (val4 != null)
			{
				int num = MessageBuilder.ConvertBcdToInt(val4.Value, 0, 1);
				if (secureComm.IsVx675)
				{
					num++;
				}
				sFResponse.Add(SFResponseLabel.PARAM, num.ToString());
			}
			val4 = val3.FindTag(14675972);
			if (val4 != null)
			{
				price = (decimal)MessageBuilder.ConvertBcdToInt(val4.Value, 0, 5) / 1000m;
				sFResponse.Add(SFResponseLabel.PARAM, $"{price:#0.00}");
			}
			val4 = val3.FindTag(14675999);
			if (val4 != null)
			{
				weighable = 0;
				weighable = MessageBuilder.ConvertBcdToInt(val4.Value, 0, 1);
				sFResponse.Add(SFResponseLabel.PARAM, weighable.ToString());
			}
		}
		return sFResponse.GetString();
	}

	public string SetDepartment(int id, string name, int vatId, decimal price, int weighable, decimal limit)
	{
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Expected O, but got Unknown
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>
		{
			id.ToString(),
			name,
			vatId.ToString(),
			price.ToString(),
			weighable.ToString(),
			limit.ToString()
		});
		if (secureComm.GetVersion() < 4 || secureComm.IsVx675)
		{
			return SetDepartment(id, name, vatId, price, weighable);
		}
		List<byte> list = new List<byte>();
		SFResponse sFResponse = null;
		list.AddRange(MessageBuilder.HexToByteArray(14675969));
		list.AddRange(MessageBuilder.AddLength(1));
		list.AddRange(MessageBuilder.ConvertIntToBCD(id, 1));
		list.AddRange(MessageBuilder.HexToByteArray(14675992));
		list.AddRange(MessageBuilder.AddLength(name.Length));
		list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(Utils.FixTurkishUpperCase(name)));
		list.AddRange(MessageBuilder.HexToByteArray(14675988));
		list.AddRange(MessageBuilder.AddLength(1));
		list.AddRange(MessageBuilder.ConvertIntToBCD(vatId, 1));
		price = TruncateDigitsAfterComma(price, 2);
		byte[] array = MessageBuilder.ConvertDecimalToBCD(price, 3);
		list.AddRange(MessageBuilder.HexToByteArray(14675972));
		list.AddRange(MessageBuilder.AddLength(array.Length));
		list.AddRange(array);
		list.AddRange(MessageBuilder.HexToByteArray(14675999));
		list.AddRange(MessageBuilder.AddLength(1));
		list.AddRange(MessageBuilder.ConvertIntToBCD(weighable, 1));
		byte[] array2 = MessageBuilder.ConvertDecimalToBCD(limit, 3);
		list.AddRange(MessageBuilder.HexToByteArray(14676037));
		list.AddRange(MessageBuilder.AddLength(array2.Length));
		list.AddRange(array2);
		FPUResponse val = Send(new FPURequest((Command)19, list.ToArray()));
		sFResponse = new SFResponse();
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		GMPMessage val2 = GMPMessage.Parse(val.Data);
		GMPGroup val3 = val2.FindGroup(57152);
		if (val3 != null)
		{
			GMPField val4 = val3.FindTag(14675992);
			if (val4 != null)
			{
				sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.DefaultEncoding.GetString(val4.Value));
			}
			val4 = val3.FindTag(14675988);
			if (val4 != null)
			{
				sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.ConvertBcdToInt(val4.Value, 0, 1).ToString());
			}
			val4 = val3.FindTag(14675972);
			if (val4 != null)
			{
				price = (decimal)MessageBuilder.ConvertBcdToInt(val4.Value, 0, 5) / 1000m;
				sFResponse.Add(SFResponseLabel.PARAM, $"{price:#0.00}");
			}
			val4 = val3.FindTag(14675999);
			if (val4 != null)
			{
				weighable = 0;
				weighable = MessageBuilder.ConvertBcdToInt(val4.Value, 0, 1);
				sFResponse.Add(SFResponseLabel.PARAM, weighable.ToString());
			}
			val4 = val3.FindTag(14676037);
			if (val4 != null)
			{
				limit = (decimal)MessageBuilder.ConvertBcdToInt(val4.Value, 0, 5) / 1000m;
				sFResponse.Add(SFResponseLabel.PARAM, $"{limit:#0.00}");
			}
		}
		return sFResponse.GetString();
	}

	public string GetDepartment(int deptId)
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Expected O, but got Unknown
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string> { deptId.ToString() });
		SFResponse sFResponse = new SFResponse();
		List<byte> list = new List<byte>();
		list.AddRange(MessageBuilder.HexToByteArray(14675969));
		list.AddRange(MessageBuilder.AddLength(1));
		list.AddRange(MessageBuilder.ConvertIntToBCD(deptId, 1));
		FPUResponse val = Send(new FPURequest((Command)19, list.ToArray()));
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		if (val.ErrorCode != 0)
		{
			return sFResponse.GetString();
		}
		GMPMessage val2 = GMPMessage.Parse(val.Data);
		GMPGroup val3 = val2.FindGroup(57152);
		if (val3 != null)
		{
			GMPField val4 = val3.FindTag(14675992);
			if (val4 != null)
			{
				sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.DefaultEncoding.GetString(val4.Value));
			}
			val4 = val3.FindTag(14675988);
			if (val4 != null)
			{
				int num = MessageBuilder.ConvertBcdToInt(val4.Value, 0, 1);
				if (secureComm.IsVx675)
				{
					num++;
				}
				sFResponse.Add(SFResponseLabel.PARAM, num.ToString());
			}
			val4 = val3.FindTag(14675972);
			if (val4 != null)
			{
				decimal num2 = (decimal)MessageBuilder.ConvertBcdToInt(val4.Value, 0, 5) / 1000m;
				sFResponse.Add(SFResponseLabel.PARAM, $"{num2:#0.00}");
			}
			val4 = val3.FindTag(14675999);
			if (val4 != null)
			{
				int num3 = 0;
				sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.ConvertBcdToInt(val4.Value, 0, 1).ToString());
			}
			val4 = val3.FindTag(14676037);
			if (val4 != null)
			{
				decimal num4 = (decimal)MessageBuilder.ConvertBcdToInt(val4.Value, 0, 5) / 1000m;
				sFResponse.Add(SFResponseLabel.PARAM, $"{num4:#0.00}");
			}
		}
		return sFResponse.GetString();
	}

	public string SetCreditInfo(int id, string name)
	{
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Expected O, but got Unknown
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>
		{
			id.ToString(),
			name
		});
		SFResponse sFResponse = null;
		List<byte> list = new List<byte>();
		list.AddRange(MessageBuilder.HexToByteArray(14675984));
		list.AddRange(MessageBuilder.AddLength(1));
		list.Add((byte)id);
		if (!string.IsNullOrEmpty(name))
		{
			list.AddRange(MessageBuilder.HexToByteArray(14675992));
			list.AddRange(MessageBuilder.AddLength(15));
			name = name.PadRight(15, ' ');
			list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(name));
		}
		FPUResponse val = Send(new FPURequest((Command)21, list.ToArray()));
		sFResponse = new SFResponse();
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		if (val.ErrorCode == 0)
		{
			GMPMessage val2 = GMPMessage.Parse(val.Data);
			GMPGroup val3 = val2.FindGroup(57152);
			GMPField val4 = val3.FindTag(14675992);
			name = GetString(val4.Value);
			sFResponse.Add(SFResponseLabel.PARAM, name);
		}
		return sFResponse.GetString();
	}

	private string GetString(byte[] bytes)
	{
		return MessageBuilder.DefaultEncoding.GetString(bytes).TrimEnd(new char[1]);
	}

	public string GetCreditInfo(int id)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string> { id.ToString() });
		SFResponse sFResponse = null;
		List<byte> list = new List<byte>();
		list.AddRange(MessageBuilder.HexToByteArray(14675984));
		list.AddRange(MessageBuilder.AddLength(1));
		list.Add((byte)id);
		FPUResponse val = Send(new FPURequest((Command)21, list.ToArray()));
		sFResponse = new SFResponse();
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		if (val.ErrorCode == 0)
		{
			GMPMessage val2 = GMPMessage.Parse(val.Data);
			GMPGroup val3 = val2.FindGroup(57152);
			if (val3 != null)
			{
				GMPField val4 = val3.FindTag(14675992);
				if (val4 != null)
				{
					string @string = MessageBuilder.DefaultEncoding.GetString(val4.Value);
					sFResponse.Add(SFResponseLabel.PARAM, @string);
				}
			}
		}
		return sFResponse.GetString();
	}

	public string SetCurrencyInfo(int id, string name, decimal exchangeRate)
	{
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Expected O, but got Unknown
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>
		{
			id.ToString(),
			name,
			exchangeRate.ToString()
		});
		SFResponse sFResponse = new SFResponse();
		List<byte> list = new List<byte>();
		list.AddRange(MessageBuilder.HexToByteArray(14675984));
		list.AddRange(MessageBuilder.AddLength(1));
		list.Add((byte)id);
		list.AddRange(MessageBuilder.HexToByteArray(14675992));
		list.AddRange(MessageBuilder.AddLength(15));
		name = name.PadRight(15, ' ');
		list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(name));
		byte[] array = MessageBuilder.ConvertDecimalToBCD(exchangeRate, 4);
		list.AddRange(MessageBuilder.HexToByteArray(14675972));
		list.AddRange(MessageBuilder.AddLength(array.Length));
		list.AddRange(array);
		FPUResponse val = Send(new FPURequest((Command)26, list.ToArray()));
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		if (val.ErrorCode == 0)
		{
			GMPMessage val2 = GMPMessage.Parse(val.Data);
			GMPGroup val3 = val2.FindGroup(57152);
			if (val3 != null)
			{
				GMPField val4 = val3.FindTag(14675992);
				if (val4 != null)
				{
					string @string = MessageBuilder.DefaultEncoding.GetString(val4.Value);
					sFResponse.Add(SFResponseLabel.PARAM, @string);
				}
				val4 = val3.FindTag(14675972);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, ((decimal)MessageBuilder.ConvertBcdToInt(val4.Value, 0, 5) / 10000m).ToString());
				}
			}
		}
		return sFResponse.GetString();
	}

	public string GetCurrencyInfo(int index)
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Expected O, but got Unknown
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string> { index.ToString() });
		SFResponse sFResponse = new SFResponse();
		List<byte> list = new List<byte>();
		list.AddRange(MessageBuilder.HexToByteArray(14675984));
		list.AddRange(MessageBuilder.AddLength(1));
		list.Add((byte)index);
		FPUResponse val = Send(new FPURequest((Command)26, list.ToArray()));
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		if (val.ErrorCode == 0)
		{
			GMPMessage val2 = GMPMessage.Parse(val.Data);
			GMPGroup val3 = val2.FindGroup(57152);
			if (val3 != null)
			{
				GMPField val4 = val3.FindTag(14675992);
				if (val4 != null)
				{
					string @string = MessageBuilder.DefaultEncoding.GetString(val4.Value);
					sFResponse.Add(SFResponseLabel.PARAM, @string);
				}
				val4 = val3.FindTag(14675972);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, ((decimal)MessageBuilder.ConvertBcdToInt(val4.Value, 0, 5) / 10000m).ToString());
				}
			}
		}
		return sFResponse.GetString();
	}

	public string SetMainCategory(int id, string name)
	{
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>
		{
			id.ToString(),
			name
		});
		SFResponse sFResponse = new SFResponse();
		List<byte> list = new List<byte>();
		list.AddRange(MessageBuilder.HexToByteArray(14675989));
		list.AddRange(MessageBuilder.AddLength(1));
		list.AddRange(MessageBuilder.ConvertIntToBCD(id + 1, 1));
		list.AddRange(MessageBuilder.HexToByteArray(14675992));
		list.AddRange(MessageBuilder.AddLength(name.Length));
		list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(Utils.FixTurkishUpperCase(name)));
		FPUResponse val = Send(new FPURequest((Command)22, list.ToArray()));
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		GMPMessage val2 = GMPMessage.Parse(val.Data);
		GMPGroup val3 = val2.FindGroup(57152);
		if (val3 != null)
		{
			GMPField val4 = val3.FindTag(14675992);
			if (val4 != null)
			{
				sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.DefaultEncoding.GetString(val4.Value));
			}
		}
		return sFResponse.GetString();
	}

	public string GetMainCategory(int mainCatId)
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected O, but got Unknown
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string> { mainCatId.ToString() });
		SFResponse sFResponse = new SFResponse();
		List<byte> list = new List<byte>();
		list.AddRange(MessageBuilder.HexToByteArray(14675989));
		list.AddRange(MessageBuilder.AddLength(1));
		list.AddRange(MessageBuilder.ConvertIntToBCD(mainCatId + 1, 1));
		FPUResponse val = Send(new FPURequest((Command)22, list.ToArray()));
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		if (val.ErrorCode == 0)
		{
			sFResponse.Add(SFResponseLabel.PARAM, mainCatId.ToString());
			GMPMessage val2 = GMPMessage.Parse(val.Data);
			GMPGroup val3 = val2.FindGroup(57152);
			if (val3 != null)
			{
				GMPField val4 = val3.FindTag(14675992);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.DefaultEncoding.GetString(val4.Value));
				}
			}
		}
		return sFResponse.GetString();
	}

	public string SetSubCategory(int id, string name, int mainCatId)
	{
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Expected O, but got Unknown
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>
		{
			id.ToString(),
			name,
			mainCatId.ToString()
		});
		SFResponse sFResponse = new SFResponse();
		List<byte> list = new List<byte>();
		list.AddRange(MessageBuilder.HexToByteArray(14675990));
		list.AddRange(MessageBuilder.AddLength(1));
		list.AddRange(MessageBuilder.ConvertIntToBCD(id + 1, 1));
		list.AddRange(MessageBuilder.HexToByteArray(14675992));
		list.AddRange(MessageBuilder.AddLength(name.Length));
		list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(Utils.FixTurkishUpperCase(name)));
		list.AddRange(MessageBuilder.HexToByteArray(14675989));
		list.AddRange(MessageBuilder.AddLength(1));
		list.AddRange(MessageBuilder.ConvertIntToBCD(mainCatId, 1));
		FPUResponse val = Send(new FPURequest((Command)23, list.ToArray()));
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		GMPMessage val2 = GMPMessage.Parse(val.Data);
		GMPGroup val3 = val2.FindGroup(57152);
		if (val3 != null)
		{
			GMPField val4 = val3.FindTag(14675992);
			if (val4 != null)
			{
				string @string = MessageBuilder.DefaultEncoding.GetString(val4.Value);
				sFResponse.Add(SFResponseLabel.PARAM, @string);
			}
			val4 = val3.FindTag(14675989);
			if (val4 != null)
			{
				sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.ConvertBcdToInt(val4.Value, 0, 1).ToString());
			}
		}
		return sFResponse.GetString();
	}

	public string GetSubCategory(int subCatId)
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected O, but got Unknown
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string> { subCatId.ToString() });
		SFResponse sFResponse = new SFResponse();
		List<byte> list = new List<byte>();
		list.AddRange(MessageBuilder.HexToByteArray(14675990));
		list.AddRange(MessageBuilder.AddLength(1));
		list.AddRange(MessageBuilder.ConvertIntToBCD(subCatId + 1, 1));
		FPUResponse val = Send(new FPURequest((Command)23, list.ToArray()));
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		if (val.ErrorCode == 0)
		{
			sFResponse.Add(SFResponseLabel.PARAM, subCatId.ToString());
			GMPMessage val2 = GMPMessage.Parse(val.Data);
			GMPGroup val3 = val2.FindGroup(57152);
			if (val3 != null)
			{
				GMPField val4 = val3.FindTag(14675992);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.DefaultEncoding.GetString(val4.Value));
				}
				val4 = val3.FindTag(14675989);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.ConvertBcdToInt(val4.Value, 0, 1).ToString());
				}
			}
		}
		return sFResponse.GetString();
	}

	public string SaveCashier(int id, string name, string password)
	{
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Expected O, but got Unknown
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>
		{
			id.ToString(),
			name,
			password
		});
		SFResponse sFResponse = new SFResponse();
		List<byte> list = new List<byte>();
		list.AddRange(MessageBuilder.HexToByteArray(14675979));
		list.AddRange(MessageBuilder.AddLength(1));
		list.AddRange(MessageBuilder.ConvertIntToBCD(id, 1));
		list.AddRange(MessageBuilder.HexToByteArray(14675992));
		list.AddRange(MessageBuilder.AddLength(name.Length));
		list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(name));
		if (!string.IsNullOrEmpty(password))
		{
			int num = Convert.ToInt32(password);
			list.AddRange(MessageBuilder.HexToByteArray(14675994));
			list.AddRange(MessageBuilder.AddLength(3));
			list.AddRange(MessageBuilder.ConvertIntToBCD(num, 3));
		}
		FPUResponse val = Send(new FPURequest((Command)24, list.ToArray()));
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		GMPMessage val2 = GMPMessage.Parse(val.Data);
		GMPGroup val3 = val2.FindGroup(57152);
		if (val3 != null)
		{
			GMPField val4 = val3.FindTag(14675992);
			if (val4 != null)
			{
				string @string = MessageBuilder.DefaultEncoding.GetString(val4.Value);
				sFResponse.Add(SFResponseLabel.PARAM, @string);
			}
		}
		return sFResponse.GetString();
	}

	public string GetCashier(int cashierId)
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Expected O, but got Unknown
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string> { cashierId.ToString() });
		SFResponse sFResponse = new SFResponse();
		List<byte> list = new List<byte>();
		list.AddRange(MessageBuilder.HexToByteArray(14675979));
		list.AddRange(MessageBuilder.AddLength(1));
		list.AddRange(MessageBuilder.ConvertIntToBCD(cashierId, 1));
		FPUResponse val = Send(new FPURequest((Command)24, list.ToArray()));
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		if (val.ErrorCode == 0)
		{
			GMPMessage val2 = GMPMessage.Parse(val.Data);
			GMPGroup val3 = val2.FindGroup(57152);
			if (val3 != null)
			{
				GMPField val4 = val3.FindTag(14675992);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.DefaultEncoding.GetString(val4.Value));
				}
			}
		}
		return sFResponse.GetString();
	}

	public string SignInCashier(int id, string password)
	{
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>
		{
			id.ToString(),
			password
		});
		List<byte> list = new List<byte>();
		list.AddRange(MessageBuilder.HexToByteArray(14675979));
		list.AddRange(MessageBuilder.AddLength(1));
		list.AddRange(MessageBuilder.ConvertIntToBCD(id, 1));
		list.AddRange(MessageBuilder.HexToByteArray(14675994));
		list.AddRange(MessageBuilder.AddLength(password.Length));
		list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(password));
		byte[] data = list.ToArray();
		return SendCommand((Command)135, data);
	}

	public string CheckCashierIsValid(int id, string password)
	{
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>
		{
			id.ToString(),
			password
		});
		List<byte> list = new List<byte>();
		list.AddRange(MessageBuilder.HexToByteArray(14675979));
		list.AddRange(MessageBuilder.AddLength(1));
		list.AddRange(MessageBuilder.ConvertIntToBCD(id, 1));
		list.AddRange(MessageBuilder.HexToByteArray(14675994));
		list.AddRange(MessageBuilder.AddLength(password.Length));
		list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(password));
		list.AddRange(MessageBuilder.HexToByteArray(14675999));
		list.AddRange(MessageBuilder.AddLength(1));
		list.AddRange(MessageBuilder.ConvertIntToBCD(1, 1));
		return SendCommand((Command)135, list.ToArray());
	}

	public string GetLogo(int index)
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Expected O, but got Unknown
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string> { index.ToString() });
		SFResponse sFResponse = new SFResponse();
		List<byte> list = new List<byte>();
		list.AddRange(MessageBuilder.HexToByteArray(14675984));
		list.AddRange(MessageBuilder.AddLength(1));
		list.Add((byte)index);
		FPUResponse val = Send(new FPURequest((Command)17, list.ToArray()));
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		if (val.ErrorCode == 0)
		{
			GMPMessage val2 = GMPMessage.Parse(val.Data);
			GMPGroup val3 = val2.FindGroup(57152);
			if (val3 != null)
			{
				GMPField val4 = val3.FindTag(14675992);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.DefaultEncoding.GetString(val4.Value).Trim());
				}
			}
		}
		return sFResponse.GetString();
	}

	public string SetLogo(int index, string line)
	{
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Expected O, but got Unknown
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>
		{
			index.ToString(),
			line
		});
		SFResponse sFResponse = new SFResponse();
		List<byte> list = new List<byte>();
		list.AddRange(MessageBuilder.HexToByteArray(14675984));
		list.AddRange(MessageBuilder.AddLength(1));
		list.Add((byte)index);
		list.AddRange(MessageBuilder.HexToByteArray(14675992));
		list.AddRange(MessageBuilder.AddLength(line.Trim().Length));
		list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(line.Trim()));
		FPUResponse val = Send(new FPURequest((Command)17, list.ToArray()));
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		return sFResponse.GetString();
	}

	public DateTime GetDateTime()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>());
		DateTime result = DateTime.MinValue;
		try
		{
			FPUResponse val = Send(new FPURequest((Command)129, new byte[0]));
			GMPMessage val2 = GMPMessage.Parse(val.Data);
			GMPGroup val3 = val2.FindGroup(57152);
			GMPField val4 = val3.FindTag(14647817);
			if (val4 != null)
			{
				result = MessageBuilder.ConvertBytesToDate(val4.Value, 0);
			}
			val4 = val3.FindTag(14647818);
			if (val4 != null)
			{
				DateTime dateTime = MessageBuilder.ConvertBytesToTime(val4.Value, 0);
				result = new DateTime(result.Year, result.Month, result.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);
			}
		}
		catch
		{
		}
		return result;
	}

	public string SetDateTime(DateTime date, DateTime time)
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>
		{
			date.ToString(),
			time.ToString()
		});
		List<byte> list = new List<byte>();
		if (date != DateTime.MinValue)
		{
			list.AddRange(MessageBuilder.GetDateInBytes(date));
		}
		if (time != DateTime.MinValue)
		{
			list.AddRange(MessageBuilder.GetTimeInBytes(time));
		}
		FPUResponse val = Send(new FPURequest((Command)98, list.ToArray()));
		SFResponse sFResponse = new SFResponse();
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		return sFResponse.GetString();
	}

	public string GetVATRate(int index)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string> { index.ToString() });
		List<byte> list = new List<byte>();
		list.AddRange(MessageBuilder.HexToByteArray(14675988));
		list.AddRange(MessageBuilder.AddLength(1));
		list.Add((byte)(index + 1));
		FPUResponse val = Send(new FPURequest((Command)18, list.ToArray()));
		SFResponse sFResponse = new SFResponse();
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		if (val.ErrorCode == 0)
		{
			GMPMessage val2 = GMPMessage.Parse(val.Data);
			GMPGroup val3 = val2.FindGroup(57152);
			if (val3 != null)
			{
				GMPField val4 = val3.FindTag(14675985);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.ConvertBcdToInt(val4.Value, 0, 1).ToString());
				}
			}
		}
		return sFResponse.GetString();
	}

	public string SetVATRate(int index, decimal taxRate)
	{
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Expected O, but got Unknown
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>
		{
			index.ToString(),
			taxRate.ToString()
		});
		List<byte> list = new List<byte>();
		list.AddRange(MessageBuilder.HexToByteArray(14675988));
		list.AddRange(MessageBuilder.AddLength(1));
		list.Add((byte)(index + 1));
		int num = Convert.ToInt32(taxRate);
		if ((decimal)num == -1m)
		{
			num = 255;
		}
		list.AddRange(MessageBuilder.HexToByteArray(14675985));
		list.AddRange(MessageBuilder.AddLength(2));
		list.AddRange(MessageBuilder.ConvertIntToBCD(num, 2));
		FPUResponse val = Send(new FPURequest((Command)18, list.ToArray()));
		SFResponse sFResponse = new SFResponse();
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		if (val.ErrorCode == 0)
		{
			GMPMessage val2 = GMPMessage.Parse(val.Data);
			GMPGroup val3 = val2.FindGroup(57152);
			if (val3 != null)
			{
				GMPField val4 = val3.FindTag(14675985);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.ConvertBcdToInt(val4.Value, 0, 1).ToString());
				}
			}
		}
		return sFResponse.GetString();
	}

	public string SaveProduct(int productId, string productName, int deptId, decimal price, int weighable, string barcode, int subCatId)
	{
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Expected O, but got Unknown
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>
		{
			productId.ToString(),
			productName,
			deptId.ToString(),
			price.ToString(),
			weighable.ToString(),
			barcode,
			subCatId.ToString()
		});
		SFResponse sFResponse = new SFResponse();
		List<byte> list = new List<byte>();
		int num = Convert.ToInt32(productId);
		list.AddRange(MessageBuilder.HexToByteArray(14675970));
		list.AddRange(MessageBuilder.AddLength(3));
		list.AddRange(MessageBuilder.ConvertIntToBCD(num, 3));
		list.AddRange(MessageBuilder.HexToByteArray(14675992));
		list.AddRange(MessageBuilder.AddLength(productName.Length));
		list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(Utils.FixTurkishUpperCase(productName)));
		list.AddRange(MessageBuilder.HexToByteArray(14675969));
		list.AddRange(MessageBuilder.AddLength(1));
		list.AddRange(MessageBuilder.ConvertIntToBCD(deptId, 1));
		price = TruncateDigitsAfterComma(price, 2);
		byte[] array = MessageBuilder.ConvertDecimalToBCD(price, 3);
		list.AddRange(MessageBuilder.HexToByteArray(14675972));
		list.AddRange(MessageBuilder.AddLength(array.Length));
		list.AddRange(array);
		list.AddRange(MessageBuilder.HexToByteArray(14675999));
		list.AddRange(MessageBuilder.AddLength(1));
		list.AddRange(MessageBuilder.ConvertIntToBCD(weighable, 1));
		if (!string.IsNullOrEmpty(barcode))
		{
			list.AddRange(MessageBuilder.HexToByteArray(14675980));
			list.AddRange(MessageBuilder.AddLength(barcode.Length));
			list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(barcode));
		}
		list.AddRange(MessageBuilder.HexToByteArray(14675990));
		list.AddRange(MessageBuilder.AddLength(1));
		list.AddRange(MessageBuilder.ConvertIntToBCD(subCatId, 1));
		FPUResponse val = Send(new FPURequest((Command)20, list.ToArray()));
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		if (val.ErrorCode == 0)
		{
			GMPMessage val2 = GMPMessage.Parse(val.Data);
			GMPGroup val3 = val2.FindGroup(57152);
			if (val3 != null)
			{
				GMPField val4 = val3.FindTag(14675992);
				if (val4 != null)
				{
					string @string = MessageBuilder.DefaultEncoding.GetString(val4.Value);
					sFResponse.Add(SFResponseLabel.PARAM, @string);
				}
				val4 = val3.FindTag(14675969);
				if (val4 != null)
				{
					deptId = MessageBuilder.ConvertBcdToInt(val4.Value, 0, 1);
					deptId = ((deptId <= 8) ? (deptId + 1) : 0);
					sFResponse.Add(SFResponseLabel.PARAM, deptId.ToString());
				}
				val4 = val3.FindTag(14675972);
				if (val4 != null)
				{
					price = (decimal)MessageBuilder.ConvertBcdToInt(val4.Value, 0, 5) / 1000m;
					sFResponse.Add(SFResponseLabel.PARAM, price.ToString());
				}
				val4 = val3.FindTag(14675999);
				if (val4 != null)
				{
					weighable = MessageBuilder.ConvertBcdToInt(val4.Value, 0, 1);
					sFResponse.Add(SFResponseLabel.PARAM, weighable.ToString());
				}
				val4 = val3.FindTag(14675980);
				if (val4 != null)
				{
					barcode = MessageBuilder.DefaultEncoding.GetString(val4.Value);
					sFResponse.Add(SFResponseLabel.PARAM, barcode);
				}
				val4 = val3.FindTag(14675990);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, (MessageBuilder.ConvertBcdToInt(val4.Value, 0, 1) + 1).ToString());
				}
			}
		}
		return sFResponse.GetString();
	}

	public string GetProduct(int pluNo)
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Expected O, but got Unknown
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string> { pluNo.ToString() });
		SFResponse sFResponse = new SFResponse();
		List<byte> list = new List<byte>();
		list.AddRange(MessageBuilder.HexToByteArray(14675970));
		list.AddRange(MessageBuilder.AddLength(3));
		list.AddRange(MessageBuilder.ConvertIntToBCD(pluNo, 3));
		FPUResponse val = Send(new FPURequest((Command)20, list.ToArray()));
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		if (val.ErrorCode == 0)
		{
			GMPMessage val2 = GMPMessage.Parse(val.Data);
			GMPGroup val3 = val2.FindGroup(57152);
			if (val3 != null)
			{
				GMPField val4 = val3.FindTag(14675992);
				if (val4 != null)
				{
					string @string = MessageBuilder.DefaultEncoding.GetString(val4.Value);
					sFResponse.Add(SFResponseLabel.PARAM, @string);
				}
				val4 = val3.FindTag(14675969);
				if (val4 != null)
				{
					int num = MessageBuilder.ConvertBcdToInt(val4.Value, 0, 1);
					sFResponse.Add(SFResponseLabel.PARAM, ((num <= 8) ? (num + 1) : 0).ToString());
				}
				val4 = val3.FindTag(14675972);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, ((decimal)MessageBuilder.ConvertBcdToInt(val4.Value, 0, 5) / 1000m).ToString());
				}
				val4 = val3.FindTag(14675999);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.ConvertBcdToInt(val4.Value, 0, 1).ToString());
				}
				val4 = val3.FindTag(14675980);
				if (val4 != null)
				{
					string string2 = MessageBuilder.DefaultEncoding.GetString(val4.Value);
					sFResponse.Add(SFResponseLabel.PARAM, string2);
				}
				val4 = val3.FindTag(14675990);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, (MessageBuilder.ConvertBcdToInt(val4.Value, 0, 1) + 1).ToString());
				}
			}
		}
		return sFResponse.GetString();
	}

	public string SaveGMPConnectionInfo(string ip, int port)
	{
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Expected O, but got Unknown
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>
		{
			ip,
			port.ToString()
		});
		FPUResponse val = null;
		SFResponse sFResponse = new SFResponse();
		List<byte> list = new List<byte>();
		try
		{
			list.AddRange(MessageBuilder.HexToByteArray(14663941));
			list.AddRange(MessageBuilder.AddLength(6));
			for (int i = 0; i < ip.Length / 2; i++)
			{
				list.AddRange(MessageBuilder.ConvertIntToBCD(int.Parse(ip.Substring(i * 2, 2)), 1));
			}
		}
		catch (Exception ex)
		{
			throw ex;
		}
		byte[] array = MessageBuilder.ConvertIntToBCD(port, port.ToString().Length);
		list.AddRange(MessageBuilder.HexToByteArray(14676004));
		list.AddRange(MessageBuilder.AddLength(array.Length));
		list.AddRange(array);
		int connTimeout = secureComm.ConnTimeout;
		secureComm.ConnTimeout = 60000;
		try
		{
			val = Send(new FPURequest((Command)27, list.ToArray()));
		}
		finally
		{
			secureComm.ConnTimeout = connTimeout;
		}
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		return sFResponse.GetString();
	}

	public string LoadGraphicLogo(System.Drawing.Image imageObj, int index = 0)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Expected O, but got Unknown
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>());
		SFResponse sFResponse = new SFResponse();
		FPUResponse val = null;
		int num = secureComm.BufferSize - 250;
		Bitmap val2 = new Bitmap(imageObj);
		Rectangle rectangle = new Rectangle(0, 0, ((System.Drawing.Image)val2).Width, ((System.Drawing.Image)val2).Height);
		Bitmap val3 = val2.Clone(rectangle, (PixelFormat)196865);
		byte[] array;
		using (MemoryStream memoryStream = new MemoryStream())
		{
			((System.Drawing.Image)val3).Save((Stream)memoryStream, ImageFormat.Bmp);
			array = memoryStream.ToArray();
		}
		List<byte> list = null;
		int num2 = (int)Math.Round((decimal)array.Length / (decimal)num, 0, MidpointRounding.AwayFromZero);
		for (int i = 1; i <= num2; i++)
		{
			int num3 = num;
			list = new List<byte>();
			if (num2 == i)
			{
				num3 = Math.Min(num, array.Length - (i - 1) * num);
			}
			byte[] array2 = new byte[num3];
			Buffer.BlockCopy(array, (i - 1) * num, array2, 0, num3);
			list.AddRange(MessageBuilder.HexToByteArray(14675984));
			list.AddRange(MessageBuilder.AddLength(1));
			list.Add((byte)index);
			list.AddRange(MessageBuilder.HexToByteArray(14675975));
			list.AddRange(MessageBuilder.AddLength(1));
			list.Add((byte)num2);
			list.AddRange(MessageBuilder.HexToByteArray(14675974));
			list.AddRange(MessageBuilder.AddLength(1));
			list.Add((byte)i);
			list.AddRange(MessageBuilder.HexToByteArray(14676007));
			list.AddRange(MessageBuilder.AddLength(array2.Length));
			list.AddRange(array2);
			int connTimeout = secureComm.ConnTimeout;
			secureComm.ConnTimeout = 60000;
			try
			{
				val = Send(new FPURequest((Command)28, list.ToArray()));
			}
			finally
			{
				secureComm.ConnTimeout = connTimeout;
			}
			sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
			sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
			if (val.ErrorCode == 0)
			{
			}
		}
		return sFResponse.GetString();
	}

	public string GetProgramOptions(int progEnum)
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Expected O, but got Unknown
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string> { progEnum.ToString() });
		SFResponse sFResponse = new SFResponse();
		List<byte> list = new List<byte>();
		string text = Convert.ToString(progEnum + 1);
		list.AddRange(MessageBuilder.HexToByteArray(14675997));
		list.AddRange(MessageBuilder.AddLength(text.Length));
		list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(text));
		FPUResponse val = Send(new FPURequest((Command)25, list.ToArray()));
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		if (val.ErrorCode == 0)
		{
			ProgramOption programOption = ParsePrmOption(val.Data);
			Settings settings = (Settings)progEnum;
			if (settings == Settings.RECEIPT_LIMIT)
			{
				if (!string.IsNullOrEmpty(programOption.Value))
				{
					programOption.Value = $"{Convert.ToDecimal(programOption.Value) / 1000m}";
				}
				sFResponse.Add(SFResponseLabel.PARAM, $"{programOption.Value:#0.00}");
			}
			else
			{
				sFResponse.Add(SFResponseLabel.PARAM, programOption.Value);
			}
		}
		return sFResponse.GetString();
	}

	public string SaveProgramOptions(int progEnum, string progValue)
	{
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Expected O, but got Unknown
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>
		{
			progEnum.ToString(),
			progValue
		});
		SFResponse sFResponse = new SFResponse();
		List<byte> list = new List<byte>();
		string text = Convert.ToString(progEnum + 1);
		list.AddRange(MessageBuilder.HexToByteArray(14675997));
		list.AddRange(MessageBuilder.AddLength(text.Length));
		list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(text));
		string text2 = progValue;
		Settings settings = (Settings)progEnum;
		if (settings == Settings.RECEIPT_LIMIT)
		{
			string[] array = text2.Split(new char[1] { ',' });
			text2 = ((array.Length <= 1) ? $"{int.Parse(array[0]) * 1000}" : $"{array[0]}{array[1].PadRight(3, '0')}");
		}
		list.AddRange(MessageBuilder.HexToByteArray(14675998));
		list.AddRange(MessageBuilder.AddLength(text2.Length));
		list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(text2));
		FPUResponse val = Send(new FPURequest((Command)25, list.ToArray()));
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		if (val.ErrorCode == 0)
		{
			ProgramOption programOption = ParsePrmOption(val.Data);
			Settings settings2 = (Settings)progEnum;
			if (settings2 == Settings.RECEIPT_LIMIT)
			{
				if (!string.IsNullOrEmpty(programOption.Value))
				{
					programOption.Value = $"{Convert.ToDecimal(programOption.Value) / 1000m}";
				}
				sFResponse.Add(SFResponseLabel.PARAM, $"{programOption.Value:#0.00}");
			}
			else
			{
				sFResponse.Add(SFResponseLabel.PARAM, programOption.Value);
			}
		}
		return sFResponse.GetString();
	}

	public string SendMultipleProduct(string[] productLines)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected O, but got Unknown
		//IL_048f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0499: Expected O, but got Unknown
		//IL_04c3: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>());
		string text = "PRODUCT_DB";
		if (File.Exists(text))
		{
			File.Delete(text);
		}
		File.Copy("etc/PRODUCT_DB_BLANK", text, overwrite: true);
		SQLiteConnection val = new SQLiteConnection("Data Source=" + text + ";Version=3;");
		try
		{
			((DbConnection)(object)val).Open();
			int num = 1;
			SQLiteCommand val2 = val.CreateCommand();
			try
			{
				((DbCommand)(object)val2).CommandText = "begin";
				((DbCommand)(object)val2).ExecuteNonQuery();
				foreach (string text2 in productLines)
				{
					int num2 = 0;
					int num3 = 0;
					string empty = string.Empty;
					string empty2 = string.Empty;
					string empty3 = string.Empty;
					string empty4 = string.Empty;
					string empty5 = string.Empty;
					string empty6 = string.Empty;
					string empty7 = string.Empty;
					string empty8 = string.Empty;
					try
					{
						num3 = 1;
						empty = text2.Substring(num2, num3);
						num2 += num3;
						num3 = 6;
						empty2 = text2.Substring(num2, num3);
						num2 += num3;
						num3 = 20;
						empty3 = text2.Substring(num2, num3);
						num2 += num3;
						num3 = 20;
						empty4 = text2.Substring(num2, num3);
						num2 += num3;
						num3 = 9;
						empty5 = text2.Substring(num2, num3);
						num2 += num3;
						num3 = 2;
						empty6 = text2.Substring(num2, num3);
						num2 += num3;
						num3 = 4;
						empty7 = text2.Substring(num2, num3);
						num2 += num3;
						num3 = 1;
						empty8 = text2.Substring(num2, num3);
						num2 += num3;
					}
					catch
					{
						continue;
					}
					int value = 0;
					if (empty8 == "E")
					{
						value = 1;
					}
					((DbCommand)(object)val2).CommandText = $"INSERT INTO PLU (OID, PluNo, Name, Price, DeptCode, Barcode, Weightable, SubCategory, OwnerFlag) values ({num}, {num}, '{empty4}', {(Convert.ToDecimal(empty5) * 1000m).ToString()}, {int.Parse(empty6)}, '{empty3}', {Convert.ToInt32(value)}, {0}, {0})";
					((DbCommand)(object)val2).ExecuteNonQuery();
					int num4 = Convert.ToInt32((decimal)(num * 100 / productLines.Length));
					this.OnFileSendingProgress(this, new OnFileSendingProgressEventArgs(num4.ToString()));
					num++;
				}
				((DbCommand)(object)val2).CommandText = "end";
				((DbCommand)(object)val2).ExecuteNonQuery();
				((DbConnection)(object)val).Close();
				SQLiteConnection.ClearAllPools();
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		SFResponse sFResponse = new SFResponse();
		FPUResponse val3 = null;
		int num5 = secureComm.BufferSize - 250;
		byte[] array = File.ReadAllBytes(text);
		List<byte> list = null;
		int num6 = (int)Math.Round((decimal)array.Length / (decimal)num5, 0, MidpointRounding.AwayFromZero);
		for (int j = 1; j <= num6; j++)
		{
			int num7 = num5;
			list = new List<byte>();
			if (num6 == j)
			{
				num7 = Math.Min(num5, array.Length - (j - 1) * num5);
			}
			byte[] array2 = new byte[num7];
			Buffer.BlockCopy(array, (j - 1) * num5, array2, 0, num7);
			int num8 = 0;
			list.AddRange(MessageBuilder.HexToByteArray(57211));
			num8 = list.Count;
			if (j == 1)
			{
				list.AddRange(MessageBuilder.HexToByteArray(14675976));
				list.AddRange(MessageBuilder.AddLength(text.Length));
				list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(text));
				list.AddRange(MessageBuilder.HexToByteArray(14675975));
				list.AddRange(MessageBuilder.AddLength(3));
				list.AddRange(MessageBuilder.ConvertIntToBCD(num6, 3));
			}
			list.AddRange(MessageBuilder.HexToByteArray(14675974));
			list.AddRange(MessageBuilder.AddLength(3));
			list.AddRange(MessageBuilder.ConvertIntToBCD(j, 3));
			list.AddRange(MessageBuilder.HexToByteArray(14676039));
			list.AddRange(MessageBuilder.AddLength(array2.Length));
			list.AddRange(array2);
			list.InsertRange(num8, MessageBuilder.AddLength(list.Count - num8));
			int connTimeout = secureComm.ConnTimeout;
			secureComm.ConnTimeout = 60000;
			try
			{
				val3 = Send(new FPURequest((Command)145, list.ToArray()));
			}
			finally
			{
				secureComm.ConnTimeout = connTimeout;
			}
			sFResponse.Add(SFResponseLabel.ERROR_CODE, val3.ErrorCode);
			sFResponse.Add(SFResponseLabel.STATUS, val3.FPUState);
			if (val3.ErrorCode == 0)
			{
			}
			int num9 = Convert.ToInt32((decimal)(j * 100 / num6));
			this.OnFileSendingProgress(this, new OnFileSendingProgressEventArgs(num9.ToString()));
		}
		return sFResponse.GetString();
	}

	public string SetEndOfReceiptNote(int index, string line)
	{
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Expected O, but got Unknown
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>
		{
			index.ToString(),
			line
		});
		SFResponse sFResponse = new SFResponse();
		List<byte> list = new List<byte>();
		list.AddRange(MessageBuilder.HexToByteArray(14675984));
		list.AddRange(MessageBuilder.AddLength(1));
		list.Add((byte)(index - 1));
		list.AddRange(MessageBuilder.HexToByteArray(14675992));
		list.AddRange(MessageBuilder.AddLength(line.Length));
		list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(Utils.FixTurkishUpperCase(line)));
		FPUResponse val = Send(new FPURequest((Command)30, list.ToArray()));
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		if (val.ErrorCode == 0)
		{
			GMPMessage val2 = GMPMessage.Parse(val.Data);
			GMPGroup val3 = val2.FindGroup(57152);
			if (val3 != null)
			{
				GMPField val4 = val3.FindTag(14675992);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.DefaultEncoding.GetString(val4.Value));
				}
			}
		}
		return sFResponse.GetString();
	}

	public string GetEndOfReceiptNote(int index)
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string> { index.ToString() });
		SFResponse sFResponse = new SFResponse();
		List<byte> list = new List<byte>();
		list.AddRange(MessageBuilder.HexToByteArray(14675984));
		list.AddRange(MessageBuilder.AddLength(1));
		list.Add((byte)(index - 1));
		FPUResponse val = Send(new FPURequest((Command)30, list.ToArray()));
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		if (val.ErrorCode == 0)
		{
			GMPMessage val2 = GMPMessage.Parse(val.Data);
			GMPGroup val3 = val2.FindGroup(57152);
			if (val3 != null)
			{
				GMPField val4 = val3.FindTag(14675992);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.DefaultEncoding.GetString(val4.Value));
				}
			}
		}
		return sFResponse.GetString();
	}

	public string PrintDocumentHeader()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>());
		SFResponse sFResponse = new SFResponse();
		List<byte> list = new List<byte>();
		FPUResponse val = Send(new FPURequest((Command)33, list.ToArray()));
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		if (val.ErrorCode == 0)
		{
			GMPMessage val2 = GMPMessage.Parse(val.Data);
			GMPGroup val3 = val2.FindGroup(57152);
			if (val3 != null)
			{
				GMPField val4 = val3.FindTag(14675981);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.ConvertBcdToInt(val4.Value, 0, 4).ToString());
				}
			}
		}
		return sFResponse.GetString();
	}

	public string PrintDocumentHeader(string tckn_vkn, decimal amount, int docType)
	{
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Expected O, but got Unknown
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>
		{
			tckn_vkn,
			amount.ToString(),
			docType.ToString()
		});
		SFResponse sFResponse = new SFResponse();
		byte[] array = null;
		List<byte> list = new List<byte>();
		list = new List<byte>();
		list.AddRange(MessageBuilder.HexToByteArray(14675977));
		list.AddRange(MessageBuilder.AddLength(2));
		list.AddRange(MessageBuilder.ConvertIntToBCD(docType, 2));
		if (docType != 4 && docType != 6)
		{
			list.AddRange(MessageBuilder.HexToByteArray(14676008));
			list.AddRange(MessageBuilder.AddLength(tckn_vkn.Length));
			list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(tckn_vkn));
		}
		amount = TruncateDigitsAfterComma(amount, 2);
		array = MessageBuilder.ConvertDecimalToBCD(amount, 3);
		list.AddRange(MessageBuilder.HexToByteArray(14675972));
		list.AddRange(MessageBuilder.AddLength(array.Length));
		list.AddRange(array);
		FPUResponse val = Send(new FPURequest((Command)139, list.ToArray()));
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		if (val.ErrorCode == 0)
		{
			GMPMessage val2 = GMPMessage.Parse(val.Data);
			GMPGroup val3 = val2.FindGroup(57152);
			if (val3 != null)
			{
				GMPField val4 = val3.FindTag(14675981);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.ConvertBcdToInt(val4.Value, 0, 4).ToString());
				}
			}
		}
		return sFResponse.GetString();
	}

	public string PrintDocumentHeader(int docType, string tckn_vkn, string docSerial, DateTime docDateTime)
	{
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Expected O, but got Unknown
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>
		{
			docType.ToString(),
			tckn_vkn,
			docSerial,
			docDateTime.ToString()
		});
		if (secureComm.GetVersion() < 3)
		{
			return PrintDocumentHeader(tckn_vkn, 0.0m, docType);
		}
		SFResponse sFResponse = new SFResponse();
		List<byte> list = new List<byte>();
		list = new List<byte>();
		list.AddRange(MessageBuilder.HexToByteArray(14675977));
		list.AddRange(MessageBuilder.AddLength(2));
		list.AddRange(MessageBuilder.ConvertIntToBCD(docType, 2));
		if (string.IsNullOrEmpty(tckn_vkn))
		{
			tckn_vkn = "11111111111";
		}
		list.AddRange(MessageBuilder.HexToByteArray(14676008));
		list.AddRange(MessageBuilder.AddLength(tckn_vkn.Length));
		list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(tckn_vkn));
		if (string.IsNullOrEmpty(docSerial))
		{
			docSerial = "FP123456";
		}
		docSerial = Utils.FixTurkishUpperCase(docSerial);
		list.AddRange(MessageBuilder.HexToByteArray(14676032));
		list.AddRange(MessageBuilder.AddLength(docSerial.Length));
		list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(docSerial));
		list.AddRange(MessageBuilder.GetDateInBytes(docDateTime));
		FPUResponse val = Send(new FPURequest((Command)139, list.ToArray()));
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		if (val.ErrorCode == 0)
		{
			GMPMessage val2 = GMPMessage.Parse(val.Data);
			GMPGroup val3 = val2.FindGroup(57152);
			if (val3 != null)
			{
				GMPField val4 = val3.FindTag(14675981);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.ConvertBcdToInt(val4.Value, 0, 4).ToString());
				}
			}
		}
		return sFResponse.GetString();
	}

	public string PrintAdvanceDocumentHeader(string tckn, string name, decimal amount)
	{
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Expected O, but got Unknown
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>
		{
			tckn,
			name,
			amount.ToString()
		});
		if (secureComm.GetVersion() < 3)
		{
			return PrintDocumentHeader(tckn, amount, 6);
		}
		SFResponse sFResponse = new SFResponse();
		byte[] array = null;
		List<byte> list = new List<byte>();
		list = new List<byte>();
		list.AddRange(MessageBuilder.HexToByteArray(14675977));
		list.AddRange(MessageBuilder.AddLength(2));
		list.AddRange(MessageBuilder.ConvertIntToBCD(6, 2));
		if (string.IsNullOrEmpty(tckn))
		{
			tckn = "11111111111";
		}
		tckn = Utils.FixTurkishUpperCase(tckn);
		list.AddRange(MessageBuilder.HexToByteArray(14676008));
		list.AddRange(MessageBuilder.AddLength(tckn.Length));
		list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(tckn));
		if (string.IsNullOrEmpty(name))
		{
			name = "DEFAULT DEFAULT";
		}
		name = Utils.FixTurkishUpperCase(name);
		list.AddRange(MessageBuilder.HexToByteArray(14676033));
		list.AddRange(MessageBuilder.AddLength(name.Length));
		list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(name));
		amount = TruncateDigitsAfterComma(amount, 2);
		array = MessageBuilder.ConvertDecimalToBCD(amount, 3);
		list.AddRange(MessageBuilder.HexToByteArray(14675972));
		list.AddRange(MessageBuilder.AddLength(array.Length));
		list.AddRange(array);
		FPUResponse val = Send(new FPURequest((Command)139, list.ToArray()));
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		if (val.ErrorCode == 0)
		{
			GMPMessage val2 = GMPMessage.Parse(val.Data);
			GMPGroup val3 = val2.FindGroup(57152);
			if (val3 != null)
			{
				GMPField val4 = val3.FindTag(14675981);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.ConvertBcdToInt(val4.Value, 0, 4).ToString());
				}
			}
		}
		return sFResponse.GetString();
	}

	public string PrintCollectionDocumentHeader(string invoiceSerial, DateTime invoiceDate, decimal amount, string subscriberNo, string institutionName, decimal comissionAmount)
	{
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Expected O, but got Unknown
		//IL_025f: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>
		{
			invoiceSerial.ToString(),
			invoiceDate.ToString(),
			amount.ToString(),
			subscriberNo,
			institutionName,
			comissionAmount.ToString()
		});
		SFResponse sFResponse = new SFResponse();
		byte[] array = null;
		List<byte> list = new List<byte>();
		list = new List<byte>();
		list.AddRange(MessageBuilder.HexToByteArray(14675977));
		list.AddRange(MessageBuilder.AddLength(2));
		list.AddRange(MessageBuilder.ConvertIntToBCD(7, 2));
		if (!string.IsNullOrEmpty(invoiceSerial))
		{
			invoiceSerial = Utils.FixTurkishUpperCase(invoiceSerial);
			list.AddRange(MessageBuilder.HexToByteArray(14676032));
			list.AddRange(MessageBuilder.AddLength(invoiceSerial.Length));
			list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(invoiceSerial));
		}
		amount = TruncateDigitsAfterComma(amount, 2);
		array = MessageBuilder.ConvertDecimalToBCD(amount, 3);
		list.AddRange(MessageBuilder.HexToByteArray(14675972));
		list.AddRange(MessageBuilder.AddLength(array.Length));
		list.AddRange(array);
		if (!string.IsNullOrEmpty(subscriberNo))
		{
			subscriberNo = Utils.FixTurkishUpperCase(subscriberNo);
			list.AddRange(MessageBuilder.HexToByteArray(14676034));
			list.AddRange(MessageBuilder.AddLength(subscriberNo.Length));
			list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(subscriberNo));
		}
		institutionName = Utils.FixTurkishUpperCase(institutionName);
		list.AddRange(MessageBuilder.HexToByteArray(14676033));
		list.AddRange(MessageBuilder.AddLength(institutionName.Length));
		list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(institutionName));
		if (comissionAmount > 0m)
		{
			list.AddRange(MessageBuilder.HexToByteArray(14676035));
			list.AddRange(MessageBuilder.AddLength(1));
			list.Add(2);
			array = MessageBuilder.ConvertDecimalToBCD(comissionAmount, 3);
			list.AddRange(MessageBuilder.HexToByteArray(14676036));
			list.AddRange(MessageBuilder.AddLength(array.Length));
			list.AddRange(array);
		}
		bool flag = true;
		list.AddRange(MessageBuilder.GetDateInBytes(invoiceDate));
		FPUResponse val = Send(new FPURequest((Command)139, list.ToArray()));
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		if (val.ErrorCode == 0)
		{
			GMPMessage val2 = GMPMessage.Parse(val.Data);
			GMPGroup val3 = val2.FindGroup(57152);
			if (val3 != null)
			{
				GMPField val4 = val3.FindTag(14675981);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.ConvertBcdToInt(val4.Value, 0, 4).ToString());
				}
			}
		}
		return sFResponse.GetString();
	}

	public string PrintCurrentAccountCollectionDocumentHeader(string tcknVkn, string customerName, string docSerial, DateTime docDate, decimal amount)
	{
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Expected O, but got Unknown
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>
		{
			tcknVkn,
			customerName,
			docSerial,
			docDate.ToString(),
			amount.ToString()
		});
		SFResponse sFResponse = new SFResponse();
		byte[] array = null;
		List<byte> list = new List<byte>();
		list = new List<byte>();
		list.AddRange(MessageBuilder.HexToByteArray(14675977));
		list.AddRange(MessageBuilder.AddLength(2));
		list.AddRange(MessageBuilder.ConvertIntToBCD(8, 2));
		if (string.IsNullOrEmpty(tcknVkn))
		{
			tcknVkn = "11111111111";
		}
		tcknVkn = Utils.FixTurkishUpperCase(tcknVkn);
		list.AddRange(MessageBuilder.HexToByteArray(14676008));
		list.AddRange(MessageBuilder.AddLength(tcknVkn.Length));
		list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(tcknVkn));
		if (string.IsNullOrEmpty(customerName))
		{
			customerName = "";
		}
		customerName = Utils.FixTurkishUpperCase(customerName);
		list.AddRange(MessageBuilder.HexToByteArray(14676033));
		list.AddRange(MessageBuilder.AddLength(customerName.Length));
		list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(customerName));
		if (!string.IsNullOrEmpty(docSerial))
		{
			docSerial = Utils.FixTurkishUpperCase(docSerial);
			list.AddRange(MessageBuilder.HexToByteArray(14676032));
			list.AddRange(MessageBuilder.AddLength(docSerial.Length));
			list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(docSerial));
		}
		bool flag = true;
		list.AddRange(MessageBuilder.GetDateInBytes(docDate));
		amount = TruncateDigitsAfterComma(amount, 2);
		array = MessageBuilder.ConvertDecimalToBCD(amount, 3);
		list.AddRange(MessageBuilder.HexToByteArray(14675972));
		list.AddRange(MessageBuilder.AddLength(array.Length));
		list.AddRange(array);
		FPUResponse val = Send(new FPURequest((Command)139, list.ToArray()));
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		if (val.ErrorCode == 0)
		{
			GMPMessage val2 = GMPMessage.Parse(val.Data);
			GMPGroup val3 = val2.FindGroup(57152);
			if (val3 != null)
			{
				GMPField val4 = val3.FindTag(14675981);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.ConvertBcdToInt(val4.Value, 0, 4).ToString());
				}
			}
		}
		return sFResponse.GetString();
	}

	public string PrintFoodDocumentHeader()
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Expected O, but got Unknown
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>());
		SFResponse sFResponse = new SFResponse();
		List<byte> list = new List<byte>();
		list = new List<byte>();
		list.AddRange(MessageBuilder.HexToByteArray(14675977));
		list.AddRange(MessageBuilder.AddLength(2));
		list.AddRange(MessageBuilder.ConvertIntToBCD(4, 2));
		FPUResponse val = Send(new FPURequest((Command)139, list.ToArray()));
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		if (val.ErrorCode == 0)
		{
			GMPMessage val2 = GMPMessage.Parse(val.Data);
			GMPGroup val3 = val2.FindGroup(57152);
			if (val3 != null)
			{
				GMPField val4 = val3.FindTag(14675981);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.ConvertBcdToInt(val4.Value, 0, 4).ToString());
				}
			}
		}
		return sFResponse.GetString();
	}

	public string PrintParkDocument(string plate, DateTime entrenceDate)
	{
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Expected O, but got Unknown
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>
		{
			plate,
			entrenceDate.ToString()
		});
		SFResponse sFResponse = new SFResponse();
		List<byte> list = new List<byte>();
		list = new List<byte>();
		list.AddRange(MessageBuilder.HexToByteArray(14675977));
		list.AddRange(MessageBuilder.AddLength(2));
		list.AddRange(MessageBuilder.ConvertIntToBCD(5, 2));
		plate = Utils.FixTurkishUpperCase(plate);
		list.AddRange(MessageBuilder.HexToByteArray(14676009));
		list.AddRange(MessageBuilder.AddLength(plate.Length));
		list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(plate));
		list.AddRange(MessageBuilder.GetDateTimeInBytes(entrenceDate));
		FPUResponse val = Send(new FPURequest((Command)139, list.ToArray()));
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		if (val.ErrorCode == 0)
		{
			GMPMessage val2 = GMPMessage.Parse(val.Data);
			GMPGroup val3 = val2.FindGroup(57152);
			if (val3 != null)
			{
				GMPField val4 = val3.FindTag(14675981);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.ConvertBcdToInt(val4.Value, 0, 4).ToString());
				}
			}
		}
		return sFResponse.GetString();
	}

	public string PrintInvoiceHeader(DateTime invoiceDT, string serial, string orderNo, Customer customerInfo)
	{
		//IL_03b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bb: Expected O, but got Unknown
		//IL_03d5: Unknown result type (might be due to invalid IL or missing references)
		if (!fiscalId.StartsWith("FT"))
		{
			throw new Exception("This method is not supported for this device");
		}
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>
		{
			invoiceDT.ToString(),
			serial,
			orderNo
		});
		SFResponse sFResponse = new SFResponse();
		List<byte> list = new List<byte>();
		list = new List<byte>();
		int num = 0;
		list.AddRange(MessageBuilder.HexToByteArray(57213));
		num = list.Count;
		list.AddRange(MessageBuilder.GetDateInBytes(invoiceDT));
		serial = Utils.FixTurkishUpperCase(serial);
		list.AddRange(MessageBuilder.HexToByteArray(14675982));
		list.AddRange(MessageBuilder.AddLength(serial.Length));
		list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(serial));
		orderNo = Utils.FixTurkishUpperCase(orderNo);
		list.AddRange(MessageBuilder.HexToByteArray(14676032));
		list.AddRange(MessageBuilder.AddLength(orderNo.Length));
		list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(orderNo));
		list.InsertRange(num, MessageBuilder.AddLength(list.Count - num));
		list.AddRange(MessageBuilder.HexToByteArray(57214));
		num = list.Count;
		if (string.IsNullOrEmpty(customerInfo.TCKN_VKN))
		{
			customerInfo.TCKN_VKN = "11111111111";
		}
		list.AddRange(MessageBuilder.HexToByteArray(14676008));
		list.AddRange(MessageBuilder.AddLength(customerInfo.TCKN_VKN.Length));
		list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(customerInfo.TCKN_VKN));
		if (customerInfo.Name == null)
		{
			customerInfo.Name = "";
		}
		customerInfo.Name = Utils.FixTurkishUpperCase(customerInfo.Name);
		list.AddRange(MessageBuilder.HexToByteArray(14676033));
		list.AddRange(MessageBuilder.AddLength(customerInfo.Name.Length));
		list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(customerInfo.Name));
		if (customerInfo.Label == null)
		{
			customerInfo.Label = "";
		}
		customerInfo.Label = Utils.FixTurkishUpperCase(customerInfo.Label);
		list.AddRange(MessageBuilder.HexToByteArray(14676041));
		list.AddRange(MessageBuilder.AddLength(customerInfo.Label.Length));
		list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(customerInfo.Label));
		if (customerInfo.TaxOffice == null)
		{
			customerInfo.TaxOffice = "";
		}
		customerInfo.TaxOffice = Utils.FixTurkishUpperCase(customerInfo.TaxOffice);
		list.AddRange(MessageBuilder.HexToByteArray(14676042));
		list.AddRange(MessageBuilder.AddLength(customerInfo.TaxOffice.Length));
		list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(customerInfo.TaxOffice));
		if (customerInfo.AddressList != null)
		{
			string text = "";
			foreach (string address in customerInfo.AddressList)
			{
				text += Utils.FixTurkishUpperCase(address);
				text += "\n";
			}
			list.AddRange(MessageBuilder.HexToByteArray(14676040));
			list.AddRange(MessageBuilder.AddLength(text.Length));
			list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(text));
		}
		list.InsertRange(num, MessageBuilder.AddLength(list.Count - num));
		FPUResponse val = Send(new FPURequest((Command)148, list.ToArray()));
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		if (val.ErrorCode == 0)
		{
			GMPField val2 = null;
			GMPMessage val3 = GMPMessage.Parse(val.Data);
			GMPGroup val4 = val3.FindGroup(57152);
			if (val4 != null)
			{
				val2 = val4.FindTag(14675981);
				if (val2 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.ConvertBcdToInt(val2.Value, 0, 4).ToString());
				}
				val2 = val4.FindTag(14675995);
				if (val2 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.ConvertBcdToInt(val2.Value, 0, 4).ToString());
				}
			}
			val4 = val3.FindGroup(57203);
			if (val4 != null)
			{
				foreach (GMPField tag in val4.Tags)
				{
					if (tag.Tag == 14675985)
					{
						sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.ConvertBcdToInt(tag.Value, 0, 2).ToString());
					}
					else
					{
						sFResponse.AddNull(1);
					}
					if (tag.Tag == 14675972)
					{
						sFResponse.Add(SFResponseLabel.PARAM, $"{(decimal)MessageBuilder.ConvertBcdToInt(tag.Value, 0, tag.Length) / 1000m:#0.00}");
					}
					else
					{
						sFResponse.AddNull(1);
					}
				}
			}
		}
		return sFResponse.GetString();
	}

	public string PrintReturnDocumentHeader(DateTime invoiceDT, string serial, string orderNo, Customer customerInfo)
	{
		//IL_038e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0398: Expected O, but got Unknown
		//IL_03af: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>
		{
			invoiceDT.ToString(),
			serial,
			orderNo
		});
		SFResponse sFResponse = new SFResponse();
		List<byte> list = new List<byte>();
		list = new List<byte>();
		int num = 0;
		list.AddRange(MessageBuilder.HexToByteArray(57213));
		num = list.Count;
		list.AddRange(MessageBuilder.GetDateInBytes(invoiceDT));
		serial = Utils.FixTurkishUpperCase(serial);
		list.AddRange(MessageBuilder.HexToByteArray(14675982));
		list.AddRange(MessageBuilder.AddLength(serial.Length));
		list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(serial));
		orderNo = Utils.FixTurkishUpperCase(orderNo);
		list.AddRange(MessageBuilder.HexToByteArray(14676032));
		list.AddRange(MessageBuilder.AddLength(orderNo.Length));
		list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(orderNo));
		list.InsertRange(num, MessageBuilder.AddLength(list.Count - num));
		list.AddRange(MessageBuilder.HexToByteArray(57214));
		num = list.Count;
		if (string.IsNullOrEmpty(customerInfo.TCKN_VKN))
		{
			customerInfo.TCKN_VKN = "11111111111";
		}
		list.AddRange(MessageBuilder.HexToByteArray(14676008));
		list.AddRange(MessageBuilder.AddLength(customerInfo.TCKN_VKN.Length));
		list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(customerInfo.TCKN_VKN));
		if (customerInfo.Name == null)
		{
			customerInfo.Name = "";
		}
		customerInfo.Name = Utils.FixTurkishUpperCase(customerInfo.Name);
		list.AddRange(MessageBuilder.HexToByteArray(14676033));
		list.AddRange(MessageBuilder.AddLength(customerInfo.Name.Length));
		list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(customerInfo.Name));
		if (customerInfo.Label == null)
		{
			customerInfo.Label = "";
		}
		customerInfo.Label = Utils.FixTurkishUpperCase(customerInfo.Label);
		list.AddRange(MessageBuilder.HexToByteArray(14676041));
		list.AddRange(MessageBuilder.AddLength(customerInfo.Label.Length));
		list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(customerInfo.Label));
		if (customerInfo.TaxOffice == null)
		{
			customerInfo.TaxOffice = "";
		}
		customerInfo.TaxOffice = Utils.FixTurkishUpperCase(customerInfo.TaxOffice);
		list.AddRange(MessageBuilder.HexToByteArray(14676042));
		list.AddRange(MessageBuilder.AddLength(customerInfo.TaxOffice.Length));
		list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(customerInfo.TaxOffice));
		if (customerInfo.AddressList != null)
		{
			string text = "";
			foreach (string address in customerInfo.AddressList)
			{
				text += Utils.FixTurkishUpperCase(address);
				text += "\n";
			}
			list.AddRange(MessageBuilder.HexToByteArray(14676040));
			list.AddRange(MessageBuilder.AddLength(text.Length));
			list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(text));
		}
		list.InsertRange(num, MessageBuilder.AddLength(list.Count - num));
		FPUResponse val = Send(new FPURequest((Command)151, list.ToArray()));
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		if (val.ErrorCode == 0)
		{
			GMPField val2 = null;
			GMPMessage val3 = GMPMessage.Parse(val.Data);
			GMPGroup val4 = val3.FindGroup(57152);
			if (val4 != null)
			{
				val2 = val4.FindTag(14675981);
				if (val2 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.ConvertBcdToInt(val2.Value, 0, 4).ToString());
				}
				val2 = val4.FindTag(14675995);
				if (val2 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.ConvertBcdToInt(val2.Value, 0, 4).ToString());
				}
			}
		}
		return sFResponse.GetString();
	}

	public string PrintSelfEmployementHeader(Customer customer, Service[] services)
	{
		//IL_0466: Unknown result type (might be due to invalid IL or missing references)
		//IL_0470: Expected O, but got Unknown
		//IL_048a: Unknown result type (might be due to invalid IL or missing references)
		if (!fiscalId.StartsWith("FT"))
		{
			throw new Exception("This method is not supported for this device");
		}
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>());
		SFResponse sFResponse = new SFResponse();
		List<byte> list = new List<byte>();
		list = new List<byte>();
		int num = 0;
		list.AddRange(MessageBuilder.HexToByteArray(57214));
		num = list.Count;
		if (string.IsNullOrEmpty(customer.TCKN_VKN))
		{
			customer.TCKN_VKN = "11111111111";
		}
		list.AddRange(MessageBuilder.HexToByteArray(14676008));
		list.AddRange(MessageBuilder.AddLength(customer.TCKN_VKN.Length));
		list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(customer.TCKN_VKN));
		if (customer.Name == null)
		{
			customer.Name = "";
		}
		customer.Name = Utils.FixTurkishUpperCase(customer.Name);
		list.AddRange(MessageBuilder.HexToByteArray(14676033));
		list.AddRange(MessageBuilder.AddLength(customer.Name.Length));
		list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(customer.Name));
		if (customer.Label == null)
		{
			customer.Label = "";
		}
		customer.Label = Utils.FixTurkishUpperCase(customer.Label);
		list.AddRange(MessageBuilder.HexToByteArray(14676041));
		list.AddRange(MessageBuilder.AddLength(customer.Label.Length));
		list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(customer.Label));
		if (customer.TaxOffice == null)
		{
			customer.TaxOffice = "";
		}
		customer.TaxOffice = Utils.FixTurkishUpperCase(customer.TaxOffice);
		list.AddRange(MessageBuilder.HexToByteArray(14676042));
		list.AddRange(MessageBuilder.AddLength(customer.TaxOffice.Length));
		list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(customer.TaxOffice));
		if (customer.AddressList != null)
		{
			string text = "";
			foreach (string address in customer.AddressList)
			{
				text += Utils.FixTurkishUpperCase(address);
				text += "\n";
			}
			list.AddRange(MessageBuilder.HexToByteArray(14676040));
			list.AddRange(MessageBuilder.AddLength(text.Length));
			list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(text));
		}
		list.InsertRange(num, MessageBuilder.AddLength(list.Count - num));
		foreach (Service val in services)
		{
			list.AddRange(MessageBuilder.HexToByteArray(57201));
			num = list.Count;
			if (val.Definition == null)
			{
				val.Definition = "";
			}
			val.Definition = Utils.FixTurkishUpperCase(val.Definition);
			list.AddRange(MessageBuilder.HexToByteArray(14675992));
			list.AddRange(MessageBuilder.AddLength(val.Definition.Length));
			list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(val.Definition));
			decimal num2 = TruncateDigitsAfterComma(val.BrutAmount, 2);
			byte[] array = MessageBuilder.ConvertDecimalToBCD(num2, 3);
			list.AddRange(MessageBuilder.HexToByteArray(14675972));
			list.AddRange(MessageBuilder.AddLength(array.Length));
			list.AddRange(array);
			list.AddRange(MessageBuilder.HexToByteArray(14676043));
			list.AddRange(MessageBuilder.AddLength(1));
			list.AddRange(MessageBuilder.ConvertIntToBCD(val.StoppageRate, 1));
			list.AddRange(MessageBuilder.HexToByteArray(14675985));
			list.AddRange(MessageBuilder.AddLength(1));
			list.AddRange(MessageBuilder.ConvertIntToBCD(val.VATRate, 1));
			list.AddRange(MessageBuilder.HexToByteArray(14676044));
			list.AddRange(MessageBuilder.AddLength(1));
			list.AddRange(MessageBuilder.ConvertIntToBCD(val.WageRate, 1));
			list.InsertRange(num, MessageBuilder.AddLength(list.Count - num));
		}
		FPUResponse val2 = Send(new FPURequest((Command)149, list.ToArray()));
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val2.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val2.FPUState);
		if (val2.ErrorCode == 0)
		{
			GMPField val3 = null;
			GMPMessage val4 = GMPMessage.Parse(val2.Data);
			GMPGroup val5 = val4.FindGroup(57152);
			if (val5 != null)
			{
				val3 = val5.FindTag(14675981);
				if (val3 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.ConvertBcdToInt(val3.Value, 0, 4).ToString());
				}
				val3 = val5.FindTag(14675995);
				if (val3 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.ConvertBcdToInt(val3.Value, 0, 4).ToString());
				}
			}
			val5 = val4.FindGroup(57203);
			if (val5 != null)
			{
				foreach (GMPField tag in val5.Tags)
				{
					if (tag.Tag == 14675985)
					{
						sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.ConvertBcdToInt(tag.Value, 0, 2).ToString());
					}
					else
					{
						sFResponse.AddNull(1);
					}
					if (tag.Tag == 14675972)
					{
						sFResponse.Add(SFResponseLabel.PARAM, $"{(decimal)MessageBuilder.ConvertBcdToInt(tag.Value, 0, tag.Length) / 1000m:#0.00}");
					}
					else
					{
						sFResponse.AddNull(1);
					}
				}
			}
		}
		return sFResponse.GetString();
	}

	public string PrintItem(int PLUNo, decimal quantity, decimal amount, string name, string barcode, int deptId, int weighable)
	{
		//IL_0288: Unknown result type (might be due to invalid IL or missing references)
		//IL_0292: Expected O, but got Unknown
		//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>
		{
			PLUNo.ToString(),
			quantity.ToString(),
			amount.ToString(),
			name,
			barcode,
			deptId.ToString(),
			weighable.ToString()
		});
		byte[] array = null;
		int num = 0;
		List<byte> list = new List<byte>();
		SFResponse sFResponse = new SFResponse();
		for (int i = 0; i < 1; i++)
		{
			list.AddRange(MessageBuilder.HexToByteArray(57201));
			num = list.Count;
			list.AddRange(MessageBuilder.HexToByteArray(14675970));
			list.AddRange(MessageBuilder.AddLength(3));
			list.AddRange(MessageBuilder.ConvertIntToBCD(PLUNo, 3));
			list.AddRange(MessageBuilder.HexToByteArray(14675971));
			array = MessageBuilder.ConvertDecimalToBCD(quantity, 3);
			list.AddRange(MessageBuilder.AddLength(array.Length));
			list.AddRange(array);
			if (amount != -1m)
			{
				amount = TruncateDigitsAfterComma(amount, 2);
				list.AddRange(MessageBuilder.HexToByteArray(14675972));
				array = MessageBuilder.ConvertDecimalToBCD(amount, 3);
				list.AddRange(MessageBuilder.AddLength(array.Length));
				list.AddRange(array);
			}
			if (!string.IsNullOrEmpty(name))
			{
				list.AddRange(MessageBuilder.HexToByteArray(14675992));
				list.AddRange(MessageBuilder.AddLength(name.Length));
				list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(Utils.FixTurkishUpperCase(name)));
			}
			if (!string.IsNullOrEmpty(barcode))
			{
				list.AddRange(MessageBuilder.HexToByteArray(14675980));
				list.AddRange(MessageBuilder.AddLength(barcode.Length));
				list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(barcode));
			}
			if (deptId != -1)
			{
				list.AddRange(MessageBuilder.HexToByteArray(14675969));
				list.AddRange(MessageBuilder.AddLength(1));
				list.AddRange(MessageBuilder.ConvertIntToBCD(deptId, 1));
			}
			if (weighable != -1)
			{
				list.AddRange(MessageBuilder.HexToByteArray(14675999));
				list.AddRange(MessageBuilder.AddLength(1));
				list.AddRange(MessageBuilder.ConvertIntToBCD(weighable, 1));
			}
			list.InsertRange(num, MessageBuilder.AddLength(list.Count - num));
		}
		FPUResponse val = Send(new FPURequest((Command)34, list.ToArray()));
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		if (val.ErrorCode == 0)
		{
			GMPMessage val2 = GMPMessage.Parse(val.Data);
			GMPGroup val3 = val2.FindGroup(57203);
			if (val3 != null)
			{
				GMPField val4 = val3.FindTag(14675972);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, $"{(decimal)MessageBuilder.ConvertBcdToInt(val4.Value, 0, val4.Length) / 1000m:#0.00}");
				}
			}
		}
		return sFResponse.GetString();
	}

	private string PrintItems(List<JSONItem> itemList)
	{
		//IL_0363: Unknown result type (might be due to invalid IL or missing references)
		//IL_032b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0335: Expected O, but got Unknown
		//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Expected O, but got Unknown
		byte[] array = null;
		int num = 0;
		List<byte> list = new List<byte>();
		SFResponse sFResponse = new SFResponse();
		FPUResponse val = null;
		foreach (JSONItem item in itemList)
		{
			list.AddRange(MessageBuilder.HexToByteArray(57201));
			num = list.Count;
			list.AddRange(MessageBuilder.HexToByteArray(14675970));
			list.AddRange(MessageBuilder.AddLength(3));
			list.AddRange(MessageBuilder.ConvertIntToBCD(item.Id, 3));
			list.AddRange(MessageBuilder.HexToByteArray(14675971));
			array = MessageBuilder.ConvertDecimalToBCD(item.Quantity, 3);
			list.AddRange(MessageBuilder.AddLength(array.Length));
			list.AddRange(array);
			if (item.Price != -1m)
			{
				item.Price = TruncateDigitsAfterComma(item.Price, 2);
				list.AddRange(MessageBuilder.HexToByteArray(14675972));
				array = MessageBuilder.ConvertDecimalToBCD(item.Price, 3);
				list.AddRange(MessageBuilder.AddLength(array.Length));
				list.AddRange(array);
			}
			if (!string.IsNullOrEmpty(item.Name))
			{
				list.AddRange(MessageBuilder.HexToByteArray(14675992));
				list.AddRange(MessageBuilder.AddLength(item.Name.Length));
				list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(Utils.FixTurkishUpperCase(item.Name)));
			}
			if (!string.IsNullOrEmpty(item.Barcode))
			{
				list.AddRange(MessageBuilder.HexToByteArray(14675980));
				list.AddRange(MessageBuilder.AddLength(item.Barcode.Length));
				list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(item.Barcode));
			}
			if (item.DeptId != -1)
			{
				list.AddRange(MessageBuilder.HexToByteArray(14675969));
				list.AddRange(MessageBuilder.AddLength(1));
				list.AddRange(MessageBuilder.ConvertIntToBCD(item.DeptId, 1));
			}
			if (item.Status != -1)
			{
				list.AddRange(MessageBuilder.HexToByteArray(14675999));
				list.AddRange(MessageBuilder.AddLength(1));
				list.AddRange(MessageBuilder.ConvertIntToBCD(item.Status, 1));
			}
			list.InsertRange(num, MessageBuilder.AddLength(list.Count - num));
			if (list.Count > secureComm.BufferSize - 250)
			{
				int connTimeout = secureComm.ConnTimeout;
				secureComm.ConnTimeout = 300000;
				try
				{
					val = Send(new FPURequest((Command)34, list.ToArray()));
				}
				finally
				{
					secureComm.ConnTimeout = connTimeout;
				}
				array = null;
				num = 0;
				list = new List<byte>();
			}
		}
		if (list.Count > 0)
		{
			int connTimeout2 = secureComm.ConnTimeout;
			secureComm.ConnTimeout = 300000;
			try
			{
				val = Send(new FPURequest((Command)34, list.ToArray()));
			}
			finally
			{
				secureComm.ConnTimeout = connTimeout2;
			}
		}
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		if (val.ErrorCode == 0)
		{
			GMPMessage val2 = GMPMessage.Parse(val.Data);
			GMPGroup val3 = val2.FindGroup(57203);
			if (val3 != null)
			{
				GMPField val4 = val3.FindTag(14675972);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, $"{(decimal)MessageBuilder.ConvertBcdToInt(val4.Value, 0, val4.Length) / 1000m:#0.00}");
				}
			}
		}
		return sFResponse.GetString();
	}

	private string PrintDeptItems(List<JSONItem> itemList)
	{
		//IL_0620: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ac: Expected O, but got Unknown
		//IL_05d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04db: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e5: Expected O, but got Unknown
		//IL_0510: Unknown result type (might be due to invalid IL or missing references)
		byte[] array = null;
		int num = 0;
		List<byte> list = new List<byte>();
		SFResponse sFResponse = new SFResponse();
		FPUResponse val = null;
		foreach (JSONItem item in itemList)
		{
			list.AddRange(MessageBuilder.HexToByteArray(57201));
			num = list.Count;
			list.AddRange(MessageBuilder.HexToByteArray(14675969));
			list.AddRange(MessageBuilder.AddLength(1));
			list.AddRange(MessageBuilder.ConvertIntToBCD(item.DeptId, 1));
			list.AddRange(MessageBuilder.HexToByteArray(14675971));
			array = MessageBuilder.ConvertDecimalToBCD(item.Quantity, 3);
			list.AddRange(MessageBuilder.AddLength(array.Length));
			list.AddRange(array);
			item.Price = TruncateDigitsAfterComma(item.Price, 2);
			list.AddRange(MessageBuilder.HexToByteArray(14675972));
			array = MessageBuilder.ConvertDecimalToBCD(item.Price, 3);
			list.AddRange(MessageBuilder.AddLength(array.Length));
			list.AddRange(array);
			if (!string.IsNullOrEmpty(item.Name))
			{
				list.AddRange(MessageBuilder.HexToByteArray(14675992));
				list.AddRange(MessageBuilder.AddLength(item.Name.Length));
				list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(Utils.FixTurkishUpperCase(item.Name)));
				if (!string.IsNullOrEmpty(item.NoteLine1))
				{
					list.AddRange(MessageBuilder.HexToByteArray(14675992));
					list.AddRange(MessageBuilder.AddLength(item.NoteLine1.Length));
					list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(Utils.FixTurkishUpperCase(item.NoteLine1)));
				}
				if (!string.IsNullOrEmpty(item.NoteLine2))
				{
					list.AddRange(MessageBuilder.HexToByteArray(14675992));
					list.AddRange(MessageBuilder.AddLength(item.NoteLine2.Length));
					list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(Utils.FixTurkishUpperCase(item.NoteLine2)));
				}
			}
			if (item.Status != -1)
			{
				list.AddRange(MessageBuilder.HexToByteArray(14675999));
				list.AddRange(MessageBuilder.AddLength(1));
				list.AddRange(MessageBuilder.ConvertIntToBCD(item.Status, 1));
			}
			list.InsertRange(num, MessageBuilder.AddLength(list.Count - num));
			if (item.Adj != null)
			{
				list.AddRange(MessageBuilder.HexToByteArray(57207));
				num = list.Count;
				if (item.Adj.Type == AdjustmentType.PercentDiscount || item.Adj.Type == AdjustmentType.PercentFee)
				{
					list.AddRange(MessageBuilder.HexToByteArray(14675985));
					list.AddRange(MessageBuilder.AddLength(2));
					list.AddRange(MessageBuilder.ConvertIntToBCD(Convert.ToInt32(item.Adj.Percentage), 2));
				}
				else
				{
					item.Adj.Amount = TruncateDigitsAfterComma(item.Adj.Amount, 2);
					list.AddRange(MessageBuilder.HexToByteArray(14675972));
					array = MessageBuilder.ConvertDecimalToBCD(item.Adj.Amount, 3);
					list.AddRange(MessageBuilder.AddLength(array.Length));
					list.AddRange(array);
				}
				if (item.Adj.Type == AdjustmentType.Fee || item.Adj.Type == AdjustmentType.PercentFee)
				{
					list.AddRange(MessageBuilder.HexToByteArray(14675999));
					list.Add(1);
					list.Add(1);
				}
				if (!string.IsNullOrEmpty(item.Adj.NoteLine1))
				{
					list.AddRange(MessageBuilder.HexToByteArray(14675992));
					list.AddRange(MessageBuilder.AddLength(item.Adj.NoteLine1.Length));
					list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(Utils.FixTurkishUpperCase(item.Adj.NoteLine1)));
				}
				if (!string.IsNullOrEmpty(item.Adj.NoteLine2))
				{
					list.AddRange(MessageBuilder.HexToByteArray(14675992));
					list.AddRange(MessageBuilder.AddLength(item.Adj.NoteLine2.Length));
					list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(Utils.FixTurkishUpperCase(item.Adj.NoteLine2)));
				}
				list.InsertRange(num, MessageBuilder.AddLength(list.Count - num));
			}
			if (list.Count <= secureComm.BufferSize - 250)
			{
				continue;
			}
			int connTimeout = secureComm.ConnTimeout;
			secureComm.ConnTimeout = 300000;
			try
			{
				val = Send(new FPURequest((Command)45, list.ToArray()));
				if (val.ErrorCode != 0)
				{
					sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
					sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
					return sFResponse.GetString();
				}
			}
			finally
			{
				secureComm.ConnTimeout = connTimeout;
			}
			array = null;
			num = 0;
			list = new List<byte>();
		}
		if (list.Count > 0)
		{
			int connTimeout2 = secureComm.ConnTimeout;
			secureComm.ConnTimeout = 300000;
			try
			{
				val = Send(new FPURequest((Command)45, list.ToArray()));
				if (val.ErrorCode != 0)
				{
					sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
					sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
					return sFResponse.GetString();
				}
			}
			finally
			{
				secureComm.ConnTimeout = connTimeout2;
			}
		}
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		if (val.ErrorCode == 0)
		{
			GMPMessage val2 = GMPMessage.Parse(val.Data);
			GMPGroup val3 = val2.FindGroup(57203);
			if (val3 != null)
			{
				GMPField val4 = val3.FindTag(14675972);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, $"{(decimal)MessageBuilder.ConvertBcdToInt(val4.Value, 0, val4.Length) / 1000m:#0.00}");
				}
			}
		}
		return sFResponse.GetString();
	}

	public string PrintDepartment(int deptId, decimal quantity, decimal amount, string name, int weighable)
	{
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Expected O, but got Unknown
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>
		{
			deptId.ToString(),
			quantity.ToString(),
			amount.ToString(),
			name,
			weighable.ToString()
		});
		byte[] array = null;
		int num = 0;
		List<byte> list = new List<byte>();
		SFResponse sFResponse = new SFResponse();
		list.AddRange(MessageBuilder.HexToByteArray(57201));
		num = list.Count;
		list.AddRange(MessageBuilder.HexToByteArray(14675969));
		list.AddRange(MessageBuilder.AddLength(1));
		list.AddRange(MessageBuilder.ConvertIntToBCD(deptId, 1));
		list.AddRange(MessageBuilder.HexToByteArray(14675971));
		array = MessageBuilder.ConvertDecimalToBCD(quantity, 3);
		list.AddRange(MessageBuilder.AddLength(array.Length));
		list.AddRange(array);
		amount = TruncateDigitsAfterComma(amount, 2);
		list.AddRange(MessageBuilder.HexToByteArray(14675972));
		array = MessageBuilder.ConvertDecimalToBCD(amount, 3);
		list.AddRange(MessageBuilder.AddLength(array.Length));
		list.AddRange(array);
		if (!string.IsNullOrEmpty(name))
		{
			list.AddRange(MessageBuilder.HexToByteArray(14675992));
			list.AddRange(MessageBuilder.AddLength(name.Length));
			list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(Utils.FixTurkishUpperCase(name)));
		}
		if (weighable != -1)
		{
			list.AddRange(MessageBuilder.HexToByteArray(14675999));
			list.AddRange(MessageBuilder.AddLength(1));
			list.AddRange(MessageBuilder.ConvertIntToBCD(weighable, 1));
		}
		list.InsertRange(num, MessageBuilder.AddLength(list.Count - num));
		FPUResponse val = Send(new FPURequest((Command)45, list.ToArray()));
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		if (val.ErrorCode == 0)
		{
			GMPMessage val2 = GMPMessage.Parse(val.Data);
			GMPGroup val3 = val2.FindGroup(57203);
			if (val3 != null)
			{
				GMPField val4 = val3.FindTag(14675972);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, $"{(decimal)MessageBuilder.ConvertBcdToInt(val4.Value, 0, val4.Length) / 1000m:#0.00}");
				}
			}
		}
		return sFResponse.GetString();
	}

	public string PrintAdjustment(int adjustmentType, decimal amount, int percentage)
	{
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Expected O, but got Unknown
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>
		{
			adjustmentType.ToString(),
			amount.ToString(),
			percentage.ToString()
		});
		SFResponse sFResponse = new SFResponse();
		byte[] array = null;
		List<byte> list = new List<byte>();
		int num = 0;
		list.AddRange(MessageBuilder.HexToByteArray(57207));
		num = list.Count;
		if (adjustmentType == 3 || adjustmentType == 1)
		{
			list.AddRange(MessageBuilder.HexToByteArray(14675985));
			list.AddRange(MessageBuilder.AddLength(2));
			list.AddRange(MessageBuilder.ConvertIntToBCD(Convert.ToInt32(percentage), 2));
		}
		else
		{
			amount = TruncateDigitsAfterComma(amount, 2);
			list.AddRange(MessageBuilder.HexToByteArray(14675972));
			array = MessageBuilder.ConvertDecimalToBCD(amount, 3);
			list.AddRange(MessageBuilder.AddLength(array.Length));
			list.AddRange(array);
		}
		if (adjustmentType == 0 || adjustmentType == 1)
		{
			list.AddRange(MessageBuilder.HexToByteArray(14675999));
			list.Add(1);
			list.Add(1);
		}
		list.InsertRange(num, MessageBuilder.AddLength(list.Count - num));
		FPUResponse val = Send(new FPURequest((Command)35, list.ToArray()));
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		if (val.ErrorCode == 0)
		{
			GMPMessage val2 = GMPMessage.Parse(val.Data);
			GMPGroup val3 = val2.FindGroup(57203);
			if (val3 != null)
			{
				GMPField val4 = val3.FindTag(14675972);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, $"{(decimal)MessageBuilder.ConvertBcdToInt(val4.Value, 0, val4.Length) / 1000m:#0.00}");
				}
			}
		}
		return sFResponse.GetString();
	}

	public string Correct()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>());
		SFResponse sFResponse = new SFResponse();
		FPUResponse val = Send(new FPURequest((Command)36, new byte[0]));
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		if (val.ErrorCode == 0)
		{
			GMPMessage val2 = GMPMessage.Parse(val.Data);
			GMPGroup val3 = val2.FindGroup(57203);
			if (val3 != null)
			{
				GMPField val4 = val3.FindTag(14675972);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, $"{(decimal)MessageBuilder.ConvertBcdToInt(val4.Value, 0, val4.Length) / 1000m:#0.00}");
				}
			}
		}
		return sFResponse.GetString();
	}

	public string Void(int PLUNo, decimal quantity)
	{
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Expected O, but got Unknown
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>
		{
			PLUNo.ToString(),
			quantity.ToString()
		});
		SFResponse sFResponse = new SFResponse();
		byte[] array = null;
		List<byte> list = new List<byte>();
		int num = 0;
		list.AddRange(MessageBuilder.HexToByteArray(57201));
		num = list.Count;
		list.AddRange(MessageBuilder.HexToByteArray(14675970));
		list.AddRange(MessageBuilder.AddLength(3));
		list.AddRange(MessageBuilder.ConvertIntToBCD(PLUNo, 3));
		list.AddRange(MessageBuilder.HexToByteArray(14675971));
		array = MessageBuilder.ConvertDecimalToBCD(quantity, 3);
		list.AddRange(MessageBuilder.AddLength(array.Length));
		list.AddRange(array);
		list.InsertRange(num, MessageBuilder.AddLength(list.Count - num));
		FPUResponse val = Send(new FPURequest((Command)37, list.ToArray()));
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		if (val.ErrorCode == 0)
		{
			GMPMessage val2 = GMPMessage.Parse(val.Data);
			GMPGroup val3 = val2.FindGroup(57203);
			if (val3 != null)
			{
				GMPField val4 = val3.FindTag(14675972);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, $"{(decimal)MessageBuilder.ConvertBcdToInt(val4.Value, 0, val4.Length) / 1000m:#0.00}");
				}
			}
		}
		return sFResponse.GetString();
	}

	public string VoidDepartment(int deptId, string deptName, decimal quantity)
	{
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Expected O, but got Unknown
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>
		{
			deptId.ToString(),
			deptName,
			quantity.ToString()
		});
		SFResponse sFResponse = new SFResponse();
		byte[] array = null;
		List<byte> list = new List<byte>();
		int num = 0;
		list.AddRange(MessageBuilder.HexToByteArray(57201));
		num = list.Count;
		list.AddRange(MessageBuilder.HexToByteArray(14675969));
		list.AddRange(MessageBuilder.AddLength(3));
		list.AddRange(MessageBuilder.ConvertIntToBCD(deptId, 3));
		list.AddRange(MessageBuilder.HexToByteArray(14675971));
		array = MessageBuilder.ConvertDecimalToBCD(quantity, 3);
		list.AddRange(MessageBuilder.AddLength(array.Length));
		list.AddRange(array);
		list.InsertRange(num, MessageBuilder.AddLength(list.Count - num));
		if (!string.IsNullOrEmpty(deptName))
		{
			list.AddRange(MessageBuilder.HexToByteArray(14675992));
			list.AddRange(MessageBuilder.AddLength(deptName.Length));
			list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(deptName));
		}
		FPUResponse val = Send(new FPURequest((Command)37, list.ToArray()));
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		if (val.ErrorCode == 0)
		{
			GMPMessage val2 = GMPMessage.Parse(val.Data);
			GMPGroup val3 = val2.FindGroup(57203);
			if (val3 != null)
			{
				GMPField val4 = val3.FindTag(14675972);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, $"{(decimal)MessageBuilder.ConvertBcdToInt(val4.Value, 0, val4.Length) / 1000m:#0.00}");
				}
			}
		}
		return sFResponse.GetString();
	}

	public string PrintSubtotal(bool isQuery)
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Expected O, but got Unknown
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string> { isQuery.ToString() });
		SFResponse sFResponse = new SFResponse();
		FPUResponse val;
		if (isQuery)
		{
			List<byte> list = new List<byte>();
			list.AddRange(MessageBuilder.HexToByteArray(14675999));
			list.AddRange(MessageBuilder.AddLength(1));
			list.AddRange(MessageBuilder.ConvertIntToBCD(1, 1));
			val = Send(new FPURequest((Command)38, list.ToArray()));
		}
		else
		{
			val = Send(new FPURequest((Command)38, new byte[0]));
		}
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		if (val.ErrorCode == 0)
		{
			GMPMessage val2 = GMPMessage.Parse(val.Data);
			GMPGroup val3 = val2.FindGroup(57203);
			if (val3 != null)
			{
				GMPField val4 = val3.FindTag(14675972);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, $"{(decimal)MessageBuilder.ConvertBcdToInt(val4.Value, 0, val4.Length) / 1000m:#0.00}");
				}
			}
			val3 = val2.FindGroup(57204);
			if (val3 != null)
			{
				GMPField val5 = val3.FindTag(14675972);
				if (val5 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, $"{(decimal)MessageBuilder.ConvertBcdToInt(val5.Value, 0, val5.Length) / 1000m:#0.00}");
				}
			}
		}
		return sFResponse.GetString();
	}

	public string PrintSubtotal(decimal stoppageAmount)
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Expected O, but got Unknown
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string> { stoppageAmount.ToString() });
		SFResponse sFResponse = new SFResponse();
		List<byte> list = new List<byte>();
		stoppageAmount = TruncateDigitsAfterComma(stoppageAmount, 2);
		list.AddRange(MessageBuilder.HexToByteArray(14676044));
		byte[] array = MessageBuilder.ConvertDecimalToBCD(stoppageAmount, 3);
		list.AddRange(MessageBuilder.AddLength(array.Length));
		list.AddRange(array);
		FPUResponse val = Send(new FPURequest((Command)38, list.ToArray()));
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		if (val.ErrorCode == 0)
		{
			GMPMessage val2 = GMPMessage.Parse(val.Data);
			GMPGroup val3 = val2.FindGroup(57203);
			if (val3 != null)
			{
				GMPField val4 = val3.FindTag(14675972);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, $"{(decimal)MessageBuilder.ConvertBcdToInt(val4.Value, 0, val4.Length) / 1000m:#0.00}");
				}
			}
			val3 = val2.FindGroup(57204);
			if (val3 != null)
			{
				GMPField val5 = val3.FindTag(14675972);
				if (val5 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, $"{(decimal)MessageBuilder.ConvertBcdToInt(val5.Value, 0, val5.Length) / 1000m:#0.00}");
				}
			}
		}
		return sFResponse.GetString();
	}

	public string PrintPayment(int paymentType, int index, decimal paidTotal)
	{
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Expected O, but got Unknown
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>
		{
			paymentType.ToString(),
			index.ToString(),
			paidTotal.ToString()
		});
		SFResponse sFResponse = new SFResponse();
		FPUResponse val = null;
		byte[] array = null;
		List<byte> list = new List<byte>();
		int num = 0;
		list.AddRange(MessageBuilder.HexToByteArray(57204));
		num = list.Count;
		list.AddRange(MessageBuilder.HexToByteArray(14675973));
		list.AddRange(MessageBuilder.AddLength(1));
		list.AddRange(MessageBuilder.ConvertIntToBCD(paymentType + 1, 1));
		if (paymentType == 3 || paymentType == 1)
		{
			list.AddRange(MessageBuilder.HexToByteArray(14675984));
			list.AddRange(MessageBuilder.AddLength(1));
			list.AddRange(MessageBuilder.ConvertIntToBCD(index, 1));
		}
		if (paymentType == 2 && index > 0)
		{
			string text = index.ToString();
			list.AddRange(MessageBuilder.HexToByteArray(14675992));
			list.AddRange(MessageBuilder.AddLength(text.Length));
			list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(text));
		}
		paidTotal = TruncateDigitsAfterComma(paidTotal, 2);
		list.AddRange(MessageBuilder.HexToByteArray(14675972));
		array = MessageBuilder.ConvertDecimalToBCD(paidTotal, 3);
		list.AddRange(MessageBuilder.AddLength(array.Length));
		list.AddRange(array);
		list.InsertRange(num, MessageBuilder.AddLength(list.Count - num));
		int connTimeout = secureComm.ConnTimeout;
		if (paymentType == 5)
		{
			secureComm.ConnTimeout = 30000;
		}
		try
		{
			val = Send(new FPURequest((Command)39, list.ToArray()));
		}
		finally
		{
			secureComm.ConnTimeout = connTimeout;
		}
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		if (val.ErrorCode == 0)
		{
			GMPMessage val2 = GMPMessage.Parse(val.Data);
			GMPGroup val3 = val2.FindGroup(57203);
			if (val3 != null)
			{
				GMPField val4 = val3.FindTag(14675972);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, $"{(decimal)MessageBuilder.ConvertBcdToInt(val4.Value, 0, val4.Length) / 1000m:#0.00}");
				}
			}
			val3 = val2.FindGroup(57204);
			if (val3 != null)
			{
				GMPField val5 = val3.FindTag(14675972);
				if (val5 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, $"{(decimal)MessageBuilder.ConvertBcdToInt(val5.Value, 0, val5.Length) / 1000m:#0.00}");
				}
			}
		}
		return sFResponse.GetString();
	}

	public string VoidPayment(int paymentSequenceNo)
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Expected O, but got Unknown
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string> { paymentSequenceNo.ToString() });
		SFResponse sFResponse = new SFResponse();
		List<byte> list = new List<byte>();
		int num = 0;
		list.AddRange(MessageBuilder.HexToByteArray(57204));
		num = list.Count;
		list.AddRange(MessageBuilder.HexToByteArray(14675984));
		list.AddRange(MessageBuilder.AddLength(1));
		list.AddRange(MessageBuilder.ConvertIntToBCD(paymentSequenceNo, 1));
		list.InsertRange(num, MessageBuilder.AddLength(list.Count - num));
		FPUResponse val;
		try
		{
			secureComm.ConnTimeout = 300000;
			val = Send(new FPURequest((Command)44, list.ToArray()));
		}
		catch (Exception ex)
		{
			throw ex;
		}
		finally
		{
			secureComm.ConnTimeout = 15000;
		}
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		if (val.ErrorCode == 0)
		{
			GMPMessage val2 = GMPMessage.Parse(val.Data);
			GMPGroup val3 = val2.FindGroup(57203);
			if (val3 != null)
			{
				GMPField val4 = val3.FindTag(14675972);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, $"{(decimal)MessageBuilder.ConvertBcdToInt(val4.Value, 0, val4.Length) / 1000m:#0.00}");
				}
			}
			val3 = val2.FindGroup(57204);
			if (val3 != null)
			{
				GMPField val5 = val3.FindTag(14675972);
				if (val5 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, $"{(decimal)MessageBuilder.ConvertBcdToInt(val5.Value, 0, val5.Length) / 1000m:#0.00}");
				}
			}
		}
		return sFResponse.GetString();
	}

	public string CloseReceipt(bool slipCopy)
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Expected O, but got Unknown
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string> { slipCopy.ToString() });
		List<byte> list = new List<byte>();
		SFResponse sFResponse = new SFResponse();
		if (slipCopy)
		{
			list.AddRange(MessageBuilder.HexToByteArray(14675999));
			list.AddRange(MessageBuilder.AddLength(1));
			list.AddRange(MessageBuilder.ConvertIntToBCD(1, 1));
		}
		if (slipCopy)
		{
			secureComm.ConnTimeout = 300000;
		}
		FPUResponse val;
		try
		{
			val = Send(new FPURequest((Command)40, list.ToArray()));
		}
		finally
		{
			secureComm.ConnTimeout = 15000;
		}
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		if (val.ErrorCode == 0)
		{
			GMPMessage val2 = GMPMessage.Parse(val.Data);
			GMPGroup val3 = val2.FindGroup(57152);
			if (val3 != null)
			{
				GMPField val4 = val3.FindTag(14675981);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.ConvertBcdToInt(val4.Value, 0, 4).ToString());
				}
			}
		}
		return sFResponse.GetString();
	}

	public string VoidReceipt()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>());
		SFResponse sFResponse = new SFResponse();
		FPUResponse val = Send(new FPURequest((Command)41, new byte[0]));
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		if (val.ErrorCode == 0)
		{
			GMPMessage val2 = GMPMessage.Parse(val.Data);
			GMPGroup val3 = val2.FindGroup(57152);
			if (val3 != null)
			{
				GMPField val4 = val3.FindTag(14675981);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.ConvertBcdToInt(val4.Value, 0, 4).ToString());
				}
			}
		}
		return sFResponse.GetString();
	}

	public string SendSlipContent(int type, byte[] content)
	{
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Expected O, but got Unknown
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		List<string> list = new List<string>();
		list.Add($"Type : {type}");
		LogMethodInfo(MethodBase.GetCurrentMethod(), list);
		List<byte> list2 = new List<byte>();
		FPUResponse val = null;
		SFResponse sFResponse = new SFResponse();
		list2.AddRange(MessageBuilder.HexToByteArray(14675984));
		list2.Add(1);
		list2.Add((byte)type);
		list2.AddRange(MessageBuilder.HexToByteArray(14675992));
		list2.AddRange(MessageBuilder.AddLength(content.Length));
		list2.AddRange(content);
		val = Send(new FPURequest((Command)47, list2.ToArray()));
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		return sFResponse.GetString();
	}

	public string PrintRemarkLine(string[] lines)
	{
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Expected O, but got Unknown
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>(lines));
		List<byte> list = new List<byte>();
		FPUResponse val = null;
		SFResponse sFResponse = new SFResponse();
		foreach (string text in lines)
		{
			list.AddRange(MessageBuilder.HexToByteArray(14675992));
			list.AddRange(MessageBuilder.AddLength(text.Length));
			list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(text));
		}
		val = Send(new FPURequest((Command)42, list.ToArray()));
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		return sFResponse.GetString();
	}

	public string PrintReceiptBarcode(string barcode)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Expected O, but got Unknown
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string> { barcode });
		List<byte> list = new List<byte>();
		SFResponse sFResponse = new SFResponse();
		list.AddRange(MessageBuilder.HexToByteArray(14675980));
		list.AddRange(MessageBuilder.AddLength(barcode.Length));
		list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(barcode));
		FPUResponse val = Send(new FPURequest((Command)43, list.ToArray()));
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		return sFResponse.GetString();
	}

	public string PrintReceiptBarcode(int barcodeType, string barcode)
	{
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Expected O, but got Unknown
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>
		{
			barcodeType.ToString(),
			barcode
		});
		List<byte> list = new List<byte>();
		SFResponse sFResponse = new SFResponse();
		list.AddRange(MessageBuilder.HexToByteArray(14675984));
		list.AddRange(MessageBuilder.AddLength(1));
		list.AddRange(MessageBuilder.ConvertIntToBCD(barcodeType, 1));
		list.AddRange(MessageBuilder.HexToByteArray(14675980));
		list.AddRange(MessageBuilder.AddLength(barcode.Length));
		list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(barcode));
		FPUResponse val = Send(new FPURequest((Command)43, list.ToArray()));
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		return sFResponse.GetString();
	}

	public string PrintJSONDocument(string jsonStr)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Invalid comparison between Unknown and I4
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Invalid comparison between Unknown and I4
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Invalid comparison between Unknown and I4
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Invalid comparison between Unknown and I4
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string> { jsonStr });
		string text = "";
		SFResponseObject sFResponseObject = null;
		JSONDocument jSONDocument = JsonConvert.DeserializeObject<JSONDocument>(jsonStr);
		FPUResponse val = Send(new FPURequest((Command)129, new byte[0]));
		if (((int)val.FPUState == 2 || (int)val.FPUState == 1 || (int)val.FPUState == 13 || (int)val.FPUState == 4) && val.ErrorCode == 0)
		{
			if (jSONDocument.FiscalItems != null && jSONDocument.FiscalItems.Count > 0)
			{
				List<JSONItem> list = new List<JSONItem>();
				foreach (JSONItem fiscalItem in jSONDocument.FiscalItems)
				{
					if (fiscalItem.Adj != null)
					{
						if (list.Count > 0)
						{
							PrintItems(list);
							list.Clear();
							list = new List<JSONItem>();
						}
						text = PrintItem(fiscalItem.Id, fiscalItem.Quantity, fiscalItem.Price, fiscalItem.Name, fiscalItem.Barcode, fiscalItem.DeptId, fiscalItem.Status);
						sFResponseObject = SFResponse.GetObjectByString(text);
						if (sFResponseObject.errorCode != 0)
						{
							if (sFResponseObject.errorCode == 8)
							{
								JSONDocAfterNoPaper = jSONDocument;
								NoPaperFlag = true;
								lastStatus = sFResponseObject.statusCode;
							}
							return text;
						}
						text = PrintAdjustment((int)fiscalItem.Adj.Type, fiscalItem.Adj.Amount, fiscalItem.Adj.Percentage);
						sFResponseObject = SFResponse.GetObjectByString(text);
						if (sFResponseObject.errorCode != 0)
						{
							if (sFResponseObject.errorCode == 8)
							{
								JSONDocAfterNoPaper = jSONDocument;
								NoPaperFlag = true;
								lastStatus = sFResponseObject.statusCode;
							}
							return text;
						}
					}
					else
					{
						list.Add(fiscalItem);
					}
				}
				if (list.Count > 0)
				{
					text = PrintItems(list);
					sFResponseObject = SFResponse.GetObjectByString(text);
					if (sFResponseObject.errorCode != 0)
					{
						if (sFResponseObject.errorCode == 8)
						{
							JSONDocAfterNoPaper = jSONDocument;
							NoPaperFlag = true;
							lastStatus = sFResponseObject.statusCode;
						}
						return text;
					}
					list.Clear();
					list = new List<JSONItem>();
				}
			}
			if (jSONDocument.Adjustments != null && jSONDocument.Adjustments.Count > 0)
			{
				text = PrintSubtotal(isQuery: false);
				foreach (Adjustment adjustment in jSONDocument.Adjustments)
				{
					text = PrintAdjustment((int)adjustment.Type, adjustment.Amount, adjustment.Percentage);
					sFResponseObject = SFResponse.GetObjectByString(text);
					if (sFResponseObject.errorCode != 0)
					{
						if (sFResponseObject.errorCode == 8)
						{
							JSONDocAfterNoPaper = jSONDocument;
							NoPaperFlag = true;
							lastStatus = sFResponseObject.statusCode;
						}
						return text;
					}
				}
			}
			List<PaymentInfo> list2 = new List<PaymentInfo>();
			if (jSONDocument.Payments != null && jSONDocument.Payments.Count > 0)
			{
				foreach (PaymentInfo payment in jSONDocument.Payments)
				{
					if (payment.viaByEFT)
					{
						list2.Add(payment);
						continue;
					}
					text = PrintPayment((int)payment.Type, payment.Index, payment.PaidTotal);
					sFResponseObject = SFResponse.GetObjectByString(text);
					if (sFResponseObject.errorCode == 0)
					{
						continue;
					}
					if (sFResponseObject.errorCode == 8)
					{
						JSONDocAfterNoPaper = jSONDocument;
						NoPaperFlag = true;
						lastStatus = sFResponseObject.statusCode;
					}
					return text;
				}
			}
			if (jSONDocument.FooterNotes != null && jSONDocument.FooterNotes.Count > 0)
			{
				text = PrintRemarkLine(jSONDocument.FooterNotes.ToArray());
				sFResponseObject = SFResponse.GetObjectByString(text);
				if (sFResponseObject.errorCode != 0)
				{
					return text;
				}
			}
			text = CheckPrinterStatus();
			sFResponseObject = SFResponse.GetObjectByString(text);
			if (sFResponseObject.errorCode != 0)
			{
				return text;
			}
			if (sFResponseObject.statusCode == 5)
			{
				text = CloseReceipt(slipCopy: false);
			}
		}
		return text;
	}

	public string PrintJSONDocumentDeptOnly(string jsonStr)
	{
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string> { jsonStr });
		string text = "";
		SFResponseObject sFResponseObject = null;
		JSONDocument jSONDocument = JsonConvert.DeserializeObject<JSONDocument>(jsonStr);
		FPUResponse val = Send(new FPURequest((Command)129, new byte[0]));
		if (((int)val.FPUState == 2 || (int)val.FPUState == 1 || (int)val.FPUState == 13 || (int)val.FPUState == 4) && val.ErrorCode == 0)
		{
			if ((int)val.FPUState == 1)
			{
				text = PrintDocumentHeader();
				sFResponseObject = SFResponse.GetObjectByString(text);
				if (sFResponseObject.errorCode != 0)
				{
					return text;
				}
			}
			if ((int)val.FPUState != 4)
			{
				if (jSONDocument.FiscalItems != null && jSONDocument.FiscalItems.Count > 0)
				{
					List<JSONItem> list = new List<JSONItem>();
					int num = 0;
					foreach (JSONItem fiscalItem in jSONDocument.FiscalItems)
					{
						if (!fiscalItem.isVoided)
						{
							list.Add(fiscalItem);
						}
						if (!fiscalItem.isVoided && ++num != jSONDocument.FiscalItems.Count)
						{
							continue;
						}
						if (list.Count > 0)
						{
							text = PrintDeptItems(list);
							sFResponseObject = SFResponse.GetObjectByString(text);
							if (sFResponseObject.errorCode != 0)
							{
								if (sFResponseObject.errorCode == 8 && sFResponseObject.statusCode == 2)
								{
									JSONDocAfterNoPaper = jSONDocument;
									NoPaperFlag = true;
									lastStatus = sFResponseObject.statusCode;
								}
								return text;
							}
							list.Clear();
						}
						if (!fiscalItem.isVoided)
						{
							continue;
						}
						text = VoidDepartment(fiscalItem.DeptId, fiscalItem.Name, fiscalItem.Quantity);
						sFResponseObject = SFResponse.GetObjectByString(text);
						if (sFResponseObject.errorCode != 0)
						{
							if (sFResponseObject.errorCode == 8 && sFResponseObject.statusCode == 2)
							{
								JSONDocAfterNoPaper = jSONDocument;
								NoPaperFlag = true;
								lastStatus = sFResponseObject.statusCode;
							}
							return text;
						}
					}
				}
				if (jSONDocument.Adjustments != null && jSONDocument.Adjustments.Count > 0)
				{
					text = PrintSubtotal(isQuery: false);
					foreach (Adjustment adjustment in jSONDocument.Adjustments)
					{
						text = PrintAdjustment((int)adjustment.Type, adjustment.Amount, adjustment.Percentage);
						sFResponseObject = SFResponse.GetObjectByString(text);
						if (sFResponseObject.errorCode != 0)
						{
							if (sFResponseObject.errorCode == 8)
							{
								JSONDocAfterNoPaper = jSONDocument;
								NoPaperFlag = true;
								lastStatus = sFResponseObject.statusCode;
							}
							return text;
						}
					}
				}
			}
			List<PaymentInfo> list2 = new List<PaymentInfo>();
			if (jSONDocument.Payments != null && jSONDocument.Payments.Count > 0)
			{
				foreach (PaymentInfo payment in jSONDocument.Payments)
				{
					if (payment.viaByEFT)
					{
						list2.Add(payment);
						continue;
					}
					text = PrintPayment((int)payment.Type, payment.Index, payment.PaidTotal);
					sFResponseObject = SFResponse.GetObjectByString(text);
					if (sFResponseObject.errorCode == 0)
					{
						continue;
					}
					if (sFResponseObject.errorCode == 8)
					{
						JSONDocAfterNoPaper = jSONDocument;
						NoPaperFlag = true;
						lastStatus = sFResponseObject.statusCode;
					}
					return text;
				}
			}
			if (jSONDocument.SlipContents != null && jSONDocument.SlipContents.Count > 0)
			{
				foreach (JSONSlipContent slipContent in jSONDocument.SlipContents)
				{
					int type = slipContent.Type;
					List<byte> list3 = new List<byte>();
					foreach (JSONSlipLine line in slipContent.Lines)
					{
						if (line.Text.Length != 0)
						{
							byte item = 3;
							if (line.Style != null && line.Style.Equals("Bold"))
							{
								item = 3;
							}
							byte[] bytes = MessageBuilder.DefaultEncoding.GetBytes(line.Text);
							list3.Add(item);
							list3.Add(1);
							list3.Add((byte)bytes.Length);
							list3.AddRange(bytes);
						}
						if (line.ImageNumber != 0)
						{
						}
						if (line.Feed > 0)
						{
							for (int i = 0; i < line.Feed; i++)
							{
								list3.Add(3);
								list3.Add(1);
								list3.Add(1);
								list3.Add(10);
							}
						}
					}
					text = SendSlipContent(type, list3.ToArray());
					sFResponseObject = SFResponse.GetObjectByString(text);
				}
			}
			if (jSONDocument.FooterNotes != null && jSONDocument.FooterNotes.Count > 0)
			{
				text = PrintRemarkLine(jSONDocument.FooterNotes.ToArray());
				sFResponseObject = SFResponse.GetObjectByString(text);
				if (sFResponseObject.errorCode != 0)
				{
					return text;
				}
			}
			if (jSONDocument.EndOfReceiptInfo != null && jSONDocument.EndOfReceiptInfo.BarcodeFlag && !string.IsNullOrEmpty(jSONDocument.EndOfReceiptInfo.Barcode))
			{
				text = PrintReceiptBarcode(jSONDocument.EndOfReceiptInfo.Barcode);
				sFResponseObject = SFResponse.GetObjectByString(text);
				if (sFResponseObject.errorCode != 0)
				{
					return text;
				}
			}
			if (jSONDocument.EndOfReceiptInfo.CloseReceiptFlag)
			{
				text = CheckPrinterStatus();
				sFResponseObject = SFResponse.GetObjectByString(text);
				if (sFResponseObject.errorCode != 0)
				{
					return text;
				}
				if (sFResponseObject.statusCode == 5)
				{
					text = CloseReceipt(slipCopy: false);
				}
			}
		}
		else if ((int)val.FPUState == 5 && (jSONDocument.FooterNotes.Count > 0 || jSONDocument.EndOfReceiptInfo.BarcodeFlag))
		{
			if (jSONDocument.FooterNotes != null && jSONDocument.FooterNotes.Count > 0)
			{
				text = PrintRemarkLine(new string[1] { " " });
				sFResponseObject = SFResponse.GetObjectByString(text);
				if (sFResponseObject.errorCode != 0)
				{
					return text;
				}
				text = PrintRemarkLine(jSONDocument.FooterNotes.ToArray());
				sFResponseObject = SFResponse.GetObjectByString(text);
				if (sFResponseObject.errorCode != 0)
				{
					return text;
				}
			}
			if (jSONDocument.EndOfReceiptInfo != null && jSONDocument.EndOfReceiptInfo.BarcodeFlag && !string.IsNullOrEmpty(jSONDocument.EndOfReceiptInfo.Barcode))
			{
				text = PrintReceiptBarcode(jSONDocument.EndOfReceiptInfo.Barcode);
				sFResponseObject = SFResponse.GetObjectByString(text);
				if (sFResponseObject.errorCode != 0)
				{
					return text;
				}
			}
		}
		return text;
	}

	public string PrintSalesDocument(string jsonStr)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected O, but got Unknown
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Invalid comparison between Unknown and I4
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Invalid comparison between Unknown and I4
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Invalid comparison between Unknown and I4
		//IL_0a1b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a25: Expected O, but got Unknown
		//IL_0a3f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Invalid comparison between Unknown and I4
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Invalid comparison between Unknown and I4
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string> { jsonStr });
		JSONDocument jSONDocument = JsonConvert.DeserializeObject<JSONDocument>(jsonStr);
		SFResponse sFResponse = new SFResponse();
		byte[] array = null;
		int num = 0;
		List<byte> list = new List<byte>();
		FPUResponse val = Send(new FPURequest((Command)129, new byte[0]));
		if (((int)val.FPUState == 2 || (int)val.FPUState == 1 || (int)val.FPUState == 13 || (int)val.FPUState == 4) && val.ErrorCode == 0)
		{
			if ((int)val.FPUState != 4)
			{
				foreach (JSONItem fiscalItem in jSONDocument.FiscalItems)
				{
					list.AddRange(MessageBuilder.HexToByteArray(57201));
					num = list.Count;
					list.AddRange(MessageBuilder.HexToByteArray(14675969));
					list.AddRange(MessageBuilder.AddLength(1));
					list.AddRange(MessageBuilder.ConvertIntToBCD(fiscalItem.DeptId, 1));
					list.AddRange(MessageBuilder.HexToByteArray(14675971));
					array = MessageBuilder.ConvertDecimalToBCD(fiscalItem.Quantity, 3);
					list.AddRange(MessageBuilder.AddLength(array.Length));
					list.AddRange(array);
					fiscalItem.Price = TruncateDigitsAfterComma(fiscalItem.Price, 2);
					list.AddRange(MessageBuilder.HexToByteArray(14675972));
					array = MessageBuilder.ConvertDecimalToBCD(fiscalItem.Price, 3);
					list.AddRange(MessageBuilder.AddLength(array.Length));
					list.AddRange(array);
					if (!string.IsNullOrEmpty(fiscalItem.Name))
					{
						list.AddRange(MessageBuilder.HexToByteArray(14675992));
						list.AddRange(MessageBuilder.AddLength(fiscalItem.Name.Length));
						list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(Utils.FixTurkishUpperCase(fiscalItem.Name)));
						if (!string.IsNullOrEmpty(fiscalItem.NoteLine1))
						{
							list.AddRange(MessageBuilder.HexToByteArray(14675992));
							list.AddRange(MessageBuilder.AddLength(fiscalItem.NoteLine1.Length));
							list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(Utils.FixTurkishUpperCase(fiscalItem.NoteLine1)));
						}
						if (!string.IsNullOrEmpty(fiscalItem.NoteLine2))
						{
							list.AddRange(MessageBuilder.HexToByteArray(14675992));
							list.AddRange(MessageBuilder.AddLength(fiscalItem.NoteLine2.Length));
							list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(Utils.FixTurkishUpperCase(fiscalItem.NoteLine2)));
						}
					}
					if (fiscalItem.Status != -1)
					{
						list.AddRange(MessageBuilder.HexToByteArray(14675999));
						list.AddRange(MessageBuilder.AddLength(1));
						list.AddRange(MessageBuilder.ConvertIntToBCD(fiscalItem.Status, 1));
					}
					list.InsertRange(num, MessageBuilder.AddLength(list.Count - num));
					if (fiscalItem.Adj != null)
					{
						list.AddRange(MessageBuilder.HexToByteArray(57207));
						num = list.Count;
						if (fiscalItem.Adj.Type == AdjustmentType.PercentDiscount || fiscalItem.Adj.Type == AdjustmentType.PercentFee)
						{
							list.AddRange(MessageBuilder.HexToByteArray(14675985));
							list.AddRange(MessageBuilder.AddLength(2));
							list.AddRange(MessageBuilder.ConvertIntToBCD(Convert.ToInt32(fiscalItem.Adj.Percentage), 2));
						}
						else
						{
							fiscalItem.Adj.Amount = TruncateDigitsAfterComma(fiscalItem.Adj.Amount, 2);
							list.AddRange(MessageBuilder.HexToByteArray(14675972));
							array = MessageBuilder.ConvertDecimalToBCD(fiscalItem.Adj.Amount, 3);
							list.AddRange(MessageBuilder.AddLength(array.Length));
							list.AddRange(array);
						}
						if (fiscalItem.Adj.Type == AdjustmentType.Fee || fiscalItem.Adj.Type == AdjustmentType.PercentFee)
						{
							list.AddRange(MessageBuilder.HexToByteArray(14675999));
							list.Add(1);
							list.Add(1);
						}
						if (!string.IsNullOrEmpty(fiscalItem.Adj.NoteLine1))
						{
							list.AddRange(MessageBuilder.HexToByteArray(14675992));
							list.AddRange(MessageBuilder.AddLength(fiscalItem.Adj.NoteLine1.Length));
							list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(Utils.FixTurkishUpperCase(fiscalItem.Adj.NoteLine1)));
						}
						if (!string.IsNullOrEmpty(fiscalItem.Adj.NoteLine2))
						{
							list.AddRange(MessageBuilder.HexToByteArray(14675992));
							list.AddRange(MessageBuilder.AddLength(fiscalItem.Adj.NoteLine2.Length));
							list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(Utils.FixTurkishUpperCase(fiscalItem.Adj.NoteLine2)));
						}
						list.InsertRange(num, MessageBuilder.AddLength(list.Count - num));
					}
				}
			}
			list.AddRange(MessageBuilder.HexToByteArray(57203));
			num = list.Count;
			list.AddRange(MessageBuilder.HexToByteArray(14675972));
			list.AddRange(MessageBuilder.AddLength(1));
			list.AddRange(MessageBuilder.ConvertIntToBCD(1, 1));
			if (jSONDocument.EndOfReceiptInfo != null && jSONDocument.EndOfReceiptInfo.BarcodeFlag && !string.IsNullOrEmpty(jSONDocument.EndOfReceiptInfo.Barcode))
			{
				list.AddRange(MessageBuilder.HexToByteArray(14675980));
				list.AddRange(MessageBuilder.AddLength(jSONDocument.EndOfReceiptInfo.Barcode.Length));
				list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(jSONDocument.EndOfReceiptInfo.Barcode));
			}
			if (jSONDocument.FooterNotes != null && jSONDocument.FooterNotes.Count > 0)
			{
				foreach (string footerNote in jSONDocument.FooterNotes)
				{
					list.AddRange(MessageBuilder.HexToByteArray(14675992));
					list.AddRange(MessageBuilder.AddLength(footerNote.Length));
					list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(footerNote));
				}
			}
			list.InsertRange(num, MessageBuilder.AddLength(list.Count - num));
			foreach (Adjustment adjustment in jSONDocument.Adjustments)
			{
				num = 0;
				list.AddRange(MessageBuilder.HexToByteArray(57207));
				num = list.Count;
				if (adjustment.Type == AdjustmentType.PercentDiscount || adjustment.Type == AdjustmentType.PercentFee)
				{
					list.AddRange(MessageBuilder.HexToByteArray(14675985));
					list.AddRange(MessageBuilder.AddLength(2));
					list.AddRange(MessageBuilder.ConvertIntToBCD(Convert.ToInt32(adjustment.Percentage), 2));
				}
				else
				{
					adjustment.Amount = TruncateDigitsAfterComma(adjustment.Amount, 2);
					list.AddRange(MessageBuilder.HexToByteArray(14675972));
					array = MessageBuilder.ConvertDecimalToBCD(adjustment.Amount, 3);
					list.AddRange(MessageBuilder.AddLength(array.Length));
					list.AddRange(array);
				}
				if (adjustment.Type == AdjustmentType.Fee || adjustment.Type == AdjustmentType.PercentFee)
				{
					list.AddRange(MessageBuilder.HexToByteArray(14675999));
					list.Add(1);
					list.Add(1);
				}
				list.InsertRange(num, MessageBuilder.AddLength(list.Count - num));
			}
			if (jSONDocument.Payments != null && jSONDocument.Payments.Count > 0)
			{
				foreach (PaymentInfo payment in jSONDocument.Payments)
				{
					list.AddRange(MessageBuilder.HexToByteArray(57204));
					num = list.Count;
					list.AddRange(MessageBuilder.HexToByteArray(14675973));
					list.AddRange(MessageBuilder.AddLength(1));
					list.AddRange(MessageBuilder.ConvertIntToBCD((int)(payment.Type + 1), 1));
					if (payment.Type == PaymentType.FCURRENCY || payment.Type == PaymentType.CREDIT)
					{
						list.AddRange(MessageBuilder.HexToByteArray(14675984));
						list.AddRange(MessageBuilder.AddLength(1));
						list.AddRange(MessageBuilder.ConvertIntToBCD(payment.Index, 1));
					}
					if (payment.Type == PaymentType.CHECK && payment.Index > 0)
					{
						string text = payment.Index.ToString();
						list.AddRange(MessageBuilder.HexToByteArray(14675992));
						list.AddRange(MessageBuilder.AddLength(text.Length));
						list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(text));
					}
					payment.PaidTotal = TruncateDigitsAfterComma(payment.PaidTotal, 2);
					list.AddRange(MessageBuilder.HexToByteArray(14675972));
					array = MessageBuilder.ConvertDecimalToBCD(payment.PaidTotal, 3);
					list.AddRange(MessageBuilder.AddLength(array.Length));
					list.AddRange(array);
					list.InsertRange(num, MessageBuilder.AddLength(list.Count - num));
				}
			}
		}
		val = Send(new FPURequest((Command)46, list.ToArray()));
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		if (val.ErrorCode == 0)
		{
			GMPMessage val2 = GMPMessage.Parse(val.Data);
			GMPGroup val3 = val2.FindGroup(57152);
			if (val3 != null)
			{
				GMPField val4 = val3.FindTag(14675995);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.ConvertBcdToInt(val4.Value, 0, 4).ToString());
				}
				else
				{
					sFResponse.AddNull(1);
				}
				val4 = val3.FindTag(14675981);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.ConvertBcdToInt(val4.Value, 0, 4).ToString());
				}
				else
				{
					sFResponse.AddNull(1);
				}
				val4 = val3.FindTag(14647817);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, $"{MessageBuilder.ConvertBytesToDate(val4.Value, 0):yyyy-MM-dd}");
				}
				else
				{
					sFResponse.AddNull(1);
				}
				val4 = val3.FindTag(14647818);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, $"{MessageBuilder.ConvertBytesToTime(val4.Value, 0):HH:mm}");
				}
				else
				{
					sFResponse.AddNull(1);
				}
				val4 = val3.FindTag(14675977);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.ConvertBcdToInt(val4.Value, 0, 1).ToString());
				}
				else
				{
					sFResponse.AddNull(1);
				}
			}
			val3 = val2.FindGroup(57204);
			if (val3 != null)
			{
				sFResponse.Add(SFResponseLabel.PARAM, $"{val3.Tags.Count / 4}");
				foreach (GMPField tag in val3.Tags)
				{
					if (tag.Tag == 14675973)
					{
						int num2 = MessageBuilder.ConvertBcdToInt(tag.Value, 0, tag.Length);
						sFResponse.Add(SFResponseLabel.PARAM, $"{num2}");
					}
					if (tag.Tag == 14675984)
					{
						sFResponse.Add(SFResponseLabel.PARAM, $"{MessageBuilder.ConvertBcdToInt(tag.Value, 0, tag.Length)}");
					}
					if (tag.Tag == 14675972)
					{
						sFResponse.Add(SFResponseLabel.PARAM, $"{(decimal)MessageBuilder.ConvertBcdToInt(tag.Value, 0, tag.Length) / 1000m:#0.00}");
					}
					if (tag.Tag == 14675992)
					{
						sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.DefaultEncoding.GetString(tag.Value).Trim(new char[1]));
					}
				}
			}
		}
		return sFResponse.GetString();
	}

	public string PrintEDocumentCopy(int docType, string[] lines)
	{
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Expected O, but got Unknown
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string> { docType.ToString() });
		List<byte> list = new List<byte>();
		list.AddRange(MessageBuilder.HexToByteArray(14675977));
		list.AddRange(MessageBuilder.AddLength(2));
		list.AddRange(MessageBuilder.ConvertIntToBCD(docType, 2));
		foreach (string text in lines)
		{
			list.AddRange(MessageBuilder.HexToByteArray(14675992));
			list.AddRange(MessageBuilder.AddLength(48));
			list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(text.PadRight(48, ' ')));
		}
		FPUResponse val = Send(new FPURequest((Command)147, list.ToArray()));
		SFResponse sFResponse = new SFResponse();
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		return sFResponse.GetString();
	}

	public string PrintSlip(int type, string[] lines)
	{
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Expected O, but got Unknown
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string> { type.ToString() });
		List<byte> list = new List<byte>();
		list.AddRange(MessageBuilder.HexToByteArray(14675977));
		list.AddRange(MessageBuilder.AddLength(1));
		list.AddRange(MessageBuilder.ConvertIntToBCD(type, 1));
		foreach (string text in lines)
		{
			list.AddRange(MessageBuilder.HexToByteArray(14675992));
			list.AddRange(MessageBuilder.AddLength(text.Length));
			list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(text));
		}
		FPUResponse val = Send(new FPURequest((Command)173, list.ToArray()));
		SFResponse sFResponse = new SFResponse();
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		return sFResponse.GetString();
	}

	public string GetReportContent()
	{
		List<byte> list = new List<byte>();
		return SendReport((Command)64, list.ToArray());
	}

	public string PrintXReport(int copy)
	{
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string> { copy.ToString() });
		List<byte> list = new List<byte>();
		list.AddRange(MessageBuilder.HexToByteArray(14675999));
		list.Add(1);
		list.Add((byte)copy);
		return SendReport((Command)49, list.ToArray());
	}

	public string PrintXReport(int count, decimal amount, bool isAffectDrawer)
	{
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>
		{
			count.ToString(),
			amount.ToString(),
			isAffectDrawer.ToString()
		});
		List<byte> list = new List<byte>();
		int num = 0;
		list.AddRange(MessageBuilder.HexToByteArray(14675999));
		list.Add(1);
		list.Add((byte)num);
		byte[] array = MessageBuilder.ConvertIntToBCD(count, 2);
		list.AddRange(MessageBuilder.HexToByteArray(14675984));
		list.AddRange(MessageBuilder.AddLength(array.Length));
		list.AddRange(array);
		amount = TruncateDigitsAfterComma(amount, 2);
		array = MessageBuilder.ConvertDecimalToBCD(amount, 3);
		list.AddRange(MessageBuilder.HexToByteArray(14675972));
		list.AddRange(MessageBuilder.AddLength(array.Length));
		list.AddRange(array);
		int num2 = 0;
		if (isAffectDrawer)
		{
			num2 = 1;
		}
		list.AddRange(MessageBuilder.HexToByteArray(14675998));
		list.AddRange(MessageBuilder.AddLength(1));
		list.AddRange(MessageBuilder.ConvertIntToBCD(num2, 1));
		return SendReport((Command)49, list.ToArray());
	}

	public string PrintXPluReport(int firstPlu, int lastPlu, int copy)
	{
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>
		{
			firstPlu.ToString(),
			lastPlu.ToString(),
			copy.ToString()
		});
		List<byte> list = new List<byte>();
		list.AddRange(MessageBuilder.HexToByteArray(14675999));
		list.Add(1);
		list.Add((byte)copy);
		list.AddRange(MessageBuilder.HexToByteArray(14675970));
		list.AddRange(MessageBuilder.AddLength(3));
		list.AddRange(MessageBuilder.ConvertIntToBCD(firstPlu, 3));
		list.AddRange(MessageBuilder.HexToByteArray(14675970));
		list.AddRange(MessageBuilder.AddLength(3));
		list.AddRange(MessageBuilder.ConvertIntToBCD(lastPlu, 3));
		return SendReport((Command)50, list.ToArray());
	}

	public string PrintSystemInfoReport(int copy)
	{
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string> { copy.ToString() });
		List<byte> list = new List<byte>();
		list.AddRange(MessageBuilder.HexToByteArray(14675999));
		list.Add(1);
		list.Add((byte)copy);
		return SendReport((Command)51, list.ToArray());
	}

	public string PrintReceiptTotalReport(int copy)
	{
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string> { copy.ToString() });
		List<byte> list = new List<byte>();
		list.AddRange(MessageBuilder.HexToByteArray(14675999));
		list.Add(1);
		list.Add((byte)copy);
		return SendReport((Command)52, list.ToArray());
	}

	protected string SendZReport(int copy, byte[] buffer)
	{
		List<byte> list = new List<byte>();
		list.AddRange(MessageBuilder.HexToByteArray(14675999));
		list.Add(1);
		list.Add((byte)copy);
		if (buffer != null && buffer.Length != 0)
		{
			list.AddRange(buffer);
		}
		return SendReport((Command)65, list.ToArray());
	}

	public string PrintZReport()
	{
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>());
		return SendZReport(3, null);
	}

	public string PrintZReport(int copy)
	{
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>());
		return SendZReport(copy, null);
	}

	public string PrintZReport(int count, decimal amount, bool isAffectDrawer)
	{
		return PrintZReportWReturn(3, count, amount, isAffectDrawer);
	}

	public string PrintZReport(int copy, int count, decimal amount, bool isAffectDrawer)
	{
		return PrintZReportWReturn(copy, count, amount, isAffectDrawer);
	}

	private string PrintZReportWReturn(int copy, int count, decimal amount, bool isAffectDrawer)
	{
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>());
		List<byte> list = new List<byte>();
		byte[] array = MessageBuilder.ConvertIntToBCD(count, 2);
		list.AddRange(MessageBuilder.HexToByteArray(14675984));
		list.AddRange(MessageBuilder.AddLength(array.Length));
		list.AddRange(array);
		amount = TruncateDigitsAfterComma(amount, 2);
		array = MessageBuilder.ConvertDecimalToBCD(amount, 3);
		list.AddRange(MessageBuilder.HexToByteArray(14675972));
		list.AddRange(MessageBuilder.AddLength(array.Length));
		list.AddRange(array);
		int num = 0;
		if (isAffectDrawer)
		{
			num = 1;
		}
		list.AddRange(MessageBuilder.HexToByteArray(14675998));
		list.AddRange(MessageBuilder.AddLength(1));
		list.AddRange(MessageBuilder.ConvertIntToBCD(num, 1));
		return SendZReport(3, list.ToArray());
	}

	public string PrintPeriodicZZReport(int firstZ, int lastZ, int copy, bool detail)
	{
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>
		{
			firstZ.ToString(),
			lastZ.ToString(),
			copy.ToString(),
			detail.ToString()
		});
		List<byte> list = new List<byte>();
		list.AddRange(MessageBuilder.HexToByteArray(14675999));
		list.Add(1);
		list.Add((byte)copy);
		list.AddRange(MessageBuilder.HexToByteArray(14675995));
		list.AddRange(MessageBuilder.AddLength(3));
		list.AddRange(MessageBuilder.ConvertIntToBCD(firstZ, 3));
		list.AddRange(MessageBuilder.HexToByteArray(14675995));
		list.AddRange(MessageBuilder.AddLength(3));
		list.AddRange(MessageBuilder.ConvertIntToBCD(lastZ, 3));
		Command command = (Command)66;
		if (detail)
		{
			command = (Command)67;
		}
		return SendReport(command, list.ToArray());
	}

	public string PrintPeriodicDateReport(DateTime firstDay, DateTime lastDay, int copy, bool detail)
	{
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>
		{
			firstDay.ToString(),
			lastDay.ToString(),
			copy.ToString(),
			detail.ToString()
		});
		List<byte> list = new List<byte>();
		list.AddRange(MessageBuilder.HexToByteArray(14675999));
		list.Add(1);
		list.Add((byte)copy);
		list.AddRange(MessageBuilder.GetDateInBytes(firstDay));
		list.AddRange(MessageBuilder.GetDateInBytes(lastDay));
		Command command = (Command)68;
		if (detail)
		{
			command = (Command)69;
		}
		return SendReport(command, list.ToArray());
	}

	public string PrintEJPeriodic(DateTime day, int copy)
	{
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>
		{
			day.ToString(),
			copy.ToString()
		});
		List<byte> list = new List<byte>();
		list.AddRange(MessageBuilder.HexToByteArray(14675999));
		list.Add(1);
		list.Add((byte)copy);
		list.AddRange(MessageBuilder.GetDateInBytes(day));
		list.AddRange(MessageBuilder.GetTimeInBytes(day));
		return SendReport((Command)83, list.ToArray());
	}

	public string PrintEJPeriodic(DateTime startTime, DateTime endTime, int copy)
	{
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>
		{
			startTime.ToString(),
			endTime.ToString(),
			copy.ToString()
		});
		List<byte> list = new List<byte>();
		list.AddRange(MessageBuilder.HexToByteArray(14675999));
		list.Add(1);
		list.Add((byte)copy);
		list.AddRange(MessageBuilder.GetDateTimeInBytes(startTime));
		list.AddRange(MessageBuilder.GetDateTimeInBytes(endTime));
		return SendReport((Command)83, list.ToArray());
	}

	public string PrintODocPeriodic(DateTime firstDate, DateTime lastDate, int oDocType)
	{
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>
		{
			firstDate.ToString(),
			lastDate.ToString(),
			oDocType.ToString()
		});
		List<byte> list = new List<byte>();
		list.AddRange(MessageBuilder.GetDateInBytes(firstDate));
		list.AddRange(MessageBuilder.GetDateInBytes(lastDate));
		list.AddRange(MessageBuilder.HexToByteArray(14675977));
		list.Add(1);
		list.Add((byte)oDocType);
		return SendReport((Command)84, list.ToArray());
	}

	public string PrintODocPeriodic(int oDocType)
	{
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string> { oDocType.ToString() });
		List<byte> list = new List<byte>();
		list.AddRange(MessageBuilder.HexToByteArray(14675977));
		list.Add(1);
		list.Add((byte)oDocType);
		return SendReport((Command)84, list.ToArray());
	}

	public string PrintEJPeriodic(int ZStartId, int docStartId, int ZEndId, int docEndId, int copy)
	{
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>
		{
			ZStartId.ToString(),
			docStartId.ToString(),
			ZEndId.ToString(),
			docEndId.ToString(),
			copy.ToString()
		});
		return PrintEJReport(ZStartId, docStartId, ZEndId, docEndId, copy);
	}

	public string PrintEJDetail(int copy)
	{
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string> { copy.ToString() });
		List<byte> list = new List<byte>();
		list.AddRange(MessageBuilder.HexToByteArray(14675999));
		list.Add(1);
		list.Add((byte)copy);
		return SendReport((Command)81, list.ToArray());
	}

	public string PrintEndDayReport()
	{
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>());
		return SendReport((Command)166, new byte[0]);
	}

	public string EnterServiceMode(string password)
	{
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string> { password });
		List<byte> list = new List<byte>();
		list.AddRange(MessageBuilder.HexToByteArray(14675994));
		list.AddRange(MessageBuilder.AddLength(password.Length));
		list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(password));
		return SendCommand((Command)144, list.ToArray());
	}

	public string ExitServiceMode(string password)
	{
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string> { password });
		List<byte> list = new List<byte>();
		list.AddRange(MessageBuilder.HexToByteArray(14675994));
		list.AddRange(MessageBuilder.AddLength(password.Length));
		list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(password));
		return SendCommand((Command)111, list.ToArray());
	}

	public string ClearDailyMemory()
	{
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>());
		return SendCommand((Command)97, null);
	}

	public string FactorySettings()
	{
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>());
		return SendCommand((Command)99, null);
	}

	public string CloseFM()
	{
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>());
		return SendCommand((Command)100, null);
	}

	public string SetExternalDevAddress(string ip, int port)
	{
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>
		{
			ip,
			port.ToString()
		});
		List<byte> list = new List<byte>();
		string[] array = ip.Split(new char[1] { ',' });
		list.AddRange(MessageBuilder.HexToByteArray(14663941));
		list.AddRange(MessageBuilder.AddLength(6));
		string text = "";
		for (int i = 0; i < array.Length; i++)
		{
			text += int.Parse(array[i]).ToString().PadLeft(3, '0');
		}
		for (int j = 0; j < text.Length / 2; j++)
		{
			list.AddRange(MessageBuilder.ConvertIntToBCD(int.Parse(text.Substring(j * 2, 2)), 1));
		}
		if (port > 0)
		{
			byte[] array2 = MessageBuilder.ConvertIntToBCD(Convert.ToInt32(port), port.ToString().Length);
			list.AddRange(MessageBuilder.HexToByteArray(14676004));
			list.AddRange(MessageBuilder.AddLength(array2.Length));
			list.AddRange(array2);
		}
		return SendCommand((Command)101, list.ToArray());
	}

	public string UpdateFirmware(string ip, int port)
	{
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>
		{
			ip,
			port.ToString()
		});
		List<byte> list = new List<byte>();
		list.AddRange(MessageBuilder.HexToByteArray(14663941));
		list.AddRange(MessageBuilder.AddLength(6));
		string[] array = ip.Split(new char[1] { ',' });
		string text = "";
		for (int i = 0; i < array.Length; i++)
		{
			text += int.Parse(array[i]).ToString().PadLeft(3, '0');
		}
		for (int j = 0; j < text.Length / 2; j++)
		{
			list.AddRange(MessageBuilder.ConvertIntToBCD(int.Parse(text.Substring(j * 2, 2)), 1));
		}
		byte[] array2 = MessageBuilder.ConvertIntToBCD(port, port.ToString().Length);
		list.AddRange(MessageBuilder.HexToByteArray(14676004));
		list.AddRange(MessageBuilder.AddLength(array2.Length));
		list.AddRange(array2);
		return SendCommand((Command)102, list.ToArray());
	}

	public string PrintLogs(DateTime date)
	{
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string> { date.ToString() });
		List<byte> list = new List<byte>();
		list.AddRange(MessageBuilder.GetDateInBytes(date));
		int connTimeout = secureComm.ConnTimeout;
		secureComm.ConnTimeout = 60000;
		try
		{
			return SendCommand((Command)103, list.ToArray());
		}
		finally
		{
			secureComm.ConnTimeout = connTimeout;
		}
	}

	public string CreateDB()
	{
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>());
		int connTimeout = secureComm.ConnTimeout;
		secureComm.ConnTimeout = 60000;
		try
		{
			return SendCommand((Command)104, null);
		}
		finally
		{
			secureComm.ConnTimeout = connTimeout;
		}
	}

	public bool Connect(object commObj, DeviceInfo sInfo)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Expected O, but got Unknown
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		serverInfo = sInfo;
		if (File.Exists("T300.txt") || fiscalId.StartsWith("FT"))
		{
			secureComm = (ISecureComm)new T300SecureComm(serverInfo, fiscalId, (string)null);
		}
		else
		{
			secureComm = (ISecureComm)new SecureComm(serverInfo, fiscalId, (string)null);
		}
		secureComm.SetCommObject(commObj);
		bool flag = secureComm.Connect();
		if (flag)
		{
			secureComm.ConnTimeout = 15000;
			supportedBufferSize = secureComm.BufferSize;
			if (BackgroundWorker.IsThreadAvailableForRestart)
			{
				Thread thread = new Thread(BackgroundWorker.Start);
				thread.Name = "BackgroundWorkerThread";
				thread.IsBackground = true;
				thread.Priority = ThreadPriority.BelowNormal;
				thread.Start(this);
			}
		}
		return flag;
	}

	public void SetCommObject(object commObj)
	{
		secureComm.SetCommObject(commObj);
		secureComm.ConnTimeout = 15000;
	}

	public string CheckPrinterStatus()
	{
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>());
		return SendCommand((Command)129, null);
	}

	public string GetLastResponse()
	{
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>());
		return SendCommand((Command)130, null);
	}

	public string CashIn(decimal amount)
	{
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string> { amount.ToString() });
		byte[] array = null;
		List<byte> list = new List<byte>();
		amount = TruncateDigitsAfterComma(amount, 2);
		array = MessageBuilder.ConvertDecimalToBCD(amount, 3);
		list.AddRange(MessageBuilder.HexToByteArray(14675972));
		list.AddRange(MessageBuilder.AddLength(array.Length));
		list.AddRange(array);
		return SendCommand((Command)137, list.ToArray());
	}

	public string CashOut(decimal amount)
	{
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string> { amount.ToString() });
		byte[] array = null;
		List<byte> list = new List<byte>();
		amount = TruncateDigitsAfterComma(amount, 2);
		array = MessageBuilder.ConvertDecimalToBCD(amount, 3);
		list.AddRange(MessageBuilder.HexToByteArray(14675972));
		list.AddRange(MessageBuilder.AddLength(array.Length));
		list.AddRange(array);
		return SendCommand((Command)138, list.ToArray());
	}

	public string ChangeKeyLockStatus(bool isLock)
	{
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string> { isLock.ToString() });
		byte[] array = null;
		List<byte> list = new List<byte>();
		array = MessageBuilder.ConvertIntToBCD(isLock ? 1 : 0, 1);
		list.AddRange(MessageBuilder.HexToByteArray(14676035));
		list.AddRange(MessageBuilder.AddLength(array.Length));
		list.AddRange(array);
		return SendCommand((Command)176, list.ToArray());
	}

	public string CloseNFReceipt()
	{
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>());
		return SendCommand((Command)141, null);
	}

	public string Fiscalize(string password)
	{
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string> { password });
		List<byte> list = new List<byte>();
		list.AddRange(MessageBuilder.HexToByteArray(14675994));
		list.AddRange(MessageBuilder.AddLength(password.Length));
		list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(password));
		int connTimeout = secureComm.ConnTimeout;
		secureComm.ConnTimeout = 60000;
		try
		{
			return SendCommand((Command)133, list.ToArray());
		}
		finally
		{
			secureComm.ConnTimeout = connTimeout;
		}
	}

	public string StartFMTest()
	{
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>());
		List<byte> list = new List<byte>();
		list.AddRange(MessageBuilder.HexToByteArray(14675999));
		list.AddRange(MessageBuilder.AddLength(1));
		list.AddRange(MessageBuilder.ConvertIntToBCD(1, 1));
		return SendCommand((Command)133, list.ToArray());
	}

	public string GetDrawerInfo()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>());
		SFResponse sFResponse = new SFResponse();
		FPUResponse val = Send(new FPURequest((Command)115, new byte[0]));
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		GMPMessage val2 = GMPMessage.Parse(val.Data);
		GMPGroup val3 = val2.FindGroup(57203);
		foreach (GMPField tag in val3.Tags)
		{
			if (tag.Tag == 14675971)
			{
				sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.ConvertBcdToInt(tag.Value, 0, tag.Length).ToString());
			}
			if (tag.Tag == 14675972)
			{
				sFResponse.Add(SFResponseLabel.PARAM, ((decimal)MessageBuilder.ConvertBcdToInt(tag.Value, 0, tag.Length) / 1000m).ToString());
			}
		}
		val3 = val2.FindGroup(57201);
		foreach (GMPField tag2 in val3.Tags)
		{
			if (tag2.Tag == 14675971)
			{
				sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.ConvertBcdToInt(tag2.Value, 0, tag2.Length));
			}
			if (tag2.Tag == 14675972)
			{
				sFResponse.Add(SFResponseLabel.PARAM, ((decimal)MessageBuilder.ConvertBcdToInt(tag2.Value, 0, tag2.Length) / 1000m).ToString());
			}
		}
		val3 = val2.FindGroup(57202);
		foreach (GMPField tag3 in val3.Tags)
		{
			if (tag3.Tag == 14675971)
			{
				sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.ConvertBcdToInt(tag3.Value, 0, tag3.Length).ToString());
			}
			if (tag3.Tag == 14675972)
			{
				sFResponse.Add(SFResponseLabel.PARAM, ((decimal)MessageBuilder.ConvertBcdToInt(tag3.Value, 0, tag3.Length) / 1000m).ToString());
			}
		}
		val3 = val2.FindGroup(57207);
		foreach (GMPField tag4 in val3.Tags)
		{
			if (tag4.Tag == 14675972)
			{
				sFResponse.Add(SFResponseLabel.PARAM, ((decimal)MessageBuilder.ConvertBcdToInt(tag4.Value, 0, tag4.Length) / 1000m).ToString());
			}
		}
		val3 = val2.FindGroup(57212);
		if (val3 != null && val3.Tags.Count > 0)
		{
			bool flag = true;
			int num = 0;
			foreach (GMPField tag5 in val3.Tags)
			{
				if (tag5.Tag == 14675973)
				{
					int num2 = MessageBuilder.ConvertBcdToInt(tag5.Value, 0, tag5.Length);
					if (flag && num2 == 2)
					{
						sFResponse.AddNull(3);
					}
					else
					{
						num++;
					}
					flag = false;
					sFResponse.Add(SFResponseLabel.PARAM, num2.ToString());
				}
				if (tag5.Tag == 14675971)
				{
					sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.ConvertBcdToInt(tag5.Value, 0, tag5.Length).ToString());
				}
				if (tag5.Tag == 14675972)
				{
					sFResponse.Add(SFResponseLabel.PARAM, (decimal)MessageBuilder.ConvertBcdToInt(tag5.Value, 0, tag5.Length) / 1000m);
				}
			}
			if (num == 1)
			{
				sFResponse.AddNull(3);
			}
		}
		else
		{
			sFResponse.AddNull(6);
		}
		val3 = val2.FindGroup(57204);
		int num3 = 14675973;
		foreach (GMPField tag6 in val3.Tags)
		{
			if (tag6.Tag == 14675973)
			{
				sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.ConvertBcdToInt(tag6.Value, 0, tag6.Length) - 1);
				num3 = 14675973;
			}
			if (tag6.Tag == 14675984)
			{
				sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.ConvertBcdToInt(tag6.Value, 0, tag6.Length));
				num3 = 14675984;
			}
			if (tag6.Tag == 14675972)
			{
				if (num3 == 14675973)
				{
					sFResponse.AddNull(1);
				}
				sFResponse.Add(SFResponseLabel.PARAM, (decimal)MessageBuilder.ConvertBcdToInt(tag6.Value, 0, tag6.Length) / 1000m);
				num3 = 14675972;
			}
		}
		return sFResponse.GetString();
	}

	public string GetDailySummary()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>());
		SFResponse sFResponse = new SFResponse();
		FPUResponse val = Send(new FPURequest((Command)117, new byte[0]));
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		GMPMessage val2 = GMPMessage.Parse(val.Data);
		DailySummary dailySummary = new DailySummary();
		if (val2.GetGroupList() != null)
		{
			int num = val2.GetGroupList().Length;
			for (int i = 0; i < num; i++)
			{
				GMPGroup val3 = val2.FindGroup(57206 + i);
				if (val3 == null)
				{
					break;
				}
				SummaryItem summaryItem = new SummaryItem();
				foreach (GMPField tag in val3.Tags)
				{
					switch (tag.Tag)
					{
					case 14675977:
						summaryItem.DocumentType = MessageBuilder.ConvertBcdToInt(tag.Value, 0, tag.Length);
						break;
					case 14676052:
						summaryItem.ValidCount = MessageBuilder.ConvertBcdToInt(tag.Value, 0, tag.Length);
						break;
					case 14676053:
						summaryItem.ValidAmount = (double)MessageBuilder.ConvertBcdToInt(tag.Value, 0, tag.Length) / 1000.0;
						break;
					case 14676054:
						summaryItem.CancelledCount = MessageBuilder.ConvertBcdToInt(tag.Value, 0, tag.Length);
						break;
					case 14676055:
						summaryItem.CancelledAmount = (double)MessageBuilder.ConvertBcdToInt(tag.Value, 0, tag.Length) / 1000.0;
						break;
					case 14676050:
						summaryItem.VatTotal = (double)MessageBuilder.ConvertBcdToInt(tag.Value, 0, tag.Length) / 1000.0;
						break;
					case 14676056:
					{
						int num2 = 0;
						VatGroupSale vatGroupSale = new VatGroupSale();
						vatGroupSale.VatRate = MessageBuilder.ConvertBcdToInt(tag.Value, num2, 1);
						num2++;
						vatGroupSale.VatAmount = (double)MessageBuilder.ConvertBcdToInt(tag.Value, num2, 5) / 1000.0;
						num2 += 5;
						vatGroupSale.SaleAmount = (double)MessageBuilder.ConvertBcdToInt(tag.Value, num2, 5) / 1000.0;
						num2 += 5;
						summaryItem.VatGroupSales.Add(vatGroupSale);
						break;
					}
					case 14676057:
						summaryItem.Cash = (double)MessageBuilder.ConvertBcdToInt(tag.Value, 0, tag.Length) / 1000.0;
						break;
					case 14676058:
						summaryItem.Credit = (double)MessageBuilder.ConvertBcdToInt(tag.Value, 0, tag.Length) / 1000.0;
						break;
					case 14676059:
						summaryItem.Other = (double)MessageBuilder.ConvertBcdToInt(tag.Value, 0, tag.Length) / 1000.0;
						break;
					}
				}
				dailySummary.summaries.Add(summaryItem);
			}
		}
		GMPGroup val4 = val2.FindGroup(57203);
		if (val4 != null)
		{
			foreach (GMPField tag2 in val4.Tags)
			{
				switch (tag2.Tag)
				{
				case 14675972:
					dailySummary.CummulativeTotal = (double)MessageBuilder.ConvertBcdToInt(tag2.Value, 0, tag2.Length) / 1000.0;
					break;
				case 14676050:
					dailySummary.CummulativeVat = (double)MessageBuilder.ConvertBcdToInt(tag2.Value, 0, tag2.Length) / 1000.0;
					break;
				}
			}
		}
		string value = JsonConvert.SerializeObject((object)dailySummary);
		sFResponse.Add(SFResponseLabel.PARAM, value);
		return sFResponse.GetString();
	}

	public string GetLastDocumentInfo(bool lastZ)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string> { lastZ.ToString() });
		Command val = (Command)(lastZ ? 113 : 114);
		FPUResponse val2 = Send(new FPURequest(val, new byte[0]));
		SFResponse sFResponse = new SFResponse();
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val2.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val2.FPUState);
		if (val2.ErrorCode == 0 && val2.Data != null)
		{
			GMPMessage val3 = GMPMessage.Parse(val2.Data);
			GMPGroup val4 = val3.FindGroup(57152);
			if (val4 != null)
			{
				GMPField val5 = val4.FindTag(14675981);
				if (val5 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.ConvertBcdToInt(val5.Value, 0, 4));
				}
				else
				{
					sFResponse.AddNull(1);
				}
				val5 = val4.FindTag(14675995);
				if (val5 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.ConvertBcdToInt(val5.Value, 0, 4));
				}
				else
				{
					sFResponse.AddNull(1);
				}
				val5 = val4.FindTag(14675996);
				if (val5 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.ConvertBcdToInt(val5.Value, 0, 4));
				}
				else
				{
					sFResponse.AddNull(1);
				}
				val5 = val4.FindTag(14675977);
				if (val5 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.ConvertBcdToInt(val5.Value, 0, 1));
				}
				else
				{
					sFResponse.AddNull(1);
				}
				val5 = val4.FindTag(14647817);
				if (val5 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, $"{MessageBuilder.ConvertBytesToDate(val5.Value, 0):yyyy-MM-dd}");
				}
				else
				{
					sFResponse.AddNull(1);
				}
				val5 = val4.FindTag(14647818);
				if (val5 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, $"{MessageBuilder.ConvertBytesToTime(val5.Value, 0):HH:mm}");
				}
				else
				{
					sFResponse.AddNull(1);
				}
			}
			else
			{
				sFResponse.AddNull(6);
			}
			val4 = val3.FindGroup(57203);
			if (val4 != null)
			{
				foreach (GMPField tag in val4.Tags)
				{
					if (tag.Tag == 14675971)
					{
						sFResponse.Add(SFResponseLabel.PARAM, $"{MessageBuilder.ConvertBcdToInt(tag.Value, 0, tag.Length)}");
					}
					if (tag.Tag == 14675972)
					{
						sFResponse.Add(SFResponseLabel.PARAM, $"{(decimal)MessageBuilder.ConvertBcdToInt(tag.Value, 0, tag.Length) / 1000m:#0.00}");
					}
				}
			}
			else
			{
				sFResponse.AddNull(2);
			}
			val4 = val3.FindGroup(57201);
			if (val4 != null)
			{
				foreach (GMPField tag2 in val4.Tags)
				{
					if (tag2.Tag == 14675971)
					{
						sFResponse.Add(SFResponseLabel.PARAM, $"{MessageBuilder.ConvertBcdToInt(tag2.Value, 0, tag2.Length)}");
					}
					if (tag2.Tag == 14675972)
					{
						sFResponse.Add(SFResponseLabel.PARAM, $"{(decimal)MessageBuilder.ConvertBcdToInt(tag2.Value, 0, tag2.Length) / 1000m:#0.00}");
					}
				}
			}
			else
			{
				sFResponse.AddNull(2);
			}
			val4 = val3.FindGroup(57202);
			if (val4 != null)
			{
				foreach (GMPField tag3 in val4.Tags)
				{
					if (tag3.Tag == 14675971)
					{
						sFResponse.Add(SFResponseLabel.PARAM, $"{MessageBuilder.ConvertBcdToInt(tag3.Value, 0, tag3.Length)}");
					}
					if (tag3.Tag == 14675972)
					{
						sFResponse.Add(SFResponseLabel.PARAM, $"{(decimal)MessageBuilder.ConvertBcdToInt(tag3.Value, 0, tag3.Length) / 1000m:#0.00}");
					}
				}
			}
			else
			{
				sFResponse.AddNull(2);
			}
			val4 = val3.FindGroup(57207);
			if (val4 != null)
			{
				foreach (GMPField tag4 in val4.Tags)
				{
					if (tag4.Tag == 14675972)
					{
						sFResponse.Add(SFResponseLabel.PARAM, $"{(decimal)MessageBuilder.ConvertBcdToInt(tag4.Value, 0, tag4.Length) / 1000m:#0.00}");
					}
				}
			}
			else
			{
				sFResponse.AddNull(1);
			}
			val4 = val3.FindGroup(57204);
			if (val4 != null)
			{
				foreach (GMPField tag5 in val4.Tags)
				{
					if (tag5.Tag == 14675973)
					{
						int num = MessageBuilder.ConvertBcdToInt(tag5.Value, 0, tag5.Length);
						sFResponse.Add(SFResponseLabel.PARAM, $"{num}");
					}
					if (tag5.Tag == 14675984)
					{
						sFResponse.Add(SFResponseLabel.PARAM, $"{MessageBuilder.ConvertBcdToInt(tag5.Value, 0, tag5.Length)}");
					}
					if (tag5.Tag == 14675972)
					{
						sFResponse.Add(SFResponseLabel.PARAM, $"{(decimal)MessageBuilder.ConvertBcdToInt(tag5.Value, 0, tag5.Length) / 1000m:#0.00}");
					}
				}
			}
			else
			{
				sFResponse.AddNull(3);
			}
		}
		return sFResponse.GetString();
	}

	public string GetServiceCode()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>());
		string value = "";
		FPUResponse val = Send(new FPURequest((Command)143, (byte[])null));
		SFResponse sFResponse = new SFResponse();
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		if (val.ErrorCode == 0 && val.Data != null)
		{
			GMPMessage val2 = GMPMessage.Parse(val.Data);
			GMPGroup val3 = val2.FindGroup(57152);
			if (val3 != null)
			{
				GMPField val4 = val3.FindTag(14675984);
				if (val4 != null)
				{
					value = $"{MessageBuilder.ConvertBcdToInt(val4.Value, 0, 2):D4}";
				}
			}
		}
		sFResponse.Add(SFResponseLabel.PARAM, value);
		return sFResponse.GetString();
	}

	public string InterruptReport()
	{
		return ClearError();
	}

	public string ClearError()
	{
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>());
		string text = SendCommand((Command)131, null);
		if (NoPaperFlag && JSONDocAfterNoPaper != null && JSONDocAfterNoPaper.FiscalItems.Count > 0)
		{
			decimal num = 0.0m;
			int num2 = 1;
			SFResponseObject objectByString;
			try
			{
				objectByString = SFResponse.GetObjectByString(PrintSubtotal(isQuery: true));
			}
			catch (Exception ex)
			{
				if (num2 != 1)
				{
					throw ex;
				}
				Thread.Sleep(200);
				objectByString = SFResponse.GetObjectByString(PrintSubtotal(isQuery: true));
			}
			if (objectByString.errorCode == 0)
			{
				num = decimal.Parse(objectByString.paramList[0]);
				decimal num3 = 0.0m;
				while (num3 != num && JSONDocAfterNoPaper.FiscalItems.Count != 0)
				{
					if (JSONDocAfterNoPaper.FiscalItems[0].Adj != null)
					{
						decimal num4 = Math.Round(JSONDocAfterNoPaper.FiscalItems[0].Quantity * JSONDocAfterNoPaper.FiscalItems[0].Price, 2);
						num3 += Math.Round(num4 + JSONDocAfterNoPaper.FiscalItems[0].Adj.Amount, 2);
					}
					else
					{
						num3 += Math.Round(JSONDocAfterNoPaper.FiscalItems[0].Quantity * JSONDocAfterNoPaper.FiscalItems[0].Price, 2);
					}
					JSONDocAfterNoPaper.FiscalItems.RemoveAt(0);
				}
				if (JSONDocAfterNoPaper.FiscalItems.Count > 0)
				{
					string jsonStr = JsonConvert.SerializeObject((object)JSONDocAfterNoPaper);
					Thread.Sleep(50);
					text = PrintJSONDocumentDeptOnly(jsonStr);
				}
				else if (lastStatus == 2)
				{
					string jsonStr2 = JsonConvert.SerializeObject((object)JSONDocAfterNoPaper);
					Thread.Sleep(50);
					text = PrintJSONDocumentDeptOnly(jsonStr2);
				}
				else if (lastStatus == 5)
				{
					text = CloseReceipt(slipCopy: false);
				}
			}
			else if (objectByString.errorCode == 3 && objectByString.statusCode == 5)
			{
				text = CloseReceipt(slipCopy: false);
			}
		}
		else if (!NoPaperFlag && !onReporting)
		{
			try
			{
				string lastDocumentInfo = GetLastDocumentInfo(lastZ: false);
				string[] array = lastDocumentInfo.Split(new char[1] { '|' });
				text = text + "|" + array[2] + "|" + array[3];
			}
			catch
			{
			}
		}
		NoPaperFlag = false;
		JSONDocAfterNoPaper = null;
		lastStatus = 1;
		return text;
	}

	public string OpenDrawer()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>());
		if (!secureComm.IsVx675)
		{
			FPUResponse val = Send(new FPURequest((Command)146, new byte[0]));
			SFResponse sFResponse = new SFResponse();
			sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
			sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
			return sFResponse.GetString();
		}
		return null;
	}

	public string SaveNetworkSettings(string ip, string subnet, string gateway)
	{
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Expected O, but got Unknown
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string> { ip, subnet, gateway });
		List<byte> list = new List<byte>();
		FPUResponse val = null;
		SFResponse sFResponse = new SFResponse();
		try
		{
			if (ip == string.Empty)
			{
				list.AddRange(MessageBuilder.HexToByteArray(14663941));
				list.AddRange(MessageBuilder.AddLength(0));
				list.AddRange(MessageBuilder.HexToByteArray(14675984));
				list.AddRange(MessageBuilder.AddLength(1));
				list.AddRange(MessageBuilder.ConvertIntToBCD(0, 1));
			}
			else
			{
				list.AddRange(ConvertAddIp(ip));
				list.AddRange(MessageBuilder.HexToByteArray(14675984));
				list.AddRange(MessageBuilder.AddLength(1));
				list.AddRange(MessageBuilder.ConvertIntToBCD(0, 1));
				if (subnet != string.Empty)
				{
					list.AddRange(ConvertAddIp(subnet));
					list.AddRange(MessageBuilder.HexToByteArray(14675984));
					list.AddRange(MessageBuilder.AddLength(1));
					list.AddRange(MessageBuilder.ConvertIntToBCD(1, 1));
				}
				if (gateway != string.Empty)
				{
					list.AddRange(ConvertAddIp(gateway));
					list.AddRange(MessageBuilder.HexToByteArray(14675984));
					list.AddRange(MessageBuilder.AddLength(1));
					list.AddRange(MessageBuilder.ConvertIntToBCD(2, 1));
				}
			}
			val = Send(new FPURequest((Command)29, list.ToArray()));
			sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
			sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		}
		catch (Exception ex)
		{
			throw ex;
		}
		return sFResponse.GetString();
	}

	public string SetEJLimit(int index)
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Expected O, but got Unknown
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string> { index.ToString() });
		List<byte> list = new List<byte>();
		list.AddRange(MessageBuilder.HexToByteArray(14676005));
		list.AddRange(MessageBuilder.AddLength(3));
		list.AddRange(MessageBuilder.ConvertIntToBCD(index, 3));
		FPUResponse val = Send(new FPURequest((Command)142, list.ToArray()));
		SFResponse sFResponse = new SFResponse();
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		return sFResponse.GetString();
	}

	public string StartEJ()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>());
		FPUResponse val = null;
		int connTimeout = secureComm.ConnTimeout;
		secureComm.ConnTimeout = 120000;
		try
		{
			val = Send(new FPURequest((Command)134, new byte[0]));
		}
		finally
		{
			secureComm.ConnTimeout = connTimeout;
		}
		SFResponse sFResponse = new SFResponse();
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		return sFResponse.GetString();
	}

	public string StartFM(int fiscalNo)
	{
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string> { fiscalNo.ToString() });
		byte[] array = null;
		List<byte> list = new List<byte>();
		int num = fiscalNo;
		array = MessageBuilder.ConvertIntToBCD(num, 4);
		list.AddRange(MessageBuilder.HexToByteArray(14675978));
		list.AddRange(MessageBuilder.AddLength(array.Length));
		list.AddRange(array);
		int connTimeout = secureComm.ConnTimeout;
		secureComm.ConnTimeout = 120000;
		try
		{
			return SendCommand((Command)132, list.ToArray());
		}
		finally
		{
			secureComm.ConnTimeout = connTimeout;
		}
	}

	public string StartNFReceipt()
	{
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>());
		return SendCommand((Command)139, null);
	}

	public string StartNFDocument(int documentType)
	{
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string> { documentType.ToString() });
		List<byte> list = new List<byte>();
		list.AddRange(MessageBuilder.HexToByteArray(14675977));
		list.AddRange(MessageBuilder.AddLength(2));
		list.AddRange(MessageBuilder.ConvertIntToBCD(documentType, 2));
		return SendCommand((Command)139, list.ToArray());
	}

	public string TransferFile(string fileName)
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Expected O, but got Unknown
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string> { fileName });
		SFResponse sFResponse = new SFResponse();
		List<byte> list = new List<byte>();
		list.AddRange(MessageBuilder.HexToByteArray(14675976));
		list.AddRange(MessageBuilder.AddLength(fileName.Length));
		list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(fileName));
		FPUResponse val = Send(new FPURequest((Command)145, list.ToArray()));
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		if (val.ErrorCode == 0 && val.Detail != null && val.Detail.Length != 0)
		{
			string text = Directory.GetCurrentDirectory() + "/";
			List<byte> list2 = new List<byte>();
			GMPField[] detail = val.Detail;
			foreach (GMPField val2 in detail)
			{
				if (val2.Tag == 14675976)
				{
					fileName = MessageBuilder.DefaultEncoding.GetString(val2.Value);
				}
				if (val2.Tag == 14675992)
				{
					list2.AddRange(val2.Value);
				}
			}
			if (fileName != string.Empty)
			{
				if (!Directory.Exists(text))
				{
					Directory.CreateDirectory(text);
				}
				fileName = text + Path.GetFileName(fileName);
				if (File.Exists(fileName))
				{
					File.Delete(fileName);
				}
				File.Create(fileName).Close();
				File.WriteAllBytes(fileName, list2.ToArray());
			}
		}
		return sFResponse.GetString();
	}

	public string WriteNFLine(string[] lines)
	{
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Expected O, but got Unknown
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Expected O, but got Unknown
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>(lines));
		FPUResponse val = null;
		List<byte> list = new List<byte>();
		foreach (string text in lines)
		{
			list.AddRange(MessageBuilder.HexToByteArray(14675992));
			list.AddRange(MessageBuilder.AddLength(48));
			list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(text.PadRight(48, ' ')));
			if (list.Count > secureComm.BufferSize - 250)
			{
				val = Send(new FPURequest((Command)140, list.ToArray()));
				list.Clear();
			}
		}
		if (list.Count > 0)
		{
			val = Send(new FPURequest((Command)140, list.ToArray()));
		}
		SFResponse sFResponse = new SFResponse();
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		return sFResponse.GetString();
	}

	private string GetPrinterDate()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		FPUResponse val = Send(new FPURequest((Command)129, new byte[0]));
		DateTime dateTime = default(DateTime);
		GMPMessage val2 = GMPMessage.Parse(val.Data);
		GMPGroup val3 = val2.FindGroup(57152);
		GMPField val4 = val3.FindTag(14647817);
		if (val4 != null)
		{
			dateTime = MessageBuilder.ConvertBytesToDate(val4.Value, 0);
		}
		val4 = val3.FindTag(14647818);
		if (val4 != null)
		{
			DateTime dateTime2 = MessageBuilder.ConvertBytesToTime(val4.Value, 0);
			dateTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime2.Hour, dateTime2.Minute, dateTime2.Second);
		}
		return dateTime.ToString("dd/MM/yyyy HH:mm");
	}

	private ProgramOption ParsePrmOption(byte[] data)
	{
		ProgramOption programOption = new ProgramOption();
		GMPMessage val = GMPMessage.Parse(data);
		GMPGroup val2 = val.FindGroup(57152);
		if (val2 != null)
		{
			GMPField val3 = val2.FindTag(14675998);
			if (val3 != null)
			{
				programOption.Value = MessageBuilder.DefaultEncoding.GetString(val3.Value);
			}
		}
		return programOption;
	}

	protected string SendReport(Command command, byte[] buffer)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		SFResponse sFResponse = null;
		FPUResponse val = null;
		onReporting = true;
		secureComm.ConnTimeout = 300000;
		try
		{
			val = Send(new FPURequest(command, buffer));
		}
		finally
		{
			secureComm.ConnTimeout = 15000;
		}
		sFResponse = new SFResponse();
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		if (val.ErrorCode == 0)
		{
			if (val.Data != null)
			{
				GMPMessage val2 = GMPMessage.Parse(val.Data);
				GMPGroup val3 = val2.FindGroup(57152);
				if (val3 != null)
				{
					GMPField val4 = val3.FindTag(14675981);
					if (val4 != null)
					{
						sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.ConvertBcdToInt(val4.Value, 0, 4).ToString());
					}
					else
					{
						sFResponse.AddNull(1);
					}
					val4 = val3.FindTag(14675995);
					if (val4 != null)
					{
						sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.ConvertBcdToInt(val4.Value, 0, 4).ToString());
					}
					else
					{
						sFResponse.AddNull(1);
					}
					val4 = val3.FindTag(14675992);
					if (val4 != null)
					{
						sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.DefaultEncoding.GetString(val4.Value));
					}
					else
					{
						sFResponse.AddNull(1);
					}
				}
			}
		}
		else if (val.ErrorCode == 8)
		{
			NoPaperFlag = true;
		}
		onReporting = false;
		return sFResponse.GetString();
	}

	private string PrintEJReport(int firstZNo, int firstDocId, int lastZNo, int lastDocId, int copy)
	{
		List<byte> list = new List<byte>();
		list.AddRange(MessageBuilder.HexToByteArray(14675999));
		list.Add(1);
		list.Add((byte)copy);
		list.AddRange(MessageBuilder.HexToByteArray(14675995));
		list.AddRange(MessageBuilder.AddLength(3));
		list.AddRange(MessageBuilder.ConvertIntToBCD(firstZNo, 3));
		list.AddRange(MessageBuilder.HexToByteArray(14675981));
		list.AddRange(MessageBuilder.AddLength(3));
		list.AddRange(MessageBuilder.ConvertIntToBCD(firstDocId, 3));
		list.AddRange(MessageBuilder.HexToByteArray(14675995));
		list.AddRange(MessageBuilder.AddLength(3));
		list.AddRange(MessageBuilder.ConvertIntToBCD(lastZNo, 3));
		list.AddRange(MessageBuilder.HexToByteArray(14675981));
		list.AddRange(MessageBuilder.AddLength(3));
		list.AddRange(MessageBuilder.ConvertIntToBCD(lastDocId, 3));
		return SendReport((Command)82, list.ToArray());
	}

	protected string SendCommand(Command command, byte[] data)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Invalid comparison between Unknown and I4
		SFResponse sFResponse = null;
		if (data == null)
		{
			data = new byte[0];
		}
		FPUResponse val = Send(new FPURequest(command, data));
		sFResponse = new SFResponse();
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		if (val.Data != null)
		{
			GMPMessage val2 = GMPMessage.Parse(val.Data);
			GMPGroup val3 = val2.FindGroup(57152);
			if (val3 != null)
			{
				GMPField val4 = val3.FindTag(14647817);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, $"{MessageBuilder.ConvertBytesToDate(val4.Value, 0):yyyy-MM-dd}");
				}
				else
				{
					sFResponse.AddNull(1);
				}
				val4 = val3.FindTag(14647818);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, $"{MessageBuilder.ConvertBytesToTime(val4.Value, 0):HH:mm}");
				}
				else
				{
					sFResponse.AddNull(1);
				}
				val4 = val3.FindTag(14675992);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, $"{MessageBuilder.DefaultEncoding.GetString(val4.Value)}");
				}
				else
				{
					sFResponse.AddNull(1);
				}
				val4 = val3.FindTag(14675972);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, $"{(decimal)MessageBuilder.ConvertBcdToInt(val4.Value, 0, 5) / 1000m:#0.00}");
				}
				else
				{
					sFResponse.AddNull(1);
				}
				val4 = val3.FindTag(14675981);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.ConvertBcdToInt(val4.Value, 0, 4).ToString());
				}
				else
				{
					sFResponse.AddNull(1);
				}
				val4 = val3.FindTag(14675984);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.ConvertBcdToInt(val4.Value, 0, val4.Value.Length).ToString());
					if ((int)command == 143)
					{
						sFResponse.Add(SFResponseLabel.PARAM, $"{MessageBuilder.ConvertBcdToInt(val4.Value, 0, 2):D4}");
					}
					else
					{
						sFResponse.AddNull(1);
					}
				}
				else
				{
					sFResponse.AddNull(1);
				}
				val4 = val3.FindTag(14676006);
				if (val4 != null)
				{
					string value = val4.Value[0].ToString();
					sFResponse.Add(SFResponseLabel.PARAM, value);
				}
				else
				{
					sFResponse.AddNull(1);
				}
			}
		}
		return sFResponse.GetString();
	}

	private IEnumerable<byte> ConvertAddIp(string ipAddr)
	{
		List<byte> list = new List<byte>();
		if (ipAddr == "")
		{
			list.AddRange(MessageBuilder.HexToByteArray(14663941));
			list.AddRange(MessageBuilder.AddLength(0));
			return list;
		}
		string text = "";
		try
		{
			IPAddress.Parse(ipAddr);
			string[] array = ipAddr.Split(new char[1] { '.' });
			for (int i = 0; i < array.Length; i++)
			{
				text += int.Parse(array[i]).ToString().PadLeft(3, '0');
			}
			list.AddRange(MessageBuilder.HexToByteArray(14663941));
			list.AddRange(MessageBuilder.AddLength(6));
			for (int j = 0; j < text.Length / 2; j++)
			{
				list.AddRange(MessageBuilder.ConvertIntToBCD(int.Parse(text.Substring(j * 2, 2)), 1));
			}
		}
		catch
		{
		}
		return list;
	}

	public string SendTestData(byte[] data)
	{
		//IL_006b: Expected O, but got Unknown
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		if (secureComm != null)
		{
			FPUResponse val = null;
			SFResponse sFResponse = new SFResponse();
			try
			{
				val = Send(new FPURequest((Command)45, data));
				if (val.ErrorCode != 0)
				{
					sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
					sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
					return sFResponse.GetString();
				}
				return null;
			}
			catch (PortClosedException val2)
			{
				PortClosedException val3 = val2;
				Logger.Log((Exception)(object)val3);
				throw val3;
			}
			catch (Exception ex)
			{
				Logger.Log(ex);
				throw ex;
			}
		}
		return "Bağlantı Yok!";
	}

	protected FPUResponse Send(FPURequest request)
	{
		//IL_005c: Expected O, but got Unknown
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		if (secureComm != null)
		{
			BackgroundWorker.isWorkingMainPrinter = true;
			BackgroundWorker.dateTimeLastOperation = DateTime.Now;
			FPUResponse val = null;
			Logger.Log((LogLevel)4, $"REQUEST  = CMD:{request.Command} Data:{BitConverter.ToString(request.Request, 0)}");
			try
			{
				val = secureComm.Send(request);
			}
			catch (PortClosedException val2)
			{
				PortClosedException val3 = val2;
				Logger.Log((Exception)(object)val3);
				throw val3;
			}
			catch (Exception ex)
			{
				Logger.Log(ex);
				throw ex;
			}
			finally
			{
				BackgroundWorker.isWorkingMainPrinter = false;
				BackgroundWorker.dateTimeLastOperation = DateTime.Now;
			}
			if ((int)request.Command == 0 || val.SequenceNum != request.Sequence)
			{
			}
			try
			{
				SetLog(request, val);
			}
			catch
			{
			}
			return val;
		}
		currentLog = "Bağlantı yok";
		return null;
	}

	private void SetLog(FPURequest request, FPUResponse response)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		string text = "";
		string text2 = "";
		text += request.Command;
		text += "|";
		text += request.Sequence;
		text += "|";
		try
		{
			text += Utililty.GetStateMessage(response.FPUState);
		}
		catch
		{
			text += Utililty.GetStateMessage((State)1);
		}
		try
		{
			text2 = Utililty.GetErrorMessage(response.ErrorCode);
		}
		catch
		{
			text2 = Utililty.GetErrorMessage(6);
		}
		text += "|";
		text += response.ErrorCode;
		text += "|";
		text += text2;
		currentLog = text;
		Logger.Log((LogLevel)4, "RESPONSE : " + currentLog);
	}

	public string GetLastLog()
	{
		return currentLog;
	}

	private void Connection_OnReportLine(object sender, OnMessageEventArgs e)
	{
		GMPField[] array = GMPField.Parse(e.Buffer);
		GMPField[] array2 = array;
		foreach (GMPField val in array2)
		{
			if (val.Tag == 14675992)
			{
				string @string = MessageBuilder.DefaultEncoding.GetString(val.Value);
				if (this.OnReportLine != null)
				{
					this.OnReportLine(this, new OnReportLineEventArgs(@string));
				}
			}
		}
	}

	public string GetEFTCardInfo(decimal amount)
	{
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Expected O, but got Unknown
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string> { amount.ToString() });
		SFResponse sFResponse = new SFResponse();
		byte[] array = null;
		List<byte> list = new List<byte>();
		int num = 0;
		list.AddRange(MessageBuilder.HexToByteArray(57204));
		num = list.Count;
		amount = TruncateDigitsAfterComma(amount, 2);
		list.AddRange(MessageBuilder.HexToByteArray(14675972));
		array = MessageBuilder.ConvertDecimalToBCD(amount, 3);
		list.AddRange(MessageBuilder.AddLength(array.Length));
		list.AddRange(array);
		list.InsertRange(num, MessageBuilder.AddLength(list.Count - num));
		secureComm.ConnTimeout = 300000;
		FPUResponse val = Send(new FPURequest((Command)167, list.ToArray()));
		secureComm.ConnTimeout = 15000;
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		if (val.ErrorCode == 0)
		{
			GMPMessage val2 = GMPMessage.Parse(val.Data);
			GMPGroup val3 = val2.FindGroup(57204);
			if (val3 != null)
			{
				GMPField val4 = val3.FindTag(14676014);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.ConvertBcdToInt(val4.Value, 0, 3).ToString());
				}
				val4 = val3.FindTag(14676015);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.DefaultEncoding.GetString(val4.Value));
				}
				val4 = val3.FindTag(14676013);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.ConvertBcdToInt(val4.Value, 0, 1).ToString());
				}
				val4 = val3.FindTag(14676012);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.DefaultEncoding.GetString(val4.Value));
				}
				val4 = val3.FindTag(14676011);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.DefaultEncoding.GetString(val4.Value));
				}
			}
		}
		return sFResponse.GetString();
	}

	public string GetEFTAuthorisation(decimal amount, int installment, string cardNumber)
	{
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Expected O, but got Unknown
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>
		{
			amount.ToString(),
			installment.ToString(),
			cardNumber
		});
		SFResponse sFResponse = new SFResponse();
		byte[] array = null;
		List<byte> list = new List<byte>();
		int num = 0;
		list.AddRange(MessageBuilder.HexToByteArray(57204));
		num = list.Count;
		amount = TruncateDigitsAfterComma(amount, 2);
		list.AddRange(MessageBuilder.HexToByteArray(14675972));
		array = MessageBuilder.ConvertDecimalToBCD(amount, 3);
		list.AddRange(MessageBuilder.AddLength(array.Length));
		list.AddRange(array);
		list.AddRange(MessageBuilder.HexToByteArray(14675983));
		list.AddRange(MessageBuilder.AddLength(1));
		list.AddRange(MessageBuilder.ConvertIntToBCD(installment, 1));
		byte[] bytes = MessageBuilder.DefaultEncoding.GetBytes(cardNumber);
		list.AddRange(MessageBuilder.HexToByteArray(14676012));
		list.AddRange(MessageBuilder.AddLength(bytes.Length));
		list.AddRange(bytes);
		list.InsertRange(num, MessageBuilder.AddLength(list.Count - num));
		secureComm.ConnTimeout = 300000;
		FPUResponse val;
		try
		{
			val = Send(new FPURequest((Command)160, list.ToArray()));
		}
		finally
		{
			secureComm.ConnTimeout = 15000;
		}
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		if (val.ErrorCode == 0)
		{
			GMPMessage val2 = GMPMessage.Parse(val.Data);
			GMPGroup val3 = val2.FindGroup(57203);
			if (val3 != null)
			{
				GMPField val4 = val3.FindTag(14675972);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, $"{(decimal)MessageBuilder.ConvertBcdToInt(val4.Value, 0, val4.Length) / 1000m:#0.00}");
				}
			}
			val3 = val2.FindGroup(57204);
			if (val3 != null)
			{
				GMPField val5 = val3.FindTag(14676016);
				if (val5 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.DefaultEncoding.GetString(val5.Value));
				}
				val5 = val3.FindTag(14675972);
				if (val5 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, $"{(decimal)MessageBuilder.ConvertBcdToInt(val5.Value, 0, val5.Length) / 1000m:#0.00}");
				}
				val5 = val3.FindTag(14675983);
				if (val5 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, $"{MessageBuilder.ConvertBcdToInt(val5.Value, 0, val5.Length)}");
				}
				val5 = val3.FindTag(14676017);
				if (val5 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.DefaultEncoding.GetString(val5.Value));
				}
				val5 = val3.FindTag(14676012);
				if (val5 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.DefaultEncoding.GetString(val5.Value));
				}
				val5 = val3.FindTag(14676018);
				if (val5 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.DefaultEncoding.GetString(val5.Value));
				}
				val5 = val3.FindTag(14676038);
				if (val5 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, $"{MessageBuilder.ConvertBcdToInt(val5.Value, 0, val5.Length)}");
				}
				else
				{
					sFResponse.Add(SFResponseLabel.PARAM, 0);
				}
				val5 = val3.FindTag(14676019);
				if (val5 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.ConvertBcdToInt(val5.Value, 0, val5.Value.Length));
				}
				else
				{
					sFResponse.Add(SFResponseLabel.PARAM, 0);
				}
				val5 = val3.FindTag(14676020);
				if (val5 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.ConvertBcdToInt(val5.Value, 0, val5.Value.Length));
				}
				else
				{
					sFResponse.Add(SFResponseLabel.PARAM, 0);
				}
				val5 = val3.FindTag(14676051);
				if (val5 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, $"{(decimal)MessageBuilder.ConvertBcdToInt(val5.Value, 0, val5.Length) / 1000m:#0.00}");
				}
			}
		}
		return sFResponse.GetString();
	}

	public string VoidEFTPayment(int acquierID, int batchNo, int stanNo)
	{
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Expected O, but got Unknown
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>
		{
			acquierID.ToString(),
			batchNo.ToString(),
			stanNo.ToString()
		});
		SFResponse sFResponse = new SFResponse();
		List<byte> list = new List<byte>();
		int num = 0;
		list.AddRange(MessageBuilder.HexToByteArray(57210));
		num = list.Count;
		list.AddRange(MessageBuilder.HexToByteArray(14676017));
		list.AddRange(MessageBuilder.AddLength(acquierID.ToString().Length));
		list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(acquierID.ToString()));
		list.AddRange(MessageBuilder.HexToByteArray(14676019));
		list.AddRange(MessageBuilder.AddLength(3));
		list.AddRange(MessageBuilder.ConvertIntToBCD(batchNo, 3));
		list.AddRange(MessageBuilder.HexToByteArray(14676020));
		list.AddRange(MessageBuilder.AddLength(3));
		list.AddRange(MessageBuilder.ConvertIntToBCD(stanNo, 3));
		list.InsertRange(num, MessageBuilder.AddLength(list.Count - num));
		FPUResponse val;
		try
		{
			secureComm.ConnTimeout = 300000;
			val = Send(new FPURequest((Command)169, list.ToArray()));
		}
		catch (Exception ex)
		{
			throw ex;
		}
		finally
		{
			secureComm.ConnTimeout = 15000;
		}
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		if (val.ErrorCode == 0)
		{
			GMPMessage val2 = GMPMessage.Parse(val.Data);
			GMPGroup val3 = val2.FindGroup(57204);
			if (val3 != null)
			{
				GMPField val4 = val3.FindTag(14675972);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, $"{(decimal)MessageBuilder.ConvertBcdToInt(val4.Value, 0, val4.Length) / 1000m:#0.00}");
				}
				val4 = val3.FindTag(14676017);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.DefaultEncoding.GetString(val4.Value));
				}
				val4 = val3.FindTag(14676018);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.DefaultEncoding.GetString(val4.Value));
				}
				val4 = val3.FindTag(14676012);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.DefaultEncoding.GetString(val4.Value));
				}
			}
		}
		return sFResponse.GetString();
	}

	public string RefundEFTPayment(int acquierID)
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Expected O, but got Unknown
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string> { acquierID.ToString() });
		SFResponse sFResponse = new SFResponse();
		List<byte> list = new List<byte>();
		int num = 0;
		list.AddRange(MessageBuilder.HexToByteArray(57210));
		num = list.Count;
		list.AddRange(MessageBuilder.HexToByteArray(14676017));
		list.AddRange(MessageBuilder.AddLength(acquierID.ToString().Length));
		list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(acquierID.ToString()));
		list.InsertRange(num, MessageBuilder.AddLength(list.Count - num));
		FPUResponse val;
		try
		{
			secureComm.ConnTimeout = 300000;
			val = Send(new FPURequest((Command)170, list.ToArray()));
		}
		catch (Exception ex)
		{
			throw ex;
		}
		finally
		{
			secureComm.ConnTimeout = 15000;
		}
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		return sFResponse.GetString();
	}

	public string RefundEFTPayment(int acquierID, decimal amount)
	{
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Expected O, but got Unknown
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string> { acquierID.ToString() });
		SFResponse sFResponse = new SFResponse();
		List<byte> list = new List<byte>();
		int num = 0;
		list.AddRange(MessageBuilder.HexToByteArray(57210));
		num = list.Count;
		list.AddRange(MessageBuilder.HexToByteArray(14676017));
		list.AddRange(MessageBuilder.AddLength(acquierID.ToString().Length));
		list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(acquierID.ToString()));
		amount = TruncateDigitsAfterComma(amount, 2);
		list.AddRange(MessageBuilder.HexToByteArray(14675972));
		byte[] array = MessageBuilder.ConvertDecimalToBCD(amount, 3);
		list.AddRange(MessageBuilder.AddLength(array.Length));
		list.AddRange(array);
		list.InsertRange(num, MessageBuilder.AddLength(list.Count - num));
		FPUResponse val;
		try
		{
			secureComm.ConnTimeout = 300000;
			val = Send(new FPURequest((Command)170, list.ToArray()));
		}
		catch (Exception ex)
		{
			throw ex;
		}
		finally
		{
			secureComm.ConnTimeout = 15000;
		}
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		if (val.ErrorCode == 0)
		{
			GMPMessage val2 = GMPMessage.Parse(val.Data);
			GMPGroup val3 = val2.FindGroup(57204);
			if (val3 != null)
			{
				GMPField val4 = val3.FindTag(14675972);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, $"{(decimal)MessageBuilder.ConvertBcdToInt(val4.Value, 0, val4.Length) / 1000m:#0.00}");
				}
				val4 = val3.FindTag(14676017);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.DefaultEncoding.GetString(val4.Value));
				}
				val4 = val3.FindTag(14676018);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.DefaultEncoding.GetString(val4.Value));
				}
				val4 = val3.FindTag(14676012);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.DefaultEncoding.GetString(val4.Value));
				}
			}
		}
		return sFResponse.GetString();
	}

	public string GetEFTSlipCopy(int acquierID, int batchNo, int stanNo, int zNo, int receiptNo)
	{
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Expected O, but got Unknown
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>
		{
			acquierID.ToString(),
			batchNo.ToString(),
			stanNo.ToString(),
			zNo.ToString(),
			receiptNo.ToString()
		});
		SFResponse sFResponse = new SFResponse();
		List<byte> list = new List<byte>();
		int num = 0;
		list.AddRange(MessageBuilder.HexToByteArray(57210));
		num = list.Count;
		list.AddRange(MessageBuilder.HexToByteArray(14676017));
		list.AddRange(MessageBuilder.AddLength(acquierID.ToString().Length));
		list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(acquierID.ToString()));
		list.AddRange(MessageBuilder.HexToByteArray(14676019));
		list.AddRange(MessageBuilder.AddLength(3));
		list.AddRange(MessageBuilder.ConvertIntToBCD(batchNo, 3));
		list.AddRange(MessageBuilder.HexToByteArray(14676020));
		list.AddRange(MessageBuilder.AddLength(3));
		list.AddRange(MessageBuilder.ConvertIntToBCD(stanNo, 3));
		list.AddRange(MessageBuilder.HexToByteArray(14675995));
		list.AddRange(MessageBuilder.AddLength(3));
		list.AddRange(MessageBuilder.ConvertIntToBCD(zNo, 3));
		list.AddRange(MessageBuilder.HexToByteArray(14675981));
		list.AddRange(MessageBuilder.AddLength(3));
		list.AddRange(MessageBuilder.ConvertIntToBCD(receiptNo, 3));
		list.InsertRange(num, MessageBuilder.AddLength(list.Count - num));
		FPUResponse val;
		try
		{
			secureComm.ConnTimeout = 300000;
			val = Send(new FPURequest((Command)172, list.ToArray()));
		}
		catch (Exception ex)
		{
			throw ex;
		}
		finally
		{
			secureComm.ConnTimeout = 15000;
		}
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		return sFResponse.GetString();
	}

	public string GetBankListOnEFT()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>());
		SFResponse sFResponse = new SFResponse();
		secureComm.ConnTimeout = 300000;
		FPUResponse val;
		try
		{
			val = Send(new FPURequest((Command)171, new byte[0]));
		}
		finally
		{
			secureComm.ConnTimeout = 15000;
		}
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		if (val.ErrorCode == 0)
		{
			GMPMessage val2 = GMPMessage.Parse(val.Data);
			GMPGroup val3 = val2.FindGroup(57210);
			if (val3 != null)
			{
				foreach (GMPField tag in val3.Tags)
				{
					if (tag.Tag == 14676021)
					{
						sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.DefaultEncoding.GetString(tag.Value));
					}
				}
			}
		}
		return sFResponse.GetString();
	}

	public string GetSalesInfo()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>());
		SFResponse sFResponse = new SFResponse();
		FPUResponse val = Send(new FPURequest((Command)116, new byte[0]));
		sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
		sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
		if (val.ErrorCode == 0)
		{
			GMPMessage val2 = GMPMessage.Parse(val.Data);
			GMPGroup val3 = val2.FindGroup(57152);
			if (val3 != null)
			{
				GMPField val4 = val3.FindTag(14675981);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.ConvertBcdToInt(val4.Value, 0, 4));
				}
				else
				{
					sFResponse.AddNull(1);
				}
				val4 = val3.FindTag(14675995);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.ConvertBcdToInt(val4.Value, 0, 4));
				}
				else
				{
					sFResponse.AddNull(1);
				}
				val4 = val3.FindTag(14675977);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.ConvertBcdToInt(val4.Value, 0, 1));
				}
				else
				{
					sFResponse.AddNull(1);
				}
				val4 = val3.FindTag(14647817);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, $"{MessageBuilder.ConvertBytesToDate(val4.Value, 0):yyyy-MM-dd}");
				}
				else
				{
					sFResponse.AddNull(1);
				}
				val4 = val3.FindTag(14647818);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, $"{MessageBuilder.ConvertBytesToTime(val4.Value, 0):HH:mm}");
				}
				else
				{
					sFResponse.AddNull(1);
				}
				val4 = val3.FindTag(14675972);
				if (val4 != null)
				{
					sFResponse.Add(SFResponseLabel.PARAM, $"{(decimal)MessageBuilder.ConvertBcdToInt(val4.Value, 0, val4.Length) / 1000m:#0.00}");
				}
			}
			else
			{
				sFResponse.AddNull(6);
			}
			val3 = val2.FindGroup(57204);
			if (val3 != null)
			{
				sFResponse.Add(SFResponseLabel.PARAM, $"{val3.Tags.Count / 4}");
				foreach (GMPField tag in val3.Tags)
				{
					if (tag.Tag == 14675973)
					{
						int num = MessageBuilder.ConvertBcdToInt(tag.Value, 0, tag.Length);
						sFResponse.Add(SFResponseLabel.PARAM, $"{num}");
					}
					if (tag.Tag == 14675984)
					{
						sFResponse.Add(SFResponseLabel.PARAM, $"{MessageBuilder.ConvertBcdToInt(tag.Value, 0, tag.Length)}");
					}
					if (tag.Tag == 14675972)
					{
						sFResponse.Add(SFResponseLabel.PARAM, $"{(decimal)MessageBuilder.ConvertBcdToInt(tag.Value, 0, tag.Length) / 1000m:#0.00}");
					}
					if (tag.Tag == 14675992)
					{
						sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.DefaultEncoding.GetString(tag.Value).Trim(new char[1]));
					}
				}
			}
			else
			{
				sFResponse.AddNull(3);
			}
			val3 = val2.FindGroup(57184);
			if (val3 != null)
			{
				sFResponse.Add(SFResponseLabel.PARAM, $"{val3.Tags.Count / 4}");
				foreach (GMPField tag2 in val3.Tags)
				{
					if (tag2.Tag == 14675988)
					{
						sFResponse.Add(SFResponseLabel.PARAM, MessageBuilder.ConvertBcdToInt(tag2.Value, 0, 1).ToString());
					}
					if (tag2.Tag == 14675972)
					{
						sFResponse.Add(SFResponseLabel.PARAM, $"{(decimal)MessageBuilder.ConvertBcdToInt(tag2.Value, 0, tag2.Length) / 1000m:#0.00}");
					}
					if (tag2.Tag == 14676050)
					{
						sFResponse.Add(SFResponseLabel.PARAM, $"{(decimal)MessageBuilder.ConvertBcdToInt(tag2.Value, 0, tag2.Length) / 1000m:#0.00}");
					}
				}
			}
			else
			{
				sFResponse.AddNull(4);
			}
		}
		return sFResponse.GetString();
	}

	public string SaveCardInfoList(string[] cardList)
	{
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Expected O, but got Unknown
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Expected O, but got Unknown
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>(cardList));
		List<byte> list = new List<byte>();
		SFResponse sFResponse = new SFResponse();
		int num = 50;
		int i = 0;
		int num2 = num - i;
		for (; i < cardList.Length; i++)
		{
			string[] array = cardList[i].Split(new char[1] { '|' });
			int num3 = Convert.ToInt32(array[0]);
			int num4 = Convert.ToInt32(array[2]);
			string text = array[1];
			list.AddRange(MessageBuilder.HexToByteArray(14676014));
			list.AddRange(MessageBuilder.AddLength(3));
			list.AddRange(MessageBuilder.ConvertIntToBCD(num3, 3));
			list.AddRange(MessageBuilder.HexToByteArray(14676015));
			list.AddRange(MessageBuilder.AddLength(text.Length));
			list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(text));
			list.AddRange(MessageBuilder.HexToByteArray(14676012));
			list.AddRange(MessageBuilder.AddLength(3));
			list.AddRange(MessageBuilder.ConvertIntToBCD(num4, 3));
			if (i == num)
			{
				FPUResponse val = Send(new FPURequest((Command)168, list.ToArray()));
				sFResponse.Add(SFResponseLabel.ERROR_CODE, val.ErrorCode);
				sFResponse.Add(SFResponseLabel.STATUS, val.FPUState);
				num2 = num - i;
				list.Clear();
			}
		}
		if (num2 > 0)
		{
			sFResponse = new SFResponse();
			FPUResponse val2 = Send(new FPURequest((Command)168, list.ToArray()));
			sFResponse.Add(SFResponseLabel.ERROR_CODE, val2.ErrorCode);
			sFResponse.Add(SFResponseLabel.STATUS, val2.FPUState);
		}
		return sFResponse.GetString();
	}

	protected void LogMethodInfo(MethodBase method, List<string> methodParams)
	{
		ParameterInfo[] parameters = method.GetParameters();
		string text = "";
		for (int i = 0; i < methodParams.Count; i++)
		{
			int num = i;
			if (parameters.Length < methodParams.Count)
			{
				num = 0;
			}
			text += $"{parameters[num].Name} = {methodParams[i]}";
			if (i != methodParams.Count - 1)
			{
				text += ", ";
			}
		}
		Logger.Log((LogLevel)4, $"METHOD : {method.Name} [ {text} ]");
	}

	private decimal TruncateDigitsAfterComma(decimal value, int places)
	{
		decimal num = Convert.ToDecimal(Math.Pow(10.0, places));
		return Math.Truncate(value * num) / num;
	}

	public string GetECRVersion()
	{
		return secureComm.ECRVersion;
	}

	protected int getSecurecomVersion()
	{
		return secureComm.GetVersion();
	}

    }
}