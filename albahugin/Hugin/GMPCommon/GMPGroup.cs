using System.Collections.Generic;

namespace Hugin.GMPCommon { 

public class GMPGroup : GMPItem
{
	private int groupTag;

	private List<GMPField> tags;

	public int Tag => groupTag;

	public int Length
	{
		get
		{
			int num = 0;
			foreach (GMPField tag in Tags)
			{
				num += tag.Length + 3 + 1;
			}
			return num;
		}
	}

	public byte[] Value
	{
		get
		{
			List<byte> list = new List<byte>();
			foreach (GMPField tag in Tags)
			{
				list.AddRange(MessageBuilder.HexToByteArray(tag.Tag));
				list.AddRange(MessageBuilder.AddLength(tag.Length));
				list.AddRange(tag.Value);
			}
			return list.ToArray();
		}
	}

	public List<GMPField> Tags => tags;

	public GMPGroup(int groupTag)
	{
		this.groupTag = groupTag;
		tags = new List<GMPField>();
	}

	public void Add(GMPField tlv)
	{
		tags.Add(tlv);
	}

	public GMPField FindTag(int tag)
	{
		return Tags.Find((GMPField tlvItem) => tlvItem.Tag == tag);
	}
}
}