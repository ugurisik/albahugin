using System;
using System.Text;

namespace albahugin.Hugin.DiffieHellman { 



public class DiffieHellman : IDisposable
{
	private static StrongNumberProvider _strongRng = new StrongNumberProvider();

	private int bits = 256;

	private BigInteger prime;

	private BigInteger g;

	private BigInteger mine;

	private BigInteger givenPubKey;

	private byte[] pubKey;

	private byte[] key;

	private string representation;

	internal BigInteger Prime
	{
		get
		{
			return prime;
		}
		set
		{
			prime = value;
		}
	}

	internal BigInteger G
	{
		get
		{
			return g;
		}
		set
		{
			g = value;
		}
	}

	internal BigInteger GivenPubKey
	{
		get
		{
			return givenPubKey;
		}
		set
		{
			givenPubKey = value;
		}
	}

	internal byte[] PubKey
	{
		get
		{
			return pubKey;
		}
		set
		{
			pubKey = value;
		}
	}

	public byte[] Key
	{
		get
		{
			return key;
		}
		set
		{
			key = value;
		}
	}

	public DiffieHellman()
	{
	}

	public DiffieHellman(int bits)
	{
		this.bits = bits;
	}

	~DiffieHellman()
	{
		Dispose();
	}

	public DiffieHellman GenerateRequest()
	{
		prime = BigInteger.GenPseudoPrime(bits, 30, _strongRng);
		mine = BigInteger.GenPseudoPrime(bits, 30, _strongRng);
		g = 5;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(prime.ToString(36));
		stringBuilder.Append("|");
		stringBuilder.Append(g.ToString(36));
		stringBuilder.Append("|");
		using (BigInteger bigInteger = g.ModPow(mine, prime))
		{
			stringBuilder.Append(bigInteger.ToString(36));
		}
		representation = stringBuilder.ToString();
		return this;
	}

	public DiffieHellman GenerateResponse(string request)
	{
		string[] array = request.Split(new char[1] { '|' });
		using (BigInteger n = new BigInteger(array[0], 36))
		{
			BigInteger bigInteger3 = new BigInteger(array[1], 36);
			BigInteger exp = BigInteger.GenPseudoPrime(bits, 30, _strongRng);
			using (BigInteger bigInteger = new BigInteger(array[2], 36))
			{
				BigInteger bigInteger2 = bigInteger.ModPow(exp, n);
				key = bigInteger2.GetBytes();
			}
			BigInteger bigInteger4 = bigInteger3.ModPow(exp, n);
			representation = bigInteger4.ToString(36);
			pubKey = bigInteger4.GetBytes();
		}
		return this;
	}

	public DiffieHellman GeneratePubKey()
	{
		mine = BigInteger.GenPseudoPrime(bits, 30, _strongRng);
		using (BigInteger bigInteger = g.ModPow(mine, prime))
		{
			representation = bigInteger.ToString(36);
			pubKey = bigInteger.GetBytes();
		}
		return this;
	}

	public DiffieHellman GenerateResponse()
	{
		using (BigInteger bigInteger = givenPubKey.ModPow(mine, prime))
		{
			key = bigInteger.GetBytes();
		}
		return this;
	}

	public void HandleResponse(string response)
	{
		using (BigInteger bigInteger = new BigInteger(response, 36))
		{
			BigInteger bigInteger2 = bigInteger.ModPow(mine, prime);
			key = bigInteger2.GetBytes();
		}
		Dispose();
	}

	public override string ToString()
	{
		return representation;
	}

	public void Dispose()
	{
		if ((object)prime != null)
		{
			prime.Dispose();
		}
		if ((object)mine != null)
		{
			mine.Dispose();
		}
		if ((object)g != null)
		{
			g.Dispose();
		}
		prime = null;
		mine = null;
		g = null;
		representation = null;
		GC.Collect();
		GC.Collect();
	}
}
}