using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;

namespace ICD.Common.Logging.Activities
{
	public sealed class ActivityContext : IActivityContext
	{
		/// <summary>
		/// Raised when an activity changes.
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
				if (m_Activities.TryGetValue(activity.Key, out old) && EqualContent(activity, old))
					return;

				m_Activities[activity.Key] = activity;
			}
			finally
			{
				m_ActivitiesSection.Leave();
			}

			OnActivityChanged.Raise(this, new GenericEventArgs<Activity>(activity));
		}

		/// <summary>
		/// Returns true if the content of the given activities are equal.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		private static bool EqualContent(Activity a, Activity b)
		{
			return a.Priority == b.Priority &&
			       a.Key == b.Key &&
			       a.Severity == b.Severity &&
			       a.Message == b.Message;
		}

		public IEnumerator<Activity> GetEnumerator()
		{
			return m_ActivitiesSection.Execute(() => m_Activities.Values.Order().ToList().GetEnumerator());
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}