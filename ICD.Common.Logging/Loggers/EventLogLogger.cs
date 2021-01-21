#if !SIMPLSHARP
using System;
using System.Diagnostics;
using System.Security;
using ICD.Common.Utils.Services.Logging;

namespace ICD.Common.Logging.Loggers
{
	public sealed class EventLogLogger : ISystemLogger
	{
		private readonly string m_Source;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="source"></param>
		public EventLogLogger(string source)
		{
			m_Source = source;

			try
			{
				if (!EventLog.SourceExists(m_Source))
					EventLog.CreateEventSource(m_Source, "Application");
			}
			catch (SecurityException e)
			{
				// Note - If debugging you can simply add a registry key:
				// Computer\HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\EventLog\Application\[Source]
				// This should be created by the installer on release
				throw new SecurityException("Application must have admin privileges to manage event sources", e);
			}
		}

		/// <summary>
		/// Adds the given log item to the logger.
		/// </summary>
		/// <param name="item"></param>
		public void AddEntry(LogItem item)
		{
			EventLogEntryType type;

			switch (item.Severity)
			{
				case eSeverity.Emergency:
				case eSeverity.Alert:
				case eSeverity.Critical:
				case eSeverity.Error:
					type = EventLogEntryType.Error;
					break;
				case eSeverity.Warning:
					type = EventLogEntryType.Warning;
					break;
				case eSeverity.Notice:
				case eSeverity.Informational:
					type = EventLogEntryType.Information;
					break;
				case eSeverity.Debug:
					return;
				default:
					throw new ArgumentOutOfRangeException();
			}

			EventLog.WriteEntry(m_Source, item.Message, type);
		}
	}
}
#endif
