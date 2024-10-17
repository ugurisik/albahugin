using System;
using System.IO;

namespace Hugin.GMPCommon { 

public class Logger
{
	private static LogLevel logLevel = LogLevel.INFO;

	private static string logFileName = $"SCLogger-{DateTime.Now:dd-MM-yyyy}.txt";

	private static string logFileDirectory = Directory.GetCurrentDirectory();

	private static bool isLoggingStart = true;

	public static LogLevel Level
	{
		get
		{
			return logLevel;
		}
		set
		{
			logLevel = value;
		}
	}

	public static string LogFileDirectory
	{
		get
		{
			return logFileDirectory;
		}
		set
		{
			try
			{
				if (!Directory.Exists(value))
				{
					Directory.CreateDirectory(value);
				}
			}
			catch
			{
				isLoggingStart = false;
			}
			logFileDirectory = value;
		}
	}

	public static void Enter(object strClass, string strFunc)
	{
		if (logLevel >= LogLevel.DEBUG)
		{
			AddLog($"Entered {strClass} \t {strFunc}");
		}
	}

	public static void Exit(object strClass, string strFunc)
	{
		if (logLevel >= LogLevel.DEBUG)
		{
			AddLog($"Exit {strClass} \t {strFunc}");
		}
	}

	public static void DebugLine(object strClass, string strFunc, int line)
	{
		if (logLevel >= LogLevel.DEBUG)
		{
			AddLog($"Exit {strClass} \t {strFunc}:{line:D5}");
		}
	}

	private static void AddLog(string log)
	{
		try
		{
			if (isLoggingStart)
			{
				log = $"{DateTime.Now:dd/MM/yyyy HH.mm.ss.fff} : {log}{Environment.NewLine}";
				StreamWriter streamWriter = new StreamWriter(logFileDirectory + "\\" + logFileName, append: true);
				streamWriter.Write(log);
				streamWriter.Close();
				return;
			}
		}
		catch
		{
		}
	}

	public static void Log(LogLevel level, string log)
	{
		if (logLevel >= level)
		{
			AddLog(log);
		}
	}

	public static void Log(LogLevel level, byte[] buffer)
	{
		if (logLevel >= level)
		{
			AddLog(BitConverter.ToString(buffer, 0));
		}
	}

	public static void Log(LogLevel level, byte[] buffer, string note)
	{
		if (logLevel >= level)
		{
			AddLog(note);
			AddLog(BitConverter.ToString(buffer, 0));
		}
	}

	public static void Log(Exception ex)
	{
		if (logLevel >= LogLevel.ERROR)
		{
			AddLog(ex.Message);
			AddLog(ex.StackTrace);
		}
	}

	public static void Log<T>(LogLevel level, object o)
	{
		try
		{
			if (logLevel < level)
			{
			}
		}
		catch
		{
		}
	}
}
}