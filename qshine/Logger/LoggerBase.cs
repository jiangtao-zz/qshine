using System;
using System.Diagnostics;

namespace qshine
{
	public abstract class LoggerBase:ILogger
	{
		string _category;
		TraceSwitch _switch;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:qshine.LoggerBase"/> class.
		/// </summary>
		/// <param name="category">Category.</param>
		public LoggerBase(string category)
		{
			Category = category;
		}

		/// <summary>
		/// Gets or sets the category.
		/// </summary>
		/// <value>The category.</value>
		public string Category
		{
			get {return _category;}
			set {_category=value;}
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="T:qshine.LoggerBase"/> is logging enabled.
		/// </summary>
		/// <value><c>true</c> if is logging enabled; otherwise, <c>false</c>.</value>
		public virtual bool IsLoggingEnabled { 
			get {
				return IsDebugEnabled || IsErrorEnabled || IsFatalEnabled || IsInfoEnabled || IsTraceEnabled || IsWarnEnabled;
			} 
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="T:qshine.LoggerBase"/> is fatal enabled.
		/// </summary>
		/// <value><c>true</c> if is fatal enabled; otherwise, <c>false</c>.</value>
		public abstract bool IsFatalEnabled { get; }

		/// <summary>
		/// Gets a value indicating whether this <see cref="T:qshine.LoggerBase"/> is error enabled.
		/// </summary>
		/// <value><c>true</c> if is error enabled; otherwise, <c>false</c>.</value>
		public abstract bool IsErrorEnabled { get; }

		/// <summary>
		/// Gets a value indicating whether this <see cref="T:qshine.LoggerBase"/> is warn enabled.
		/// </summary>
		/// <value><c>true</c> if is warn enabled; otherwise, <c>false</c>.</value>
		public abstract bool IsWarnEnabled { get; }

		/// <summary>
		/// Gets a value indicating whether this <see cref="T:qshine.LoggerBase"/> is info enabled.
		/// </summary>
		/// <value><c>true</c> if is info enabled; otherwise, <c>false</c>.</value>
		public abstract bool IsInfoEnabled { get; }

		/// <summary>
		/// Gets a value indicating whether this <see cref="T:qshine.LoggerBase"/> is trace enabled.
		/// </summary>
		/// <value><c>true</c> if is trace enabled; otherwise, <c>false</c>.</value>
		public abstract bool IsTraceEnabled { get; }

		/// <summary>
		/// Gets a value indicating whether this <see cref="T:qshine.LoggerBase"/> is debug enabled.
		/// </summary>
		/// <value><c>true</c> if is debug enabled; otherwise, <c>false</c>.</value>
		public abstract bool IsDebugEnabled { get; }

		/// <summary>
		/// Debug the specified message and properties.
		/// </summary>
		/// <returns>The debug.</returns>
		/// <param name="message">Message.</param>
		/// <param name="properties">Properties.</param>
		public virtual void Debug(string message, params object[] properties)
		{
			Log(TraceEventType.Verbose, message, null, properties);
		}

		/// <summary>
		/// Debug the specified ex, message and properties.
		/// </summary>
		/// <returns>The debug.</returns>
		/// <param name="ex">Ex.</param>
		/// <param name="message">Message.</param>
		/// <param name="properties">Properties.</param>
		public virtual void Debug(Exception ex, string message, params object[] properties)
		{
            Log(TraceEventType.Verbose, message, ex, properties);
		}

		/// <summary>
		/// Error the specified message and properties.
		/// </summary>
		/// <returns>The error.</returns>
		/// <param name="message">Message.</param>
		/// <param name="properties">Properties.</param>
		public virtual void Error(string message, params object[] properties)
		{
            Log(TraceEventType.Error, message, null, properties);
		}

		/// <summary>
		/// Error the specified ex, message and properties.
		/// </summary>
		/// <returns>The error.</returns>
		/// <param name="ex">Ex.</param>
		/// <param name="message">Message.</param>
		/// <param name="properties">Properties.</param>
		public virtual void Error(Exception ex, string message, params object[] properties)
		{
            Log(TraceEventType.Error, message, ex, properties);
		}

		/// <summary>
		/// Fatal the specified message and properties.
		/// </summary>
		/// <returns>The fatal.</returns>
		/// <param name="message">Message.</param>
		/// <param name="properties">Properties.</param>
		public virtual void Fatal(string message, params object[] properties)
		{
            Log(TraceEventType.Critical, message, null, properties);
		}

		/// <summary>
		/// Fatal the specified ex, message and properties.
		/// </summary>
		/// <returns>The fatal.</returns>
		/// <param name="ex">Ex.</param>
		/// <param name="message">Message.</param>
		/// <param name="properties">Properties.</param>
		public virtual void Fatal(Exception ex, string message, params object[] properties)
		{
            Log(TraceEventType.Critical, message, ex, properties);
		}

		/// <summary>
		/// Info the specified message and properties.
		/// </summary>
		/// <returns>The info.</returns>
		/// <param name="message">Message.</param>
		/// <param name="properties">Properties.</param>
		public virtual void Info(string message, params object[] properties)
		{
			Log(TraceEventType.Information, message, null, properties);
		}

		/// <summary>
		/// Info the specified ex, message and properties.
		/// </summary>
		/// <returns>The info.</returns>
		/// <param name="ex">Ex.</param>
		/// <param name="message">Message.</param>
		/// <param name="properties">Properties.</param>
		public virtual void Info(Exception ex, string message, params object[] properties)
		{
			Log(TraceEventType.Information, message, ex, properties);
		}

		/// <summary>
		/// Trace the specified message and properties.
		/// </summary>
		/// <returns>The trace.</returns>
		/// <param name="message">Message.</param>
		/// <param name="properties">Properties.</param>
		public virtual void Trace(string message, params object[] properties)
		{
            Log(TraceEventType.Verbose, message, null, properties);
		}

		/// <summary>
		/// Trace the specified ex, message and properties.
		/// </summary>
		/// <returns>The trace.</returns>
		/// <param name="ex">Ex.</param>
		/// <param name="message">Message.</param>
		/// <param name="properties">Properties.</param>
		public virtual void Trace(Exception ex, string message, params object[] properties)
		{
			Log(TraceEventType.Verbose, message, ex, properties);
		}

		/// <summary>
		/// Warn the specified message and properties.
		/// </summary>
		/// <returns>The warn.</returns>
		/// <param name="message">Message.</param>
		/// <param name="properties">Properties.</param>
		public virtual void Warn(string message, params object[] properties)
		{
            Log(TraceEventType.Warning, message, null, properties);
		}

		/// <summary>
		/// Warn the specified ex, message and properties.
		/// </summary>
		/// <returns>The warn.</returns>
		/// <param name="ex">Ex.</param>
		/// <param name="message">Message.</param>
		/// <param name="properties">Properties.</param>
		public virtual void Warn(Exception ex, string message, params object[] properties)
		{
            Log(TraceEventType.Warning, message, ex, properties);
		}

		/// <summary>
		/// Log the specified severity, message, ex and properties.
		/// </summary>
		/// <returns>The log.</returns>
		/// <param name="severity">Severity.</param>
		/// <param name="message">Message.</param>
		/// <param name="ex">Ex.</param>
		/// <param name="properties">Properties.</param>
		public abstract void Log(TraceEventType severity, string message, Exception ex = null, params object[] properties);
	}
}
