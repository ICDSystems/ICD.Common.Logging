using System;
using System.Collections.Generic;
using ICD.Common.Utils;
using ICD.Common.Utils.Services;
using ICD.Common.Utils.Services.Logging;

namespace ICD.Common.Logging.LoggingContexts
{
	public sealed class ServiceLoggingContext : AbstractLoggingContext
	{
		private readonly object m_Target;
		private readonly Dictionary<string, object> m_ErrorStates;
		private readonly SafeCriticalSection m_ErrorsStatesSection;

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
			m_ErrorStates = new Dictionary<string, object>();
			m_ErrorsStatesSection = new SafeCriticalSection();
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

		/// <summary>
		/// Sets the current error state for the given key.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="severity"></param>
		/// <param name="value"></param>
		public override void Set<T>(string key, eSeverity severity, T value)
		{
			m_ErrorsStatesSection.Enter();

			try
			{
				object current;
				if (m_ErrorStates.TryGetValue(key, out current) &&
				    EqualityComparer<T>.Default.Equals((T)current, value))
					return;

				m_ErrorStates[key] = value;
			}
			finally
			{
				m_ErrorsStatesSection.Leave();
			}

			string message = string.Format("{0} set to: {1}", key, GetStringForLogValue(value));

			Log(severity, message);
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

		private static string GetStringForLogValue<T>(T value)
		{
// ReSharper disable CompareNonConstrainedGenericWithNull
			if (value == null)
// ReSharper restore CompareNonConstrainedGenericWithNull
				return "NULL";

			Type underlying = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
			if (underlying.IsEnum)
				return StringUtils.NiceName(value);

			return value.ToString();
		}

		#endregion
	}
}
