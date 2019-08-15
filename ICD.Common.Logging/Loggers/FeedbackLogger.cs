using System;
using ICD.Common.Properties;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services.Logging;

namespace ICD.Common.Logging.Loggers
{
	/// <summary>
	/// Simple logger that raises logs via events.
	/// </summary>
	[PublicAPI]
	public sealed class FeedbackLogger : ISystemLogger
	{
		[PublicAPI]
		public event EventHandler<LogItemEventArgs> OnAddEntry;

		/// <summary>
		/// Raises OnAddEntry.
		/// </summary>
		/// <param name="item"></param>
		public void AddEntry(LogItem item)
		{
			OnAddEntry.Raise(this, new LogItemEventArgs(item));
		}
	}
}
