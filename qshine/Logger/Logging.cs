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
    /// High level static logger API.
	/// 	1. Get logger instance from .NET trace logger provider for system logging message
	/// 	2. Get logger instance from a plug-in logger provider for application logging message
	/// 	3. Provide general logger instance for application logging message
	/// 	4. Provide debugging methods for DEBUG version only
	/// 	
	/// Before the application logger provider initialized, a .NET trace logger will be used for message logging.
	/// The framewrok code will use .NET trace logger to log message based on application configure setting.
	/// To enable trace logger you need configure application diagnostics section for particular source.
	/// The source name usually is the method name or a category name.
	/// 	var systemLogger = Logger.TraceLoggerProvider.GetLogger(category);
	/// 	systemLogger.Error(ex);
	/// 
	/// The application use Logger.GetLogger(category) to get a logger instance for specific category (source).
	/// 	var logger = Logger.GetLogger("database");
	/// 	logger.Info("ExecuteSql({0})",sql);
	/// 
	/// You also can call static Logger methods to write log message in general category.
	/// 	Logger.Error(ex, "Failed to call method ABC()");
	/// 
	/// 
	/// A thread level logger can be find in Logger.Current. It will use current Logger instance for message logging. 
	/// 
	/// 
    /// </summary>
    public class Logger
    {
        #region fields

		static IDictionary<string, ILogger> _logInstances = new Dictionary<string, ILogger>();
		static object lockObject = new object();

		#endregion

		#region GetLogger

		//[ThreadStatic]
		static ILogger _currentLogger;
		/// <summary>
		/// Gets the logger for a specified category.
        /// </summary>
        /// <param name="category">logging category</param>
        /// <returns>logger</returns>
        public static ILogger GetLogger(string category)
        {
			if (!_logInstances.ContainsKey(category))
			{
				lock(lockObject)
				{
					if (!_logInstances.ContainsKey(category))
					{
						var logger = LoggerProvider.GetLogger(category);
						_logInstances.Add(category, logger);
					}
				}
			}
            _currentLogger = _logInstances[category];
			return _currentLogger;
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
			where T:class
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
				if (_logerProvider == null || _logerProvider == TraceLoggerProvider)
				{
					_logerProvider = Configuration.EnvironmentManager.GetProvider<ILoggerProvider>()
					                              ??TraceLoggerProvider;
				}
				return _logerProvider;
			}
			set
			{
				_logerProvider = value;
			}
		}

		static ILoggerProvider _systemLoggerProvider = new TraceLoggerProvider();
		/// <summary>
		/// Gets the system logger provider.
		/// The system logger provider use .NET diagnostics Trace component to write logging message
		/// </summary>
		/// <value>The system logger provider.</value>
		public static ILoggerProvider TraceLoggerProvider
		{
			get
			{
				if (_systemLoggerProvider == null)
				{
					_systemLoggerProvider = new TraceLoggerProvider();
				}
				return _systemLoggerProvider;
			}
		}

		static ILogger _logger;
		static ILogger GeneralLogger
		{
			get
			{
				if (_logger == null)
				{
					_logger = GetLogger();
				}
				return _logger;
			}
		}

		#endregion

        #region status
        /// <summary>
        /// Check logger enabled or disabled
        /// </summary>
        public static bool IsLoggingEnabled
        {
            get
            {
                return GeneralLogger.IsLoggingEnabled;
            }
        }
        #endregion

        #region Fatal
        /// <summary>
        /// Logging critical message and additional properties.
        /// </summary>
        /// <param name="message">A primary message string to be logged.</param>
        /// <param name="properties">Additional collection of properties to be logged. If the property type is an Exception, the call stack will be logged</param>
		public static void Fatal(string message, params object[] properties)
        {
            GeneralLogger.Fatal(message, properties);
        }

        /// <summary>
        /// Logging critical exception and additional properties.
        /// </summary>
        /// <param name="message">An exception object to be logged</param>
        /// <param name="ex"></param>
        /// <param name="properties">Additional collection of properties to be logged.</param>
        public static void Fatal(Exception ex, string message = null, params object[] properties)
        {
            GeneralLogger.Fatal(ex, message, properties);
        }
        #endregion

        #region Error
		/// <summary>
		/// Log the specified <c>Error</c> level message and properties.
		/// </summary>
		/// <returns>The error.</returns>
		/// <param name="message">Message.</param>
		/// <param name="properties">Properties.</param>
		public static void Error(string message, params object[] properties)
		{
			GeneralLogger.Error(message, properties);
		}

		/// <summary>
		/// Log the specified <c>Error</c> level ex, message and properties.
		/// </summary>
		/// <returns>The error.</returns>
		/// <param name="ex">Ex.</param>
		/// <param name="message">Message.</param>
		/// <param name="properties">Properties.</param>
		public static void Error(Exception ex, string message = null, params object[] properties)
		{
            GeneralLogger.Error(ex, message, properties);
		}

        #endregion

        #region Warn
		/// <summary>
		/// Log the specified <c>Warn</c> level message and properties.
		/// </summary>
		/// <returns>The error.</returns>
		/// <param name="message">Message.</param>
		/// <param name="properties">Properties.</param>
		public static void Warn(string message, params object[] properties)
		{
			GeneralLogger.Warn(message, properties);
		}

		/// <summary>
		/// Log the specified <c>Warn</c> level ex, message and properties.
		/// </summary>
		/// <returns>The error.</returns>
		/// <param name="ex">Ex.</param>
		/// <param name="message">Message.</param>
		/// <param name="properties">Properties.</param>
		public static void Warn(Exception ex, string message = null, params object[] properties)
		{
            GeneralLogger.Warn(ex, message, properties);
		}
        #endregion

        #region Info
		/// <summary>
		/// Log the specified <c>Info</c> level message and properties.
		/// </summary>
		/// <returns>The error.</returns>
		/// <param name="message">Message.</param>
		/// <param name="properties">Properties.</param>
		public static void Info(string message, params object[] properties)
		{
			GeneralLogger.Info(message, properties);
		}

		/// <summary>
		/// Log the specified <c>Info</c> level ex, message and properties.
		/// </summary>
		/// <returns>The error.</returns>
		/// <param name="ex">Ex.</param>
		/// <param name="message">Message.</param>
		/// <param name="properties">Properties.</param>
		public static void Info(Exception ex, string message = null, params object[] properties)
		{
            GeneralLogger.Info(ex, message, properties);
		}
        #endregion

        #region Debug
		/// <summary>
		/// Log the specified <c>Debug</c> level message and properties.
		/// </summary>
		/// <returns>The error.</returns>
		/// <param name="message">Message.</param>
		/// <param name="properties">Properties.</param>
		public static void Debug(string message, params object[] properties)
		{
			GeneralLogger.Debug(message, properties);
		}

		/// <summary>
		/// Log the specified <c>Debug</c> level ex, message and properties.
		/// </summary>
		/// <returns>The error.</returns>
		/// <param name="ex">Ex.</param>
		/// <param name="message">Message.</param>
		/// <param name="properties">Properties.</param>
		public static void Debug(Exception ex, string message = null, params object[] properties)
		{
            GeneralLogger.Debug(ex, message, properties);
		}
        #endregion

		#region Trace
		/// <summary>
		/// Log the specified <c>Trace</c> level message and properties.
		/// </summary>
		/// <returns>The error.</returns>
		/// <param name="message">Message.</param>
		/// <param name="properties">Properties.</param>
		public static void Trace(string message, params object[] properties)
		{
			GeneralLogger.Trace(message, properties);
		}

		/// <summary>
		/// Log the specified <c>Debug</c> level ex, message and properties.
		/// </summary>
		/// <returns>The error.</returns>
		/// <param name="ex">Ex.</param>
		/// <param name="message">Message.</param>
		/// <param name="properties">Properties.</param>
		public static void Trace(Exception ex, string message = null, params object[] properties)
		{
            GeneralLogger.Trace(ex, message, properties);
		}
		#endregion

        #region utility
        /// <summary>
        /// Get exception call stack information
        /// </summary>
        /// <param name="ex">Exception object</param>
        /// <returns>Call stack of the exception</returns>
        public static string GetExceptionCallStack(Exception ex)
        {
            return ex.ToString();
        }

        /// <summary>
        /// Get object detail properties information
        /// </summary>
        /// <param name="value">object to be inspected</param>
        /// <returns>Detail object information.</returns>
        public static string GetPropertyDetail(object value)
        {
            return ObjectInspector.OutputValues(value);
        }
        #endregion

    }

    /// <summary>
    /// Inspect object properties value
    /// </summary>
    internal class ObjectInspector
    {
        const char indent = ' ';//'\t'

        private ObjectInspector() { }

        /// <summary>
        /// inspect and format class property detail information
        /// </summary>
        /// <param name="obj">object to be inspected</param>
        /// <returns>Formatted detail object property information</returns>
        public static string OutputValues(object obj)
        {
            return FormattingObject(obj, 0);
        }
        #region private
        private static void FormattingArray(object obj, StringBuilder sb, int level)
        {
            var po = obj as Array;
            for (var i = 0; i < po.Length; i++)
            {
                var pobj = po.GetValue(i);
                sb.Append(indent, level + 1);
                FormatingArrayValue(pobj, i, null, sb, level);
            }
        }

        private static void FormattingDictionary(object obj, StringBuilder sb, int level)
        {
            var dictionary = obj as IDictionary;
            var count = dictionary.Count;
            var myObject = new object[count];
            var key = new object[count];
            if (count > 0)
            {
                var i = 0;
                foreach (var x in dictionary.Keys)
                {
                    key[i] = x;
                    i++;
                }

                i = 0;
                foreach (var x in dictionary.Values)
                {
                    myObject[i] = x;
                    i++;
                }

                for (i = 0; i < count; i++)
                {
                    sb.Append(indent, level + 1);
                    FormatingArrayValue(myObject[i], i, key[i], sb, level);
                }
            }
        }

        private static void FormattingList(object obj, StringBuilder sb, int level)
        {
            var list = obj as IList;

            //formatting IList
            var i = 0;
            var count = list.Count;
            if (count > 0)
            {
                foreach (var x in list)
                {
                    sb.Append(indent, level + 1);
                    FormatingArrayValue(x, i, null, sb, level);
                    i++;
                }
            }
        }

        private static void FormattingSimpleObject(object obj, StringBuilder sb, int level)
        {
            sb.Append(indent, level + 1);
            sb.AppendLine(GetSimpleObjectValue(obj));
        }

        private static string FormattingObject(object obj, int level)
        {
            const int MAXIMUM_LEVEL = 10;

            var tp = obj.GetType();
            var sb = new StringBuilder();

            //output property type name
            if (level == 0)
            {
                sb.Append(tp.Name);
            }
            //Set maximum level to 10 to avoid unexpected overflow just in case.
            else if (level > MAXIMUM_LEVEL)
            {
                return "...";
            }

            sb.AppendLine(" {");

            //Exception
            var exceptionObject = obj as Exception;
            if (exceptionObject != null)
            {
                sb.Append(Logger.GetExceptionCallStack(exceptionObject));
            }
            //formatting array
            else if (tp.IsArray)
            {
                FormattingArray(obj, sb, level);
            }
            //formatting generic
            else if (tp.IsGenericType && IsSystemType(tp))
            {
                //IDictionary
                if (obj is IDictionary)
                {
                    FormattingDictionary(obj, sb, level);
                }
                //IList
                else if (obj is IList)
                {
                    FormattingList(obj, sb, level);
                }
                //Other unknown structure
                else
                {
                    FormattingSimpleObject(obj, sb, level);
                }
            }
            //formatting class
            else if (tp.IsClass)
            {
                //formating system type
                if (IsSystemType(tp))
                {
                    FormattingSimpleObject(obj, sb, level);
                }
                //formatting user class
                else
                {
                    //formatting all properties
                    foreach (var pinfo in tp.GetProperties())
                    {
                        //only formatting readable properties
                        if (pinfo.CanRead)
                        {
                            FormatObjectValue(pinfo.GetValue(obj, null), pinfo.Name, pinfo.PropertyType, sb, level);
                        }
                    }
                    //formatting all fields
                    foreach (var finfo in tp.GetFields())
                    {
                        //only for public fields
                        if (finfo.IsPublic)
                        {
                            FormatObjectValue(finfo.GetValue(obj), finfo.Name, finfo.FieldType, sb, level);
                        }
                    }
                }
            }
            else
            {
                FormattingSimpleObject(obj, sb, level);
            }
            sb.Append(indent, level + 1);
            sb.AppendLine("}");
            return sb.ToString();
        }

        private static void FormatingArrayValue(object obj, int i, object key, StringBuilder sb, int level)
        {
            if (obj == null)
            {
                sb.AppendLine(string.Format(CultureInfo.CurrentCulture, "[{0}]=null", i));
            }
            else
            {
                if (key != null)
                {
                    sb.Append(string.Format(CultureInfo.CurrentCulture, "{0}[{1}:{2}]={3}", obj.GetType().Name, i, key, FormattingObject(obj, level + 1)));
                }
                else
                {
                    sb.Append(string.Format(CultureInfo.CurrentCulture, "{0}[{1}]={2}", obj.GetType().Name, i, FormattingObject(obj, level + 1)));
                }
            }
        }


        private static void FormatObjectValue(object obj, string name, Type tp, StringBuilder sb, int level)
        {
            sb.Append(indent, level + 1);
            sb.Append(tp.Name + " ");
            sb.Append(name + "=");
            if (tp.IsArray || tp.IsGenericType || !IsSystemType(tp))
            {
                sb.Append(FormattingObject(obj, level + 1));
            }
            else
            {
                sb.Append(GetSimpleObjectValue(obj));
                sb.AppendLine(";");
            }
        }

        private static string GetSimpleObjectValue(object obj)
        {
            if (obj == null)
            {
                return "null";
            }

            var name = obj.GetType().Name;
            switch (name)
            {
                case "String":
                    return string.Format(CultureInfo.CurrentCulture, "\"{0}\"", obj);
                case "DateTime":
                case "DateSpan":
                    return string.Format(CultureInfo.CurrentCulture, "#{0}#", obj);
                default:
                    return obj.ToString();
            }
        }

        private static bool IsSystemType(Type t)
        {
            return t.FullName.StartsWith("System", StringComparison.Ordinal);
        }
        #endregion
    }

}
