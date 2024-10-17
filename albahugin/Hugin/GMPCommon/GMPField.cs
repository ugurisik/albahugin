using System;
using System.Collections.Generic;

namespace Hugin.GMPCommon { 

public class GMPField : GMPItem
{
	private int tag;

	private int length;

	private byte[] value;

	public int Tag => tag;

	public int Length => length;

	public byte[] Value => value;

	public GMPField(int tag, byte[] value)
	{
		this.tag = tag;
		this.value = value;
		length = value.Length;
	}

	public GMPField(int tag, int length, byte[] value)
		: this(tag, value)
	{
		this.length = length;
	}

	public GMPField(int tag, string value)
		: this(tag, value.Length, MessageBuilder.DefaultEncoding.GetBytes(value))
	{
	}

	public static GMPField[] Parse(byte[] bytesRead)
	{
		int outOffset = 0;
		List<GMPField> list = new List<GMPField>();
		while (outOffset < bytesRead.Length)
		{
			int num = MessageBuilder.GetTag(bytesRead, outOffset, out outOffset);
			if (num == 14675986)
			{
				break;
			}
			int num2 = MessageBuilder.GetLength(bytesRead, outOffset, out outOffset);
			byte[] destinationArray = new byte[num2];
			Array.Copy(bytesRead, outOffset, destinationArray, 0, num2);
			outOffset += num2;
			list.Add(new GMPField(num, num2, destinationArray));
		}
		return list.ToArray();
	}
}
}