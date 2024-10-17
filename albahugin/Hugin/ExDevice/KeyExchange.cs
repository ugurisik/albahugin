using System;
using albahugin.Hugin.DiffieHellman;
using albahugin.Hugin.ExDevice;
using Hugin.GMPCommon;

namespace Hugin.ExDevice {

    internal class KeyExchange : HSState
    {
        private const string PRM_GMP3_PRF_LABEL = "GMP-3 istek";

        private const string COMPUTE_KEYS_LABEL = "GMP-3 anahtarlar";

        private const string CER_KAMU_SM_PRODUCER = "KAMU SM";

        public override HSState NextState => new Close();

        public override HSMessageContext Process(byte[] buffer)
        {
            GMPMessage val = GMPMessage.Parse(buffer);
            CheckErrorCode(val);
            GMPField val2 = val.FindTag(14675715);
            context.ExDH.GivenPubKey = new BigInteger(val2.Value);
            context.ExDH.GenerateResponse();
            byte[] array = new byte[val2.Value.Length];
            Buffer.BlockCopy(val2.Value, 0, array, 0, val2.Value.Length);
            val2 = val.FindTag(14675722);
            try
            {
                if (!CertificateManager.Verify(context.EcrCertificate, array, val2.Value))
                {
                    throw new Exception("IMZA DOĞRULAMA\nHATASI");
                }
            }
            catch
            {
            }
            byte[] array2 = new byte[32];
            Buffer.BlockCopy(context.ExRandom, 0, array2, 0, 16);
            Buffer.BlockCopy(context.EcrRandom, 0, array2, 16, 16);
            AppendLog("DH Created Key", context.ExDH.Key);
            byte[] array3 = HSMessageContext.PRFForTLS1_2(context.ExDH.Key, "GMP-3 istek", array2, 32);
            AppendLog("masterKey", array3);
            context.keyHMAC = HSMessageContext.PRFForTLS1_2(array3, "GMP-3 anahtarlar", array2, 32);
            AppendLog("keyHMAC", context.keyHMAC);
            context.keyIV = HSMessageContext.PRFForTLS1_2(context.keyHMAC, "GMP-3 anahtarlar", array2, 32);
            AppendLog("keyIV", context.keyIV);
            Array.Resize(ref context.keyIV, 16);
            context.keyEnc = HSMessageContext.PRFForTLS1_2(context.keyIV, "GMP-3 anahtarlar", array2, 32);
            AppendLog("Encrypt Key", context.keyEnc);
            context.CreateCryptoTransformer();
            return context;
        }

        private void AppendLog(string remark, byte[] buffer)
        {
            Logger.Log((LogLevel)6, buffer, remark);
        }

        public override GMPMessage GetMessage()
        {
            //IL_0006: Unknown result type (might be due to invalid IL or missing references)
            //IL_000c: Expected O, but got Unknown
            //IL_0034: Unknown result type (might be due to invalid IL or missing references)
            //IL_003e: Expected O, but got Unknown
            //IL_0056: Unknown result type (might be due to invalid IL or missing references)
            //IL_0060: Expected O, but got Unknown
            GMPMessage msg = new GMPMessage(16747106);
            AddExDevInfo(ref msg);
            AddEcrDevInfo(ref msg);
            msg.AddItem((GMPItem)new GMPField(14675715, context.ExDH.PubKey));
            msg.AddItem((GMPItem)new GMPField(14675721, MessageBuilder.ConvertIntToBCD(context.PosIndex, 1)));
            return msg;
        }
    }


}

