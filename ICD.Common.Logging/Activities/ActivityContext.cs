using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Logging.LoggingContexts;
using ICD.Common.Utils;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;

namespace ICD.Common.Logging.Activities
{
	public sealed class ActivityContext : IActivityContext
	{
		/// <summary>
		/// Raised when the most pressing activity changes.
		/// </summary>
		public event EventHandler<GenericEventArgs<Activity>> OnActivityChanged;

		private readonly Dictionary<string, Activity> m_Activities;
		private readonly SafeCriticalSection m_ActivitiesSection;

		/// <summary>
		/// Constructor.
		/// </summary>
		public ActivityContext()
		{
			m_Activities = new Dictionary<string, Activity>();
			m_ActivitiesSection = new SafeCriticalSection();
		}

		/// <summary>
		/// Sets the current activity for the given key.
		/// </summary>
		/// <param name="activity"></param>
		public void LogActivity(Activity activity)
		{
			m_ActivitiesSection.Enter();

			try
			{
				Activity old;
				if (m_Activities.TryGetValue(activity.Key, out old) && activity == old)
					return;

				m_Activities[activity.Key] = activity;
			}
			finally
			{
				m_ActivitiesSection.Leave();
			}

			OnActivityChanged.Raise(this, new GenericEventArgs<Activity>(activity));
		}

		public IEnumerator<Activity> GetEnumerator()
		{
			return m_ActivitiesSection.Execute(() => m_Activities.Values.ToList().GetEnumerator());
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}