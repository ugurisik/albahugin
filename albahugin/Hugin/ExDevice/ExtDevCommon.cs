namespace Hugin.ExDevice { 



public class ExtDevCommon
{
	private static string fiscalId = "";

	private static int sequenceNum = 1;

	private static bool isT300 = false;

	internal static int SequenceNum
	{
		get
		{
			return sequenceNum;
		}
		set
		{
			if (value > 999999)
			{
				value = 1;
			}
			sequenceNum = value;
		}
	}

	internal static string FiscalId
	{
		get
		{
			return fiscalId;
		}
		set
		{
			fiscalId = value;
		}
	}

	internal static bool IsT300
	{
		get
		{
			return isT300;
		}
		set
		{
			isT300 = value;
		}
	}
}
}