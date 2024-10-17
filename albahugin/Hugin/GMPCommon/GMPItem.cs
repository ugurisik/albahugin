namespace Hugin.GMPCommon { 

public interface GMPItem
{
	int Tag { get; }

	int Length { get; }

	byte[] Value { get; }
}
}