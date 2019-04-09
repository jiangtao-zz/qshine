using System;
using System.Text;
using qshine.Logger;

namespace qshine.LogInterceptor
{
	/// <summary>
	/// Implement logging for DbClient methods which uses method inspector
	/// </summary>
	public class DbClientLog
		:IInterceptorHandler
	{
        /// <summary>
        /// Load all events
        /// </summary>
		public void LoadInterceptorHandler()
		{
			var interceptor = Interceptor.Get<DbClient>();
			if (interceptor != null)
			{
				interceptor.OnEnter += MethodEnter;
				interceptor.OnExit += MethodExit;
				interceptor.OnException += MethodException;
			}
		}
		/// <summary>
		///Add logging when method begin
		/// </summary>
		/// <param name="sender">Sender. dbhelper instance</param>
		/// <param name="args">Arguments.</param>
		void MethodEnter(object sender, InterceptorEventArgs args)
		{
			args.DataBag.Add("time",DateTime.Now);

			Log.DevDebug("{0} begin", args.MethodName);
		}

		/// <summary>
		///Add logging when method end
		/// </summary>
		/// <param name="sender">Sender. dbhelper instance</param>
		/// <param name="args">Arguments.</param>
		void MethodExit(object sender, InterceptorEventArgs args)
		{
			DbParameters parameters = null;
			
			var msgBuilder = new StringBuilder(args.MethodName);
			msgBuilder.Append("(");
			for (int i = 0; i < args.Args.Count;i++)
			{
				if (i != 0)
				{
					msgBuilder.Append(",");
				}
				msgBuilder.Append(args.Args[i]);

				parameters = args.Args[i] as qshine.DbParameters;

			}
			DateTime entryTime = args.DataBag.ContainsKey("time") ? (DateTime)args.DataBag["time"] : DateTime.Now;
			var timespan = DateTime.Now - entryTime;
			msgBuilder.AppendFormat(") \nExecution time {0} ms",timespan.TotalMilliseconds);
			msgBuilder.AppendLine();

			if (parameters != null && parameters.Params != null)
			{
				msgBuilder.AppendLine("qshine.DbParameters");
				foreach (var p in parameters.Params)
				{
					msgBuilder.AppendFormat("{0}:{1}({2}) = {3}\n", p.Direction, p.ParameterName, p.DbType, p.Value);
				}
			}


			if (args.Exception == null)
			{
				msgBuilder.AppendFormat("Result: {0}", args.Result);
			}
			else
			{
				msgBuilder.AppendFormat("Exception: {0}", args.Exception.Message);
			}
			Log.DevDebug(msgBuilder.ToString());
			Log.DevDebug("{0} end", args.MethodName);
		}

		/// <summary>
		/// Log DbHelper exception
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="args">Arguments.</param>
		void MethodException(object sender, InterceptorEventArgs args)
		{
			Logger.Error(args.Exception,"{0} exception.", args.MethodName);
		}

		ILogger _logger;
		ILogger Logger
		{
			get
			{
				if (_logger == null)
				{
					_logger = Log.GetLogger();
				}
				return _logger;
			}
		}
	}
}
