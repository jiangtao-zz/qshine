using System;
using System.Diagnostics;

namespace qshine
{
	/// <summary>
	/// Trace logger provider.
	/// The Trace Logger use Debug/Trace to write logging message based on .NET diagnostics configure section setting.
	/// The configure setting source and switch will be used to determine whether trace logging enabled.
	/// The listener must setup in trace section instead of source section. 
	/// </summary>
	public class TraceLoggerProvider : ILoggerProvider
	{
		/// <summary>
		/// Gets the logger.
		/// </summary>
		/// <returns>The logger.</returns>
		/// <param name="category">Category.</param>
		public ILogger GetLogger(string category)
		{
			return new TraceLogger(category);
		}
	}
	/// <summary>
	/// Trace logger.
	/// </summary>
	public class TraceLogger : LoggerBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="T:qshine.TraceLogger"/> class.
		/// </summary>
		/// <param name="category">Category.</param>
		public TraceLogger(string category) : base(category) { }

		TraceSource _source;

		TraceSource TraceSource
		{
			get
			{
				if (_source == null)
				{
					_source = new TraceSource(Category);
				}
				return _source;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="T:qshine.TraceLogger"/> is fatal enabled.
		/// </summary>
		/// <value><c>true</c> if is fatal enabled; otherwise, <c>false</c>.</value>
		public override bool IsFatalEnabled
		{
			get
			{
				return base.IsFatalEnabled ||
                    (TraceSource.Switch != null && TraceSource.Switch.ShouldTrace(TraceEventType.Critical));
			}
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="T:qshine.TraceLogger"/> is error enabled.
		/// </summary>
		/// <value><c>true</c> if is error enabled; otherwise, <c>false</c>.</value>
		public override bool IsErrorEnabled
		{
			get
			{
				return base.IsErrorEnabled ||
                    (TraceSource.Switch != null && TraceSource.Switch.ShouldTrace(TraceEventType.Error));
			}
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="T:qshine.TraceLogger"/> is warn enabled.
		/// </summary>
		/// <value><c>true</c> if is warn enabled; otherwise, <c>false</c>.</value>
		public override bool IsWarnEnabled
		{
			get
			{
				return base.IsWarnEnabled ||
                    (TraceSource.Switch != null && TraceSource.Switch.ShouldTrace(TraceEventType.Warning));
			}
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="T:qshine.TraceLogger"/> is info enabled.
		/// </summary>
		/// <value><c>true</c> if is info enabled; otherwise, <c>false</c>.</value>
		public override bool IsInfoEnabled
		{
			get
			{
				return base.IsInfoEnabled ||
                    (TraceSource.Switch != null && TraceSource.Switch.ShouldTrace(TraceEventType.Information));
			}
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="T:qshine.TraceLogger"/> is trace enabled.
		/// </summary>
		/// <value><c>true</c> if is trace enabled; otherwise, <c>false</c>.</value>
		public override bool IsTraceEnabled
		{
			get
			{
				return base.IsInfoEnabled ||
                    (TraceSource.Switch != null && TraceSource.Switch.ShouldTrace(TraceEventType.Verbose));
			}
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="T:qshine.TraceLogger"/> is debug enabled.
		/// </summary>
		/// <value><c>true</c> if is debug enabled; otherwise, <c>false</c>.</value>
		public override bool IsDebugEnabled
		{
			get
			{
				return base.IsDebugEnabled ||
                    (TraceSource.Switch != null && TraceSource.Switch.ShouldTrace(TraceEventType.Verbose));
			}
		}

        bool LoggingEnabled(TraceEventType severity)
        {
            return
                (severity == TraceEventType.Critical && IsFatalEnabled) ||
                (severity == TraceEventType.Error && IsErrorEnabled) ||
                (severity == TraceEventType.Warning && IsWarnEnabled) ||
                (severity == TraceEventType.Information && IsInfoEnabled) ||
                (severity == TraceEventType.Verbose && (IsDebugEnabled || IsTraceEnabled))
                ;
        }

        /// <summary>
        /// Log the specified severity, message, ex and properties.
        /// </summary>
        /// <returns>The log.</returns>
        /// <param name="severity">Severity.</param>
        /// <param name="message">Message.</param>
        /// <param name="ex">Ex.</param>
        /// <param name="properties">Properties.</param>
        public override void Log(TraceEventType severity, string message, Exception ex = null, params object[] properties)
		{
			if (LoggingEnabled(severity))
			{
				var formattedMessage = string.Format("{0}", string.IsNullOrEmpty(message) ? string.Empty :
					string.Format(message, properties));
				if (ex != null)
				{
					formattedMessage += "\r\n" + string.Format("Detail:{0}", ex);
					formattedMessage += "\r\n";
				}
#if DEBUG
				System.Diagnostics.Debug.WriteLine(formattedMessage);
#else
				System.Diagnostics.Trace.WriteLine(formattedMessage);

//				_source.TraceEvent(severity, 0, formattedMessage);
#endif
                if (_source != null)
                {
                    _source.Close();
                }
			}
		}

	}
}
