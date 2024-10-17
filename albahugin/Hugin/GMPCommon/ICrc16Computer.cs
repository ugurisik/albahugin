namespace Hugin.GMPCommon { 

public interface ICrc16Computer
{
	ushort ComputeChecksum(byte[] bytes, int offset, int length);
}
}