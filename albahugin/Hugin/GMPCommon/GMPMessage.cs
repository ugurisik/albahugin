using System;
using System.Collections.Generic;

namespace Hugin.GMPCommon { 

public class GMPMessage
{
	private int msgType;

	private string terminalNo;

	private List<GMPItem> items;

	public int MsgType => msgType;

	public string TerminalNo
	{
		get
		{
			return terminalNo;
		}
		set
		{
			terminalNo = value;
		}
	}

	private GMPMessage()
	{
		items = new List<GMPItem>();
	}

	public GMPMessage(int msgType)
		: this()
	{
		this.msgType = msgType;
	}

	public void AddItem(GMPItem item)
	{
		items.Add(item);
	}

	public void InsertItem(int index, GMPItem item)
	{
		items.Insert(index, item);
	}

	public GMPGroup FindGroup(int groupTag)
	{
		GMPItem gMPItem = items.Find((GMPItem item) => item is GMPGroup && item.Tag == groupTag);
		return (GMPGroup)gMPItem;
	}

	public GMPField FindTag(int tag)
	{
		GMPItem gMPItem = items.Find((GMPItem item) => item is GMPField && item.Tag == tag);
		return (GMPField)gMPItem;
	}

	public GMPItem[] GetGroupList()
	{
		List<GMPItem> list = items.FindAll((GMPItem item) => item is GMPGroup);
		return list.ToArray();
	}

	public GMPItem[] GetTagList()
	{
		List<GMPItem> list = items.FindAll((GMPItem item) => item is GMPField);
		return list.ToArray();
	}

	public static GMPMessage Parse(byte[] bytesRead)
	{
		int num = 0;
		int num2 = bytesRead.Length;
		int num3 = 0;
		int num4 = bytesRead[num++] * 256 + bytesRead[num++];
		string @string = MessageBuilder.DefaultEncoding.GetString(bytesRead, num, 12);
		num += 12;
		int tag = MessageBuilder.GetTag(bytesRead, num, out num);
		GMPMessage gMPMessage = new GMPMessage(tag);
		gMPMessage.terminalNo = @string;
		num4 = MessageBuilder.GetLength(bytesRead, num, out num);
		while (true)
		{
			try
			{
				if (num >= num2)
				{
					break;
				}
				bool flag = false;
				int num5 = num;
				int tag2 = MessageBuilder.GetTag(bytesRead, num, out num);
				flag = num - num5 == 3;
				int length = MessageBuilder.GetLength(bytesRead, num, out num);
				if (tag2 == 14675986)
				{
					num += length;
					continue;
				}
				byte[] array = new byte[length];
				Array.Copy(bytesRead, num, array, 0, length);
				if (flag)
				{
					GMPField gMPField = new GMPField(tag2, length, array);
					if (num3 > 0)
					{
						GMPGroup gMPGroup = gMPMessage.FindGroup(num3);
						if (gMPGroup == null)
						{
							gMPGroup = new GMPGroup(tag2);
						}
						gMPGroup.Add(gMPField);
					}
					else
					{
						gMPMessage.AddItem(gMPField);
					}
					num += length;
				}
				else
				{
					num3 = tag2;
					gMPMessage.AddItem(new GMPGroup(tag2));
				}
				continue;
			}
			catch (Exception)
			{
				continue;
			}
		}
		return gMPMessage;
	}

	public static GMPMessage Parse2(byte[] bytesRead)
	{
		int outOffset = 0;
		int num = bytesRead.Length;
		int num2 = 0;
		int num3 = 1;
		GMPMessage gMPMessage = new GMPMessage(num3);
		while (true)
		{
			try
			{
				if (outOffset >= num)
				{
					break;
				}
				bool flag = false;
				int num4 = outOffset;
				int tag = MessageBuilder.GetTag(bytesRead, outOffset, out outOffset);
				flag = outOffset - num4 == 3;
				int length = MessageBuilder.GetLength(bytesRead, outOffset, out outOffset);
				if (tag == 14675986)
				{
					outOffset += length;
					continue;
				}
				byte[] array = new byte[length];
				Array.Copy(bytesRead, outOffset, array, 0, length);
				if (flag)
				{
					GMPField gMPField = new GMPField(tag, length, array);
					if (num2 > 0)
					{
						GMPGroup gMPGroup = gMPMessage.FindGroup(num2);
						if (gMPGroup == null)
						{
							gMPGroup = new GMPGroup(tag);
						}
						gMPGroup.Add(gMPField);
					}
					else
					{
						gMPMessage.AddItem(gMPField);
					}
					outOffset += length;
				}
				else
				{
					num2 = tag;
					gMPMessage.AddItem(new GMPGroup(tag));
				}
				continue;
			}
			catch (Exception)
			{
				continue;
			}
		}
		return gMPMessage;
	}

	public byte[] ToByte()
	{
		List<byte> list = new List<byte>();
		foreach (GMPItem item in items)
		{
			list.AddRange(MessageBuilder.HexToByteArray(item.Tag));
			list.AddRange(MessageBuilder.AddLength(item.Length));
			list.AddRange(item.Value);
		}
		return list.ToArray();
	}
}
}