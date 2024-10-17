using System;
using Hugin.GMPCommon;

namespace Hugin.POS.CompactPrinter.FP300 { 

internal class SFResponse
{
	private const char SPEC_CHAR = '|';

	private static SFResponseObject respObj = null;

	public SFResponse()
	{
		respObj = null;
	}

	public void Add(SFResponseLabel label, object value)
	{
		if (respObj == null)
		{
			respObj = new SFResponseObject();
		}
		switch (label)
		{
		case SFResponseLabel.ERROR_CODE:
			try
			{
				Utililty.GetErrorMessage((int)value);
				respObj.errorCode = (int)value;
				break;
			}
			catch
			{
				respObj.errorCode = 6;
				break;
			}
		case SFResponseLabel.STATUS:
			respObj.statusCode = (int)value;
			break;
		case SFResponseLabel.PARAM:
			respObj.paramList.Add(Convert.ToString(value));
			break;
		case SFResponseLabel.NULL:
			respObj.paramList.Add(null);
			break;
		}
	}

	public string GetString()
	{
		string text = string.Empty;
		if (respObj == null)
		{
			return string.Empty;
		}
		if (!string.IsNullOrEmpty(respObj.errorCode.ToString()))
		{
			text += respObj.errorCode;
		}
		text += "|";
		if (!string.IsNullOrEmpty(respObj.statusCode.ToString()))
		{
			text += respObj.statusCode;
		}
		text += "|";
		foreach (string param in respObj.paramList)
		{
			text += param;
			text += "|";
		}
		text = text.Remove(text.Length - 1);
		Logger.Log((LogLevel)4, "RES DATA = " + text);
		return text;
	}

	public void AddNull(int count)
	{
		if (respObj == null)
		{
			respObj = new SFResponseObject();
		}
		for (int i = 0; i < count; i++)
		{
			respObj.paramList.Add(null);
		}
	}

	public static SFResponseObject GetObjectByString(string str)
	{
		if (string.IsNullOrEmpty(str))
		{
			return null;
		}
		SFResponseObject sFResponseObject = null;
		string[] array = str.Split(new char[1] { '|' });
		if (array.Length != 0)
		{
			sFResponseObject = new SFResponseObject();
			sFResponseObject.errorCode = int.Parse(array[0]);
			sFResponseObject.statusCode = int.Parse(array[1]);
			if (array.Length >= 3)
			{
				for (int i = 2; i < array.Length; i++)
				{
					sFResponseObject.paramList.Add(array[i]);
				}
			}
		}
		return sFResponseObject;
	}
}
}