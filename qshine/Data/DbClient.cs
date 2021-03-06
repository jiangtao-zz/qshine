using qshine.Specification;
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
    /// Provide database access service.
    /// DbClient is not a thread safe class. 
    /// 
    /// </summary>
    public class DbClient : IDisposable
    {
        /// <summary>
        /// The interceptor must be a static instance (singleton).
        /// </summary>
        static Interceptor _interceptor = Interceptor.Get<DbClient>();

        readonly bool _inscopeSession = false;

        #region Ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="T:qshine.DbClient"/> class for default configued database.
        /// The default default configured database is set by environment configuration default database connection string.
        /// </summary>
        public DbClient()
            : this(new Database())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:qshine.DbClient"/> class with given database provider name and connection string.
        /// Same database context will be reused within same call context.
        /// </summary>
        /// <param name="providerName">database provider name</param>
        /// <param name="connectionString">database connection string</param>
		public DbClient(string providerName, string connectionString)
            : this(new Database(providerName, connectionString))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:qshine.DbClient"/> class for a given database instance.
        /// Same database context will be reused within same call context.
        /// </summary>
        /// <param name="database">Database instance.</param>
        public DbClient(Database database)
        {
            var session = DbSession.GetCurrentTransactionSession(database);

            if (session == null)
            {
                session = new DbSession(database);
                _inscopeSession = true;
            }
            Session = session;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:qshine.DbClient"/> class for a given database context.
        /// </summary>
        /// <param name="session"></param>
        public DbClient(DbSession session)
        {
            Session = session;
        }

        #endregion

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
                if (disposing && _inscopeSession)
                {
                    Session.Dispose();
                    _disposed = true;
                }
            }
        }

        #endregion
        /// <summary>
        /// DBClient session
        /// </summary>
        public DbSession Session { get; private set; }

        /// <summary>
        /// Execute Sql statement or a stored procedure and return number of rows affected.
        /// </summary>
        /// <param name="commandType">A CommandType object to indicate a Sql command or a storedprocedure command</param>
        /// <param name="commandString">SQL statement or stored procedure</param>
        /// <param name="parameters">input parameters and output parameters</param>
        public int ExecuteNonQuery(CommandType commandType, string commandString, DbParameters parameters = null)
        {
            int method()
            {
                var result = 0;
                using (var command = CreateCommand())
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
            }

            return _interceptor.JoinPoint(method, this, "ExecuteNonQuery", commandType, commandString, parameters);
        }

        /// <summary>
        /// Execute Sql statement or a stored procedure and return first value selected from the sql.
        /// </summary>
        /// <param name="commandType">A CommandType object to indicate a Sql command or a storedprocedure command</param>
        /// <param name="commandString">SQL statement or stored procedure</param>
        /// <param name="parameters">input parameters and output parameters</param>
        /// <returns>Return first selected value from query</returns>
        /// <example>
        /// <![CDATA[
        ///     ExecuteScalar(CommandType.Text,
        ///         "select 1 from tb1 where name=:p1 and age=:p2", 
        ///         DbParameters.New.Input("p1",name).Input("p2",age).Output<int>("p3"))
        /// ]]>
        /// </example>
        public object ExecuteScalar(CommandType commandType, string commandString, DbParameters parameters = null)
        {
            return _interceptor.JoinPoint<object>(() =>
            {
                object result = null;
                using (var command = CreateCommand())
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
        /// Execute a SQL statement or StoredProcedure and retrieve batch data from IDataReader through callback method.
        /// The callback method need deal with data reader for each record.
        /// </summary>
        /// <param name="readerData">A function to process data reader. The reader will be dispose after process completed</param>
        /// <param name="commandType">A CommandType object to indicate a Sql command or a storedprocedure command</param>
        /// <param name="commandString">SQL statement or stored procedure</param>
        /// <param name="parameters">input parameters and output parameters</param>
        public void ExecuteReader(Action<IDataReader> readerData, CommandType commandType, string commandString, DbParameters parameters = null)
        {
            _interceptor.JoinPoint(() =>
            {
                using (var command = CreateCommand())
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
            }, this, "ExecuteReader", commandType, commandString, parameters);
        }

        /// <summary>
        /// Execute a SQL statement with parameters
        /// </summary>
        /// <param name="commandString">SQL statement</param>
        /// <param name="parameters">input and output parameters for SQL statement.</param>
        /// <returns>Return rows affected</returns>
        public int Sql(string commandString, DbParameters parameters = null)
        {
            return Sql(new DbSqlStatement(commandString, parameters));
        }

        /// <summary>
        /// Execute a SQL statements with parameters
        /// </summary>
        /// <param name="batchStatements">sql statement</param>
        /// <param name="results">Receives validation results
        /// It also receives batch exception if some sqls failed.</param>
        /// <returns></returns>
        public bool Sql(List<string> batchStatements, Validator results)
        {
            if (batchStatements == null || batchStatements.Count == 0) return false;

            return Sql(batchStatements.Select(item => new DbSqlStatement(item)).ToList(),
                results);
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
        /// <param name="batchStatements">sql statements</param>
        /// <param name="results">Receive validation results</param>
        /// <returns>True to indicate success.</returns>
        public bool Sql(List<DbSqlStatement> batchStatements, Validator results)
        {
            if (batchStatements == null || batchStatements.Count == 0) return false;

            bool isSuccess = true;

            foreach (var c in batchStatements)
            {
                if (c == null || string.IsNullOrWhiteSpace(c.Sql)) continue;

                if (results != null)
                {
                    try
                    {
                        Sql(c);
                    }
                    catch (Exception ex)
                    {
                        isSuccess = false;
                        ex.Data.Add("sql", c);
                        results.AddValidationError(ex);
                    }
                }
                else
                {
                    Sql(c);
                }
            }

            return isSuccess;
        }

        /// <summary>
        /// Execute sql when condition satisfied
        /// </summary>
        /// <param name="sql">conditional sql instance.</param>
        /// <param name="results">Receive validation results</param>
        /// <returns></returns>
        public bool Sql(ConditionalSql sql, Validator results)
        {
            Check.HaveValue(sql, "sql");

            bool result = true;
            if (sql.ConditionSql != null && sql.Condition != null && !string.IsNullOrWhiteSpace(sql.ConditionSql.Sql))
            {
                object v = SqlSelect(sql.ConditionSql.Sql, sql.ConditionSql.Parameters);
                result = sql.Condition(v == null ? string.Empty : v.ToString());
            }
            if (result)
            {
                return Sql(sql.Sqls, results);
            }
            return false;
        }

        /// <summary>
        /// Execute condition sqls
        /// </summary>
        /// <param name="batchSqls">a list of condition sql</param>
        /// <returns></returns>
        public bool Sql(List<ConditionalSql> batchSqls)
        {
            return Sql(batchSqls, new Validator());
        }

        /// <summary>
        /// Execute conditional sqls.
        /// </summary>
        /// <param name="batchSqls">conditional sqls</param>
        /// <param name="results">Receive validation results</param>
        /// <returns>true if no any exception.</returns>
        public bool Sql(List<ConditionalSql> batchSqls, Validator results)
        {
            if (batchSqls == null || batchSqls.Count == 0) return false;

            foreach (var c in batchSqls)
            {
                Sql(c, results);
            }

            return results.ValidationResults.IsValid;
        }

        /// <summary>
        /// Execute a stored procedure
        /// </summary>
        /// <param name="storedProcedure">Stored procedure name</param>
        /// <param name="parameters">input and output parameters for stored procedure </param>
        /// <remarks>
        /// Using output parameter object to retrieve data from a stored procedure
        /// </remarks>
        public void StoredProcedure(string storedProcedure, DbParameters parameters = null)
        {
            ExecuteNonQuery(CommandType.StoredProcedure, storedProcedure, parameters);
        }

        /// <summary>
        /// Execute a SQL statement
        /// </summary>
        /// <param name="commandString">SQL statement</param>
        /// <param name="parameters">input and output parameters for SQL statement.</param>
        /// <returns>Return rows affected</returns>
        public object SqlSelect(string commandString, DbParameters parameters = null)
        {
            return ExecuteScalar(CommandType.Text, commandString, parameters);
        }

        /// <summary>
        /// Execute a SQL statement and retrieve batch data from IDataReader
        /// </summary>
        /// <param name="readerData">A function to process data reader. The reader will be dispose after process completed</param>
        /// <param name="commandString">SQL statement</param>
        /// <param name="parameters">input parameters for SQL statement.</param>
        public void SqlReader(Action<IDataReader> readerData, string commandString, DbParameters parameters = null)
        {
            ExecuteReader(readerData, CommandType.Text, commandString, parameters);
        }

        /// <summary>
        /// Execute SQL and read data into data table and allow data work offline.
        /// </summary>
        /// <param name="commandString">Sql statement</param>
        /// <param name="parameters">arguments for Sql statement</param>
        /// <returns>A data table that hold a set of records retrieved from Sql</returns>
        public DataTable SqlDataTable(string commandString, DbParameters parameters = null)
        {
            return _interceptor.JoinPoint(() =>
            {
                DataSet ds = new DataSet();
                using (var adapter = Session.Database.CreateAdapter())
                {
                    using (var command = CreateCommand())
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
                        if (t != null)
                        {
                            result.Add(t);
                        }
                        else
                        {
                            break;
                        }
                    }
                }, commandString, parameters);
                return result;
            }, "Retrieve", commandString, parameters);
        }

        /// <summary>
        /// Convert common text to boolean type value
        /// </summary>
        /// <param name="value">Common boolean text such as
        /// 1, -1, "t", "y"
        /// </param>
        /// <returns></returns>
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
        /// Build insert statement and execute sql
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
        /// <param name="p"></param>
        /// <returns></returns>
        public string ParameterName(string p)
        {
            return Session.Database.ParameterPrefix + p;
        }



        #region Private

        /// <summary>
        /// Get an opened database connection
        /// </summary>
        private IDbCommand CreateCommand()
        {
            return Session.CreateCommand();
        }

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
                    _customDbTypeMapper = Session.Database.DbTypeMappers;
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

            if (!hasMapped && native.Direction != ParameterDirection.Input)
            {
                common.Value = native.Value;
            }
        }

        #endregion

    }

    /// <summary>
    /// DbClient extension
    /// </summary>
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
