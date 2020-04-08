using ICD.Common.Utils.Services.Logging;

namespace ICD.Common.Logging.LoggingContexts
{
	public abstract class AbstractLoggingContext : ILoggingContext
	{
		/// <summary>
		/// Adds the log item.
		/// </summary>
		/// <param name="severity"></param>
		/// <param name="message"></param>
		public abstract void Log(eSeverity severity, string message);

		/// <summary>
		/// Adds the log item with string formatting.
		/// </summary>
		/// <param name="severity">Severity Code</param>
		/// <param name="message">Message Text format string</param>
		/// <param name="args">objects to format into the string</param>
		public abstract void Log(eSeverity severity, string message, params object[] args);

		/// <summary>
		/// Sets the current error state for the given key.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="severity"></param>
		/// <param name="value"></param>
		public abstract void Set<T>(string key, eSeverity severity, T value);
	}
}
