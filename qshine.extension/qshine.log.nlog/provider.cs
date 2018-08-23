﻿using System;
using System.Diagnostics;
using qshine.Configuration;
namespace qshine.log.nlog
{
	public class Provider:ILoggerProvider
	{
		static Provider()
		{
			//Find nlog.config file from first level config folder.
			const string nlogConfigFileName = "nlog.config";

			var filePath = EnvironmentManager.GetConfigFilePathIfAny(nlogConfigFileName);
			if(!string.IsNullOrEmpty(filePath)){
				NLog.LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration(filePath);
			}
		}

		public ILogger GetLogger(string category)
		{
			return new Logger(category);
		}
	}

	public class Logger : LoggerBase
	{
		NLog.ILogger _logger;
		public Logger(string category)
			:base(category)
		{
			_logger = NLog.LogManager.GetLogger(category);
		}

		public override bool IsDebugEnabled
		{
			get
			{
				return _logger.IsDebugEnabled;
			}
		}

		public override bool IsErrorEnabled
		{
			get
			{
				return _logger.IsErrorEnabled;
			}
		}

		public override bool IsFatalEnabled
		{
			get
			{
				return _logger.IsFatalEnabled;
			}
		}

		public override bool IsInfoEnabled
		{
			get
			{
				return _logger.IsInfoEnabled;
			}
		}

		public override bool IsTraceEnabled
		{
			get
			{
				return _logger.IsTraceEnabled;
			}
		}

		public override bool IsWarnEnabled
		{
			get
			{
				return _logger.IsWarnEnabled;
			}
		}

		public override void Debug(string message, params object[] properties)
		{
			_logger.Debug(message, properties);
		}

		public override void Debug(Exception ex, string message, params object[] properties)
		{
			_logger.Debug(ex, message, properties);
		}

		public override void Trace(string message, params object[] properties)
		{
			_logger.Trace(message, properties);
		}

		public override void Trace(Exception ex, string message, params object[] properties)
		{
			_logger.Trace(ex, message, properties);
		}

		public override void Log(TraceEventType severity, string message, Exception ex = null, params object[] properties)
		{
			switch(severity)
			{
				case TraceEventType.Critical:
					if (ex == null)
					{
						_logger.Fatal(message, properties);
					}
					else
					{
						_logger.Fatal(ex, message, properties);
					}
					break;
				case TraceEventType.Error:
					if (ex == null)
					{
						_logger.Error(message, properties);
					}
					else
					{
						_logger.Error(ex, message, properties);
					}
					break;
				case TraceEventType.Warning:
					if (ex == null)
					{
						_logger.Warn(message, properties);
					}
					else
					{
						_logger.Warn(ex, message, properties);
					}
					break;
				case TraceEventType.Information:
					if (ex == null)
					{
						_logger.Info(message, properties);
					}
					else
					{
						_logger.Info(ex, message, properties);
					}
					break;
			}
		}
	}
}
