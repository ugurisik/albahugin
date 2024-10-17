namespace Hugin.GMPCommon { 

public class Utililty
{
	private static string[] stateMessages = new string[15]
	{
		"BEKLEMEDE", "SATIŞ İÇİNDE", "ARATOPLAM", "ÖDEME BAŞLADI", "FİŞ KAPAT", "BİLGİ FİŞİ", "ÖZEL FİŞ", "SERVİS BAŞLADI", "SERVİS GEREKLİ", "KASİYER GİRİŞİ",
		"MALİ MODDA DEĞİL", "ELEKTRİK KESİNTİSİ", "FATURA BAŞLADI", "ONAY BEKLEMEKTE", "KASA KULLANIMDA"
	};

	private static string[,] errorMessages = new string[101, 2]
	{
		{ "0", "İŞLEM BAŞARILI" },
		{ "1", "VERİ EKSİK GELMİŞ (UZUNLUK KADAR GELMELİ)" },
		{ "2", "VERİ DEĞİŞMİŞ" },
		{ "3", "UYGULAMA DURUMU UYGUN DEĞİL" },
		{ "4", "BÖYLE BİR KOMUT DESTEKLENMİYOR" },
		{ "5", "PARAMETRE GEÇERSİZ" },
		{ "6", "OPERASYON BAŞARISIZ" },
		{ "7", "SİLME GEREKLİ (HATA SONRASI)" },
		{ "8", "KAĞIT YOK" },
		{ "9", "CİHAZ EŞLEME YAPILAMADI." },
		{ "11", "MALİ BELLEK BİLGİLERİ ALINIRKEN HATA OLUŞTU" },
		{ "12", "MALİ BELLEK TAKILI DEĞİL" },
		{ "13", "MALİ BELLEK UYUMSUZLUĞU" },
		{ "14", "MALİ BELLEK FORMATLANMALI" },
		{ "15", "MALİ BELLEK FORMATLANIRKEN HATA OLUŞTU" },
		{ "16", "MALİ BELLEK MALİLEŞTİRME YAPILAMADI" },
		{ "17", "GÜNLÜK Z LİMİT" },
		{ "18", "MALİ BELLEK DOLDU" },
		{ "19", "MALİ BELLEK DAHA ÖNCE FORMATLANMIŞ" },
		{ "20", "MALİ BELLEK KAPATILMIŞ" },
		{ "21", "GEÇERSİZ MALİ BELLEK" },
		{ "22", "SERTİFİKALAR YÜKLENEMEDİ" },
		{ "31", "EKÜ BİLGİLERİ ALINIRKEN HATA OLUŞTU" },
		{ "32", "EKÜ ÇIKARILDI" },
		{ "33", "EKÜ KASAYA AİT DEĞİL" },
		{ "34", "ESKİ EKÜ (SADECE EKÜ RAPORLARI)" },
		{ "35", "YENİ EKÜ TAKILDI, ONAY BEKLİYOR" },
		{ "36", "EKÜ DEĞİŞTİRİLEMEZ, Z GEREKLİ" },
		{ "37", "YENİ EKÜYE GEÇİLEMİYOR" },
		{ "38", "EKÜ DOLDU, Z GEREKLİ" },
		{ "39", "EKÜ DAHA ÖNCE FORMATLANMIŞ" },
		{ "51", "FİŞ LİMİTİ AŞILDI" },
		{ "52", "FİŞ KALEM ADEDİ AŞILDI" },
		{ "53", "SATIŞ İŞLEMİ GEÇERSİZ" },
		{ "54", "İPTAL İŞLEMİ GEÇERSİZ" },
		{ "55", "DÜZELTME İŞLEMİ YAPILAMAZ" },
		{ "56", "İNDİRİM/ARTIRIM İŞLEMİ YAPILAMAZ" },
		{ "57", "ÖDEME İŞLEMİ GEÇERSİZ" },
		{ "58", "ASGARİ ÖDEME SAYISI AŞILDI" },
		{ "59", "GÜNLÜK ÜRÜN SATIŞI AŞILDI" },
		{ "60", "DEPARTMAN LİMİTİ AŞILDI" },
		{ "71", "KDV ORANI TANIMSIZ" },
		{ "72", "KISIM TANIMLANMAMIŞ" },
		{ "73", "TANIMSIZ ÜRÜN" },
		{ "74", "KREDİLİ ÖDEME BİLGİSİ EKSİK/GEÇERSİZ" },
		{ "75", "DÖVİZLİ ÖDEME BİLGİSİ EKSİK/GEÇERSİZ" },
		{ "76", "EKÜDE KAYIT BULUNAMADI" },
		{ "77", "MALİ BELLEKTE KAYIT BULUNAMADI" },
		{ "78", "ALT ÜRÜN GRUBU TANIMLI DEĞİL" },
		{ "79", "DOSYA BULUNAMADI" },
		{ "91", "KASİYER YETKİSİ YETERSİZ" },
		{ "92", "SATIŞ VAR " },
		{ "93", "SON FİŞ Z DEĞİL " },
		{ "94", "KASADA YETERLİ PARA YOK" },
		{ "95", "GÜNLÜK FİŞ SAYISI LİMİT AŞILDI" },
		{ "96", "GÜNLÜK TOPLAM AŞILDI" },
		{ "97", "KASA MALİ DEĞİL" },
		{ "111", "SATIR UZUNLUĞU BEKLENENDEN FAZLA" },
		{ "112", "KDV ORANI GEÇERSİZ" },
		{ "113", "DEPT NUMARASI GEÇERSİZ" },
		{ "114", "PLU NUMARASI GEÇERSİZ" },
		{ "115", "GEÇERSİZ TANIM (ÜRÜN ADI, KISIM ADI, KREDİ ADI...VS)" },
		{ "116", "BARKOD GEÇERSİZ" },
		{ "117", "GEÇERSİZ OPSİYON" },
		{ "118", "TOPLAM TUTMUYOR" },
		{ "119", "GEÇERSİZ MİKTAR" },
		{ "120", "GEÇERSİZ TUTAR" },
		{ "121", "MALİ NUMARA HATALI" },
		{ "122", "KASA MEŞGUL" },
		{ "131", "KAPAKLAR AÇILDI" },
		{ "132", "MALİ BELLEK MESH ZARAR VERİLDİ" },
		{ "133", "HUB MESH ZARAR VERİLDİ" },
		{ "134", "Z ALINMALI(24 SAAT GEÇTİ)" },
		{ "135", "DOĞRU EKÜ TAK, YENİDEN BAŞLAT" },
		{ "136", "SERTİFİKA YÜKLENEMEDİ" },
		{ "137", "TARİH-SAAT AYARLAYIN" },
		{ "138", "GÜNLÜK İLE MALİ BELLEK UYUMSUZ" },
		{ "139", "VERİTABANI HATASI" },
		{ "140", "LOG HATASI" },
		{ "141", "SRAM HATASI" },
		{ "142", "SERTİFİKA UYUMSUZ" },
		{ "143", "VERSİYON HATASI" },
		{ "144", "GÜNLÜK LOG SAYISI AŞILDI" },
		{ "145", "YAZARKASAYI YENİDEN BAŞLAT" },
		{ "146", "KASİYER/SERVİS GÜNLÜK YANLIŞ ŞİFRE GİRİŞİ SAYISINI AŞTI" },
		{ "147", "MALİLEŞTİRME YAPILDI. YENİDEN BAŞLAT" },
		{ "148", "GİB'e BAĞLANILAMADI. TEKRAR DENE(İŞLEM DURDURMA)" },
		{ "149", "SERTİFİKA İNDİRİLDİ. YENİDEN BAŞLAT" },
		{ "150", "GÜVENLİ ALAN FORMATLANAMADI" },
		{ "151", "JUMPER ÇIKART TAK" },
		{ "170", "BAĞLI EFT YOK" },
		{ "171", "EFT-POS DURUMU UYGUN DEĞİL" },
		{ "172", "HATALI KART" },
		{ "173", "TUTAR UYUŞMUYOR" },
		{ "174", "PROVİZYON YOK" },
		{ "175", "DESTEKLENMEYEN TAKSİT SAYISI" },
		{ "176", "EFT İPTAL BAŞARISIZ" },
		{ "177", "EFT İADE BAŞARISIZ" },
		{ "178", "EFT EK NÜSHA İŞLEMİ BAŞARISIZ" },
		{ "179", "MEVCUT MODDA BU İŞLEM GERÇEKLEŞTİRİLEMEZ" },
		{ "180", "GEÇERSİZ EFT MODU" }
	};

	public static string GetErrorMessage(int errorCode)
	{
		if (errorCode == 42)
		{
			errorCode = 0;
		}
		string text = string.Concat(errorCode);
		for (int i = 0; i < errorMessages.LongLength; i++)
		{
			if (errorMessages[i, 0] == text)
			{
				text = errorMessages[i, 1];
				break;
			}
		}
		return text;
	}

	public static string GetStateMessage(State state)
	{
		return stateMessages[(int)(state - 1)];
	}
}
}