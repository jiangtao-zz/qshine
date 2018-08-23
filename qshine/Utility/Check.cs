using System;
using qshine.Globalization;

namespace qshine
{
	/// <summary>
	/// Valifies conditions and throw exception when the confition failed.
	/// </summary>
	public class Check
	{
		/// <summary>
		/// Verifies that the specified string is not null or empty.
		/// The condition is not satisfied. 
		/// Displays a message with a parameter name if the assertion fails.
		/// </summary>
		/// <param name="value">The value to verify is not null or empty.</param>
		/// <param name="paramName">The parameter name.</param>
		public static void IsNotEmpty(string value, string paramName=null)
		{
			if (string.IsNullOrEmpty(value))
			{
				if (string.IsNullOrEmpty(paramName))
				{
					throw new ArgumentNullException();
				}
				throw new ArgumentNullException(paramName);
			}
		}
		/// <summary>
		/// Verifies that the specified object is not null.
		/// The condition is not satisfied. 
		/// Displays a message with a parameter name if the assertion fails.
		/// </summary>
		/// <param name="value">The value to verify is not null or empty.</param>
		/// <param name="paramName">The parameter name.</param>
		public static void IsNotNull(object value, string paramName = null)
		{
			if (value==null)
			{
				if (string.IsNullOrEmpty(paramName))
				{
					throw new ArgumentNullException();
				}
				throw new ArgumentNullException(paramName);
			}
		}

		/// <summary>
		/// Verifies that the given confition is satisfied.
		/// Displays a message with a parameter name if the condition failed.
		/// </summary>
		/// <param name="condition">If the condition is <c>true</c>.</param>
		/// <param name="message">Display message if condition is failed.</param>
		/// <typeparam name="T">The Exception to be thrown if the condition failed.</typeparam>
		public static void IsTrue<T>(bool condition, string message)
			where T:Exception
		{
			if (!condition)
			{
				throw (T)Activator.CreateInstance(typeof(T), message);
			}
		}
	}
}
