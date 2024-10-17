using System;
using System.Reflection;
using System.Threading;
using Hugin.GMPCommon;

namespace Hugin.POS.CompactPrinter.FP300 { 

internal static class BackgroundWorker
{
	public static bool isWorkingMainPrinter = false;

	public static DateTime dateTimeLastOperation = DateTime.Now;

	private const double waitingMinutes = 10.0;

	private static bool isThreadAvailableForRestart = true;

	public static bool IsThreadAvailableForRestart => isThreadAvailableForRestart;

	public static void Start(object value)
	{
		try
		{
			if (value.GetType() == typeof(CompactPrinter))
			{
				CompactPrinter compactPrinter = (CompactPrinter)value;
				isThreadAvailableForRestart = false;
				while (true)
				{
					Thread.Sleep(1000);
					if (compactPrinter != null && !isWorkingMainPrinter)
					{
						DateTime now = DateTime.Now;
						if ((now - dateTimeLastOperation).TotalMinutes >= 10.0)
						{
							compactPrinter.CheckPrinterStatus();
						}
					}
				}
			}
			Logger.Log((LogLevel)4, $"METHOD : {MethodBase.GetCurrentMethod().Name} ");
			Logger.Log((LogLevel)4, "Invalid Object Type");
		}
		catch (Exception ex)
		{
			Logger.Log((LogLevel)2, $"METHOD : {MethodBase.GetCurrentMethod().Name} ");
			Logger.Log(ex);
		}
		finally
		{
			isThreadAvailableForRestart = true;
		}
	}
}
}