using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Hugin.GMPCommon { 

public class MessageBuilder
{
	public static Encoding DefaultEncoding = Encoding.GetEncoding(1254);

	public static byte[] ConvertIpToBcd(string idAddress)
	{
		List<byte> list = new List<byte>();
		string[] array = idAddress.Split(',', '.');
		string text = "";
		for (int i = 0; i < array.Length; i++)
		{
			text += int.Parse(array[i]).ToString().PadLeft(3, '0');
		}
		for (int j = 0; j < text.Length / 2; j++)
		{
			list.AddRange(ConvertIntToBCD(int.Parse(text.Substring(j * 2, 2)), 1));
		}
		return list.ToArray();
	}

	public static byte[] GetDateTimeInBytes(DateTime date)
	{
		List<byte> list = new List<byte>();
		list.AddRange(GetDateInBytes(date));
		list.AddRange(GetTimeInBytes(date));
		return list.ToArray();
	}

	public static byte[] GetDateInBytes(DateTime date)
	{
		List<byte> list = new List<byte>();
		Encoding encoding = Encoding.GetEncoding(1254);
		list.AddRange(HexToByteArray(14647817));
		list.Add(3);
		list.AddRange(Date2Bytes(date));
		return list.ToArray();
	}

	public static byte[] Date2Bytes(DateTime date)
	{
		List<byte> list = new List<byte>();
		list.AddRange(ConvertIntToBCD(date.Date.Year, 1));
		list.AddRange(ConvertIntToBCD(date.Date.Month, 1));
		list.AddRange(ConvertIntToBCD(date.Date.Day, 1));
		return list.ToArray();
	}

	public static byte[] GetTimeInBytes(DateTime time)
	{
		List<byte> list = new List<byte>();
		Encoding encoding = Encoding.GetEncoding(1254);
		list.AddRange(HexToByteArray(14647818));
		list.Add(3);
		list.AddRange(Time2Bytes(time));
		return list.ToArray();
	}

	public static byte[] Time2Bytes(DateTime time)
	{
		List<byte> list = new List<byte>();
		list.AddRange(ConvertIntToBCD(time.Hour, 1));
		list.AddRange(ConvertIntToBCD(time.Minute, 1));
		list.AddRange(ConvertIntToBCD(time.Second, 1));
		return list.ToArray();
	}

	public static DateTime GetDateFromBcd(byte[] blockData, int index, out int outIndex)
	{
		outIndex = index;
		int day = ConvertBcdToInt(blockData, outIndex++, 1);
		int month = ConvertBcdToInt(blockData, outIndex++, 1);
		int year = ConvertBcdToInt(blockData, outIndex++, 1) + 2000;
		return new DateTime(year, month, day);
	}

	public static DateTime AddTimeFromBcd(byte[] blockData, int index, out int outIndex, DateTime dtToAdd)
	{
		outIndex = index;
		int num = ConvertBcdToInt(blockData, outIndex++, 1);
		int num2 = ConvertBcdToInt(blockData, outIndex++, 1);
		int num3 = ConvertBcdToInt(blockData, outIndex++, 1);
		dtToAdd = dtToAdd.AddHours(num);
		dtToAdd = dtToAdd.AddMinutes(num2);
		dtToAdd = dtToAdd.AddSeconds(num3);
		return dtToAdd;
	}

	public static byte[] AddLength(int len)
	{
		List<byte> list = new List<byte>();
		if (len > 255)
		{
			list.Add(130);
		}
		else if (len > 127)
		{
			list.Add(129);
		}
		list.AddRange(HexToByteArray(len));
		return list.ToArray();
	}

	public static byte[] HexToByteArray(int hexNum)
	{
		List<byte> list = new List<byte>();
		do
		{
			list.Insert(0, (byte)(hexNum % 256));
			hexNum /= 256;
		}
		while (hexNum != 0);
		return list.ToArray();
	}

	public static int ByteArrayToHex(byte[] bytesArray, int offset, int len)
	{
		int num = 0;
		for (int i = 0; i < len; i++)
		{
			num += num * 255 + bytesArray[i + offset];
		}
		return num;
	}

	public static int GetTag(byte[] bytesArray, int offset, out int outOffset)
	{
		int num = 0;
		outOffset = offset;
		for (int i = 0; i < 3; i++)
		{
			num += num * 255 + bytesArray[i + offset];
			outOffset++;
			if (bytesArray[i + offset] < 127)
			{
				break;
			}
		}
		return num;
	}

	public static DateTime ConvertBytesToDate(byte[] bytesBCD, int offset)
	{
		int day = ConvertBcdToInt(bytesBCD, offset, 1);
		offset++;
		int month = ConvertBcdToInt(bytesBCD, offset, 1);
		offset++;
		int year = ConvertBcdToInt(bytesBCD, offset, 1) + 2000;
		offset++;
		return new DateTime(year, month, day);
	}

	public static DateTime ConvertBytesToTime(byte[] bytesBCD, int offset)
	{
		int hour = ConvertBcdToInt(bytesBCD, offset, 1);
		offset++;
		int minute = ConvertBcdToInt(bytesBCD, offset, 1);
		offset++;
		int second = ConvertBcdToInt(bytesBCD, offset, 1);
		offset++;
		return new DateTime(1970, 1, 1, hour, minute, second);
	}

	public static int ConvertBcdToInt(byte[] bytesBCD, int offset, int len)
	{
		int num = 0;
		for (int i = 0; i < len; i++)
		{
			int num2 = bytesBCD[i + offset];
			num2 = num2 / 16 * 10 + num2 % 16;
			num = num * 100 + num2;
		}
		return num;
	}

	public static string ByteArrayToString(byte[] bytesArray, int offset, int len)
	{
		List<byte> list = new List<byte>();
		for (int i = 0; i < len; i++)
		{
			list.Add(bytesArray[i + offset]);
		}
		return Encoding.ASCII.GetString(list.ToArray());
	}

	public static string GetString(byte[] bytesArray, int index, out int outIndex, Encoding encoding)
	{
		outIndex = index;
		int length = GetLength(bytesArray, outIndex, out outIndex);
		List<byte> list = new List<byte>();
		for (int i = 0; i < length; i++)
		{
			list.Add(bytesArray[outIndex]);
			outIndex++;
		}
		return encoding.GetString(list.ToArray());
	}

	public static byte[] GetBytesFromOffset(byte[] bytesArray, int offset, int len)
	{
		List<byte> list = new List<byte>();
		for (int i = 0; i < len; i++)
		{
			list.Add(bytesArray[i + offset]);
		}
		return list.ToArray();
	}

	public static byte CalculateLRC(byte[] reqPacket)
	{
		byte b = 0;
		for (int i = 0; i < reqPacket.Length; i++)
		{
			b ^= reqPacket[i];
		}
		return b;
	}

	public static short CalculateCRC(byte[] buffer, int offset, int length)
	{
		short num = 0;
		for (int i = offset; i < offset + length; i++)
		{
			num ^= buffer[i];
		}
		return num;
	}

	public static byte[] ConvertIntToBCD(int value, int bcdLen)
	{
		List<byte> list = new List<byte>();
		uint num = (uint)value;
		int num2 = 0;
		do
		{
			uint num3 = num % 100;
			num3 = num3 / 10 * 16 + num3 % 10;
			list.Insert(0, (byte)num3);
			num /= 100;
			num2++;
		}
		while (num2 < bcdLen);
		return list.ToArray();
	}

	public static byte[] ConvertDecimalToBCD(decimal value, int decimalCnt)
	{
		List<byte> list = new List<byte>();
		uint num = (uint)(value * (decimal)(int)Math.Pow(10.0, decimalCnt));
		int num2 = 0;
		while (true)
		{
			uint num3 = num % 100;
			num3 = num3 / 10 * 16 + num3 % 10;
			list.Insert(0, (byte)num3);
			num /= 100;
			if (num == 0)
			{
				break;
			}
			num2++;
		}
		return list.ToArray();
	}

	public static int GetLength(byte[] msgBytes, int offset, out int outIndex)
	{
		int num = 0;
		outIndex = offset;
		switch (msgBytes[offset])
		{
		case 129:
			num = msgBytes[outIndex + 1];
			outIndex += 2;
			break;
		case 130:
			num = msgBytes[outIndex + 1] * 256 + msgBytes[outIndex + 2];
			outIndex += 3;
			break;
		default:
			num = msgBytes[outIndex];
			outIndex++;
			break;
		}
		return num;
	}

	public static string BytesToHexString(List<byte> bytesArr)
	{
		string text = "";
		for (int i = 0; i < bytesArr.Count; i++)
		{
			text += $"{bytesArr[i]:X2}";
		}
		return text ?? "";
	}

	public static List<byte> HexStringToBytes(string strBytes)
	{
		List<byte> list = new List<byte>();
		int num = strBytes.Length / 2;
		for (int i = 0; i < num; i++)
		{
			list.Add(Convert.ToByte(strBytes.Substring(i * 2, 2), 16));
		}
		return list;
	}

	public static byte[] GetSecureRandomBytes(int count)
	{
		byte[] array = new byte[count];
		RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create();
		randomNumberGenerator.GetNonZeroBytes(array);
		return array;
	}

	private static TripleDESCryptoServiceProvider GetTripleDESProvider(byte[] tripleKey)
	{
		try
		{
			TripleDESCryptoServiceProvider tripleDESCryptoServiceProvider = new TripleDESCryptoServiceProvider();
			tripleDESCryptoServiceProvider.Mode = CipherMode.ECB;
			tripleDESCryptoServiceProvider.KeySize = 128;
			tripleDESCryptoServiceProvider.Key = tripleKey;
			tripleDESCryptoServiceProvider.Padding = PaddingMode.Zeros;
			return tripleDESCryptoServiceProvider;
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public static byte[] EncryptTriple(byte[] data, int len, byte[] tripleKey)
	{
		TripleDESCryptoServiceProvider tripleDESProvider = GetTripleDESProvider(tripleKey);
		try
		{
			MemoryStream memoryStream = new MemoryStream();
			CryptoStream cryptoStream = new CryptoStream(memoryStream, tripleDESProvider.CreateEncryptor(), CryptoStreamMode.Write);
			cryptoStream.Write(data, 0, len);
			cryptoStream.FlushFinalBlock();
			byte[] result = memoryStream.ToArray();
			cryptoStream.Close();
			memoryStream.Close();
			return result;
		}
		catch (CryptographicException)
		{
			return null;
		}
	}

	public static byte[] DecryptTriple(byte[] data, int len, byte[] tripleKey)
	{
		TripleDESCryptoServiceProvider tripleDESProvider = GetTripleDESProvider(tripleKey);
		MemoryStream stream = new MemoryStream(data);
		CryptoStream cryptoStream = new CryptoStream(stream, tripleDESProvider.CreateDecryptor(), CryptoStreamMode.Read);
		byte[] array = new byte[data.Length];
		cryptoStream.Read(array, 0, len);
		return array;
	}

	public static byte[] Create3DESKey()
	{
		TripleDES tripleDES = TripleDES.Create();
		tripleDES.KeySize = 128;
		tripleDES.GenerateKey();
		tripleDES.Mode = CipherMode.ECB;
		return tripleDES.Key;
	}

	public static byte[] EncryptRSA(byte[] rsaModulus, byte[] exponent, byte[] data)
	{
		byte[] array = null;
		RSACryptoServiceProvider rSACryptoServiceProvider = new RSACryptoServiceProvider();
		RSAParameters parameters = rSACryptoServiceProvider.ExportParameters(includePrivateParameters: false);
		parameters.Modulus = rsaModulus;
		parameters.Exponent = exponent;
		rSACryptoServiceProvider.ImportParameters(parameters);
		return rSACryptoServiceProvider.Encrypt(data, fOAEP: false);
	}

	public static bool VerifyRSA(byte[] rsaModulus, byte[] exponent, byte[] data, byte[] sign)
	{
		RSACryptoServiceProvider rSACryptoServiceProvider = new RSACryptoServiceProvider();
		RSAParameters parameters = rSACryptoServiceProvider.ExportParameters(includePrivateParameters: false);
		parameters.Modulus = rsaModulus;
		parameters.Exponent = exponent;
		rSACryptoServiceProvider.ImportParameters(parameters);
		return rSACryptoServiceProvider.VerifyHash(data, "SHA256", sign);
	}
}
}