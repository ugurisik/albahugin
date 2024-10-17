using System;
using System.Collections.Generic;
using System.Reflection;
using albahugin.Hugin.Common;
using Hugin.ExDevice;
using Hugin.GMPCommon;

namespace Hugin.POS.CompactPrinter.FP300 { 

internal class CompactPrinterInt : CompactPrinter, ICompactPrinterInt
{
	public string SetDepartment(int id, string name, int vatId, int price, int weighable)
	{
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Expected O, but got Unknown
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
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
		byte[] array = MessageBuilder.ConvertIntToBCD(price, 6);
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
				if (base.isSecurecomVx675)
				{
					num++;
				}
				sFResponse.Add(SFResponseLabel.PARAM, num.ToString());
			}
			val4 = val3.FindTag(14675972);
			if (val4 != null)
			{
				sFResponse.Add(SFResponseLabel.PARAM, $"{(decimal)MessageBuilder.ConvertBcdToInt(val4.Value, 0, 5) / 1000m:#0.00}");
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

	public string SetDepartment(int id, string name, int vatId, int price, int weighable, int limit)
	{
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Expected O, but got Unknown
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>
		{
			id.ToString(),
			name,
			vatId.ToString(),
			price.ToString(),
			weighable.ToString(),
			limit.ToString()
		});
		if (getSecurecomVersion() < 4 || base.isSecurecomVx675)
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
		byte[] array = MessageBuilder.ConvertIntToBCD(price, 6);
		list.AddRange(MessageBuilder.HexToByteArray(14675972));
		list.AddRange(MessageBuilder.AddLength(array.Length));
		list.AddRange(array);
		list.AddRange(MessageBuilder.HexToByteArray(14675999));
		list.AddRange(MessageBuilder.AddLength(1));
		list.AddRange(MessageBuilder.ConvertIntToBCD(weighable, 1));
		byte[] array2 = MessageBuilder.ConvertIntToBCD(limit, 6);
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
				sFResponse.Add(SFResponseLabel.PARAM, $"{(decimal)MessageBuilder.ConvertBcdToInt(val4.Value, 0, 5) / 1000m:#0.00}");
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
				sFResponse.Add(SFResponseLabel.PARAM, $"{(decimal)MessageBuilder.ConvertBcdToInt(val4.Value, 0, 5) / 1000m:#0.00}");
			}
		}
		return sFResponse.GetString();
	}

	public string SetCurrencyInfo(int id, string name, int exchangeRate)
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
		byte[] array = MessageBuilder.ConvertIntToBCD(exchangeRate, 4);
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

	public string SetVATRate(int index, int taxRate)
	{
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Expected O, but got Unknown
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>
		{
			index.ToString(),
			taxRate.ToString()
		});
		List<byte> list = new List<byte>();
		list.AddRange(MessageBuilder.HexToByteArray(14675988));
		list.AddRange(MessageBuilder.AddLength(1));
		list.Add((byte)(index + 1));
		int num = taxRate;
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

	public string SaveProduct(int productId, string productName, int deptId, int price, int weighable, string barcode, int subCatId)
	{
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Expected O, but got Unknown
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
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
		byte[] array = MessageBuilder.ConvertIntToBCD(price, 6);
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
					sFResponse.Add(SFResponseLabel.PARAM, (decimal)MessageBuilder.ConvertBcdToInt(val4.Value, 0, 5) / 1000m);
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

	public string PrintDocumentHeader(string tckn_vkn, int amount, int docType)
	{
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Expected O, but got Unknown
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
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
		array = MessageBuilder.ConvertIntToBCD(amount, 6);
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

	public string PrintAdvanceDocumentHeader(string tckn, string name, int amount)
	{
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Expected O, but got Unknown
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>
		{
			tckn,
			name,
			amount.ToString()
		});
		if (getSecurecomVersion() < 3)
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
		array = MessageBuilder.ConvertIntToBCD(amount, 6);
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

	public string PrintCollectionDocumentHeader(string invoiceSerial, DateTime invoiceDate, int amount, string subscriberNo, string institutionName, int comissionAmount)
	{
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Expected O, but got Unknown
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
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
		array = MessageBuilder.ConvertIntToBCD(amount, 6);
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
		if (comissionAmount > 0)
		{
			list.AddRange(MessageBuilder.HexToByteArray(14676035));
			list.AddRange(MessageBuilder.AddLength(1));
			list.Add(2);
			array = MessageBuilder.ConvertIntToBCD(comissionAmount, 6);
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

	public string PrintCurrentAccountCollectionDocumentHeader(string tcknVkn, string customerName, string docSerial, DateTime docDate, int amount)
	{
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Expected O, but got Unknown
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
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
		array = MessageBuilder.ConvertIntToBCD(amount, 6);
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

	public string PrintItem(int PLUNo, int quantity, int amount, string name, string barcode, int deptId, int weighable)
	{
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Expected O, but got Unknown
		//IL_02a7: Unknown result type (might be due to invalid IL or missing references)
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
			array = MessageBuilder.ConvertIntToBCD(quantity, 4);
			list.AddRange(MessageBuilder.AddLength(array.Length));
			list.AddRange(array);
			if ((decimal)amount != -1m)
			{
				list.AddRange(MessageBuilder.HexToByteArray(14675972));
				array = MessageBuilder.ConvertIntToBCD(amount, 6);
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

	public string PrintDepartment(int deptId, int quantity, int amount, string name, int weighable)
	{
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Expected O, but got Unknown
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
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
		array = MessageBuilder.ConvertIntToBCD(quantity, 4);
		list.AddRange(MessageBuilder.AddLength(array.Length));
		list.AddRange(array);
		list.AddRange(MessageBuilder.HexToByteArray(14675972));
		array = MessageBuilder.ConvertIntToBCD(amount, 6);
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

	public string PrintAdjustment(int adjustmentType, int amount, int percentage)
	{
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Expected O, but got Unknown
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
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
			list.AddRange(MessageBuilder.HexToByteArray(14675972));
			array = MessageBuilder.ConvertIntToBCD(amount, 6);
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

	public string Void(int PLUNo, int quantity)
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
		array = MessageBuilder.ConvertIntToBCD(quantity, 4);
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

	public string VoidDepartment(int deptId, string deptName, int quantity)
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
		array = MessageBuilder.ConvertIntToBCD(quantity, 6);
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

	public string PrintSubtotal(int stoppageAmount)
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Expected O, but got Unknown
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string> { stoppageAmount.ToString() });
		SFResponse sFResponse = new SFResponse();
		List<byte> list = new List<byte>();
		list.AddRange(MessageBuilder.HexToByteArray(14676044));
		byte[] array = MessageBuilder.ConvertIntToBCD(stoppageAmount, 6);
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

	public string PrintPayment(int paymentType, int index, int paidTotal)
	{
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Expected O, but got Unknown
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>
		{
			paymentType.ToString(),
			index.ToString(),
			paidTotal.ToString()
		});
		SFResponse sFResponse = new SFResponse();
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
		list.AddRange(MessageBuilder.HexToByteArray(14675972));
		array = MessageBuilder.ConvertIntToBCD(paidTotal, 6);
		list.AddRange(MessageBuilder.AddLength(array.Length));
		list.AddRange(array);
		list.InsertRange(num, MessageBuilder.AddLength(list.Count - num));
		FPUResponse val = Send(new FPURequest((Command)39, list.ToArray()));
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

	public string PrintXReport(int count, int amount, bool isAffectDrawer)
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
		array = MessageBuilder.ConvertIntToBCD(amount, 6);
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

	public string PrintZReport(int count, int amount, bool isAffectDrawer)
	{
		return PrintZReportWReturn(3, count, amount, isAffectDrawer);
	}

	public string PrintZReport(int copy, int count, int amount, bool isAffectDrawer)
	{
		return PrintZReportWReturn(copy, count, amount, isAffectDrawer);
	}

	private string PrintZReportWReturn(int copy, int count, int amount, bool isAffectDrawer)
	{
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string>());
		List<byte> list = new List<byte>();
		byte[] array = MessageBuilder.ConvertIntToBCD(count, 2);
		list.AddRange(MessageBuilder.HexToByteArray(14675984));
		list.AddRange(MessageBuilder.AddLength(array.Length));
		list.AddRange(array);
		array = MessageBuilder.ConvertIntToBCD(amount, 6);
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

	public string CashIn(int amount)
	{
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string> { amount.ToString() });
		byte[] array = null;
		List<byte> list = new List<byte>();
		array = MessageBuilder.ConvertIntToBCD(amount, 6);
		list.AddRange(MessageBuilder.HexToByteArray(14675972));
		list.AddRange(MessageBuilder.AddLength(array.Length));
		list.AddRange(array);
		return SendCommand((Command)137, list.ToArray());
	}

	public string CashOut(int amount)
	{
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string> { amount.ToString() });
		byte[] array = null;
		List<byte> list = new List<byte>();
		array = MessageBuilder.ConvertIntToBCD(amount, 6);
		list.AddRange(MessageBuilder.HexToByteArray(14675972));
		list.AddRange(MessageBuilder.AddLength(array.Length));
		list.AddRange(array);
		return SendCommand((Command)138, list.ToArray());
	}

	public string GetEFTCardInfo(int amount)
	{
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string> { amount.ToString() });
		SFResponse sFResponse = new SFResponse();
		byte[] array = null;
		List<byte> list = new List<byte>();
		int num = 0;
		list.AddRange(MessageBuilder.HexToByteArray(57204));
		num = list.Count;
		list.AddRange(MessageBuilder.HexToByteArray(14675972));
		array = MessageBuilder.ConvertIntToBCD(amount, 6);
		list.AddRange(MessageBuilder.AddLength(array.Length));
		list.AddRange(array);
		list.InsertRange(num, MessageBuilder.AddLength(list.Count - num));
		base.SecurecomConnTimeout = 300000;
		FPUResponse val = Send(new FPURequest((Command)167, list.ToArray()));
		base.SecurecomConnTimeout = 15000;
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

	public string GetEFTAuthorisation(int amount, int installment, string cardNumber)
	{
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Expected O, but got Unknown
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
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
		list.AddRange(MessageBuilder.HexToByteArray(14675972));
		array = MessageBuilder.ConvertIntToBCD(amount, 6);
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
		base.SecurecomConnTimeout = 300000;
		FPUResponse val;
		try
		{
			val = Send(new FPURequest((Command)160, list.ToArray()));
		}
		finally
		{
			base.SecurecomConnTimeout = 15000;
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
			}
		}
		return sFResponse.GetString();
	}

	public string RefundEFTPayment(int acquierID, int amount)
	{
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Expected O, but got Unknown
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		LogMethodInfo(MethodBase.GetCurrentMethod(), new List<string> { acquierID.ToString() });
		SFResponse sFResponse = new SFResponse();
		List<byte> list = new List<byte>();
		int num = 0;
		list.AddRange(MessageBuilder.HexToByteArray(57210));
		num = list.Count;
		list.AddRange(MessageBuilder.HexToByteArray(14676017));
		list.AddRange(MessageBuilder.AddLength(acquierID.ToString().Length));
		list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(acquierID.ToString()));
		list.AddRange(MessageBuilder.HexToByteArray(14675972));
		byte[] array = MessageBuilder.ConvertIntToBCD(amount, 6);
		list.AddRange(MessageBuilder.AddLength(array.Length));
		list.AddRange(array);
		list.InsertRange(num, MessageBuilder.AddLength(list.Count - num));
		FPUResponse val;
		try
		{
			base.SecurecomConnTimeout = 300000;
			val = Send(new FPURequest((Command)170, list.ToArray()));
		}
		catch (Exception ex)
		{
			throw ex;
		}
		finally
		{
			base.SecurecomConnTimeout = 15000;
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
}
}