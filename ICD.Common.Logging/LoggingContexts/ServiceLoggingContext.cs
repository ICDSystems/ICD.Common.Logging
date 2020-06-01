using System;
using ICD.Common.Utils.Services;
using ICD.Common.Utils.Services.Logging;

namespace ICD.Common.Logging.LoggingContexts
{
	public sealed class ServiceLoggingContext : AbstractLoggingContext
	{
		private readonly object m_Target;

		private WeakReference m_CachedLoggerService;

		#region Properties

		/// <summary>
		/// Gets the current logger service.
		/// </summary>
		private ILoggerService LoggerService
		{
			get
			{
				if (m_CachedLoggerService == null || !m_CachedLoggerService.IsAlive)
					m_CachedLoggerService = new WeakReference(ServiceProvider.GetService<ILoggerService>());
				return (ILoggerService)m_CachedLoggerService.Target;
			}
		}

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="target">The instance that is the focus of the logger.</param>
		public ServiceLoggingContext(object target)
		{
			m_Target = target;
		}

		#region Methods

		/// <summary>
		/// Adds the log item.
		/// </summary>
		/// <param name="severity"></param>
		/// <param name="message"></param>
		public override void Log(eSeverity severity, string message)
		{
			// Prepend the log message with the target
			message = string.Format("{0} - {1}", GetStringForTarget(m_Target), message);
			LogItem item = new LogItem(severity, message);

			// Log with the logger service
			LoggerService.AddEntry(item);
		}

		/// <summary>
		/// Adds the log item with string formatting.
		/// </summary>
		/// <param name="severity">Severity Code</param>
		/// <param name="message">Message Text format string</param>
		/// <param name="args">objects to format into the string</param>
		public override void Log(eSeverity severity, string message, params object[] args)
		{
			Log(severity, string.Format(message, args));
		}

		#endregion

		#region Private Methods

		private static string GetStringForTarget(object target)
		{
			// Is the target a static type?
			Type targetAsType = target as Type;
			if (targetAsType != null)
				return targetAsType.Name;

			// Otherwise get the string representation
			return string.Format("{0}", target);
		}

		#endregion
	}
}
