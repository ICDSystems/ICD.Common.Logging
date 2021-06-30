using System;
using ICD.Common.Properties;
using ICD.Common.Utils.Services.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ICD.Common.Logging.Activities
{
	public struct Activity : IComparable<Activity>, IEquatable<Activity>
	{
		public enum ePriority
		{
			Default = 0,
			Lowest = int.MaxValue,
			Low = int.MaxValue / 2,
			Medium = int.MinValue / 3,
			High = int.MinValue / 2,
			Urgent = int.MinValue
		}

		private readonly ePriority m_Priority;
		private readonly string m_Key;
		private readonly string m_Message;
		private readonly eSeverity m_Severity;
		private readonly Guid m_Uuid;

		#region Properties

		/// <summary>
		/// Used for determining which activity is currently most pressing.
		/// </summary>
		public ePriority Priority { get { return m_Priority; } }

		/// <summary>
		/// Gets the key for this activity.
		/// </summary>
		public string Key { get { return m_Key; } }

		/// <summary>
		/// Gets the message for this activity.
		/// </summary>
		public string Message { get { return m_Message; } }

		/// <summary>
		/// Gets the severity for the activity.
		/// </summary>
		[JsonConverter(typeof(StringEnumConverter))]
		public eSeverity Severity { get { return m_Severity; } }

		/// <summary>
		/// Gets the unique ID for this activity.
		/// </summary>
		public Guid Uuid { get { return m_Uuid; } }

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		public Activity(ePriority priority, string key, string message, eSeverity severity)
		{
			m_Priority = priority;
			m_Key = key;
			m_Message = message;
			m_Severity = severity;
			m_Uuid = Guid.NewGuid();
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="priority"></param>
		/// <param name="key"></param>
		/// <param name="message"></param>
		/// <param name="severity"></param>
		/// <param name="uuid"></param>
		[JsonConstructor]
		public Activity(ePriority priority, string key, string message, eSeverity severity, Guid uuid)
		{
			m_Priority = priority;
			m_Key = key;
			m_Message = message;
			m_Severity = severity;
			m_Uuid = uuid;
		}

		#region Equality

		/// <summary>
		/// Implementing default equality.
		/// </summary>
		/// <param name="a1"></param>
		/// <param name="a2"></param>
		/// <returns></returns>
		public static bool operator ==(Activity a1, Activity a2)
		{
			return a1.Equals(a2);
		}

		/// <summary>
		/// Implementing default inequality.
		/// </summary>
		/// <param name="a1"></param>
		/// <param name="a2"></param>
		/// <returns></returns>
		public static bool operator !=(Activity a1, Activity a2)
		{
			return !a1.Equals(a2);
		}

		/// <summary>
		/// Returns true if this instance is equal to the given object.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			return obj is Activity && Equals((Activity)obj);
		}

		/// <summary>
		/// Returns true if this instance is equal to the given endpoint.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		[Pure]
		public bool Equals(Activity other)
		{
			return m_Priority == other.m_Priority &&
			       m_Key == other.m_Key &&
			       m_Severity == other.m_Severity &&
			       m_Message == other.m_Message &&
				   m_Uuid == other.m_Uuid;
		}

		/// <summary>
		/// Gets the hashcode for this instance.
		/// </summary>
		/// <returns></returns>
		[Pure]
		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;
				hash = hash * 23 + (int)m_Priority;
				hash = hash * 23 + (m_Key == null ? 0 : m_Key.GetHashCode());
				hash = hash * 23 + (m_Message == null ? 0 : m_Message.GetHashCode());
				hash = hash * 23 + (int)m_Severity;
				hash = hash * 23 + m_Uuid.GetHashCode();
				return hash;
			}
		}

		public int CompareTo(Activity other)
		{
			int result = m_Priority.CompareTo(other.m_Priority);
			if (result != 0)
				return result;

			result = string.Compare(m_Key, other.m_Key, StringComparison.InvariantCulture);
			if (result != 0)
				return result;

			result = m_Severity.CompareTo(other.m_Severity);
			if (result != 0)
				return result;

			result = string.Compare(m_Message, other.m_Message, StringComparison.InvariantCulture);
			if (result != 0)
				return result;

			return m_Uuid.CompareTo(other.m_Uuid);
		}

		#endregion
	}
}
