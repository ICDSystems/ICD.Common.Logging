﻿using System.Collections.Generic;
using ICD.Common.Utils;
using ICD.Common.Utils.IO;
using ICD.Common.Utils.Services.Logging;

namespace ICD.Common.Logging.Loggers
{
	public sealed class FileLogger : ISystemLogger
	{
		private static readonly Dictionary<eSeverity, string> s_SeverityLabels =
			new Dictionary<eSeverity, string>
			{
				{eSeverity.Emergency, "Emrgcy"},
				{eSeverity.Alert, "Alert "},
				{eSeverity.Critical, "Crit  "},
				{eSeverity.Error, "Error "},
				{eSeverity.Warning, "Warn  "},
				{eSeverity.Notice, "Notice"},
				{eSeverity.Informational, "Info  "},
				{eSeverity.Debug, "Debug "}
			}; 

		private readonly SafeCriticalSection m_LogSection;
		private readonly string m_Path;

		/// <summary>
		/// Constructor.
		/// </summary>
		public FileLogger()
			: this(PathUtils.ProgramLogsPath)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="directoryPath"></param>
		public FileLogger(string directoryPath)
		{
			m_LogSection = new SafeCriticalSection();
			m_Path = BuildPathToLogFile(directoryPath);

			IcdDirectory.CreateDirectory(directoryPath);
		}

		/// <summary>
		/// Adds the given log item to the logger.
		/// </summary>
		/// <param name="item"></param>
		public void AddEntry(LogItem item)
		{
			m_LogSection.Enter();

			try
			{
				string message = Format(item);
				using (IcdStreamWriter streamWriter = IcdFile.AppendText(m_Path))
					streamWriter.WriteLine(message);
			}
			finally
			{
				m_LogSection.Leave();
			}
		}

		private static string Format(LogItem item)
		{
			//Info   | 2019-04-08 16:03:57 | (ConnectionStateManager)CiscoCodecDevice(Id=202002, Name="1D-103 Cisco SX80") - Attempting to reconnect (Attempt 1).
			return string.Format("{0} | {1:yyyy-MM-dd HH:mm:ss} | {2}",
			                     s_SeverityLabels[item.Severity],
			                     item.Timestamp.ToLocalTime(),
			                     item.Message);
		}

		private static string BuildPathToLogFile(string directoryPath)
		{
			string date = IcdEnvironment.GetLocalTime()
			                            .ToUniversalTime()
			                            .ToString("s")
			                            .Replace(':', '-') + "Z.log";

			return PathUtils.Join(directoryPath, date);
		}
	}
}