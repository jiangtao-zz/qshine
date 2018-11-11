using System;
using System.Diagnostics;

namespace qshine
{
	/// <summary>
	/// Contract for logger interface
	/// </summary>
	public interface ILogger
	{
		/// <summary>
		/// Logging critical <c>Fatal</c> level formatted message. 
		/// </summary>
		/// <param name="message">formatted message.</param>
		/// <param name="properties">format arguments</param>
		/// <remarks>Highest level. system crashes</remarks>
		void Fatal(string message, params object[] properties);
		void Fatal(Exception ex, string message, params object[] properties);

		/// <summary>
		/// Logging <c>Error</c> level formatted message. 
		/// </summary>
		/// <param name="message">formatted message.</param>
		/// <param name="properties">format arguments</param>
		/// <remarks>Error to prevent application running properly</remarks>
		void Error(string message, params object[] properties);
		void Error(Exception ex, string message, params object[] properties);

		/// <summary>
		/// Logging <c>Warn</c> level formatted message. 
		/// </summary>
		/// <param name="message">formatted message.</param>
		/// <param name="properties">format arguments</param>
		/// <remarks>Incorrect behavior, but application is continue running.</remarks>
		void Warn(string message, params object[] properties);
		void Warn(Exception ex, string message, params object[] properties);

		/// <summary>
		/// Logging <c>Info</c> level formatted message. 
		/// </summary>
		/// <param name="message">formatted message.</param>
		/// <param name="properties">format arguments</param>
		/// <remarks>Normal behavior like mail sent, user updated profile etc. </remarks>
		void Info(string message, params object[] properties);
		void Info(Exception ex, string message, params object[] properties);

		/// <summary>
		/// Logging <c>Debug</c> level formatted message. 
		/// </summary>
		/// <param name="message">formatted message.</param>
		/// <param name="properties">format arguments</param>
		/// <remarks>Executed queries, user authenticated, session expired</remarks>
		void Debug(string message, params object[] properties);
		void Debug(Exception ex, string message, params object[] properties);

		/// <summary>
		/// Logging <c>Trace</c> level formatted message. 
		/// </summary>
		/// <param name="message">formatted message.</param>
		/// <param name="properties">format arguments</param>
		/// <remarks>Lowest level, such as, method begin and method end</remarks>
		void Trace(string message, params object[] properties);
		void Trace(Exception ex, string message, params object[] properties);

		/// <summary>
		/// Gets a value indicating whether this <see cref="T:RealSuite.Infrastructure.ILogger"/> is logging enabled.
		/// </summary>
		/// <value><c>true</c> if is logging enabled; otherwise, <c>false</c>.</value>
		bool IsLoggingEnabled { get; }

		/// <summary>
		/// Gets a value indicating whether this <see cref="T:RealSuite.Infrastructure.ILogger"/> is fatal enabled.
		/// </summary>
		/// <value><c>true</c> if is fatal enabled; otherwise, <c>false</c>.</value>
		bool IsFatalEnabled { get; }

		/// <summary>
		/// Gets a value indicating whether this <see cref="T:RealSuite.Infrastructure.ILogger"/> is error enabled.
		/// </summary>
		/// <value><c>true</c> if is error enabled; otherwise, <c>false</c>.</value>
		bool IsErrorEnabled { get; }

		/// <summary>
		/// Gets a value indicating whether this <see cref="T:RealSuite.Infrastructure.ILogger"/> is warn enabled.
		/// </summary>
		/// <value><c>true</c> if is warn enabled; otherwise, <c>false</c>.</value>
		bool IsWarnEnabled { get; }

		/// <summary>
		/// Gets a value indicating whether this <see cref="T:RealSuite.Infrastructure.ILogger"/> is info enabled.
		/// </summary>
		/// <value><c>true</c> if is info enabled; otherwise, <c>false</c>.</value>
		bool IsInfoEnabled { get; }

		/// <summary>
		/// Gets a value indicating whether this <see cref="T:RealSuite.Infrastructure.ILogger"/> is debug enabled.
		/// </summary>
		/// <value><c>true</c> if is debug enabled; otherwise, <c>false</c>.</value>
		bool IsDebugEnabled { get; }

		/// <summary>
		/// Gets a value indicating whether this <see cref="T:RealSuite.Infrastructure.ILogger"/> is trace enabled.
		/// </summary>
		/// <value><c>true</c> if is trace enabled; otherwise, <c>false</c>.</value>
		bool IsTraceEnabled { get; }

		/// <summary>
		/// Gets or sets the category.
		/// </summary>
		/// <value>The category.</value>
		/// <remarks>
		/// Common categories:
		/// 	System
		/// 	Security (Authentication/Authorization)
		/// 	Database
		/// 	Validation
		/// 	Workflow
		/// 	General
		/// </remarks>
		string Category { get; set; }

        /// <summary>
        /// Enable logging
        /// </summary>
        /// <param name="logLevel"></param>
        void EnableLogging(TraceEventType logLevel);

    }
}
