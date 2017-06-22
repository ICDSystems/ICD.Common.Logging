using System;
using ICD.Common.Properties;

namespace ICD.Common.Logging.Console.Fusion
{
	public sealed class FusionErrorEventArgs : EventArgs
	{
		private readonly string m_Error;
		private readonly eFusionSeverity m_Severity;

		[PublicAPI]
		public string Error { get { return m_Error; } }

		[PublicAPI]
		public eFusionSeverity Severity { get { return m_Severity; } }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="error"></param>
		/// <param name="severity"></param>
		public FusionErrorEventArgs(string error, eFusionSeverity severity)
		{
			m_Error = error;
			m_Severity = severity;
		}
	}
}
