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
        /// Ensure the value is not null or empty.
        /// Throw ArgumentNullException exception with the parameter name if the given string object 
        /// is null or empty.
        /// </summary>
        /// <param name="value">The given value should not be null or empty.</param>
        /// <param name="paramName">The parameter name.</param>
        public static void HaveValue(string value, string paramName=null)
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
        /// Ensure the object value is not null.
		/// Throw ArgumentNullException exception with the parameter name if the given object is null.
		/// </summary>
		/// <param name="value">The given value should not be null.</param>
		/// <param name="paramName">The parameter name.</param>
		public static void HaveValue(object value, string paramName = null)
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
        /// Check for a condition and ensure the condition is true.
        /// Throw a new type of exception with proper error message if the condition is false.
        /// </summary>
        /// <param name="condition">Ensure the condition is true.</param>
        /// <param name="format">Error message format.</param>
        /// <param name="args">Error message arguments.</param>
        /// <typeparam name="T">The Exception to be thrown if the condition failed.</typeparam>
        public static void Assert<T>(bool condition, string format=null, params object[] args)
			where T:Exception
		{
			if (!condition)
			{
                string message = format;
                if (args.Length > 0)
                {
                    message = string.Format(format, args);
                }
				throw (T)Activator.CreateInstance(typeof(T), message);
			}
		}
	}
}
