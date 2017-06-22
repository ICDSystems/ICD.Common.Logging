using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;

namespace ICD.Common.Logging.Console.Fusion
{
	public enum eFusionSeverity
	{
		[PublicAPI] Ok = 0,
		[PublicAPI] Notice = 1,
		[PublicAPI] Warning = 2,
		[PublicAPI] Error = 3,
		[PublicAPI] FatalError = 4
	}

	/// <summary>
	/// FusionStringBuilder generates a fusion error report in the format:
	///		3: Message1; Message2;
	/// </summary>
	public sealed class FusionStringBuilder
	{
		private const string ERROR_DELIMITER = "; ";

		/// <summary>
		/// Called when an error's severity has changed.
		/// </summary>
		[PublicAPI]
		public event EventHandler<FusionErrorEventArgs> OnErrorSeverityChanged;

		private readonly Dictionary<string, eFusionSeverity> m_Errors;

		#region Properties

		/// <summary>
		/// When TechMode is enabled the fusion string is limited to a simple TechMode notice.
		/// </summary>
		[PublicAPI]
		public bool TechMode { get; set; }

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor.
		/// </summary>
		public FusionStringBuilder()
		{
			m_Errors = new Dictionary<string, eFusionSeverity>();
		}

		#endregion

		#region Methods

		/// <summary>
		/// Clears all errors.
		/// </summary>
		[PublicAPI]
		public void Clear()
		{
			m_Errors.Clear();
		}

		/// <summary>
		/// Sets the severity of the given error.
		/// </summary>
		/// <param name="error"></param>
		/// <param name="severity"></param>
		[PublicAPI]
		public void SetErrorSeverity(string error, eFusionSeverity severity)
		{
			eFusionSeverity oldSeverity = GetErrorSeverity(error);
			if (severity == oldSeverity)
				return;

			if (severity == eFusionSeverity.Ok)
				m_Errors.Remove(error);
			else
				m_Errors[error] = severity;

			RaiseErrorSeverityChanged(error, severity);
		}

		/// <summary>
		/// Removes the error.
		/// </summary>
		/// <param name="error"></param>
		[PublicAPI]
		public void RemoveError(string error)
		{
			SetErrorSeverity(error, eFusionSeverity.Ok);
		}

		/// <summary>
		/// Gets the severity for the given error.
		/// </summary>
		/// <param name="error"></param>
		/// <returns></returns>
		[PublicAPI]
		public eFusionSeverity GetErrorSeverity(string error)
		{
			return m_Errors.ContainsKey(error) ? m_Errors[error] : eFusionSeverity.Ok;
		}

		/// <summary>
		/// Builds the fusion string from the errors.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			int severity = (int)GetMaxSeverity();
			string errorString = GetErrorsString();

			return string.Format("{0}: {1}", severity, errorString);
		}

		/// <summary>
		/// Gets the maximum severity out of the errors.
		/// </summary>
		/// <returns></returns>
		[PublicAPI]
		public eFusionSeverity GetMaxSeverity()
		{
			if (TechMode)
				return eFusionSeverity.Notice;

			return m_Errors.Count == 0 ? eFusionSeverity.Ok : m_Errors.Values.Max();
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Gets the error portion of the fusion string.
		/// </summary>
		/// <returns></returns>
		private string GetErrorsString()
		{
			string errorString;

			if (TechMode)
				errorString = "TechMode" + ERROR_DELIMITER;
			else
			{
				// Sort errors by severity
				string[] errors = m_Errors.Where(p => p.Value != eFusionSeverity.Ok)
				                          .OrderBy(p => p.Key)
				                          .ThenByDescending(p => p.Value)
				                          .Select(p => p.Key)
				                          .ToArray();

				errorString = string.Join(ERROR_DELIMITER, errors);
				if (errors.Length > 0)
					errorString += ERROR_DELIMITER;
			}

			errorString.TrimEnd();

			return errorString;
		}

		/// <summary>
		/// Raises the OnErrorSeverityChanged event.
		/// </summary>
		/// <param name="error"></param>
		/// <param name="severity"></param>
		private void RaiseErrorSeverityChanged(string error, eFusionSeverity severity)
		{
			EventHandler<FusionErrorEventArgs> handler = OnErrorSeverityChanged;
			if (handler != null)
				handler(null, new FusionErrorEventArgs(error, severity));
		}

		#endregion
	}
}
