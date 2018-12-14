using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace qshine
{
	/// <summary>
	/// Database client class
	/// </summary>
	public class DbClient:IDisposable
	{
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
            _context = DbContext.Current;
            _database = _context.Database;
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
            Func<int> method = () =>
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
               };

            return _interceptor.JoinPoint(method, this, "ExecuteNonQuery", commandType, commandString, parameters);
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
                     if (parameters != null && parameters.Params != null)
                     {
                         AddCommandParameters(command, parameters.Params);
                     }
					 result = command.ExecuteScalar();
                     if (parameters != null && parameters.Params != null)
                     {
                         RetrieveCommandParameterValues(command, parameters.Params);
                     }
				 }
				 return result;
			 }, this, "ExecuteScalar", commandType, commandString, parameters);
		}


        /// <summary>
        /// Execute a SQL statement or StoredProcedure and retrieve batch data from IDataReader
        /// </summary>
        /// <param name="readerData">A function to process data reader. The reader will be dispose after process completed</param>
        /// <param name="commandType">A CommandType object to indicate a Sql command or a storedprocedure command</param>
        /// <param name="commandString">SQL statement or stored procedure</param>
        /// <param name="parameters">input parameters and output parameters</param>
        public void ExecuteReader(Action<IDataReader> readerData, CommandType commandType, string commandString, DbParameters parameters=null)
		{
			_interceptor.JoinPoint(() =>
			{
                using (var command = ActiveConnection.CreateCommand())
				{
					command.CommandType = commandType;
					command.CommandText = commandString;
                    if (parameters != null)
                    {
                        AddCommandParameters(command, parameters.Params);
                    }

                    using (var dataReader = command.ExecuteReader())
                    {
                        readerData(dataReader);
                    }
                    return 0;
                }
			},this, "ExecuteReader", commandType, commandString, parameters);
		}


		/// <summary>
		/// Execute a SQL statement with parameters
		/// </summary>
		/// <param name="commandString">SQL statement</param>
		/// <param name="parameters">input and output parameters for SQL statement.</param>
		/// <returns>Return rows affected</returns>
		public int Sql(string commandString, DbParameters parameters=null)
		{
			return Sql(new DbSqlStatement(commandString,parameters));
		}

        public bool Sql(List<string> batchStatements, BatchException batchException)
        {
            if (batchStatements == null || batchStatements.Count == 0) return false;

            return Sql(batchStatements.Select(item => new DbSqlStatement(item)).ToList(),
                batchException);
        }


        /// <summary>
        /// Execute a Sql statement
        /// </summary>
        /// <param name="sql">sql statement with parameters</param>
        /// <returns>result of the sql statement</returns>
        public int Sql(DbSqlStatement sql)
        {
            if (sql == null || string.IsNullOrWhiteSpace(sql.Sql)) return -1;

            int result = ExecuteNonQuery(sql.CommandType, sql.Sql, sql.Parameters);
            sql.Result = result;

            return result;
        }

        /// <summary>
        /// Execute a batch Sql statements
        /// </summary>
        /// <param name="sqls">sql statements</param>
        /// <param name="batchException">batch exception policy</param>
        /// <returns>True to indicate success.</returns>
        public bool Sql(List<DbSqlStatement> batchStatements, BatchException batchException)
        {
            if (batchStatements == null || batchStatements.Count==0) return false;

            bool result = true;

            if (batchException != null)
            {
                batchException.ChainBatchException();
            }

            foreach (var c in batchStatements)
            {
                if (c==null ||string.IsNullOrWhiteSpace(c.Sql)) continue;

                if (batchException != null)
                {
                    try
                    {
                        Sql(c);
                    }
                    catch (Exception ex)
                    {
                        result = false;
                        ex.Data.Add("sql", c);
                        //Try throw batch exception based on batch exception policy
                        batchException.TryThrow(ex);
                    }
                }
                else
                {
                    Sql(c);
                }
            }

            //Try throw batch exception based on batch exception policy
            if (batchException != null)
            {
                batchException.TryThrow();
            }

            return result;
        }

        /// <summary>
        /// Execute sql when condition satisfied
        /// </summary>
        /// <param name="sql">conditional sql instance.</param>
        /// <returns></returns>
        public bool Sql(ConditionalSql sql, BatchException batchException)
        {
            Check.HaveValue(sql, "sql");

            bool result = true;
            if (sql.ConditionSql!=null && sql.Condition!=null && !string.IsNullOrWhiteSpace(sql.ConditionSql.Sql))
            {
                object v = SqlSelect(sql.ConditionSql.Sql,sql.ConditionSql.Parameters);
                result = sql.Condition(v==null?string.Empty:v.ToString());
            }
            if (result)
            {
                return Sql(sql.Sqls, batchException);
            }
            return false;
        }

        public bool Sql(List<ConditionalSql> batchSqls)
        {
            return Sql(batchSqls, new BatchException());
        }


        public bool Sql(List<ConditionalSql> batchSqls, BatchException batchException)
        {
            if (batchSqls == null || batchSqls.Count == 0) return false;

            bool result = true;
            if (batchException != null)
            {
                batchException.ChainBatchException();
            }

            foreach (var c in batchSqls)
            {
                Sql(c, batchException);
            }

            //Try throw batch exception based on batch exception policy
            if (batchException != null)
            {
                batchException.TryThrow();
            }

            return result;
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
        /// <param name="readerData">A function to process data reader. The reader will be dispose after process completed</param>
        /// <param name="commandString">SQL statement</param>
        /// <param name="parameters">input parameters for SQL statement.</param>
        public void SqlReader(Action<IDataReader> readerData, string commandString, DbParameters parameters=null)
		{
			ExecuteReader(readerData, CommandType.Text, commandString, parameters);
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
                 DataSet ds = new DataSet();
                 using (var adapter = Context.Database.CreateAdapter())
                 {
                     using (var command = ActiveConnection.CreateCommand())
                     {
                         command.CommandType = CommandType.Text;
                         command.CommandText = commandString;
                         if (parameters != null)
                         {
                             AddCommandParameters(command, parameters.Params);
                         }
                         adapter.SelectCommand = command as DbCommand;
                         adapter.Fill(ds);
                         if (ds.Tables.Count > 0)
                         {
                             return ds.Tables[0];
                         }
                         else
                         {
                             return new DataTable();
                         }
                     }
                 }
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
        public List<T> Retrieve<T>(Func<IDataReader, T> ParseObjectFromReader, string commandString, DbParameters parameters = null)
        {
            return _interceptor.JoinPoint(() =>
            {
                var result = new List<T>();
                SqlReader((reader) =>
                {
                    while (reader.Read())
                    {
                        var t = ParseObjectFromReader(reader);
                        result.Add(t);
                    }
                }, commandString, parameters);
                return result;
            }, "Retrieve", commandString, parameters);
		}

        public static bool ToBoolean(object value)
        {
            if (value == null) return false;
            if (value is Boolean) return (bool)value;

            var s = value.ToString().ToLower();
            return s == "1"
            || s == "-1"
            || s == "t" //true
            || s == "y"; //yes
        }

        /// <summary>
        /// Insert record to table
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columns"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public int Insert(string tableName, string columns, params object[] values)
        {
            var builder = new StringBuilder();
            var parameterNames = new List<string>();
            var parameters = DbParameters.New;
            foreach (var v in values)
            {
                var p = parameters.AutoParameter(v);
                parameterNames.Add(ParameterName(p.ParameterName));
            }
            builder.AppendFormat("insert into {0}({1}) values({2})", tableName, columns, string.Join(",", parameterNames));

            return ExecuteNonQuery(CommandType.Text, builder.ToString(), parameters);
        }

        /// <summary>
        /// generate sql parameter name with a prefix
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public string ParameterName(string p)
        {
            return _database.ParameterPrefix + p;
        }



		#region Private
		private void AddCommandParameters(IDbCommand command, IList<IDbDataParameter> parameters)
		{
			if (parameters != null)
			{
				foreach (var p in parameters)
				{
					var parameter = command.CreateParameter();
                    MapParameterToNative(p, parameter);
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
                        MapParameterFromNative(parameters[i], (IDbDataParameter)command.Parameters[i]);
					}
				}
			}
		}

        List<IDbTypeMapper> _customDbTypeMapper;
        private List<IDbTypeMapper> CustomDbTypeMappers
        {
            get
            {
                if (_customDbTypeMapper == null)
                {
                    _customDbTypeMapper = _database.DbTypeMappers;
                }
                return _customDbTypeMapper;
            }
        }

        private CommonDbDataTypeMapper _commonMapper = new CommonDbDataTypeMapper();

        /// <summary>
        /// Map a common data parameter to a provider specific native data parameter
        /// </summary>
        /// <param name="common">common parameter</param>
        /// <param name="native">database native parameter</param>
        private void MapParameterToNative(IDbDataParameter common, IDbDataParameter native)
        {
            native.Direction = common.Direction;
            native.Size = common.Size;
            native.Precision = common.Precision;
            native.Scale = common.Scale;
            native.ParameterName = common.ParameterName;

            bool mapped = false;

            //perform custom DbTypeMapping if exists
            foreach (var mapper in CustomDbTypeMappers)
            {
                if (mapper.MapToNative(common, native) == true)
                {
                    mapped = true;
                    break;
                }
            }

            if (!mapped)
            {
                //perform Common DbTypeMapping
                mapped = _commonMapper.MapToNative(common, native);
            }
        }

        /// <summary>
        /// Map a data provider specific native data parameter to a common data parameter
        /// </summary>
        /// <param name="common">common parameter</param>
        /// <param name="native">database native parameter</param>
        private void MapParameterFromNative(IDbDataParameter native, IDbDataParameter common)
        {
            bool hasMapped = false;

            hasMapped = _commonMapper.MapFromNative(native, common);

            //perform custom DbTypeMapping if exists
            foreach (var mapper in CustomDbTypeMappers)
            {
                if (mapper.MapFromNative(native, common) == true)
                {
                    hasMapped = true;
                    break;
                }
            }

            if (!hasMapped && native.Direction!= ParameterDirection.Input)
            {
                common.Value = native.Value;
            }
        }

        #endregion

    }

    public static class DbClientExtension
	{

        /// <summary>
        /// Shortcut of SqlSelect
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="commandString"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        static public T SqlSelect<T>(this string commandString, DbParameters parameters = null)
        {
            using (var db = new DbClient())
            {
                var result = db.SqlSelect(commandString, parameters);
                if (result == null) return default(T);

                return (T)result;
            }
        }

        /*
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
        */
	}
	
}
