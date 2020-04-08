using System;
using System.Text;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Services.Logging;

namespace ICD.Common.Logging.LoggingContexts
{
	public interface ILoggingContext
	{
		/// <summary>
		/// Adds the log item.
		/// </summary>
		/// <param name="severity"></param>
		/// <param name="message"></param>
		void Log(eSeverity severity, string message);

		/// <summary>
		/// Adds the log item with string formatting.
		/// </summary>
		/// <param name="severity">Severity Code</param>
		/// <param name="message">Message Text format string</param>
		/// <param name="args">objects to format into the string</param>
		void Log(eSeverity severity, string message, params object[] args);

		/// <summary>
		/// Sets the current error state for the given key.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="severity"></param>
		/// <param name="value"></param>
		void Set<T>([NotNull] string key, eSeverity severity, T value);
	}

	/// <summary>
	/// Extension methods for ILoggingContext.
	/// </summary>
	public static class LoggingContextExtensions
	{
		public static void Log(this ILoggingContext extends, eSeverity severity, Exception e, string message)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			if (e == null)
				throw new ArgumentNullException("e");

#if STANDARD
			if (e is AggregateException)
			{
				extends.LogEntry(severity, e as AggregateException, message);
				return;
			}
#endif
			extends.Log(severity, string.Format("{0}: {1}{2}{3}{2}{4}", e.GetType().Name, message,
			                                         IcdEnvironment.NewLine, e.Message, e.StackTrace));
		}

#if STANDARD
		/// <summary>
		/// Logs an aggregate exception as a formatted list of inner exceptions.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="severity"></param>
		/// <param name="e"></param>
		/// <param name="message"></param>
		private static void LogEntry(this ILoggingContext extends, eSeverity severity, AggregateException e, string message)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			if (e == null)
				throw new ArgumentNullException("e");

			StringBuilder builder = new StringBuilder();

			builder.AppendFormat("{0}: {1}", e.GetType().Name, message);

			builder.Append(IcdEnvironment.NewLine);
			builder.Append('[');

			foreach (Exception inner in e.Flatten().InnerExceptions)
				builder.AppendFormat("{0}\t{1}: {2}", IcdEnvironment.NewLine, inner.GetType().Name, inner.Message);

			builder.Append(IcdEnvironment.NewLine);
			builder.Append(']');

			builder.Append(IcdEnvironment.NewLine);
			builder.Append(e.StackTrace);

			extends.Log(severity, builder.ToString());
		}
#endif

		public static void Log(this ILoggingContext extends, eSeverity severity, Exception e, string message,
		                            params object[] args)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			if (e == null)
				throw new ArgumentNullException("e");

			extends.Log(severity, e, string.Format(message, args));
		}
	}
}
