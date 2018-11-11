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
    public class ConsoleLoggerProvider : ILoggerProvider
    {
        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <returns>The logger.</returns>
        /// <param name="category">Category.</param>
        public ILogger GetLogger(string category)
        {
            return new ConsoleLogger(category);
        }
    }
    /// <summary>
    /// Trace logger.
    /// </summary>
    public class ConsoleLogger : LoggerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:qshine.TraceLogger"/> class.
        /// </summary>
        /// <param name="category">Category.</param>
        public ConsoleLogger(string category) : base(category) { }


        /// <summary>
        /// Gets a value indicating whether this <see cref="T:qshine.TraceLogger"/> is logging enabled.
        /// </summary>
        /// <value><c>true</c> if is logging enabled; otherwise, <c>false</c>.</value>
        public override bool IsLoggingEnabled
        {
            get
            {
                return true;
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
                return true;
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
                return true;
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
                return true;
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
                return true;
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
                return true;
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
                return true;
            }
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
            var formattedMessage = string.Format("{0}", string.IsNullOrEmpty(message) ? string.Empty :
                string.Format(message, properties));
            if (ex != null)
            {
                formattedMessage += "\r\n" + string.Format("Detail:{0}", ex);
                formattedMessage += "\r\n";
            }

            bool isColorChanged = false;
            if(severity== TraceEventType.Critical || severity == TraceEventType.Error)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.White;
                isColorChanged = true;
            }
            else if (severity == TraceEventType.Warning)
            {
                Console.BackgroundColor = ConsoleColor.Yellow;
                Console.ForegroundColor = ConsoleColor.White;
                isColorChanged = true;
            }
            Console.WriteLine(formattedMessage);
            if (isColorChanged)
            {
                Console.ResetColor();
            }
        }

    }
}
