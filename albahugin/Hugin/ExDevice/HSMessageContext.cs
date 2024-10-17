using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using albahugin.Hugin.Common;
using albahugin.Hugin.DiffieHellman;
using Hugin.GMPCommon;

namespace Hugin.ExDevice { 

internal class HSMessageContext
{
	internal const int MASTER_SECRET_LEN = 32;

	internal const int IV_LEN = 16;

	internal const int HMAC_LEN = 32;

	internal const int HMAC_CHECK_VALUE_LEN = 32;

	internal const int KEY_LEN_AES256 = 32;

	internal DeviceInfo EcrDevInfo;

	internal global::albahugin.Hugin.DiffieHellman.DiffieHellman ExDH;

	internal global::albahugin.Hugin.DiffieHellman.DiffieHellman EcrDH;

	internal byte[] ExRandom;

	internal byte[] EcrRandom;

	internal byte[] keyEnc;

	internal byte[] keyIV;

	internal byte[] keyHMAC;

	internal X509Certificate2 EcrCertificate;

	internal int ErrorCode;

	internal int PosIndex;

	internal bool CipherDone = false;

	internal int KeyCancelCounter;

	private KeyedHashAlgorithm keyHMACAlg = null;

	private SymmetricAlgorithm symAlg = null;

	private ICryptoTransform encryptor = null;

	private ICryptoTransform decryptor = null;

	internal byte[] TransformData(byte[] buffer, int offset, int len)
	{
		return encryptor.TransformFinalBlock(buffer, offset, len);
	}

	internal byte[] ComputeRecordMAC(byte[] buffer, int len)
	{
		return keyHMACAlg.ComputeHash(buffer, 0, len);
	}

	internal byte[] EncryptRecord(byte[] fragment, int fragmentLen, byte[] mac)
	{
		int num = fragmentLen + mac.Length;
		int num2 = 0;
		num2 = 32 - num % 32;
		num += num2;
		byte[] array = new byte[num];
		if (num2 == 0)
		{
			num2 = 32;
		}
		for (int i = 0; i < num2; i++)
		{
			array[i] = (byte)num2;
		}
		Buffer.BlockCopy(fragment, 0, array, num2, fragmentLen);
		Buffer.BlockCopy(mac, 0, array, fragmentLen + num2, mac.Length);
		Logger.Log((LogLevel)5, array, "**Prepared Message**");
		array = encryptor.TransformFinalBlock(array, 0, array.Length);
		Logger.Log((LogLevel)6, array, "**Encrypted Message**");
		return array;
	}

	internal void DecryptRecord(byte[] fragment, out byte[] dcrFragment, out byte[] dcrMAC)
	{
		int num = 0;
		int num2 = 0;
		Logger.Log((LogLevel)6, fragment, "**Encrypted Area**");
		fragment = decryptor.TransformFinalBlock(fragment, 0, fragment.Length);
		Logger.Log((LogLevel)5, fragment, "**Decrypted Area**");
		num2 = fragment[0];
		num = fragment.Length - num2;
		dcrFragment = new byte[num];
		dcrMAC = new byte[32];
		Buffer.BlockCopy(fragment, num2, dcrFragment, 0, dcrFragment.Length);
	}

	internal void CreateCryptoTransformer()
	{
		symAlg = Rijndael.Create();
		symAlg.Key = keyEnc;
		symAlg.IV = keyIV;
		symAlg.Mode = CipherMode.CBC;
		symAlg.Padding = PaddingMode.None;
		encryptor = symAlg.CreateEncryptor();
		decryptor = symAlg.CreateDecryptor();
		keyHMACAlg = new HMACSHA256(keyHMAC);
		CipherDone = true;
	}

	internal static byte[] PRFForTLS1_2(byte[] secret, string label, byte[] data, int length)
	{
		List<byte> list = new List<byte>();
		list.AddRange(Encoding.ASCII.GetBytes(label));
		list.AddRange(data);
		byte[] seed = list.ToArray();
		list.Clear();
		HMAC hmac = new HMACSHA256(secret);
		byte[] array = Expand(SHA256.Create(), hmac, seed, length);
		byte[] array2 = new byte[length];
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i] = array[i];
		}
		return array2;
	}

	private static byte[] Expand(HashAlgorithm hash, HMAC hmac, byte[] seed, int length)
	{
		int num = hash.HashSize / 8;
		int num2 = length / num;
		if (length % num > 0)
		{
			num2++;
		}
		List<byte> list = new List<byte>();
		byte[][] array = new byte[num2 + 1][];
		array[0] = seed;
		for (int i = 1; i <= num2; i++)
		{
			List<byte> list2 = new List<byte>();
			hmac.Initialize();
			hmac.TransformFinalBlock(array[i - 1], 0, array[i - 1].Length);
			array[i] = hmac.Hash;
			list2.AddRange(array[i]);
			list2.AddRange(seed);
			hmac.Initialize();
			hmac.TransformFinalBlock(list2.ToArray(), 0, list2.Count);
			list.AddRange(hmac.Hash);
			list2.Clear();
		}
		byte[] array2 = new byte[length];
		Buffer.BlockCopy(list.ToArray(), 0, array2, 0, array2.Length);
		list.Clear();
		return array2;
	}
}
}