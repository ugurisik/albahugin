using System;
using System.Security.Cryptography;

namespace albahugin.Hugin.DiffieHellman { 

internal class StrongNumberProvider
{
	private static RNGCryptoServiceProvider csp = new RNGCryptoServiceProvider();

	public uint NextUInt32()
	{
		byte[] array = new byte[4];
		csp.GetBytes(array);
		return BitConverter.ToUInt32(array, 0);
	}

	public int NextInt()
	{
		byte[] array = new byte[4];
		csp.GetBytes(array);
		return BitConverter.ToInt32(array, 0);
	}

	public float NextSingle()
	{
		float num = NextUInt32();
		float num2 = 4.2949673E+09f;
		return num / num2;
	}
}
}