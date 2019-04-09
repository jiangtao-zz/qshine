using qshine.Utility;
using System;
using System.Diagnostics;

namespace qshine.Logger
{
    /// <summary>
    /// Logger API.
    /// 	1. Get logger instance from a logger provider. A logger provider factory can produce a logger provider using different strategy. 
    /// 	The logger provider could be a plugin component from a logging framework, such as nlog, log4net.
    /// 	This library implemented two internal logging providers : .NET trace logger provider and console logger provider. 
    /// 	.NET trace logger using dotnet native tracing diagnostics function to create message log based on application config. This is the default
    /// 	logger provider if no plugin logging component found.
    /// 	Console logger is simply output the logger message to console.
    /// 	The plugin logging provider can be addon through application environment configure setting.
    /// 	2. A logger provider factory choose a logger provider based on logging category setting in configured logging component Map.
    /// 	A default logging provider component could be defined in Map setting section. The Map name is "qshine.ILoggerProvider".
    /// 	User can create its own Logger provider factory to overwrite LoggerproviderFactory.
    /// 	3. A SysLogger instance is a "System" category logger instance used by library.
    /// 	4. A DevDebug() is a debugging methods for DEBUG version only. Use it if you don't want logging message affect the production version.
    /// 	5. RegisterLogger() can be used to overwrite category specific logger created by logger provider factory.
    /// 	
    /// The library will use .NET trace logger before Logger plugin added into system.
    /// To enable trace logger you need configure application diagnostics section in app configure setting.
    /// 
    /// 	Log.SysLogger.Error(ex);
    /// 
    /// The application use Log.GetLogger(category) to get a logger instance for specific category (source).
    /// 	var logger = Log.GetLogger("database");
    /// 	logger.Info("ExecuteSql({0})",sql);
    /// 
    /// DevDebug methods are available only for developer to view the detail information during DEBUG mode. 
    /// 	Log.DevDebug("Method begin")
    /// 	Log.DevDebug("Method end")
    /// 	
    /// The plugin logger takes affect after application environment build process completed.
    /// </summary>
    public class Log
	{
        //#region static ctor for IStartupInitializer

        //static Log()
        //{
        //    //intercept Log
        //    var interceptor = Interceptor.Get<ApplicationEnvironment>();
        //    interceptor.OnSuccess += reloadLogProvider;
        //}

        ///// <summary>
        ///// Application start.
        ///// </summary>
        ///// <param name="name"></param>
        //public void Start(string name) {}

        //static void reloadLogProvider(object sender, InterceptorEventArgs args)
        //{
        //    if (args.MethodName == "Init")
        //    {
        //        //find default provider
        //        var provider = ApplicationEnvironment.Current.GetMappedProvider<ILoggerProvider>();
        //        if (provider != null)
        //        {
        //            LoggerProvider = provider;
        //            SysLoggerProvider = provider;
        //        }


        //        //var providers = ApplicationEnvironment.Current.GetProviders<ILoggerProvider>();
        //        //if (providers != null && providers.Count>0)
        //        //{
        //        //    string defaultProvider = "";
        //        //    var maps = ApplicationEnvironment.Configure.Maps;
        //        //    if (maps != null)
        //        //    {
        //        //        var mapName = typeof(ILoggerProvider).FullName;
        //        //        if (maps.ContainsKey(mapName))
        //        //        {
        //        //            var logMaps = maps[mapName];
        //        //            defaultProvider = logMaps.Default;
        //        //            foreach(var categoryMap in logMaps.Items)
        //        //            {
        //        //                var loggingCategory = categoryMap.Key;
        //        //                var loggingProviderName = categoryMap.Value;

        //        //                if (categoryMap.Value)
        //        //            }
        //        //        }
        //        //        LoggerProvider = provider;
        //        //        SysLoggerProvider = provider;
        //        //    }
        //        //}
        //    }
        //}
        //#endregion

        #region fields

        /// <summary>
        /// store Log provider per logging category.
        /// </summary>
        static SafeDictionary<string, ILogger> _logInstances = new SafeDictionary<string, ILogger>();
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
                        var provider = LogProviderFactory.CreateProvider(category);
                        if (provider == null)
                        {
                            //temporary return system logger.
                            return SysLogger;
                        }

                        var logger = provider.GetLogger(category);
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
			return GetLogger(LogCategory.General.ToString());
		}


        static ILoggerProviderFactory _logProviderFactory = null;
        /// <summary>
        /// Get/Set Logging provider factory
        /// </summary>
        public static ILoggerProviderFactory LogProviderFactory
        {
            get
            {
                if (_logProviderFactory == null) _logProviderFactory = new LoggerProviderFactory();

                return _logProviderFactory;
            }
            set
            {
                _logProviderFactory = value;
                _logInstances.Clear();
            }
        }

        /// <summary>
        /// Register a specific Logger instead Logger instance created by LogProviderFactory().
        /// 
        /// This method is best for custom Log provider
        /// </summary>
        /// <param name="category">Log category</param>
        /// <param name="logger">Logger instance</param>
        public static void RegisterLogger(string category, ILogger logger)
        {
            lock (lockObject)
            {
                //cache plugin logger instance
                if (_logInstances.ContainsKey(category))
                {
                    _logInstances[category] = logger;
                }
                else
                {
                    _logInstances.Add(category, logger);
                }
            }
        }

        ///// <summary>
        ///// The default loger provider.
        ///// </summary>
        //static ILoggerProvider _logerProvider;
        ///// <summary>
        ///// The default logger provider used before plugin-logger providered.
        ///// </summary>
        //static ILoggerProvider LoggerProvider
        //{
        //	get
        //	{
        //              if (_logerProvider == null)
        //              {
        //                  _logerProvider = SysLoggerProvider;
        //              }
        //		return _logerProvider;
        //	}
        //	set
        //	{
        //              if (_logerProvider != value)
        //              {
        //                  lock (lockObject)
        //                  {
        //                      if (_logerProvider != value)
        //                      {
        //                          _logerProvider = value;
        //                          _logInstances.Clear();
        //                      }
        //                  }
        //              }
        //	}
        //}

        #endregion

        #region SysLogger

        static ILoggerProvider _sysLoggerProvider = new TraceLoggerProvider();

        /// <summary>
        /// Get/set system logger provider.
        /// The system logger provider is used to log library system message
        /// </summary>
        public static ILoggerProvider SysLoggerProvider {
            get
            {
                return _sysLoggerProvider;
            }

            set
            {
                _sysLoggerProvider = value;
            }
        }

		/// <summary>
		/// Log system message
		/// </summary>

        /// <summary>
        /// Get system logger instance.
        /// System logger instance
        /// </summary>
		public static ILogger SysLogger
		{
			get
			{
                return SysLoggerProvider.GetLogger(LogCategory.System.ToString());
			}
		}

		#endregion

		#region DevLogger

		static ILogger DevLogger
		{
			get
			{
                return GetLogger(LogCategory.General.ToString());
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

    /// <summary>
    /// Common logging categories
    /// </summary>
    public enum LogCategory
    {
        /// <summary>
        /// System category
        /// </summary>
        System = 1000,
        /// <summary>
        /// General logging
        /// </summary>
        General,
        /// <summary>
        /// Database logging
        /// </summary>
        Database,
        /// <summary>
        /// Security logging
        /// </summary>
        Security,
        /// <summary>
        /// Server logging
        /// </summary>
        Server,
        /// <summary>
        /// Network logging
        /// </summary>
        Network,

        /// <summary>
        /// Debug Logging only
        /// </summary>
        Dev = 10000,


    }
}
