using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
namespace qshine
{
	/// <summary>
	/// Database client class
	/// </summary>
	public class DbClient:IDisposable
	{
		////Indicates the connection is managed by other component
		////private bool _isUnmanagedConnection;
		//Current database data reader
		IDataReader _currentDataReader;

		Database _database;

		/// <summary>
		/// The interceptor must be a static instance (singleton).
		/// </summary>
		static Interceptor _interceptor =Interceptor.Register(typeof(DbClient));

		/// <summary>
		/// Initializes a new instance of the <see cref="T:qshine.DbClient"/> class for current database context.
		/// Current database context can be set by external.
		/// The default current context is configured by environment configuration default database connection string.
		/// </summary>
		public DbClient()
		{
			_database = new Database();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:qshine.DbClient"/> class for a given database instance.
		/// The database instance will be added in current database context.
		/// </summary>
		/// <param name="database">Database instance.</param>
		public DbClient(Database database)
		{
			_database = database;
		}

		public DbClient(string providerName, string connectionString)
		{
			_database = new Database(providerName, connectionString);
		}

		#region Implementation of IDsiposable interface
		/// <summary>
		/// Dispose
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private bool _disposed;
		private void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					CloseConnection();
					_disposed = true;
				}
			}
		}

		#endregion

		DbContext _context;
		public DbContext Context
		{
			get
			{
				if (_context == null)
				{
					_context = new DbContext(_database);
				}
				return _context;
			}
		}

		/// <summary>
		/// Get current database connection object
		/// </summary>
		IDbConnection Connection
		{
			get
			{
				return Context.Connection;
			}
		}

		/// <summary>
		/// Get an opened database connection
		/// </summary>
		IDbConnection ActiveConnection
		{
			get
			{
				return Context.ActiveConnection;
			}
		}

		void CloseConnection()
		{
			Context.CloseConnection();
		}

		/// <summary>
		/// Execute Sql statement or a stored procedure
		/// </summary>
		/// <param name="commandType">A CommandType object to indicate a Sql command or a storedprocedure command</param>
		/// <param name="commandString">SQL statement or stored procedure</param>
		/// <param name="parameters">input parameters and output parameters</param>
		public int ExecuteNonQuery(CommandType commandType, string commandString, DbParameters parameters=null)
		{
			return _interceptor.JoinPoint<int>(() =>
			{
				var result = 0;
				using (var command = ActiveConnection.CreateCommand())
				{
					command.CommandType = commandType;
					command.CommandText = commandString;
					if (parameters != null && parameters.Params != null && parameters.Params.Count > 0)
					{
						AddCommandParameters(command, parameters.Params);
					}
					result = command.ExecuteNonQuery();
					if (parameters != null && parameters.Params != null && parameters.Params.Count > 0)
					{
						RetrieveCommandParameterValues(command, parameters.Params);
					}
				}
				return result;
			}, this, "ExecuteNonQuery",commandType,commandString,parameters);
		}

		/// <summary>
		/// Execute Sql statement or a stored procedure
		/// 
		/// ExecuteScalar(CommandType.Text,"select 1 from tb1 where name=:p1 and age=:p2", DbParameters.New.Input("p1",name).Input("p2",age).Output<int>("p3"))
		/// </summary>
		/// <param name="commandType">A CommandType object to indicate a Sql command or a storedprocedure command</param>
		/// <param name="commandString">SQL statement or stored procedure</param>
		/// <param name="parameters">input parameters and output parameters</param>
		/// <returns>Return first selected value from query</returns>
		public object ExecuteScalar(CommandType commandType, string commandString, DbParameters parameters=null)
		{
			return _interceptor.JoinPoint<object>(() =>
			 {
				 object result = null;
				 using (var command = ActiveConnection.CreateCommand())
				 {
					 command.CommandType = commandType;
					 command.CommandText = commandString;
					 AddCommandParameters(command, parameters.Params);
					 result = command.ExecuteScalar();
					RetrieveCommandParameterValues(command, parameters.Params);
				 }
				 return result;
			 }, this, "ExecuteScalar", commandType, commandString, parameters);
		}


		/// <summary>
		/// Execute a SQL statement or StoredProcedure and retrieve batch data from IDataReader
		/// </summary>
		/// <param name="commandType">A CommandType object to indicate a Sql command or a storedprocedure command</param>
		/// <param name="commandString">SQL statement or stored procedure</param>
		/// <param name="parameters">input parameters and output parameters</param>
		/// <returns>IDataReader object</returns>
		public IDataReader ExecuteReader(CommandType commandType, string commandString, DbParameters parameters=null)
		{
			return _interceptor.JoinPoint<IDataReader>(() =>
			{
				using (var command = ActiveConnection.CreateCommand())
				{
					command.CommandType = commandType;
					command.CommandText = commandString;
					AddCommandParameters(command, parameters.Params);
					if (_currentDataReader != null)
					{
						if (!_currentDataReader.IsClosed)
						{
							_currentDataReader.Close();
						}
						_currentDataReader = null;
					}
					_currentDataReader = command.ExecuteReader();
					RetrieveCommandParameterValues(command, parameters.Params);
				}
				return _currentDataReader;
			},this, "ExecuteReader", commandType, commandString, parameters);
		}

		/// <summary>
		/// Execute a SQL statement
		/// </summary>
		/// <param name="commandString">SQL statement</param>
		/// <param name="parameters">input and output parameters for SQL statement.</param>
		/// <returns>Return rows affected</returns>
		public int Sql(string commandString, DbParameters parameters=null)
		{
			return ExecuteNonQuery(CommandType.Text, commandString, parameters);
		}

		/// <summary>
		/// Execute a stored procedure
		/// </summary>
		/// <param name="storedProcedure">Stored procedure name</param>
		/// <param name="parameters">input and output parameters for stored procedure </param>
		/// <remarks>
		/// Using output parameter object to retrieve data from a stored procedure
		/// </remarks>
		public void StoredProcedure(string storedProcedure, DbParameters parameters=null)
		{
			ExecuteNonQuery(CommandType.StoredProcedure, storedProcedure, parameters);
		}

		/// <summary>
		/// Execute a SQL statement
		/// </summary>
		/// <param name="commandString">SQL statement</param>
		/// <param name="parameters">input and output parameters for SQL statement.</param>
		/// <returns>Return rows affected</returns>
		public object SqlSelect(string commandString, DbParameters parameters=null)
		{
			return ExecuteScalar(CommandType.Text, commandString, parameters);
		}

		/// <summary>
		/// Execute a SQL statement and retrieve batch data from IDataReader
		/// </summary>
		/// <param name="commandString">SQL statement</param>
		/// <param name="parameters">input parameters for SQL statement.</param>
		/// <returns>IDataReader object</returns>
		public IDataReader SqlReader(string commandString, DbParameters parameters=null)
		{
			return ExecuteReader(CommandType.Text, commandString, parameters);
		}

		/// <summary>
		/// Execute SQL and read data into data table and allow data work offline.
		/// </summary>
		/// <param name="commandString">Sql statement</param>
		/// <param name="parameters">arguments for Sql statement</param>
		/// <returns>A data table that hold a set of records retrieved from Sql</returns>
		public DataTable SqlDataTable(string commandString, DbParameters parameters=null)
		{
			return _interceptor.JoinPoint(() =>
			 {
				 var dataTable = new DataTable();
				 var reader = SqlReader(commandString, parameters);
				 dataTable.Load(reader);
				 reader.Close();
				 return dataTable;
			 }, this, "SqlDataTable", commandString, parameters);
		}

		/// <summary>
		/// Retrieve a list of entity from a sql statement
		/// </summary>
		/// <typeparam name="T">Entity type</typeparam>
		/// <param name="ParseObjectFromReader">Callback function to parse data reader into an entity object</param>
		/// <param name="commandString">SQL statement</param>
		/// <param name="parameters">input parameters for SQL statement.</param>
		/// <returns>A list of entity object</returns>
		public List<T> Retrieve<T>(Func<IDataReader, T> ParseObjectFromReader, string commandString, DbParameters parameters=null)
		{
			return _interceptor.JoinPoint(() =>
			{
				var result = new List<T>();
				using (var reader = SqlReader(commandString, parameters))
				{
					while (reader.Read())
					{
						var t = ParseObjectFromReader(reader);
						result.Add(t);
					}
					reader.Close();
				}
				return result;
			}, "Retrieve", commandString, parameters);
		}



		#region Private
		private void AddCommandParameters(IDbCommand command, IList<IDbDataParameter> parameters)
		{
			if (parameters != null)
			{
				foreach (var p in parameters)
				{
					var parameter = command.CreateParameter();
					DbParameters.MapFrom(p, parameter);
					command.Parameters.Add(parameter);
				}
			}
		}

		private void RetrieveCommandParameterValues(IDbCommand command, IList<IDbDataParameter> parameters)
		{
			if (parameters != null)
			{
				for (var i = 0; i < parameters.Count; i++)
				{
					if (parameters[i].Direction == ParameterDirection.InputOutput ||
						parameters[i].Direction == ParameterDirection.Output ||
						parameters[i].Direction == ParameterDirection.ReturnValue)
					{
						DbParameters.MapperTo(parameters[i], (IDbDataParameter)command.Parameters[i]);
					}
				}
			}
		}

		#endregion

	}
	/*
	public static class DbClientExtension
	{
		/// <summary>
		/// Gets the nullable value.
		/// </summary>
		/// <returns>The nullable value.</returns>
		/// <param name="reader">Reader.</param>
		/// <param name="index">Index.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T? GetNullableValue<T>(this IDataReader reader, int index) where T : struct
		{
			object value = reader.GetValue(index);

			if (value is DBNull)
			{
				return null;
			}


			if (value is T) {
    			return (T)value;
			} 
   			return (T)Convert.ChangeType(value, typeof(T));
		}

		/// <summary>
		/// Gets the nullable value.
		/// </summary>
		/// <returns>The nullable value.</returns>
		/// <param name="reader">Reader.</param>
		/// <param name="columnName">Column name.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T? GetNullableValue<T>(this IDataReader reader, string columnName) where T : struct
		{
			object value = reader[columnName];

			if (value is DBNull)
			{
				return null;
			}


			if (value is T) {
    			return (T)value;
			} 
   			return (T)Convert.ChangeType(value, typeof(T));
		}

		/// <summary>
		/// Gets column value. 
		/// </summary>
		/// <returns>The table column value. If value is null it return default value</returns>
		/// <param name="reader">Reader.</param>
		/// <param name="index">Index.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T GetDbValue<T>(this IDataReader reader, int index) where T : struct
		{
			object value = reader.GetValue(index);

			return ConvertObjectValue<T>(value);
		}

		public static string GetDbValue(this IDataReader reader, int index)
		{
			object value = reader.GetValue(index);

			if (value is DBNull)
			{
				return null;
			}


			return value.ToString();
		}

		public static T ConvertObjectValue<T>(object value)  where T : struct
		{
			if (value is DBNull)
			{
				return default(T);
			}


			if (value is T) {
    			return (T)value;
			} 
   			return (T)Convert.ChangeType(value, typeof(T));
		}

		/// <summary>
		/// Gets column value. 
		/// </summary>
		/// <returns>The table column value. If value is null it return default value</returns>
		/// <param name="reader">Reader.</param>
		/// <param name="columnName">Column name.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T GetDbValue<T>(this IDataReader reader, string columnName) where T : struct
		{
			object value = reader[columnName];

			return ConvertObjectValue<T>(value);
		}
	}
	*/
}
