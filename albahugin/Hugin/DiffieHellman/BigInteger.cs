using albahugin.Hugin.DiffieHellman;
using System;

namespace albahugin.Hugin.DiffieHellman { 

internal class BigInteger : IDisposable
{
	private const int maxLength = 256;

	public static readonly int[] primesBelow2000 = new int[303]
	{
		2, 3, 5, 7, 11, 13, 17, 19, 23, 29,
		31, 37, 41, 43, 47, 53, 59, 61, 67, 71,
		73, 79, 83, 89, 97, 101, 103, 107, 109, 113,
		127, 131, 137, 139, 149, 151, 157, 163, 167, 173,
		179, 181, 191, 193, 197, 199, 211, 223, 227, 229,
		233, 239, 241, 251, 257, 263, 269, 271, 277, 281,
		283, 293, 307, 311, 313, 317, 331, 337, 347, 349,
		353, 359, 367, 373, 379, 383, 389, 397, 401, 409,
		419, 421, 431, 433, 439, 443, 449, 457, 461, 463,
		467, 479, 487, 491, 499, 503, 509, 521, 523, 541,
		547, 557, 563, 569, 571, 577, 587, 593, 599, 601,
		607, 613, 617, 619, 631, 641, 643, 647, 653, 659,
		661, 673, 677, 683, 691, 701, 709, 719, 727, 733,
		739, 743, 751, 757, 761, 769, 773, 787, 797, 809,
		811, 821, 823, 827, 829, 839, 853, 857, 859, 863,
		877, 881, 883, 887, 907, 911, 919, 929, 937, 941,
		947, 953, 967, 971, 977, 983, 991, 997, 1009, 1013,
		1019, 1021, 1031, 1033, 1039, 1049, 1051, 1061, 1063, 1069,
		1087, 1091, 1093, 1097, 1103, 1109, 1117, 1123, 1129, 1151,
		1153, 1163, 1171, 1181, 1187, 1193, 1201, 1213, 1217, 1223,
		1229, 1231, 1237, 1249, 1259, 1277, 1279, 1283, 1289, 1291,
		1297, 1301, 1303, 1307, 1319, 1321, 1327, 1361, 1367, 1373,
		1381, 1399, 1409, 1423, 1427, 1429, 1433, 1439, 1447, 1451,
		1453, 1459, 1471, 1481, 1483, 1487, 1489, 1493, 1499, 1511,
		1523, 1531, 1543, 1549, 1553, 1559, 1567, 1571, 1579, 1583,
		1597, 1601, 1607, 1609, 1613, 1619, 1621, 1627, 1637, 1657,
		1663, 1667, 1669, 1693, 1697, 1699, 1709, 1721, 1723, 1733,
		1741, 1747, 1753, 1759, 1777, 1783, 1787, 1789, 1801, 1811,
		1823, 1831, 1847, 1861, 1867, 1871, 1873, 1877, 1879, 1889,
		1901, 1907, 1913, 1931, 1933, 1949, 1951, 1973, 1979, 1987,
		1993, 1997, 1999
	};

	private uint[] data = null;

	public int dataLength;

	public BigInteger()
	{
		data = new uint[256];
		dataLength = 1;
	}

	public BigInteger(long value)
	{
		data = new uint[256];
		long num = value;
		dataLength = 0;
		while (value != 0L && dataLength < 256)
		{
			data[dataLength] = (uint)(value & 0xFFFFFFFFu);
			value >>= 32;
			dataLength++;
		}
		if (num > 0)
		{
			if (value != 0L || (data[255] & 0x80000000u) != 0)
			{
				throw new ArithmeticException("Positive overflow in constructor.");
			}
		}
		else if (num < 0 && (value != -1 || (data[dataLength - 1] & 0x80000000u) == 0))
		{
			throw new ArithmeticException("Negative underflow in constructor.");
		}
		if (dataLength == 0)
		{
			dataLength = 1;
		}
	}

	public BigInteger(ulong value)
	{
		data = new uint[256];
		dataLength = 0;
		while (value != 0L && dataLength < 256)
		{
			data[dataLength] = (uint)(value & 0xFFFFFFFFu);
			value >>= 32;
			dataLength++;
		}
		if (value != 0L || (data[255] & 0x80000000u) != 0)
		{
			throw new ArithmeticException("Positive overflow in constructor.");
		}
		if (dataLength == 0)
		{
			dataLength = 1;
		}
	}

	public BigInteger(BigInteger bi)
	{
		data = new uint[256];
		dataLength = bi.dataLength;
		for (int i = 0; i < dataLength; i++)
		{
			data[i] = bi.data[i];
		}
	}

	public BigInteger(string value, int radix)
	{
		BigInteger bigInteger = new BigInteger(1L);
		BigInteger bigInteger2 = new BigInteger();
		value = value.ToUpper().Trim();
		int num = 0;
		if (value[0] == '-')
		{
			num = 1;
		}
		for (int num2 = value.Length - 1; num2 >= num; num2--)
		{
			int num3 = value[num2];
			num3 = ((num3 >= 48 && num3 <= 57) ? (num3 - 48) : ((num3 < 65 || num3 > 90) ? 9999999 : (num3 - 65 + 10)));
			if (num3 >= radix)
			{
				throw new ArithmeticException("Invalid string in constructor.");
			}
			if (value[0] == '-')
			{
				num3 = -num3;
			}
			bigInteger2 += bigInteger * num3;
			if (num2 - 1 >= num)
			{
				bigInteger *= (BigInteger)radix;
			}
		}
		if (value[0] == '-')
		{
			if ((bigInteger2.data[255] & 0x80000000u) == 0)
			{
				throw new ArithmeticException("Negative underflow in constructor.");
			}
		}
		else if ((bigInteger2.data[255] & 0x80000000u) != 0)
		{
			throw new ArithmeticException("Positive overflow in constructor.");
		}
		data = new uint[256];
		for (int i = 0; i < bigInteger2.dataLength; i++)
		{
			data[i] = bigInteger2.data[i];
		}
		dataLength = bigInteger2.dataLength;
	}

	public BigInteger(byte[] inData)
	{
		dataLength = inData.Length >> 2;
		int num = inData.Length & 3;
		if (num != 0)
		{
			dataLength++;
		}
		if (dataLength > 256)
		{
			throw new ArithmeticException("Byte overflow in constructor.");
		}
		data = new uint[256];
		int num2 = inData.Length - 1;
		int num3 = 0;
		while (num2 >= 3)
		{
			data[num3] = (uint)((inData[num2 - 3] << 24) + (inData[num2 - 2] << 16) + (inData[num2 - 1] << 8) + inData[num2]);
			num2 -= 4;
			num3++;
		}
		switch (num)
		{
		case 1:
			data[dataLength - 1] = inData[0];
			break;
		case 2:
			data[dataLength - 1] = (uint)((inData[0] << 8) + inData[1]);
			break;
		case 3:
			data[dataLength - 1] = (uint)((inData[0] << 16) + (inData[1] << 8) + inData[2]);
			break;
		}
		while (dataLength > 1 && data[dataLength - 1] == 0)
		{
			dataLength--;
		}
	}

	public BigInteger(byte[] inData, int inLen)
	{
		dataLength = inLen >> 2;
		int num = inLen & 3;
		if (num != 0)
		{
			dataLength++;
		}
		if (dataLength > 256 || inLen > inData.Length)
		{
			throw new ArithmeticException("Byte overflow in constructor.");
		}
		data = new uint[256];
		int num2 = inLen - 1;
		int num3 = 0;
		while (num2 >= 3)
		{
			data[num3] = (uint)((inData[num2 - 3] << 24) + (inData[num2 - 2] << 16) + (inData[num2 - 1] << 8) + inData[num2]);
			num2 -= 4;
			num3++;
		}
		switch (num)
		{
		case 1:
			data[dataLength - 1] = inData[0];
			break;
		case 2:
			data[dataLength - 1] = (uint)((inData[0] << 8) + inData[1]);
			break;
		case 3:
			data[dataLength - 1] = (uint)((inData[0] << 16) + (inData[1] << 8) + inData[2]);
			break;
		}
		if (dataLength == 0)
		{
			dataLength = 1;
		}
		while (dataLength > 1 && data[dataLength - 1] == 0)
		{
			dataLength--;
		}
	}

	public BigInteger(uint[] inData)
	{
		dataLength = inData.Length;
		if (dataLength > 256)
		{
			throw new ArithmeticException("Byte overflow in constructor.");
		}
		data = new uint[256];
		int num = dataLength - 1;
		int num2 = 0;
		while (num >= 0)
		{
			data[num2] = inData[num];
			num--;
			num2++;
		}
		while (dataLength > 1 && data[dataLength - 1] == 0)
		{
			dataLength--;
		}
	}

	public static implicit operator BigInteger(long value)
	{
		return new BigInteger(value);
	}

	public static implicit operator BigInteger(ulong value)
	{
		return new BigInteger(value);
	}

	public static implicit operator BigInteger(int value)
	{
		return new BigInteger(value);
	}

	public static implicit operator BigInteger(uint value)
	{
		return new BigInteger((ulong)value);
	}

	public static BigInteger operator +(BigInteger bi1, BigInteger bi2)
	{
		BigInteger bigInteger = new BigInteger();
		bigInteger.dataLength = ((bi1.dataLength > bi2.dataLength) ? bi1.dataLength : bi2.dataLength);
		long num = 0L;
		for (int i = 0; i < bigInteger.dataLength; i++)
		{
			long num2 = (long)bi1.data[i] + (long)bi2.data[i] + num;
			num = num2 >> 32;
			bigInteger.data[i] = (uint)(num2 & 0xFFFFFFFFu);
		}
		if (num != 0L && bigInteger.dataLength < 256)
		{
			bigInteger.data[bigInteger.dataLength] = (uint)num;
			bigInteger.dataLength++;
		}
		while (bigInteger.dataLength > 1 && bigInteger.data[bigInteger.dataLength - 1] == 0)
		{
			bigInteger.dataLength--;
		}
		int num3 = 255;
		if ((bi1.data[num3] & 0x80000000u) == (bi2.data[num3] & 0x80000000u) && (bigInteger.data[num3] & 0x80000000u) != (bi1.data[num3] & 0x80000000u))
		{
			throw new ArithmeticException();
		}
		return bigInteger;
	}

	public static BigInteger operator ++(BigInteger bi1)
	{
		BigInteger bigInteger = new BigInteger(bi1);
		long num = 1L;
		int num2 = 0;
		while (num != 0L && num2 < 256)
		{
			long num3 = bigInteger.data[num2];
			num3++;
			bigInteger.data[num2] = (uint)(num3 & 0xFFFFFFFFu);
			num = num3 >> 32;
			num2++;
		}
		if (num2 > bigInteger.dataLength)
		{
			bigInteger.dataLength = num2;
		}
		else
		{
			while (bigInteger.dataLength > 1 && bigInteger.data[bigInteger.dataLength - 1] == 0)
			{
				bigInteger.dataLength--;
			}
		}
		int num4 = 255;
		if ((bi1.data[num4] & 0x80000000u) == 0 && (bigInteger.data[num4] & 0x80000000u) != (bi1.data[num4] & 0x80000000u))
		{
			throw new ArithmeticException("Overflow in ++.");
		}
		return bigInteger;
	}

	public static BigInteger operator -(BigInteger bi1, BigInteger bi2)
	{
		BigInteger bigInteger = new BigInteger();
		bigInteger.dataLength = ((bi1.dataLength > bi2.dataLength) ? bi1.dataLength : bi2.dataLength);
		long num = 0L;
		for (int i = 0; i < bigInteger.dataLength; i++)
		{
			long num2 = (long)bi1.data[i] - (long)bi2.data[i] - num;
			bigInteger.data[i] = (uint)(num2 & 0xFFFFFFFFu);
			num = ((num2 >= 0) ? 0 : 1);
		}
		if (num != 0)
		{
			for (int j = bigInteger.dataLength; j < 256; j++)
			{
				bigInteger.data[j] = uint.MaxValue;
			}
			bigInteger.dataLength = 256;
		}
		while (bigInteger.dataLength > 1 && bigInteger.data[bigInteger.dataLength - 1] == 0)
		{
			bigInteger.dataLength--;
		}
		int num3 = 255;
		if ((bi1.data[num3] & 0x80000000u) != (bi2.data[num3] & 0x80000000u) && (bigInteger.data[num3] & 0x80000000u) != (bi1.data[num3] & 0x80000000u))
		{
			throw new ArithmeticException();
		}
		return bigInteger;
	}

	public static BigInteger operator --(BigInteger bi1)
	{
		BigInteger bigInteger = new BigInteger(bi1);
		bool flag = true;
		int num = 0;
		while (flag && num < 256)
		{
			long num2 = bigInteger.data[num];
			num2--;
			bigInteger.data[num] = (uint)(num2 & 0xFFFFFFFFu);
			if (num2 >= 0)
			{
				flag = false;
			}
			num++;
		}
		if (num > bigInteger.dataLength)
		{
			bigInteger.dataLength = num;
		}
		while (bigInteger.dataLength > 1 && bigInteger.data[bigInteger.dataLength - 1] == 0)
		{
			bigInteger.dataLength--;
		}
		int num3 = 255;
		if ((bi1.data[num3] & 0x80000000u) != 0 && (bigInteger.data[num3] & 0x80000000u) != (bi1.data[num3] & 0x80000000u))
		{
			throw new ArithmeticException("Underflow in --.");
		}
		return bigInteger;
	}

	public static BigInteger operator *(BigInteger bi1, BigInteger bi2)
	{
		int num = 255;
		bool flag = false;
		bool flag2 = false;
		try
		{
			if ((bi1.data[num] & 0x80000000u) != 0)
			{
				flag = true;
				bi1 = -bi1;
			}
			if ((bi2.data[num] & 0x80000000u) != 0)
			{
				flag2 = true;
				bi2 = -bi2;
			}
		}
		catch (Exception)
		{
		}
		BigInteger bigInteger = new BigInteger();
		try
		{
			for (int i = 0; i < bi1.dataLength; i++)
			{
				if (bi1.data[i] != 0)
				{
					ulong num2 = 0uL;
					int num3 = 0;
					int num4 = i;
					while (num3 < bi2.dataLength)
					{
						ulong num5 = (ulong)((long)bi1.data[i] * (long)bi2.data[num3] + bigInteger.data[num4]) + num2;
						bigInteger.data[num4] = (uint)(num5 & 0xFFFFFFFFu);
						num2 = num5 >> 32;
						num3++;
						num4++;
					}
					if (num2 != 0)
					{
						bigInteger.data[i + bi2.dataLength] = (uint)num2;
					}
				}
			}
		}
		catch (Exception)
		{
			throw new ArithmeticException("Multiplication overflow.");
		}
		bigInteger.dataLength = bi1.dataLength + bi2.dataLength;
		if (bigInteger.dataLength > 256)
		{
			bigInteger.dataLength = 256;
		}
		while (bigInteger.dataLength > 1 && bigInteger.data[bigInteger.dataLength - 1] == 0)
		{
			bigInteger.dataLength--;
		}
		if ((bigInteger.data[num] & 0x80000000u) != 0)
		{
			if (flag != flag2 && bigInteger.data[num] == 2147483648u)
			{
				if (bigInteger.dataLength == 1)
				{
					return bigInteger;
				}
				bool flag3 = true;
				for (int j = 0; j < bigInteger.dataLength - 1 && flag3; j++)
				{
					if (bigInteger.data[j] != 0)
					{
						flag3 = false;
					}
				}
				if (flag3)
				{
					return bigInteger;
				}
			}
			throw new ArithmeticException("Multiplication overflow.");
		}
		if (flag != flag2)
		{
			return -bigInteger;
		}
		return bigInteger;
	}

	public static BigInteger operator <<(BigInteger bi1, int shiftVal)
	{
		BigInteger bigInteger = new BigInteger(bi1);
		bigInteger.dataLength = ShiftLeft(bigInteger.data, shiftVal);
		return bigInteger;
	}

	private static int ShiftLeft(uint[] buffer, int shiftVal)
	{
		int num = 32;
		int num2 = buffer.Length;
		while (num2 > 1 && buffer[num2 - 1] == 0)
		{
			num2--;
		}
		for (int num3 = shiftVal; num3 > 0; num3 -= num)
		{
			if (num3 < num)
			{
				num = num3;
			}
			ulong num4 = 0uL;
			for (int i = 0; i < num2; i++)
			{
				ulong num5 = (ulong)buffer[i] << num;
				num5 |= num4;
				buffer[i] = (uint)(num5 & 0xFFFFFFFFu);
				num4 = num5 >> 32;
			}
			if (num4 != 0 && num2 + 1 <= buffer.Length)
			{
				buffer[num2] = (uint)num4;
				num2++;
			}
		}
		return num2;
	}

	public static BigInteger operator >>(BigInteger bi1, int shiftVal)
	{
		BigInteger bigInteger = new BigInteger(bi1);
		bigInteger.dataLength = ShiftRight(bigInteger.data, shiftVal);
		if ((bi1.data[255] & 0x80000000u) != 0)
		{
			for (int num = 255; num >= bigInteger.dataLength; num--)
			{
				bigInteger.data[num] = uint.MaxValue;
			}
			uint num2 = 2147483648u;
			for (int i = 0; i < 32; i++)
			{
				if ((bigInteger.data[bigInteger.dataLength - 1] & num2) != 0)
				{
					break;
				}
				bigInteger.data[bigInteger.dataLength - 1] |= num2;
				num2 >>= 1;
			}
			bigInteger.dataLength = 256;
		}
		return bigInteger;
	}

	private static int ShiftRight(uint[] buffer, int shiftVal)
	{
		int num = 32;
		int num2 = 0;
		int num3 = buffer.Length;
		while (num3 > 1 && buffer[num3 - 1] == 0)
		{
			num3--;
		}
		for (int num4 = shiftVal; num4 > 0; num4 -= num)
		{
			if (num4 < num)
			{
				num = num4;
				num2 = 32 - num;
			}
			ulong num5 = 0uL;
			for (int num6 = num3 - 1; num6 >= 0; num6--)
			{
				ulong num7 = (ulong)buffer[num6] >> num;
				num7 |= num5;
				num5 = (ulong)buffer[num6] << num2;
				buffer[num6] = (uint)num7;
			}
		}
		while (num3 > 1 && buffer[num3 - 1] == 0)
		{
			num3--;
		}
		return num3;
	}

	public static BigInteger operator ~(BigInteger bi1)
	{
		BigInteger bigInteger = new BigInteger(bi1);
		for (int i = 0; i < 256; i++)
		{
			bigInteger.data[i] = ~bi1.data[i];
		}
		bigInteger.dataLength = 256;
		while (bigInteger.dataLength > 1 && bigInteger.data[bigInteger.dataLength - 1] == 0)
		{
			bigInteger.dataLength--;
		}
		return bigInteger;
	}

	public static BigInteger operator -(BigInteger bi1)
	{
		if (bi1.dataLength == 1 && bi1.data[0] == 0)
		{
			return new BigInteger();
		}
		BigInteger bigInteger = new BigInteger(bi1);
		for (int i = 0; i < 256; i++)
		{
			bigInteger.data[i] = ~bi1.data[i];
		}
		long num = 1L;
		int num2 = 0;
		while (num != 0L && num2 < 256)
		{
			long num3 = bigInteger.data[num2];
			num3++;
			bigInteger.data[num2] = (uint)(num3 & 0xFFFFFFFFu);
			num = num3 >> 32;
			num2++;
		}
		if ((bi1.data[255] & 0x80000000u) == (bigInteger.data[255] & 0x80000000u))
		{
			throw new ArithmeticException("Overflow in negation.\n");
		}
		bigInteger.dataLength = 256;
		while (bigInteger.dataLength > 1 && bigInteger.data[bigInteger.dataLength - 1] == 0)
		{
			bigInteger.dataLength--;
		}
		return bigInteger;
	}

	public static bool operator ==(BigInteger bi1, BigInteger bi2)
	{
		return bi1.Equals(bi2);
	}

	public static bool operator !=(BigInteger bi1, BigInteger bi2)
	{
		return !bi1.Equals(bi2);
	}

	public override bool Equals(object o)
	{
		BigInteger bigInteger = (BigInteger)o;
		if (dataLength != bigInteger.dataLength)
		{
			return false;
		}
		for (int i = 0; i < dataLength; i++)
		{
			if (data[i] != bigInteger.data[i])
			{
				return false;
			}
		}
		return true;
	}

	public override int GetHashCode()
	{
		return ToString().GetHashCode();
	}

	public static bool operator >(BigInteger bi1, BigInteger bi2)
	{
		int num = 255;
		if ((bi1.data[num] & 0x80000000u) != 0 && (bi2.data[num] & 0x80000000u) == 0)
		{
			return false;
		}
		if ((bi1.data[num] & 0x80000000u) == 0 && (bi2.data[num] & 0x80000000u) != 0)
		{
			return true;
		}
		int num2 = ((bi1.dataLength > bi2.dataLength) ? bi1.dataLength : bi2.dataLength);
		num = num2 - 1;
		while (num >= 0 && bi1.data[num] == bi2.data[num])
		{
			num--;
		}
		if (num >= 0)
		{
			if (bi1.data[num] > bi2.data[num])
			{
				return true;
			}
			return false;
		}
		return false;
	}

	public static bool operator <(BigInteger bi1, BigInteger bi2)
	{
		int num = 255;
		if ((bi1.data[num] & 0x80000000u) != 0 && (bi2.data[num] & 0x80000000u) == 0)
		{
			return true;
		}
		if ((bi1.data[num] & 0x80000000u) == 0 && (bi2.data[num] & 0x80000000u) != 0)
		{
			return false;
		}
		int num2 = ((bi1.dataLength > bi2.dataLength) ? bi1.dataLength : bi2.dataLength);
		num = num2 - 1;
		while (num >= 0 && bi1.data[num] == bi2.data[num])
		{
			num--;
		}
		if (num >= 0)
		{
			if (bi1.data[num] < bi2.data[num])
			{
				return true;
			}
			return false;
		}
		return false;
	}

	public static bool operator >=(BigInteger bi1, BigInteger bi2)
	{
		return bi1 == bi2 || bi1 > bi2;
	}

	public static bool operator <=(BigInteger bi1, BigInteger bi2)
	{
		return bi1 == bi2 || bi1 < bi2;
	}

	private static void MultiByteDivide(BigInteger bi1, BigInteger bi2, BigInteger outQuotient, BigInteger outRemainder)
	{
		uint[] array = new uint[256];
		int num = bi1.dataLength + 1;
		uint[] array2 = new uint[num];
		uint num2 = 2147483648u;
		uint num3 = bi2.data[bi2.dataLength - 1];
		int num4 = 0;
		int num5 = 0;
		while (num2 != 0 && (num3 & num2) == 0)
		{
			num4++;
			num2 >>= 1;
		}
		for (int i = 0; i < bi1.dataLength; i++)
		{
			array2[i] = bi1.data[i];
		}
		ShiftLeft(array2, num4);
		bi2 <<= num4;
		int num6 = num - bi2.dataLength;
		int num7 = num - 1;
		ulong num8 = bi2.data[bi2.dataLength - 1];
		ulong num9 = bi2.data[bi2.dataLength - 2];
		int num10 = bi2.dataLength + 1;
		uint[] array3 = new uint[num10];
		while (num6 > 0)
		{
			ulong num11 = ((ulong)array2[num7] << 32) + array2[num7 - 1];
			ulong num12 = num11 / num8;
			ulong num13 = num11 % num8;
			bool flag = false;
			while (!flag)
			{
				flag = true;
				if (num12 == 4294967296L || num12 * num9 > (num13 << 32) + array2[num7 - 2])
				{
					num12--;
					num13 += num8;
					if (num13 < 4294967296L)
					{
						flag = false;
					}
				}
			}
			for (int j = 0; j < num10; j++)
			{
				array3[j] = array2[num7 - j];
			}
			BigInteger bigInteger = new BigInteger(array3);
			BigInteger bigInteger2;
			for (bigInteger2 = bi2 * (long)num12; bigInteger2 > bigInteger; bigInteger2 -= bi2)
			{
				num12--;
			}
			BigInteger bigInteger3 = bigInteger - bigInteger2;
			for (int k = 0; k < num10; k++)
			{
				array2[num7 - k] = bigInteger3.data[bi2.dataLength - k];
			}
			array[num5++] = (uint)num12;
			num7--;
			num6--;
		}
		outQuotient.dataLength = num5;
		int l = 0;
		int num14 = outQuotient.dataLength - 1;
		while (num14 >= 0)
		{
			outQuotient.data[l] = array[num14];
			num14--;
			l++;
		}
		for (; l < 256; l++)
		{
			outQuotient.data[l] = 0u;
		}
		while (outQuotient.dataLength > 1 && outQuotient.data[outQuotient.dataLength - 1] == 0)
		{
			outQuotient.dataLength--;
		}
		if (outQuotient.dataLength == 0)
		{
			outQuotient.dataLength = 1;
		}
		outRemainder.dataLength = ShiftRight(array2, num4);
		for (l = 0; l < outRemainder.dataLength; l++)
		{
			outRemainder.data[l] = array2[l];
		}
		for (; l < 256; l++)
		{
			outRemainder.data[l] = 0u;
		}
	}

	private static void SingleByteDivide(BigInteger bi1, BigInteger bi2, BigInteger outQuotient, BigInteger outRemainder)
	{
		uint[] array = new uint[256];
		int num = 0;
		for (int i = 0; i < 256; i++)
		{
			outRemainder.data[i] = bi1.data[i];
		}
		outRemainder.dataLength = bi1.dataLength;
		while (outRemainder.dataLength > 1 && outRemainder.data[outRemainder.dataLength - 1] == 0)
		{
			outRemainder.dataLength--;
		}
		ulong num2 = bi2.data[0];
		int num3 = outRemainder.dataLength - 1;
		ulong num4 = outRemainder.data[num3];
		if (num4 >= num2)
		{
			ulong num5 = num4 / num2;
			array[num++] = (uint)num5;
			outRemainder.data[num3] = (uint)(num4 % num2);
		}
		num3--;
		while (num3 >= 0)
		{
			num4 = ((ulong)outRemainder.data[num3 + 1] << 32) + outRemainder.data[num3];
			ulong num6 = num4 / num2;
			array[num++] = (uint)num6;
			outRemainder.data[num3 + 1] = 0u;
			outRemainder.data[num3--] = (uint)(num4 % num2);
		}
		outQuotient.dataLength = num;
		int j = 0;
		int num7 = outQuotient.dataLength - 1;
		while (num7 >= 0)
		{
			outQuotient.data[j] = array[num7];
			num7--;
			j++;
		}
		for (; j < 256; j++)
		{
			outQuotient.data[j] = 0u;
		}
		while (outQuotient.dataLength > 1 && outQuotient.data[outQuotient.dataLength - 1] == 0)
		{
			outQuotient.dataLength--;
		}
		if (outQuotient.dataLength == 0)
		{
			outQuotient.dataLength = 1;
		}
		while (outRemainder.dataLength > 1 && outRemainder.data[outRemainder.dataLength - 1] == 0)
		{
			outRemainder.dataLength--;
		}
	}

	public static BigInteger operator /(BigInteger bi1, BigInteger bi2)
	{
		BigInteger bigInteger = new BigInteger();
		BigInteger outRemainder = new BigInteger();
		int num = 255;
		bool flag = false;
		bool flag2 = false;
		if ((bi1.data[num] & 0x80000000u) != 0)
		{
			bi1 = -bi1;
			flag2 = true;
		}
		if ((bi2.data[num] & 0x80000000u) != 0)
		{
			bi2 = -bi2;
			flag = true;
		}
		if (bi1 < bi2)
		{
			return bigInteger;
		}
		if (bi2.dataLength == 1)
		{
			SingleByteDivide(bi1, bi2, bigInteger, outRemainder);
		}
		else
		{
			MultiByteDivide(bi1, bi2, bigInteger, outRemainder);
		}
		if (flag2 != flag)
		{
			return -bigInteger;
		}
		return bigInteger;
	}

	public static BigInteger operator %(BigInteger bi1, BigInteger bi2)
	{
		BigInteger outQuotient = new BigInteger();
		BigInteger bigInteger = new BigInteger(bi1);
		int num = 255;
		bool flag = false;
		if ((bi1.data[num] & 0x80000000u) != 0)
		{
			bi1 = -bi1;
			flag = true;
		}
		if ((bi2.data[num] & 0x80000000u) != 0)
		{
			bi2 = -bi2;
		}
		if (bi1 < bi2)
		{
			return bigInteger;
		}
		if (bi2.dataLength == 1)
		{
			SingleByteDivide(bi1, bi2, outQuotient, bigInteger);
		}
		else
		{
			MultiByteDivide(bi1, bi2, outQuotient, bigInteger);
		}
		if (flag)
		{
			return -bigInteger;
		}
		return bigInteger;
	}

	public static BigInteger operator &(BigInteger bi1, BigInteger bi2)
	{
		BigInteger bigInteger = new BigInteger();
		int num = ((bi1.dataLength > bi2.dataLength) ? bi1.dataLength : bi2.dataLength);
		for (int i = 0; i < num; i++)
		{
			uint num2 = bi1.data[i] & bi2.data[i];
			bigInteger.data[i] = num2;
		}
		bigInteger.dataLength = 256;
		while (bigInteger.dataLength > 1 && bigInteger.data[bigInteger.dataLength - 1] == 0)
		{
			bigInteger.dataLength--;
		}
		return bigInteger;
	}

	public static BigInteger operator |(BigInteger bi1, BigInteger bi2)
	{
		BigInteger bigInteger = new BigInteger();
		int num = ((bi1.dataLength > bi2.dataLength) ? bi1.dataLength : bi2.dataLength);
		for (int i = 0; i < num; i++)
		{
			uint num2 = bi1.data[i] | bi2.data[i];
			bigInteger.data[i] = num2;
		}
		bigInteger.dataLength = 256;
		while (bigInteger.dataLength > 1 && bigInteger.data[bigInteger.dataLength - 1] == 0)
		{
			bigInteger.dataLength--;
		}
		return bigInteger;
	}

	public static BigInteger operator ^(BigInteger bi1, BigInteger bi2)
	{
		BigInteger bigInteger = new BigInteger();
		int num = ((bi1.dataLength > bi2.dataLength) ? bi1.dataLength : bi2.dataLength);
		for (int i = 0; i < num; i++)
		{
			uint num2 = bi1.data[i] ^ bi2.data[i];
			bigInteger.data[i] = num2;
		}
		bigInteger.dataLength = 256;
		while (bigInteger.dataLength > 1 && bigInteger.data[bigInteger.dataLength - 1] == 0)
		{
			bigInteger.dataLength--;
		}
		return bigInteger;
	}

	public BigInteger Max(BigInteger bi)
	{
		if (this > bi)
		{
			return new BigInteger(this);
		}
		return new BigInteger(bi);
	}

	public BigInteger Min(BigInteger bi)
	{
		if (this < bi)
		{
			return new BigInteger(this);
		}
		return new BigInteger(bi);
	}

	public BigInteger Abs()
	{
		if ((data[255] & 0x80000000u) != 0)
		{
			return -this;
		}
		return new BigInteger(this);
	}

	public override string ToString()
	{
		return ToString(10);
	}

	public string ToString(int radix)
	{
		if (radix < 2 || radix > 36)
		{
			throw new ArgumentException("Radix must be >= 2 and <= 36");
		}
		string text = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
		string text2 = "";
		BigInteger bigInteger = this;
		bool flag = false;
		if ((bigInteger.data[255] & 0x80000000u) != 0)
		{
			flag = true;
			try
			{
				bigInteger = -bigInteger;
			}
			catch (Exception)
			{
			}
		}
		BigInteger bigInteger2 = new BigInteger();
		BigInteger bigInteger3 = new BigInteger();
		BigInteger bi = new BigInteger(radix);
		if (bigInteger.dataLength == 1 && bigInteger.data[0] == 0)
		{
			text2 = "0";
		}
		else
		{
			while (bigInteger.dataLength > 1 || (bigInteger.dataLength == 1 && bigInteger.data[0] != 0))
			{
				SingleByteDivide(bigInteger, bi, bigInteger2, bigInteger3);
				text2 = ((bigInteger3.data[0] >= 10) ? (text[(int)(bigInteger3.data[0] - 10)] + text2) : (bigInteger3.data[0] + text2));
				bigInteger = bigInteger2;
			}
			if (flag)
			{
				text2 = "-" + text2;
			}
		}
		return text2;
	}

	public string ToHexString()
	{
		string text = data[dataLength - 1].ToString("X");
		for (int num = dataLength - 2; num >= 0; num--)
		{
			text += data[num].ToString("X8");
		}
		return text;
	}

	public BigInteger ModPow(BigInteger exp, BigInteger n)
	{
		if ((exp.data[255] & 0x80000000u) != 0)
		{
			throw new ArithmeticException("Positive exponents only.");
		}
		BigInteger bigInteger = 1;
		bool flag = false;
		BigInteger bigInteger2;
		if ((data[255] & 0x80000000u) != 0)
		{
			bigInteger2 = -this % n;
			flag = true;
		}
		else
		{
			bigInteger2 = this % n;
		}
		if ((n.data[255] & 0x80000000u) != 0)
		{
			n = -n;
		}
		BigInteger bigInteger3 = new BigInteger();
		int num = n.dataLength << 1;
		bigInteger3.data[num] = 1u;
		bigInteger3.dataLength = num + 1;
		bigInteger3 /= n;
		int num2 = exp.BitCount();
		int num3 = 0;
		for (int i = 0; i < exp.dataLength; i++)
		{
			uint num4 = 1u;
			for (int j = 0; j < 32; j++)
			{
				if ((exp.data[i] & num4) != 0)
				{
					bigInteger = BarrettReduction(bigInteger * bigInteger2, n, bigInteger3);
				}
				num4 <<= 1;
				bigInteger2 = BarrettReduction(bigInteger2 * bigInteger2, n, bigInteger3);
				if (bigInteger2.dataLength == 1 && bigInteger2.data[0] == 1)
				{
					if (flag && (exp.data[0] & (true ? 1u : 0u)) != 0)
					{
						return -bigInteger;
					}
					return bigInteger;
				}
				num3++;
				if (num3 == num2)
				{
					break;
				}
			}
		}
		if (flag && (exp.data[0] & (true ? 1u : 0u)) != 0)
		{
			return -bigInteger;
		}
		return bigInteger;
	}

	private BigInteger BarrettReduction(BigInteger x, BigInteger n, BigInteger constant)
	{
		int num = n.dataLength;
		int num2 = num + 1;
		int num3 = num - 1;
		BigInteger bigInteger = new BigInteger();
		int num4 = num3;
		int num5 = 0;
		while (num4 < x.dataLength)
		{
			bigInteger.data[num5] = x.data[num4];
			num4++;
			num5++;
		}
		bigInteger.dataLength = x.dataLength - num3;
		if (bigInteger.dataLength <= 0)
		{
			bigInteger.dataLength = 1;
		}
		BigInteger bigInteger2 = bigInteger * constant;
		BigInteger bigInteger3 = new BigInteger();
		int num6 = num2;
		int num7 = 0;
		while (num6 < bigInteger2.dataLength)
		{
			bigInteger3.data[num7] = bigInteger2.data[num6];
			num6++;
			num7++;
		}
		bigInteger3.dataLength = bigInteger2.dataLength - num2;
		if (bigInteger3.dataLength <= 0)
		{
			bigInteger3.dataLength = 1;
		}
		BigInteger bigInteger4 = new BigInteger();
		int num8 = ((x.dataLength > num2) ? num2 : x.dataLength);
		for (int i = 0; i < num8; i++)
		{
			bigInteger4.data[i] = x.data[i];
		}
		bigInteger4.dataLength = num8;
		BigInteger bigInteger5 = new BigInteger();
		for (int j = 0; j < bigInteger3.dataLength; j++)
		{
			if (bigInteger3.data[j] != 0)
			{
				ulong num9 = 0uL;
				int num10 = j;
				int num11 = 0;
				while (num11 < n.dataLength && num10 < num2)
				{
					ulong num12 = (ulong)((long)bigInteger3.data[j] * (long)n.data[num11] + bigInteger5.data[num10]) + num9;
					bigInteger5.data[num10] = (uint)(num12 & 0xFFFFFFFFu);
					num9 = num12 >> 32;
					num11++;
					num10++;
				}
				if (num10 < num2)
				{
					bigInteger5.data[num10] = (uint)num9;
				}
			}
		}
		bigInteger5.dataLength = num2;
		while (bigInteger5.dataLength > 1 && bigInteger5.data[bigInteger5.dataLength - 1] == 0)
		{
			bigInteger5.dataLength--;
		}
		bigInteger4 -= bigInteger5;
		if ((bigInteger4.data[255] & 0x80000000u) != 0)
		{
			BigInteger bigInteger6 = new BigInteger();
			bigInteger6.data[num2] = 1u;
			bigInteger6.dataLength = num2 + 1;
			bigInteger4 += bigInteger6;
		}
		for (; bigInteger4 >= n; bigInteger4 -= n)
		{
		}
		return bigInteger4;
	}

	public BigInteger Gcd(BigInteger bi)
	{
		BigInteger bigInteger = (((data[255] & 0x80000000u) == 0) ? this : (-this));
		BigInteger bigInteger2 = (((bi.data[255] & 0x80000000u) == 0) ? bi : (-bi));
		BigInteger bigInteger3 = bigInteger2;
		while (bigInteger.dataLength > 1 || (bigInteger.dataLength == 1 && bigInteger.data[0] != 0))
		{
			bigInteger3 = bigInteger;
			bigInteger = bigInteger2 % bigInteger;
			bigInteger2 = bigInteger3;
		}
		return bigInteger3;
	}

	public void GenRandomBits(int bits, StrongNumberProvider rand)
	{
		int num = bits >> 5;
		int num2 = bits & 0x1F;
		if (num2 != 0)
		{
			num++;
		}
		if (num > 256)
		{
			throw new ArithmeticException("Number of required bits > maxLength.");
		}
		for (int i = 0; i < num; i++)
		{
			data[i] = rand.NextUInt32();
		}
		for (int j = num; j < 256; j++)
		{
			data[j] = 0u;
		}
		if (num2 != 0)
		{
			uint num3 = (uint)(1 << num2 - 1);
			data[num - 1] |= num3;
			num3 = uint.MaxValue >> 32 - num2;
			data[num - 1] &= num3;
		}
		else
		{
			data[num - 1] |= 2147483648u;
		}
		dataLength = num;
		if (dataLength == 0)
		{
			dataLength = 1;
		}
	}

	public int BitCount()
	{
		while (dataLength > 1 && data[dataLength - 1] == 0)
		{
			dataLength--;
		}
		uint num = data[dataLength - 1];
		uint num2 = 2147483648u;
		int num3 = 32;
		while (num3 > 0 && (num & num2) == 0)
		{
			num3--;
			num2 >>= 1;
		}
		return num3 + (dataLength - 1 << 5);
	}

	public bool FermatLittleTest(int confidence)
	{
		BigInteger bigInteger = (((data[255] & 0x80000000u) == 0) ? this : (-this));
		if (bigInteger.dataLength == 1)
		{
			if (bigInteger.data[0] == 0 || bigInteger.data[0] == 1)
			{
				return false;
			}
			if (bigInteger.data[0] == 2 || bigInteger.data[0] == 3)
			{
				return true;
			}
		}
		if ((bigInteger.data[0] & 1) == 0)
		{
			return false;
		}
		int num = bigInteger.BitCount();
		BigInteger bigInteger2 = new BigInteger();
		BigInteger exp = bigInteger - new BigInteger(1L);
		StrongNumberProvider strongNumberProvider = new StrongNumberProvider();
		for (int i = 0; i < confidence; i++)
		{
			bool flag = false;
			while (!flag)
			{
				int num2;
				for (num2 = 0; num2 < 2; num2 = strongNumberProvider.NextInt() * num)
				{
				}
				bigInteger2.GenRandomBits(num2, strongNumberProvider);
				int num3 = bigInteger2.dataLength;
				if (num3 > 1 || (num3 == 1 && bigInteger2.data[0] != 1))
				{
					flag = true;
				}
			}
			BigInteger bigInteger3 = bigInteger2.Gcd(bigInteger);
			if (bigInteger3.dataLength == 1 && bigInteger3.data[0] != 1)
			{
				return false;
			}
			BigInteger bigInteger4 = bigInteger2.ModPow(exp, bigInteger);
			int num4 = bigInteger4.dataLength;
			if (num4 > 1 || (num4 == 1 && bigInteger4.data[0] != 1))
			{
				return false;
			}
		}
		return true;
	}

	public bool RabinMillerTest(int confidence)
	{
		BigInteger bigInteger = (((data[255] & 0x80000000u) == 0) ? this : (-this));
		if (bigInteger.dataLength == 1)
		{
			if (bigInteger.data[0] == 0 || bigInteger.data[0] == 1)
			{
				return false;
			}
			if (bigInteger.data[0] == 2 || bigInteger.data[0] == 3)
			{
				return true;
			}
		}
		if ((bigInteger.data[0] & 1) == 0)
		{
			return false;
		}
		BigInteger bigInteger2 = bigInteger - new BigInteger(1L);
		int num = 0;
		for (int i = 0; i < bigInteger2.dataLength; i++)
		{
			uint num2 = 1u;
			for (int j = 0; j < 32; j++)
			{
				if ((bigInteger2.data[i] & num2) != 0)
				{
					i = bigInteger2.dataLength;
					break;
				}
				num2 <<= 1;
				num++;
			}
		}
		BigInteger exp = bigInteger2 >> num;
		int num3 = bigInteger.BitCount();
		BigInteger bigInteger3 = new BigInteger();
		StrongNumberProvider strongNumberProvider = new StrongNumberProvider();
		for (int k = 0; k < confidence; k++)
		{
			bool flag = false;
			while (!flag)
			{
				int num4;
				for (num4 = 0; num4 < 2; num4 = (int)(strongNumberProvider.NextSingle() * (float)num3))
				{
				}
				bigInteger3.GenRandomBits(num4, strongNumberProvider);
				int num5 = bigInteger3.dataLength;
				if (num5 > 1 || (num5 == 1 && bigInteger3.data[0] != 1))
				{
					flag = true;
				}
			}
			BigInteger bigInteger4 = bigInteger3.Gcd(bigInteger);
			if (bigInteger4.dataLength == 1 && bigInteger4.data[0] != 1)
			{
				return false;
			}
			BigInteger bigInteger5 = bigInteger3.ModPow(exp, bigInteger);
			bool flag2 = false;
			if (bigInteger5.dataLength == 1 && bigInteger5.data[0] == 1)
			{
				flag2 = true;
			}
			int num6 = 0;
			while (!flag2 && num6 < num)
			{
				if (bigInteger5 == bigInteger2)
				{
					flag2 = true;
					break;
				}
				bigInteger5 = bigInteger5 * bigInteger5 % bigInteger;
				num6++;
			}
			if (!flag2)
			{
				return false;
			}
		}
		return true;
	}

	public bool SolovayStrassenTest(int confidence)
	{
		BigInteger bigInteger = (((data[255] & 0x80000000u) == 0) ? this : (-this));
		if (bigInteger.dataLength == 1)
		{
			if (bigInteger.data[0] == 0 || bigInteger.data[0] == 1)
			{
				return false;
			}
			if (bigInteger.data[0] == 2 || bigInteger.data[0] == 3)
			{
				return true;
			}
		}
		if ((bigInteger.data[0] & 1) == 0)
		{
			return false;
		}
		int num = bigInteger.BitCount();
		BigInteger bigInteger2 = new BigInteger();
		BigInteger bigInteger3 = bigInteger - 1;
		BigInteger exp = bigInteger3 >> 1;
		StrongNumberProvider strongNumberProvider = new StrongNumberProvider();
		for (int i = 0; i < confidence; i++)
		{
			bool flag = false;
			while (!flag)
			{
				int num2;
				for (num2 = 0; num2 < 2; num2 = (int)(strongNumberProvider.NextSingle() * (float)num))
				{
				}
				bigInteger2.GenRandomBits(num2, strongNumberProvider);
				int num3 = bigInteger2.dataLength;
				if (num3 > 1 || (num3 == 1 && bigInteger2.data[0] != 1))
				{
					flag = true;
				}
			}
			BigInteger bigInteger4 = bigInteger2.Gcd(bigInteger);
			if (bigInteger4.dataLength == 1 && bigInteger4.data[0] != 1)
			{
				return false;
			}
			BigInteger bigInteger5 = bigInteger2.ModPow(exp, bigInteger);
			if (bigInteger5 == bigInteger3)
			{
				bigInteger5 = -1;
			}
			BigInteger bigInteger6 = Jacobi(bigInteger2, bigInteger);
			if (bigInteger5 != bigInteger6)
			{
				return false;
			}
		}
		return true;
	}

	public bool LucasStrongTest()
	{
		BigInteger bigInteger = (((data[255] & 0x80000000u) == 0) ? this : (-this));
		if (bigInteger.dataLength == 1)
		{
			if (bigInteger.data[0] == 0 || bigInteger.data[0] == 1)
			{
				return false;
			}
			if (bigInteger.data[0] == 2 || bigInteger.data[0] == 3)
			{
				return true;
			}
		}
		if ((bigInteger.data[0] & 1) == 0)
		{
			return false;
		}
		return LucasStrongTestHelper(bigInteger);
	}

	private bool LucasStrongTestHelper(BigInteger thisVal)
	{
		long num = 5L;
		long num2 = -1L;
		long num3 = 0L;
		for (bool flag = false; !flag; num3++)
		{
			switch (Jacobi(num, thisVal))
			{
			case -1:
				flag = true;
				continue;
			case 0:
				if (Math.Abs(num) < thisVal)
				{
					return false;
				}
				break;
			}
			if (num3 == 20)
			{
				BigInteger bigInteger = thisVal.Sqrt();
				if (bigInteger * bigInteger == thisVal)
				{
					return false;
				}
			}
			num = (Math.Abs(num) + 2) * num2;
			num2 = -num2;
		}
		long num4 = 1 - num >> 2;
		BigInteger bigInteger2 = thisVal + 1;
		int num5 = 0;
		for (int i = 0; i < bigInteger2.dataLength; i++)
		{
			uint num6 = 1u;
			for (int j = 0; j < 32; j++)
			{
				if ((bigInteger2.data[i] & num6) != 0)
				{
					i = bigInteger2.dataLength;
					break;
				}
				num6 <<= 1;
				num5++;
			}
		}
		BigInteger k = bigInteger2 >> num5;
		BigInteger bigInteger3 = new BigInteger();
		int num7 = thisVal.dataLength << 1;
		bigInteger3.data[num7] = 1u;
		bigInteger3.dataLength = num7 + 1;
		bigInteger3 /= thisVal;
		BigInteger[] array = LucasSequenceHelper(1, num4, k, thisVal, bigInteger3, 0);
		bool flag2 = false;
		if ((array[0].dataLength == 1 && array[0].data[0] == 0) || (array[1].dataLength == 1 && array[1].data[0] == 0))
		{
			flag2 = true;
		}
		for (int l = 1; l < num5; l++)
		{
			if (!flag2)
			{
				array[1] = thisVal.BarrettReduction(array[1] * array[1], thisVal, bigInteger3);
				array[1] = (array[1] - (array[2] << 1)) % thisVal;
				if (array[1].dataLength == 1 && array[1].data[0] == 0)
				{
					flag2 = true;
				}
			}
			array[2] = thisVal.BarrettReduction(array[2] * array[2], thisVal, bigInteger3);
		}
		if (flag2)
		{
			BigInteger bigInteger4 = thisVal.Gcd(num4);
			if (bigInteger4.dataLength == 1 && bigInteger4.data[0] == 1)
			{
				if ((array[2].data[255] & 0x80000000u) != 0)
				{
					array[2] += thisVal;
				}
				BigInteger bigInteger5 = num4 * Jacobi(num4, thisVal) % thisVal;
				if ((bigInteger5.data[255] & 0x80000000u) != 0)
				{
					bigInteger5 += thisVal;
				}
				if (array[2] != bigInteger5)
				{
					flag2 = false;
				}
			}
		}
		return flag2;
	}

	public bool IsProbablePrime(int confidence)
	{
		BigInteger bigInteger = (((data[255] & 0x80000000u) == 0) ? this : (-this));
		for (int i = 0; i < primesBelow2000.Length; i++)
		{
			BigInteger bigInteger2 = primesBelow2000[i];
			if (bigInteger2 >= bigInteger)
			{
				break;
			}
			BigInteger bigInteger3 = bigInteger % bigInteger2;
			if (bigInteger3.IntValue() == 0)
			{
				return false;
			}
		}
		if (bigInteger.RabinMillerTest(confidence))
		{
			return true;
		}
		return false;
	}

	public bool IsProbablePrime()
	{
		BigInteger bigInteger = (((data[255] & 0x80000000u) == 0) ? this : (-this));
		if (bigInteger.dataLength == 1)
		{
			if (bigInteger.data[0] == 0 || bigInteger.data[0] == 1)
			{
				return false;
			}
			if (bigInteger.data[0] == 2 || bigInteger.data[0] == 3)
			{
				return true;
			}
		}
		if ((bigInteger.data[0] & 1) == 0)
		{
			return false;
		}
		for (int i = 0; i < primesBelow2000.Length; i++)
		{
			BigInteger bigInteger2 = primesBelow2000[i];
			if (bigInteger2 >= bigInteger)
			{
				break;
			}
			BigInteger bigInteger3 = bigInteger % bigInteger2;
			if (bigInteger3.IntValue() == 0)
			{
				return false;
			}
		}
		BigInteger bigInteger4 = bigInteger - new BigInteger(1L);
		int num = 0;
		for (int j = 0; j < bigInteger4.dataLength; j++)
		{
			uint num2 = 1u;
			for (int k = 0; k < 32; k++)
			{
				if ((bigInteger4.data[j] & num2) != 0)
				{
					j = bigInteger4.dataLength;
					break;
				}
				num2 <<= 1;
				num++;
			}
		}
		BigInteger exp = bigInteger4 >> num;
		int num3 = bigInteger.BitCount();
		BigInteger bigInteger5 = 2;
		BigInteger bigInteger6 = bigInteger5.ModPow(exp, bigInteger);
		bool flag = false;
		if (bigInteger6.dataLength == 1 && bigInteger6.data[0] == 1)
		{
			flag = true;
		}
		int num4 = 0;
		while (!flag && num4 < num)
		{
			if (bigInteger6 == bigInteger4)
			{
				flag = true;
				break;
			}
			bigInteger6 = bigInteger6 * bigInteger6 % bigInteger;
			num4++;
		}
		if (flag)
		{
			flag = LucasStrongTestHelper(bigInteger);
		}
		return flag;
	}

	public int IntValue()
	{
		return (int)data[0];
	}

	public long LongValue()
	{
		long num = 0L;
		num = data[0];
		try
		{
			num |= (long)((ulong)data[1] << 32);
		}
		catch (Exception)
		{
			if ((data[0] & 0x80000000u) != 0)
			{
				num = (int)data[0];
			}
		}
		return num;
	}

	public static int Jacobi(BigInteger a, BigInteger b)
	{
		if ((b.data[0] & 1) == 0)
		{
			throw new ArgumentException("Jacobi defined only for odd integers.");
		}
		if (a >= b)
		{
			a %= b;
		}
		if (a.dataLength == 1 && a.data[0] == 0)
		{
			return 0;
		}
		if (a.dataLength == 1 && a.data[0] == 1)
		{
			return 1;
		}
		if (a < 0)
		{
			if (((b - 1).data[0] & 2) == 0)
			{
				return Jacobi(-a, b);
			}
			return -Jacobi(-a, b);
		}
		int num = 0;
		for (int i = 0; i < a.dataLength; i++)
		{
			uint num2 = 1u;
			for (int j = 0; j < 32; j++)
			{
				if ((a.data[i] & num2) != 0)
				{
					i = a.dataLength;
					break;
				}
				num2 <<= 1;
				num++;
			}
		}
		BigInteger bigInteger = a >> num;
		int num3 = 1;
		if (((uint)num & (true ? 1u : 0u)) != 0 && ((b.data[0] & 7) == 3 || (b.data[0] & 7) == 5))
		{
			num3 = -1;
		}
		if ((b.data[0] & 3) == 3 && (bigInteger.data[0] & 3) == 3)
		{
			num3 = -num3;
		}
		if (bigInteger.dataLength == 1 && bigInteger.data[0] == 1)
		{
			return num3;
		}
		return num3 * Jacobi(b % bigInteger, bigInteger);
	}

	public static BigInteger GenPseudoPrime(int bits, int confidence, StrongNumberProvider rand)
	{
		BigInteger bigInteger = new BigInteger();
		bool flag = false;
		while (!flag)
		{
			bigInteger.GenRandomBits(bits, rand);
			bigInteger.data[0] |= 1u;
			flag = bigInteger.IsProbablePrime(confidence);
		}
		return bigInteger;
	}

	public BigInteger GenCoPrime(int bits, StrongNumberProvider rand)
	{
		bool flag = false;
		BigInteger bigInteger = new BigInteger();
		while (!flag)
		{
			bigInteger.GenRandomBits(bits, rand);
			BigInteger bigInteger2 = bigInteger.Gcd(this);
			if (bigInteger2.dataLength == 1 && bigInteger2.data[0] == 1)
			{
				flag = true;
			}
		}
		return bigInteger;
	}

	public BigInteger ModInverse(BigInteger modulus)
	{
		BigInteger[] array = new BigInteger[2] { 0, 1 };
		BigInteger[] array2 = new BigInteger[2];
		BigInteger[] array3 = new BigInteger[2] { 0, 0 };
		int num = 0;
		BigInteger bi = modulus;
		BigInteger bigInteger = this;
		while (bigInteger.dataLength > 1 || (bigInteger.dataLength == 1 && bigInteger.data[0] != 0))
		{
			BigInteger bigInteger2 = new BigInteger();
			BigInteger bigInteger3 = new BigInteger();
			if (num > 1)
			{
				BigInteger bigInteger4 = (array[0] - array[1] * array2[0]) % modulus;
				array[0] = array[1];
				array[1] = bigInteger4;
			}
			if (bigInteger.dataLength == 1)
			{
				SingleByteDivide(bi, bigInteger, bigInteger2, bigInteger3);
			}
			else
			{
				MultiByteDivide(bi, bigInteger, bigInteger2, bigInteger3);
			}
			array2[0] = array2[1];
			array3[0] = array3[1];
			array2[1] = bigInteger2;
			array3[1] = bigInteger3;
			bi = bigInteger;
			bigInteger = bigInteger3;
			num++;
		}
		if (array3[0].dataLength > 1 || (array3[0].dataLength == 1 && array3[0].data[0] != 1))
		{
			throw new ArithmeticException("No inverse!");
		}
		BigInteger bigInteger5 = (array[0] - array[1] * array2[0]) % modulus;
		if ((bigInteger5.data[255] & 0x80000000u) != 0)
		{
			bigInteger5 += modulus;
		}
		return bigInteger5;
	}

	public byte[] GetBytes()
	{
		int num = BitCount();
		int num2 = num >> 3;
		if (((uint)num & 7u) != 0)
		{
			num2++;
		}
		byte[] array = new byte[num2];
		int num3 = 0;
		uint num4 = data[dataLength - 1];
		uint num5;
		if ((num5 = (num4 >> 24) & 0xFFu) != 0)
		{
			array[num3++] = (byte)num5;
		}
		if ((num5 = (num4 >> 16) & 0xFFu) != 0)
		{
			array[num3++] = (byte)num5;
		}
		if ((num5 = (num4 >> 8) & 0xFFu) != 0)
		{
			array[num3++] = (byte)num5;
		}
		if ((num5 = num4 & 0xFFu) != 0)
		{
			array[num3++] = (byte)num5;
		}
		int num6 = dataLength - 2;
		while (num6 >= 0)
		{
			num4 = data[num6];
			array[num3 + 3] = (byte)(num4 & 0xFFu);
			num4 >>= 8;
			array[num3 + 2] = (byte)(num4 & 0xFFu);
			num4 >>= 8;
			array[num3 + 1] = (byte)(num4 & 0xFFu);
			num4 >>= 8;
			array[num3] = (byte)(num4 & 0xFFu);
			num6--;
			num3 += 4;
		}
		return array;
	}

	public void SetBit(uint bitNum)
	{
		uint num = bitNum >> 5;
		byte b = (byte)(bitNum & 0x1Fu);
		uint num2 = (uint)(1 << (int)b);
		data[num] |= num2;
		if (num >= dataLength)
		{
			dataLength = (int)(num + 1);
		}
	}

	public void UnsetBit(uint bitNum)
	{
		uint num = bitNum >> 5;
		if (num < dataLength)
		{
			byte b = (byte)(bitNum & 0x1Fu);
			uint num2 = (uint)(1 << (int)b);
			uint num3 = 0xFFFFFFFFu ^ num2;
			data[num] &= num3;
			if (dataLength > 1 && data[dataLength - 1] == 0)
			{
				dataLength--;
			}
		}
	}

	public BigInteger Sqrt()
	{
		uint num = (uint)BitCount();
		num = (((num & 1) == 0) ? (num >> 1) : ((num >> 1) + 1));
		uint num2 = num >> 5;
		byte b = (byte)(num & 0x1Fu);
		BigInteger bigInteger = new BigInteger();
		uint num3;
		if (b == 0)
		{
			num3 = 2147483648u;
		}
		else
		{
			num3 = (uint)(1 << (int)b);
			num2++;
		}
		bigInteger.dataLength = (int)num2;
		for (int num4 = (int)(num2 - 1); num4 >= 0; num4--)
		{
			while (num3 != 0)
			{
				bigInteger.data[num4] ^= num3;
				if (bigInteger * bigInteger > this)
				{
					bigInteger.data[num4] ^= num3;
				}
				num3 >>= 1;
			}
			num3 = 2147483648u;
		}
		return bigInteger;
	}

	public static BigInteger[] LucasSequence(BigInteger P, BigInteger Q, BigInteger k, BigInteger n)
	{
		if (k.dataLength == 1 && k.data[0] == 0)
		{
			return new BigInteger[3]
			{
				0,
				2 % n,
				1 % n
			};
		}
		BigInteger bigInteger = new BigInteger();
		int num = n.dataLength << 1;
		bigInteger.data[num] = 1u;
		bigInteger.dataLength = num + 1;
		bigInteger /= n;
		int num2 = 0;
		for (int i = 0; i < k.dataLength; i++)
		{
			uint num3 = 1u;
			for (int j = 0; j < 32; j++)
			{
				if ((k.data[i] & num3) != 0)
				{
					i = k.dataLength;
					break;
				}
				num3 <<= 1;
				num2++;
			}
		}
		BigInteger k2 = k >> num2;
		return LucasSequenceHelper(P, Q, k2, n, bigInteger, num2);
	}

	private static BigInteger[] LucasSequenceHelper(BigInteger P, BigInteger Q, BigInteger k, BigInteger n, BigInteger constant, int s)
	{
		BigInteger[] array = new BigInteger[3];
		if ((k.data[0] & 1) == 0)
		{
			throw new ArgumentException("Argument k must be odd.");
		}
		int num = k.BitCount();
		uint num2 = (uint)(1 << (num & 0x1F) - 1);
		BigInteger bigInteger = 2 % n;
		BigInteger bigInteger2 = 1 % n;
		BigInteger bigInteger3 = P % n;
		BigInteger bigInteger4 = bigInteger2;
		bool flag = true;
		for (int num3 = k.dataLength - 1; num3 >= 0; num3--)
		{
			while (num2 != 0 && (num3 != 0 || num2 != 1))
			{
				if ((k.data[num3] & num2) != 0)
				{
					bigInteger4 = bigInteger4 * bigInteger3 % n;
					bigInteger = (bigInteger * bigInteger3 - P * bigInteger2) % n;
					bigInteger3 = n.BarrettReduction(bigInteger3 * bigInteger3, n, constant);
					bigInteger3 = (bigInteger3 - (bigInteger2 * Q << 1)) % n;
					if (flag)
					{
						flag = false;
					}
					else
					{
						bigInteger2 = n.BarrettReduction(bigInteger2 * bigInteger2, n, constant);
					}
					bigInteger2 = bigInteger2 * Q % n;
				}
				else
				{
					bigInteger4 = (bigInteger4 * bigInteger - bigInteger2) % n;
					bigInteger3 = (bigInteger * bigInteger3 - P * bigInteger2) % n;
					bigInteger = n.BarrettReduction(bigInteger * bigInteger, n, constant);
					bigInteger = (bigInteger - (bigInteger2 << 1)) % n;
					if (flag)
					{
						bigInteger2 = Q % n;
						flag = false;
					}
					else
					{
						bigInteger2 = n.BarrettReduction(bigInteger2 * bigInteger2, n, constant);
					}
				}
				num2 >>= 1;
			}
			num2 = 2147483648u;
		}
		bigInteger4 = (bigInteger4 * bigInteger - bigInteger2) % n;
		bigInteger = (bigInteger * bigInteger3 - P * bigInteger2) % n;
		if (flag)
		{
			flag = false;
		}
		else
		{
			bigInteger2 = n.BarrettReduction(bigInteger2 * bigInteger2, n, constant);
		}
		bigInteger2 = bigInteger2 * Q % n;
		for (int i = 0; i < s; i++)
		{
			bigInteger4 = bigInteger4 * bigInteger % n;
			bigInteger = (bigInteger * bigInteger - (bigInteger2 << 1)) % n;
			if (flag)
			{
				bigInteger2 = Q % n;
				flag = false;
			}
			else
			{
				bigInteger2 = n.BarrettReduction(bigInteger2 * bigInteger2, n, constant);
			}
		}
		array[0] = bigInteger4;
		array[1] = bigInteger;
		array[2] = bigInteger2;
		return array;
	}

	public void Dispose()
	{
		data = new uint[0];
		data = null;
		GC.Collect();
		GC.Collect();
	}
}
}