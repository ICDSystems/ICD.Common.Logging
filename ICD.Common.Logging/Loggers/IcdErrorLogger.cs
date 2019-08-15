using System;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Services.Logging;

namespace ICD.Common.Logging.Loggers
{
	/// <summary>
	/// IcdErrorLogger simply logs to the IcdErrorLog.
	/// </summary>
	[PublicAPI]
	public sealed class IcdErrorLogger : ISystemLogger
	{
		/// <summary>
		/// Logs the item.
		/// </summary>
		/// <param name="item"></param>
		public void AddEntry(LogItem item)
		{
			string message = item.Message;

			switch (item.Severity)
			{
				case eSeverity.Emergency:
				case eSeverity.Alert:
				case eSeverity.Critical:
				case eSeverity.Error:
					IcdErrorLog.Error(message);
					break;

				case eSeverity.Warning:
					IcdErrorLog.Warn(message);
					break;

				case eSeverity.Notice:
					IcdErrorLog.Notice(message);
					break;

				case eSeverity.Informational:
					IcdErrorLog.Info(message);
					break;

				case eSeverity.Debug:
					IcdErrorLog.Ok(message);
					break;

				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}
