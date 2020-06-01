using ICD.Common.Utils.Services.Logging;

namespace ICD.Common.Logging.LoggingContexts
{
	public abstract class AbstractLoggingContext : ILoggingContext
	{
		#region Methods

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

		#endregion
	}
}
