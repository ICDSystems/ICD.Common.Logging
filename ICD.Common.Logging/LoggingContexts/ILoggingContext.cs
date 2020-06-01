using System;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Services.Logging;

namespace ICD.Common.Logging.LoggingContexts
{
	public interface ILoggingContext
	{
		/// <summary>
		/// Adds the log item.
		/// </summary>
		/// <param name="severity"></param>
		/// <param name="message"></param>
		void Log(eSeverity severity, string message);

		/// <summary>
		/// Adds the log item with string formatting.
		/// </summary>
		/// <param name="severity">Severity Code</param>
		/// <param name="message">Message Text format string</param>
		/// <param name="args">objects to format into the string</param>
		void Log(eSeverity severity, string message, params object[] args);
	}

	/// <summary>
	/// Extension methods for ILoggingContext.
	/// </summary>
	public static class LoggingContextExtensions
	{
		/// <summary>
		/// Logs the given exception message and stack trace.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="severity"></param>
		/// <param name="e"></param>
		/// <param name="message"></param>
		public static void Log([NotNull] this ILoggingContext extends, eSeverity severity, [NotNull] Exception e,
		                       string message)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			if (e == null)
				throw new ArgumentNullException("e");

#if STANDARD
			if (e is AggregateException)
			{
				extends.LogEntry(severity, e as AggregateException, message);
				return;
			}
#endif
			extends.Log(severity, string.Format("{0}: {1}{2}{3}{2}{4}", e.GetType().Name, message,
			                                    IcdEnvironment.NewLine, e.Message, e.StackTrace));
		}

#if STANDARD
		/// <summary>
		/// Logs an aggregate exception as a formatted list of inner exceptions.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="severity"></param>
		/// <param name="e"></param>
		/// <param name="message"></param>
		private static void LogEntry(this ILoggingContext extends, eSeverity severity, AggregateException e, string message)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			if (e == null)
				throw new ArgumentNullException("e");

			System.Text.StringBuilder builder = new System.Text.StringBuilder();

			builder.AppendFormat("{0}: {1}", e.GetType().Name, message);

			builder.Append(IcdEnvironment.NewLine);
			builder.Append('[');

			foreach (Exception inner in e.Flatten().InnerExceptions)
				builder.AppendFormat("{0}\t{1}: {2}", IcdEnvironment.NewLine, inner.GetType().Name, inner.Message);

			builder.Append(IcdEnvironment.NewLine);
			builder.Append(']');

			builder.Append(IcdEnvironment.NewLine);
			builder.Append(e.StackTrace);

			extends.Log(severity, builder.ToString());
		}
#endif

		/// <summary>
		/// Logs the given exception message and stack trace.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="severity"></param>
		/// <param name="e"></param>
		/// <param name="message"></param>
		/// <param name="args"></param>
		public static void Log([NotNull] this ILoggingContext extends, eSeverity severity, [NotNull] Exception e,
		                       string message, params object[] args)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			if (e == null)
				throw new ArgumentNullException("e");

			extends.Log(severity, e, string.Format(message, args));
		}

		/// <summary>
		/// Logs the value change with the given severity.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="severity"></param>
		/// <param name="propertyName"></param>
		/// <param name="value"></param>
		public static void LogSetTo<T>([NotNull] this ILoggingContext extends, eSeverity severity, string propertyName, T value)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			string valueString = GetStringForLogValue(value);
			string message = string.Format("{0} set to {1}", propertyName, valueString);

			extends.Log(severity, message);
		}

		/*
		/// <summary>
		/// Sets the current error state for the given key.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <param name="severity"></param>
		/// <param name="priority"></param>
		public static void LogActivity<T>([NotNull] this ILoggingContext extends, [NotNull] string key, T value,
								  eSeverity severity, Activity.ePriority priority)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			if (string.IsNullOrEmpty(key))
				throw new ArgumentException("Key must not be null or empty");

			Func<T, Activity> lookup =
				v =>
				{
					string message = BuildMessage(key, v);
					return new Activity(priority, message, severity);
				};

			extends.LogActivity(key, value, lookup);
		}

		/// <summary>
		/// Sets the current error state for the given key.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <param name="activityMap"></param>
		public static void LogActivity<T>([NotNull] this ILoggingContext extends, [NotNull] string key, T value,
		                          [NotNull] IDictionary<NullObject<T>, Activity> activityMap)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			if (string.IsNullOrEmpty(key))
				throw new ArgumentException("Key must not be null or empty");

			if (activityMap == null)
				throw new ArgumentNullException("activityMap");

			Func<T, Activity> lookup =
				v =>
				{
					Activity activity;
// ReSharper disable RedundantCast
					if (activityMap.TryGetValue((NullObject<T>)v, out activity))
// ReSharper restore RedundantCast
						return activity;

					string message = BuildMessage(key, v);
					return new Activity(Activity.ePriority.Default, message, eSeverity.Informational);
				};

			extends.LogActivity(key, value, lookup);
		}
		*/

		private static string GetStringForLogValue(object value)
		{
			// ReSharper disable CompareNonConstrainedGenericWithNull
			if (value == null)
				// ReSharper restore CompareNonConstrainedGenericWithNull
				return "NULL";

			Type type = value.GetType();
			Type underlying = Nullable.GetUnderlyingType(type) ?? type;

			if (underlying.IsEnum)
				return StringUtils.NiceName(value);

			return value.ToString();
		}
	}
}
