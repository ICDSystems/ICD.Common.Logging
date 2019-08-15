using ICD.Common.Utils.Services.Logging;

namespace ICD.Common.Logging.Loggers
{
	public interface ISystemLogger
	{
		/// <summary>
		/// Adds the given log item to the logger.
		/// </summary>
		/// <param name="item"></param>
		void AddEntry(LogItem item);
	}
}
