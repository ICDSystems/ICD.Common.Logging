using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Logging.Console.Loggers;
using ICD.Common.Properties;
using ICD.Common.Services.Logging;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.Extensions;

namespace ICD.Common.Logging.Console
{
	/// <summary>
	/// Core of the ELogging functionality.
	/// Mainly acts as a rendevous point for log entries
	/// </summary>
	[PublicAPI]
	public sealed class LoggingCore : ILoggerService
	{
		private const int HISTORY_SIZE = 100;

		[PublicAPI]
		public event EventHandler<LogItemEventArgs> OnEntryAdded;

		[PublicAPI]
		public event EventHandler<SeverityEventArgs> OnSeverityLevelChanged;

		/// <summary>
		/// HashSet of ISystemLoggers to allow for multiple logging destinations.
		/// </summary>
		private readonly IcdHashSet<ISystemLogger> m_LoggingDestinations;

		private readonly SafeCriticalSection m_LoggingSection;

		/// <summary>
		/// Keeps track of the most recent logs.
		/// </summary>
		private readonly ScrollQueue<KeyValuePair<int, LogItem>> m_History;

		private readonly SafeCriticalSection m_HistorySection;

		private int m_LogIndex;
		private eSeverity m_SeverityLevel;

		#region Properties

		/// <summary>
		/// Gets and sets the severity level.
		/// </summary>
		[PublicAPI]
		public eSeverity SeverityLevel
		{
			get { return m_SeverityLevel; }
			set
			{
				if (value == m_SeverityLevel)
					return;

				m_SeverityLevel = value;

				OnSeverityLevelChanged.Raise(null, new SeverityEventArgs(m_SeverityLevel));
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor.
		/// </summary>
		public LoggingCore()
		{
			m_LoggingDestinations = new IcdHashSet<ISystemLogger>();
			m_History = new ScrollQueue<KeyValuePair<int, LogItem>>(HISTORY_SIZE);

			m_LoggingSection = new SafeCriticalSection();
			m_HistorySection = new SafeCriticalSection();
		}

		#endregion

		#region Methods

		/// <summary>
		/// Removes all loggers.
		/// </summary>
		[PublicAPI]
		public void Clear()
		{
			m_LoggingSection.Execute(() => m_LoggingDestinations.Clear());
		}

		/// <summary>
		/// Clears the log history.
		/// </summary>
		[PublicAPI]
		public void ClearHistory()
		{
			m_HistorySection.Execute(() => m_History.Clear());
		}

		/// <summary>
		/// Adds the log item to each logger.
		/// </summary>
		/// <param name="item">Log entry to add</param>
		[PublicAPI]
		public void AddEntry(LogItem item)
		{
			if (item.Severity > SeverityLevel)
				return;

			m_LoggingSection.Enter();

			try
			{
				if (m_LoggingDestinations.Count == 0)
					IcdErrorLog.Notice("ELogging - Entry received with no handlers subscribed - dropping");

				foreach (ISystemLogger logger in m_LoggingDestinations)
				{
					try
					{
						logger.AddEntry(item);
					}
					catch (Exception e)
					{
						IcdErrorLog.Exception(e, "Exception adding log to {0}", logger.GetType().Name);
					}
				}
			}
			finally
			{
				m_LoggingSection.Leave();
			}

			AddHistory(item);
		}

		/// <summary>
		/// Adds the logger. Returns false if it's already in the core.
		/// </summary>
		/// <param name="logger"></param>
		/// <returns></returns>
		[PublicAPI]
		public bool AddLogger(ISystemLogger logger)
		{
			m_LoggingSection.Enter();

			try
			{
				return m_LoggingDestinations.Add(logger);
			}
			finally
			{
				m_LoggingSection.Leave();
			}
		}

		/// <summary>
		/// Removes the logger. Returns false if the logger was not in the core.
		/// </summary>
		/// <param name="logger"></param>
		/// <returns></returns>
		[PublicAPI]
		public bool RemoveLogger(ISystemLogger logger)
		{
			return m_LoggingSection.Execute(() => m_LoggingDestinations.Remove(logger));
		}

		/// <summary>
		/// Returns true if the core contains a logger of the given type.
		/// </summary>
		/// <typeparam name="TLogger"></typeparam>
		/// <returns></returns>
		[PublicAPI]
		public bool ContainsLoggerOfType<TLogger>()
			where TLogger : ISystemLogger
		{
			return m_LoggingSection.Execute(() => m_LoggingDestinations.Any(l => l.GetType() == typeof(TLogger)));
		}

		/// <summary>
		/// Gets the logger of the given type.
		/// </summary>
		/// <typeparam name="TLogger"></typeparam>
		/// <returns></returns>
		[PublicAPI]
		public TLogger GetLoggerByType<TLogger>()
			where TLogger : ISystemLogger
		{
			return (TLogger)m_LoggingSection.Execute(() => m_LoggingDestinations.First(l => l.GetType() == typeof(TLogger)));
		}

		/// <summary>
		/// Gets the log history.
		/// </summary>
		/// <returns></returns>
		[PublicAPI]
		public KeyValuePair<int, LogItem>[] GetHistory()
		{
			return m_HistorySection.Execute(() => m_History.ToArray());
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Adds the item to the recent history.
		/// </summary>
		/// <param name="item"></param>
		private void AddHistory(LogItem item)
		{
			m_HistorySection.Enter();

			try
			{
				KeyValuePair<int, LogItem> entry = new KeyValuePair<int, LogItem>(m_LogIndex, item);
				m_History.Enqueue(entry);

				unchecked
				{
					m_LogIndex++;
				}
			}
			finally
			{
				m_HistorySection.Leave();
			}

			OnEntryAdded.Raise(null, new LogItemEventArgs(item));
		}

		#endregion
	}
}
