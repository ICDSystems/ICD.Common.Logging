using System;
using System.Collections.Generic;
using ICD.Common.Logging.LoggingContexts;
using ICD.Common.Utils.EventArguments;

namespace ICD.Common.Logging.Activities
{
	public interface IActivityContext : IEnumerable<Activity>
	{
		/// <summary>
		/// Raised when the most pressing activity changes.
		/// </summary>
		event EventHandler<GenericEventArgs<Activity>> OnActivityChanged;

		/// <summary>
		/// Logs an activity with a message, priority and severity,
		/// notifying telemetry of the state of an active fault or user activity.
		/// </summary>
		/// <param name="activity"></param>
		void LogActivity(Activity activity);
	}
}
