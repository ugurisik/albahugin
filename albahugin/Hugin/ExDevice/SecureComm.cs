using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using albahugin.Hugin.Common;
using albahugin.Hugin.ExDevice;
using Hugin.GMPCommon;

namespace Hugin.ExDevice { 


public class SecureComm : ISecureComm
{
	private IConnection conn;

	private DeviceInfo devInfo;

	private MatchingState matchState = MatchingState.NoMatchedDevice;

	private HSMessageContext context = null;

	private HSState state;

	private string licenseKey = "";

	private const int DEFAULT_VERSION = 2;

	private static bool isVx675 = false;

	private static CertificateManager okcCertificate = null;

	private static CertificateManager rootCertificate = null;

	private static CertificateManager subRootCertificate = null;

	private static CertificateManager crlCertificate = null;

	private static string CERT_PATH = "Certificates/";

	private static string ROOT_CERT_NAME = "Kurumsal-s1.crt";

	private static string SUB_ROOT_CERT_NAME = "OKC-S1.cer";

	private static string CRL_CERT_NAME = "OKC-S1-SIL.crl";

	private static string OKC_CERT_NAME = "";

	public static int VERSION = 2;

	private static Dictionary<DateTime, int> desktopVerTableByDate = new Dictionary<DateTime, int>
	{
		{
			DateTime.Parse("2016-02-19"),
			1
		},
		{
			DateTime.Parse("2016-03-01"),
			2
		},
		{
			DateTime.Parse("2016-04-05"),
			3
		},
		{
			DateTime.Parse("2016-09-28"),
			4
		}
	};

	private static Dictionary<int, int> vxVerTable = new Dictionary<int, int>
	{
		{ 8800, 2 },
		{ 9045, 3 }
	};

	private static string ecrVersion = "";

	private Command lastCommand = Command.NULL;

	private static ICrc16Computer crcComputer = null;

	public IConnection Connection => conn;

	public string ECRVersion => ecrVersion;

	public bool IsVx675 => isVx675;

	public int ConnTimeout
	{
		get
		{
			if (conn != null)
			{
				return conn.Timeout;
			}
			return 0;
		}
		set
		{
			if (conn != null)
			{
				conn.Timeout = value;
			}
		}
	}

	public int BufferSize
	{
		get
		{
			if (VERSION > 1011)
			{
				return 8192;
			}
			return 2048;
		}
	}

	public int GetVersion()
	{
		return VERSION;
	}

	public SecureComm(DeviceInfo devInfo, string fiscalRegNo, string licanceKey)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Expected O, but got Unknown
		this.devInfo = devInfo;
		ExtDevCommon.FiscalId = fiscalRegNo;
		state = new Start(devInfo);
		crcComputer = (ICrc16Computer)new CrcVF16();
		if (fiscalRegNo.StartsWith("FO"))
		{
			VERSION = 2;
		}
	}

	public void SetCommObject(object connector)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		if (connector is SerialPort)
		{
			conn = new SerialConnection((SerialPort)connector);
		}
		else
		{
			conn = new TCPConnection((Socket)connector);
		}
	}

	public bool Connect()
	{
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		bool result = true;
		bool flag = false;
		GMPMessage val = null;
		HSState hSState = null;
		int num = 0;
		Logger.Enter((object)this, MethodBase.GetCurrentMethod().Name);
		int timeout = conn.Timeout;
		try
		{
			conn.Timeout = 8000;
			while (!flag)
			{
				if (hSState != state)
				{
					state.Init(context);
					val = state.GetMessage();
				}
				Thread.Sleep(50);
				if (val != null)
				{
					try
					{
						SendMessage(val);
					}
					catch (OperationCanceledException ex)
					{
						if (state is Start && num == 0)
						{
							ChangeCrcComputer();
							num++;
							continue;
						}
						throw new Exception(ex.Message + "|" + ex.StackTrace);
					}
					Thread.Sleep(50);
				}
				byte[] array = conn.Read();
				if (array.Length != 0)
				{
					Log(array, "**RECEIVED BUFFER**");
				}
				context = state.Process(array);
				if (context.ErrorCode == 0)
				{
					state = state.NextState;
				}
				if (state == null)
				{
					matchState = MatchingState.Matched;
					flag = true;
					if (context.EcrDevInfo != null)
					{
						Logger.Log((LogLevel)4, "Brand : " + context.EcrDevInfo.Brand);
						Logger.Log((LogLevel)4, "IP : " + context.EcrDevInfo.IP);
						Logger.Log((LogLevel)4, "IP Protocol : " + context.EcrDevInfo.IPProtocol);
						Logger.Log((LogLevel)4, "Model : " + context.EcrDevInfo.Model);
						Logger.Log((LogLevel)4, "Port : " + context.EcrDevInfo.Port);
						Logger.Log((LogLevel)4, "Serial Number : " + context.EcrDevInfo.SerialNum);
						Logger.Log((LogLevel)4, "Terminal No : " + context.EcrDevInfo.TerminalNo);
						Logger.Log((LogLevel)4, "Version : " + context.EcrDevInfo.Version);
					}
				}
			}
		}
		catch (Exception ex2)
		{
			Logger.Log(ex2);
			throw ex2;
		}
		finally
		{
			conn.Timeout = timeout;
		}
		Logger.Exit((object)this, MethodBase.GetCurrentMethod().Name);
		return result;
	}

	private void SendMessage(GMPMessage msg)
	{
		byte[] array = FormatFPUMessage(msg);
		Logger.Enter((object)this, MethodBase.GetCurrentMethod().Name);
		conn.Send(array, 0, array.Length);
		Log(array, "**SENT BUFFER**  " + state);
		Logger.Exit((object)this, MethodBase.GetCurrentMethod().Name);
	}

	private void Log(byte[] msgBuff, string note)
	{
		if (string.IsNullOrEmpty(note))
		{
			Logger.Log((LogLevel)6, msgBuff);
		}
		else
		{
			Logger.Log((LogLevel)6, msgBuff, note);
		}
	}

	private byte[] EncapsulateMessage(int msgType, byte[] reqPacket)
	{
		List<byte> list = new List<byte>();
		list.AddRange(MessageBuilder.DefaultEncoding.GetBytes(devInfo.TerminalNo.PadRight(12, '0')));
		list.AddRange(MessageBuilder.HexToByteArray(msgType));
		list.AddRange(MessageBuilder.AddLength(reqPacket.Length));
		list.AddRange(reqPacket);
		ushort num = ComputeChecksum(list.ToArray(), 0, list.Count);
		list.Add((byte)(num >> 8));
		list.Add((byte)num);
		list.Insert(0, (byte)((list.Count - 2) % 256));
		list.Insert(0, (byte)((list.Count - 2) / 256));
		return list.ToArray();
	}

	public static ushort ComputeChecksum(byte[] body, int offset, int readLength)
	{
		return crcComputer.ComputeChecksum(body, offset, readLength);
	}

	private static void ChangeCrcComputer()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		if (crcComputer is CrcVF16)
		{
			crcComputer = (ICrc16Computer)new CrcIncng16();
		}
		else
		{
			crcComputer = (ICrc16Computer)new CrcVF16();
		}
	}

	private byte[] FormatFPUMessage(GMPMessage msg)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected O, but got Unknown
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Expected O, but got Unknown
		int num = 0;
		msg.InsertItem(num++, (GMPItem)new GMPField(14647816, 3, MessageBuilder.ConvertIntToBCD(ExtDevCommon.SequenceNum, 3)));
		msg.InsertItem(num++, (GMPItem)new GMPField(14647817, 3, MessageBuilder.Date2Bytes(DateTime.Now)));
		msg.InsertItem(num++, (GMPItem)new GMPField(14647818, 3, MessageBuilder.Time2Bytes(DateTime.Now)));
		return EncapsulateMessage(msg.MsgType, msg.ToByte());
	}

	public FPUResponse Send(FPURequest request)
	{
		int num = 0;
		bool flag = false;
		Logger.Enter((object)this, MethodBase.GetCurrentMethod().Name);
		Logger.Log<FPURequest>((LogLevel)5, (object)request);
		if (matchState == MatchingState.Matched)
		{
			List<byte> list = new List<byte>();
			List<byte> list2 = new List<byte>();
			List<byte> list3 = new List<byte>();
			Logger.Log((LogLevel)5, request.Request, "Request");
			if (FPURequest.IsInfoMessage(request.Command))
			{
				list3.AddRange(EncapsulateMessage(16747304, request.Request));
			}
			else
			{
				list2.AddRange(context.EncryptRecord(request.Request, request.Request.Length, list.ToArray()));
				list3.AddRange(EncapsulateMessage(16747297, list2.ToArray()));
			}
			try
			{
				num = conn.Send(list3.ToArray(), 0, list3.ToArray().Length);
				lastCommand = request.Command;
			}
			catch (OperationCanceledException ex)
			{
				if (lastCommand < Command.Z_DAILY || lastCommand > Command.EJ_RCPTCOPY_DATE)
				{
					throw ex;
				}
				flag = true;
			}
		}
		Logger.Exit((object)this, MethodBase.GetCurrentMethod().Name);
		FPUResponse fPUResponse = null;
		fPUResponse = ((!flag) ? new FPUResponse(Read()) : FPUResponse.CreateBusyMessage(request.Sequence, ExtDevCommon.FiscalId));
		Logger.Log<FPUResponse>((LogLevel)5, (object)fPUResponse);
		return fPUResponse;
	}

	public byte[] Read()
	{
		byte[] array = new byte[0];
		Logger.Enter((object)this, MethodBase.GetCurrentMethod().Name);
		if (matchState == MatchingState.Matched)
		{
			int num = 2;
			int num2 = 0;
			array = conn.Read();
			try
			{
				List<byte> list = new List<byte>();
				for (int i = 0; i < 12; i++)
				{
					list.Add(array[num + i]);
				}
				string @string = Encoding.ASCII.GetString(list.ToArray());
				num += 12;
				num2 = MessageBuilder.ByteArrayToHex(array, num, 3);
				num += 3;
				if (num2 != 16748321 && num2 != 16748328)
				{
					throw new InvalidOperationException("Response Message Incorrect");
				}
				int num3 = num;
				int length = MessageBuilder.GetLength(array, num, out num);
				byte[] bytesFromOffset = MessageBuilder.GetBytesFromOffset(array, num, length);
				byte[] dcrFragment = new byte[length];
				byte[] dcrMAC = new byte[0];
				if (num2 == 16748321)
				{
					context.DecryptRecord(bytesFromOffset, out dcrFragment, out dcrMAC);
					byte[] array2 = MessageBuilder.AddLength(dcrFragment.Length);
					Buffer.BlockCopy(array2, 0, array, num3, array2.Length);
					Buffer.BlockCopy(dcrFragment, 0, array, num3 + array2.Length, dcrFragment.Length);
					num = num3 + array2.Length + dcrFragment.Length;
					Array.Resize(ref array, num);
					Log(array, "**Decrypted Data**");
				}
			}
			catch (InvalidOperationException)
			{
				throw new Exception("Message id error");
			}
			catch
			{
				throw new Exception("Invalid data");
			}
		}
		Logger.Exit((object)this, MethodBase.GetCurrentMethod().Name);
		return array;
	}

	private static void LoadCertificates()
	{
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
			Console.WriteLine(CERT_PATH+ROOT_CERT_NAME);
		if (File.Exists(CERT_PATH + ROOT_CERT_NAME))
		{
			try
			{
				rootCertificate = new CertificateManager(CERT_PATH + ROOT_CERT_NAME);
			}
			catch
			{
				throw new RootCertificateNotLoadException();
			}
			if (File.Exists(CERT_PATH + SUB_ROOT_CERT_NAME))
			{
				try
				{
					subRootCertificate = new CertificateManager(CERT_PATH + SUB_ROOT_CERT_NAME);
				}
				catch
				{
					throw new LeafCertificateNotLoadException();
				}
				if (File.Exists(CERT_PATH + OKC_CERT_NAME))
				{
					try
					{
						okcCertificate = new CertificateManager(CERT_PATH + OKC_CERT_NAME);
					}
					catch
					{
						throw new OkcCertificateNotLoadException();
					}
					if (!File.Exists(CERT_PATH + CRL_CERT_NAME))
					{
						throw new Exception("CRL SERTİFİKA\nBULUNAMADI");
					}
					return;
				}
				throw new OkcCertificateNotFoundException();
			}
			throw new LeafCertificateNotFoundException();
		}
		throw new RootCertificateNotFoundException();
	}

	private void CheckCertificatesType()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		if (!CertificateManager.CheckCertificateType(rootCertificate))
		{
			throw new CertificateTypeException();
		}
		if (!CertificateManager.CheckCertificateType(subRootCertificate))
		{
			throw new CertificateTypeException();
		}
		if (!CertificateManager.CheckCertificateType(okcCertificate))
		{
			throw new CertificateTypeException();
		}
	}

	private bool VerifyCertificateChain()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (!CertificateManager.VerifyChain(rootCertificate, rootCertificate))
		{
			throw new RootCertificateVerifyException();
		}
		if (!CertificateManager.VerifyChain(rootCertificate, subRootCertificate))
		{
			throw new Exception("KÖK-ALTKÖK SERTİFİKA\nUYUŞMAZLIĞI");
		}
		if (!CertificateManager.VerifyChain(subRootCertificate, okcCertificate))
		{
			throw new Exception("ALTKÖK-ÖKC SERTİFİKA\nUYUŞMAZLIĞI");
		}
		return true;
	}

	private void CheckRevocation()
	{
		if (!CertificateManager.CheckRevocation(crlCertificate, okcCertificate))
		{
			throw new Exception("ÖKC SERTİFİKA\nİPTAL LİSTESİNDE");
		}
	}

	private bool CheckOKCPubKey(X509Certificate2 ecrCer)
	{
		bool result = false;
		string publicKeyString = ecrCer.GetPublicKeyString();
		string publicKeyString2 = okcCertificate.X509.GetPublicKeyString();
		Log(GetBytes(publicKeyString), "*** ECR PUB KEY ***");
		Log(GetBytes(publicKeyString2), "*** OKC CER FILE PUB KEY ***");
		if (publicKeyString == publicKeyString2)
		{
			result = true;
		}
		return result;
	}

	private byte[] GetBytes(string str)
	{
		byte[] array = new byte[str.Length * 2];
		Buffer.BlockCopy(str.ToCharArray(), 0, array, 0, array.Length);
		return array;
	}

	private bool CheckLicenseKey()
	{
		bool result = false;
		byte[] array = GenerateKey(ExtDevCommon.FiscalId);
		string text = "";
		int num = 0;
		byte[] array2 = array;
		foreach (byte b in array2)
		{
			text += b.ToString("X");
		}
		List<char> list = new List<char>();
		string text2 = text;
		foreach (char item in text2)
		{
			list.Add(item);
			num++;
			if (num == 16)
			{
				break;
			}
		}
		string text3 = new string(list.ToArray());
		Log(MessageBuilder.DefaultEncoding.GetBytes(text3), "*** FPU LICENSE KEY ***");
		Log(MessageBuilder.DefaultEncoding.GetBytes(licenseKey), "*** SERVICE LICENSE KEY ***");
		if (text3 == licenseKey)
		{
			result = true;
		}
		return result;
	}

	private byte[] GenerateKey(string fiscal)
	{
		string text = fiscal + "+HUGIN";
		byte b = 0;
		List<byte> list = new List<byte>();
		string text2 = text;
		for (int i = 0; i < text2.Length; i++)
		{
			byte b2 = (byte)text2[i];
			list.Add((byte)(b2 + b));
			b++;
		}
		byte[] aesEncryptionKey = HashSHA256(list.ToArray());
		return EncryptAES(aesEncryptionKey, list.ToArray());
	}

	internal static byte[] HashSHA256(byte[] data)
	{
		SHA256 sHA = SHA256.Create();
		sHA.Initialize();
		return sHA.ComputeHash(data);
	}

	internal byte[] EncryptAES(byte[] aesEncryptionKey, byte[] data)
	{
		byte[] result = new byte[data.Length];
		try
		{
			AesCryptoServiceProvider aesCryptoServiceProvider = new AesCryptoServiceProvider();
			aesCryptoServiceProvider.Mode = CipherMode.ECB;
			aesCryptoServiceProvider.Key = aesEncryptionKey;
			aesCryptoServiceProvider.Padding = PaddingMode.None;
			ICryptoTransform cryptoTransform = aesCryptoServiceProvider.CreateEncryptor();
			result = cryptoTransform.TransformFinalBlock(data, 0, data.Length);
		}
		catch
		{
		}
		return result;
	}

	internal static void SetVersion(DeviceInfo devInfo)
	{
		try
		{
			if (devInfo.TerminalNo.StartsWith("FO"))
			{
				isVx675 = true;
				string[] array = (ecrVersion = devInfo.Version.Substring(8)).Split(new char[1] { '.' });
				int num = ((array[1].Length != 1) ? int.Parse(array[1]) : (int.Parse(array[1]) * 10));
				int num2 = int.Parse(array[0]) * 100 + num;
				{
					foreach (int key in vxVerTable.Keys)
					{
						if (num2 > key)
						{
							VERSION = vxVerTable[key];
							continue;
						}
						break;
					}
					return;
				}
			}
			isVx675 = false;
			if (devInfo.Version.IndexOf('<') >= 0 && devInfo.Version.IndexOf('>') >= 0)
			{
				int length = devInfo.Version.IndexOf('>') - devInfo.Version.IndexOf('<') - 1;
				string s = devInfo.Version.Substring(devInfo.Version.IndexOf('<') + 1, length).Replace(".", "");
				ecrVersion = devInfo.Version.Substring(devInfo.Version.IndexOf('<') + 1, length);
				if (!int.TryParse(s, out VERSION))
				{
					VERSION = 2;
				}
				return;
			}
			DateTime t = DateTime.Parse(devInfo.Version.Substring(0, 10));
			foreach (DateTime key2 in desktopVerTableByDate.Keys)
			{
				if (DateTime.Compare(t, key2) > 0)
				{
					VERSION = desktopVerTableByDate[key2];
					continue;
				}
				break;
			}
		}
		catch
		{
		}
	}
}
}