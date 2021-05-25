using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.IO;
using ICD.Common.Utils.Services.Logging;

namespace ICD.Common.Logging.Loggers
{
	public sealed class FileLogger : ISystemLogger
	{
		#region Private Members

		// 10 files of 20MB
		private const long DEFAULT_LOG_SIZE_LIMIT = 20 * 1000 * 1000;
		private const int DEFAULT_LOG_COUNT = 10;

		private static readonly Dictionary<eSeverity, string> s_SeverityLabels =
			new Dictionary<eSeverity, string>
			{
				{eSeverity.Emergency, "Emrgcy"},
				{eSeverity.Alert, "Alert "},
				{eSeverity.Critical, "Crit  "},
				{eSeverity.Error, "Error "},
				{eSeverity.Warning, "Warn  "},
				{eSeverity.Notice, "Notice"},
				{eSeverity.Informational, "Info  "},
				{eSeverity.Debug, "Debug "}
			}; 

		private readonly SafeCriticalSection m_LogSection;
		private readonly string m_LogDirectory;
		private readonly string m_LogPath;
		private readonly long m_LogSizeLimit;

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor.
		/// </summary>
		public FileLogger()
			: this(PathUtils.ProgramLogsPath)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="directoryPath"></param>
		public FileLogger(string directoryPath)
			: this(directoryPath, DEFAULT_LOG_SIZE_LIMIT)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="directoryPath"></param>
		/// <param name="logSizeLimit"></param>
		private FileLogger(string directoryPath, long logSizeLimit)
		{
			m_LogSection = new SafeCriticalSection();
			m_LogDirectory = directoryPath;
			m_LogPath = BuildPathToLogFile(directoryPath);
			m_LogSizeLimit = logSizeLimit;

			IcdDirectory.CreateDirectory(directoryPath);

			HandleLogSizeLimitReached();
		}

		#endregion

		#region Methods

		/// <summary>
		/// Adds the given log item to the logger.
		/// </summary>
		/// <param name="item"></param>
		public void AddEntry(LogItem item)
		{
			m_LogSection.Enter();

			try
			{
				// Format log item.
				string message = Format(item);

				// Ensure adding the entry doesn't go over the size limit.
				if (IcdFile.Exists(m_LogPath) && System.Text.Encoding.UTF8.GetByteCount(message) + IcdFile.GetLength(m_LogPath) > m_LogSizeLimit)
					HandleLogSizeLimitReached();

				// Add the entry.
				using (IcdStreamWriter streamWriter = IcdFile.AppendText(m_LogPath))
					streamWriter.WriteLine(message);
			}
			finally
			{
				m_LogSection.Leave();
			}
		}

		#endregion

		#region Private Methods

		private static string Format(LogItem item)
		{
			//Info   | 2019-04-08 16:03:57 | (ConnectionStateManager)CiscoCodecDevice(Id=202002, Name="1D-103 Cisco SX80") - Attempting to reconnect (Attempt 1).
			return string.Format("{0} | {1:yyyy-MM-dd HH:mm:ss} | {2}",
			                     s_SeverityLabels[item.Severity],
			                     item.Timestamp.ToLocalTime(),
			                     item.Message);
		}

		private static string BuildPathToLogFile(string directoryPath)
		{
			string date = IcdEnvironment.GetUtcTime()
			                            .ToString("s")
			                            .Replace(':', '-') + "Z.log";

			return PathUtils.Join(directoryPath, date);
		}

		private void HandleLogSizeLimitReached()
		{
			List<string> fileInfos = IcdDirectory.GetFiles(m_LogDirectory)
			                                     .OrderByDescending(fi => IcdFile.GetCreationTime(fi))
			                                     .ToList();

			// Delete oldest files so we end up with N - 1 files
			for (int i = fileInfos.Count - 1; i >= DEFAULT_LOG_COUNT - 1; i--)
			{
				string path = fileInfos[i];
				if (IcdFile.Exists(path))
					IcdFile.Delete(path);

				fileInfos.RemoveAt(i);
			}

			// Rename existing files into new 0 - (N-1) range
			for (int i = fileInfos.Count - 1; i >= 0; i--)
			{
				string path = fileInfos[i];

				string fileName = IcdPath.GetFileNameWithoutExtension(path);
				int underscore = fileName.LastIndexOf('_');
				string fileNameWithoutNumber = underscore < 0 ? fileName : fileName.Substring(0, underscore);

				string newFilename = string.Format("{0}_{1:D2}.log", fileNameWithoutNumber, i);
				string newPath = PathUtils.Join(m_LogDirectory, newFilename);

				IcdFile.Move(path, newPath);
			}
		}

		#endregion
	}
}
