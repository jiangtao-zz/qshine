using qshine.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Threading;

namespace qshine
{
    /// <summary>
    /// Logger API.
    /// 	1. Get logger instance from .NET trace logger provider for system logging message.
    /// 	We have two type log provider:Plugin log provider and system log provider. 
    /// 	The system log provider is dotnet native tracing diagnostics logger. It is a default logger for system.
    /// 	When a plugin logger provider added in application environment, it will be loaded automatically and replace
    /// 	the default one.
    /// 	2. Get logger instance from a plug-in logger provider for application logging message
    /// 	3. Provide general logger instance for application logging message
    /// 	4. Provide debugging methods for DEBUG version only
    /// 	
    /// Before the application logger provider initialized, a .NET trace logger will be used for message logging.
    /// The framewrok code will use .NET trace logger to log message based on application dotnet diagnostics configure setting.
    /// To enable trace logger you need configure application diagnostics section for particular source.
    /// The defult source name is "General" for SysLogger
    /// 	var systemLogger = Log.TraceLoggerProvider.GetLogger(category);
    /// 	systemLogger.Error(ex);
    /// 	or Log.SysLogger.Error(ex);
    /// 
    /// The application use Log.GetLogger(category) to get a logger instance for specific category (source).
    /// 	var logger = Log.GetLogger("database");
    /// 	logger.Info("ExecuteSql({0})",sql);
    /// 
    /// Log methods are available only for developer to view the detail code information during DEBUG mode. 
    /// All those DevDebug() code will be removed in release mode.
    /// 	Log.DevDebug("Method begin")
    /// 	Log.DevDebug("Method end")
    /// 	
    /// The plugin log will take affect after plug-in logger loaded by application environment build process.
    /// </summary>
    public class Log:IStartupInitializer
	{
        #region static ctor for IStartupInitializer
        static Log()
        {
            var interceptor = Interceptor.Get<ApplicationEnvironment>();
            interceptor.OnSuccess += reloadLogProvider;
        }

        public void Start(string name)
        {

        }

        static void reloadLogProvider(object sender, InterceptorEventArgs args)
        {
            if (args.MethodName == "Init")
            {
                var provider = Configuration.ApplicationEnvironment.GetProvider<ILoggerProvider>();
                if (provider != null)
                {
                    LoggerProvider = provider;
                    SysLoggerProvider = provider;
                }
            }
        }
        #endregion

        #region fields

        static IDictionary<string, ILogger> _logInstances = new Dictionary<string, ILogger>();
		static object lockObject = new object();

		#endregion

		#region GetLogger

		/// <summary>
		/// Gets the logger for a specified category.
		/// </summary>
		/// <param name="category">logging category</param>
		/// <returns>logger</returns>
		public static ILogger GetLogger(string category)
		{
            
            //cache plugin logger instance
			if (!_logInstances.ContainsKey(category))
			{
				lock (lockObject)
				{
					if (!_logInstances.ContainsKey(category))
					{
						var logger = LoggerProvider.GetLogger(category);
						_logInstances.Add(category, logger);
					}
				}
			}
			var currentLogger = _logInstances[category];
			return currentLogger;
		}

		/// <summary>
		/// Gets the logger for a specified class type as category.
		/// </summary>
		/// <returns>The logger.</returns>
		/// <param name="type">Log category.</param>
		public static ILogger GetLogger(Type type)
		{
			return GetLogger(type.FullName);
		}

		/// <summary>
		/// Gets the logger for a specified class type as category.
		/// </summary>
		/// <returns>The logger.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static ILogger GetLogger<T>()
			where T : class
		{
			return GetLogger(typeof(T));
		}

		/// <summary>
		/// Gets the logger for general category.
		/// </summary>
		/// <returns>The logger.</returns>
		public static ILogger GetLogger()
		{
			const string generalCategory = "General";
			return GetLogger(generalCategory);
		}

		/// <summary>
		/// The current loger provider.
		/// </summary>
		static ILoggerProvider _logerProvider;
		/// <summary>
		/// The default logger provider used before plugin-logger providered.
		/// </summary>
		static ILoggerProvider LoggerProvider
		{
			get
			{
                if (_logerProvider == null)
                {
                    _logerProvider = SysLoggerProvider;
                }
				return _logerProvider;
			}
			set
			{
                if (_logerProvider != value)
                {
                    lock (lockObject)
                    {
                        if (_logerProvider != value)
                        {
                            _logerProvider = value;
                            _logInstances.Clear();
                        }
                    }
                }
			}
		}
		#endregion

		#region SysLogger
		/// <summary>
		/// Log system message
		/// </summary>

		static ILoggerProvider _systemLoggerProvider;
		/// <summary>
		/// Gets the system logger provider.
		/// The system logger provider use .NET diagnostics Trace component to write logging message
		/// </summary>
		/// <value>The system logger provider.</value>
		public static ILoggerProvider SysLoggerProvider
		{
			get
			{
				if (_systemLoggerProvider == null)
				{
					_systemLoggerProvider = new TraceLoggerProvider();
				}
				return _systemLoggerProvider;
			}
            set
            {
                _systemLoggerProvider = value;
                _syslogger = null;
                _devLogger = null;
            }
		}

		static ILogger _syslogger;
		public static ILogger SysLogger
		{
			get
			{
				if (_syslogger == null)
				{
					_syslogger = SysLoggerProvider.GetLogger("General");
				}
				return _syslogger;
			}
		}

		#endregion

		#region DevLogger

		static ILogger _devLogger;

		static ILogger DevLogger
		{
			get
			{
				if (_devLogger == null)
				{
					_devLogger = SysLoggerProvider.GetLogger("dev");
				}
				return _devLogger;
			}
		}
		/// <summary>
		/// Log message for developer only
		/// </summary>
		/// <param name="format">Format.</param>
		/// <param name="parameters">Parameters.</param>
		[Conditional("DEBUG")]
		public static void DevDebug(string format, params object[] parameters)
		{
			DevLogger.Debug(format,parameters);
		}

		#endregion

	}
}
