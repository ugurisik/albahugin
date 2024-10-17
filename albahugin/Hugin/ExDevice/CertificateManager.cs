using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace albahugin.Hugin.ExDevice { 

public class CertificateManager
{
	private X509Certificate2 x509Certif = null;

	public X509Certificate2 X509 => x509Certif;

	public CertificateManager(string pfxPath)
	{
		x509Certif = new X509Certificate2(pfxPath, "", X509KeyStorageFlags.Exportable);
	}

	public CertificateManager(X509Certificate2 x509cer)
	{
		x509Certif = x509cer;
	}

	public static bool VerifyChain(CertificateManager rootCertificate, CertificateManager leafCertificate)
	{
		bool flag = false;
		X509Chain x509Chain = new X509Chain();
		x509Chain.ChainPolicy.ExtraStore.Add(rootCertificate.X509);
		x509Chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
		x509Chain.ChainPolicy.RevocationFlag = X509RevocationFlag.ExcludeRoot;
		x509Chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;
		x509Chain.ChainPolicy.VerificationTime = DateTime.Now;
		x509Chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 0, 0);
		flag = x509Chain.Build(leafCertificate.X509);
		X509Store x509Store = new X509Store("Certificates/OKC-S1-SIL.CRL");
		if (flag)
		{
			flag = !x509Store.Certificates.Contains(leafCertificate.X509);
		}
		return flag;
	}

	internal static bool Verify(X509Certificate2 x509Cert, byte[] signedBytes, byte[] sign)
	{
		string xmlString = x509Cert.PublicKey.Key.ToXmlString(includePrivateParameters: false);
		RSACryptoServiceProvider rSACryptoServiceProvider = new RSACryptoServiceProvider();
		rSACryptoServiceProvider.FromXmlString(xmlString);
		SHA256 sHA = SHA256.Create();
		sHA.Initialize();
		byte[] rgbHash = sHA.ComputeHash(signedBytes);
		return rSACryptoServiceProvider.VerifyHash(rgbHash, CryptoConfig.MapNameToOID("SHA256"), sign);
	}

	public static bool CheckCertificateType(CertificateManager certificate)
	{
		if (X509Certificate2.GetCertContentType(certificate.X509.RawData) == X509ContentType.Cert)
		{
			return true;
		}
		return false;
	}

	internal static bool CheckRevocation(CertificateManager crlCertificate, CertificateManager okcCertificate)
	{
		string serialNumberString = crlCertificate.x509Certif.GetSerialNumberString();
		return true;
	}
}
}