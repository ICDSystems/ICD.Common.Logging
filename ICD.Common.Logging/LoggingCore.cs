using System;
using System.Collections.Generic;
using ICD.Common.Logging.Loggers;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services.Logging;

namespace ICD.Common.Logging
{
	[PublicAPI]
	public sealed class LoggingCore : ILoggerService
	{
		private const int HISTORY_SIZE = 100;

		/// <summary>
		/// Raised when an item is logged against the logger service.
		/// </summary>
		[PublicAPI]
		public event EventHandler<LogItemEventArgs> OnEntryAdded;

		/// <summary>
		/// Raised when the severity level changes.
		/// </summary>
		[PublicAPI]
		public event EventHandler<SeverityEventArgs> OnSeverityLevelChanged;

		/// <summary>
		/// HashSet of ISystemLoggers to allow for multiple logging destinations.
		/// </summary>
		private readonly IcdHashSet<ISystemLogger> m_Loggers;
		private readonly SafeCriticalSection m_LoggersSection;

		/// <summary>
		/// Keeps track of the most recent logs.
		/// </summary>
		private readonly ScrollQueue<KeyValuePair<int, LogItem>> m_History;
		private readonly SafeCriticalSection m_HistorySection;

		/// <summary>
		/// Log items that need to be processed.
		/// </summary>
		private readonly Queue<LogItem> m_Queue;
		private readonly SafeCriticalSection m_QueueSection;
		private readonly SafeCriticalSection m_ProcessSection;

		private int m_LogIndex;
		private eSeverity m_SeverityLevel;

		/// <summary>
		/// Gets and sets the minimum severity threshold for log items to be logged.
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

		/// <summary>
		/// Constructor.
		/// </summary>
		public LoggingCore()
		{
			m_Loggers = new IcdHashSet<ISystemLogger>();
			m_History = new ScrollQueue<KeyValuePair<int, LogItem>>(HISTORY_SIZE);
			m_Queue = new Queue<LogItem>();

			m_LoggersSection = new SafeCriticalSection();
			m_HistorySection = new SafeCriticalSection();
			m_QueueSection = new SafeCriticalSection();
			m_ProcessSection = new SafeCriticalSection();
		}

		#region Methods

		/// <summary>
		/// Adds the log item to each logger.
		/// </summary>
		/// <param name="item">Log entry to add</param>
		[PublicAPI]
		public void AddEntry(LogItem item)
		{
			if (item.Severity > SeverityLevel)
				return;

			m_QueueSection.Execute(() => m_Queue.Enqueue(item));
			ThreadingUtils.SafeInvoke(() => ProcessQueue(true));
		}

		/// <summary>
		/// Adds the logger. Returns false if it's already in the core.
		/// </summary>
		/// <param name="logger"></param>
		/// <returns></returns>
		[PublicAPI]
		public bool AddLogger([NotNull] ISystemLogger logger)
		{
			if (logger == null)
				throw new ArgumentNullException("logger");

			return m_LoggersSection.Execute(() => m_Loggers.Add(logger));
		}

		/// <summary>
		/// Removes the logger. Returns false if the logger was not in the core.
		/// </summary>
		/// <param name="logger"></param>
		/// <returns></returns>
		[PublicAPI]
		public bool RemoveLogger([NotNull] ISystemLogger logger)
		{
			if (logger == null)
				throw new ArgumentNullException("logger");

			return m_LoggersSection.Execute(() => m_Loggers.Remove(logger));
		}

		/// <summary>
		/// Gets the log history.
		/// </summary>
		/// <returns></returns>
		[PublicAPI]
		[NotNull]
		public IEnumerable<KeyValuePair<int, LogItem>> GetHistory()
		{
			return m_HistorySection.Execute(() => m_History.ToArray(m_History.Count));
		}

		/// <summary>
		/// Writes all enqueued logs.
		/// </summary>
		public void Flush()
		{
			ProcessQueue(false);
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Works through the queue of log items and sends them to the registered loggers.
		/// </summary>
		private void ProcessQueue(bool workerThread)
		{
			if (workerThread)
				if (!m_ProcessSection.TryEnter())
					return;
			else
				m_ProcessSection.Enter();

			try
			{
				LogItem item = default(LogItem);
				while (m_QueueSection.Execute(() => m_Queue.Dequeue(out item)))
					ProcessItem(item);
			}
			finally
			{
				m_ProcessSection.Leave();
			}
		}

		/// <summary>
		/// Sends the log item to the registered loggers.
		/// </summary>
		/// <param name="item"></param>
		private void ProcessItem(LogItem item)
		{
			ISystemLogger[] loggers = m_LoggersSection.Execute(() => m_Loggers.ToArray(m_Loggers.Count));
			if (loggers.Length == 0)
				IcdErrorLog.Warn("{0} - Attempted to add entry with no loggers registered", GetType().Name);

			foreach (ISystemLogger logger in loggers)
			{
				try
				{
					logger.AddEntry(item);
				}
				catch (Exception e)
				{
					IcdErrorLog.Exception(e, "{0} - Exception adding log to {1}", GetType().Name, logger.GetType().Name);
				}
			}

			AddHistory(item);

			OnEntryAdded.Raise(null, new LogItemEventArgs(item));
		}

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

				KeyValuePair<int, LogItem> removed;
				m_History.Enqueue(entry, out removed);

				unchecked
				{
					m_LogIndex++;
				}
			}
			finally
			{
				m_HistorySection.Leave();
			}
		}

		#endregion
	}
}
