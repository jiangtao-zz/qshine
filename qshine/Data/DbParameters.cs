using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;

namespace qshine
{
    /// <summary>
    /// Define Sql parameters
    /// </summary>
	public class DbParameters
	{
		List<IDbDataParameter> _dbParameters;
		IDbDataParameter _currentParameter;
        int _autoParameterOrder;

        /// <summary>
        /// instantiate a DbParameters instance
        /// </summary>
        /// <value>The new.</value>
        static public DbParameters New
		{
			get
			{
				return new DbParameters();
			}
		}

        /// <summary>
        /// Ctor.
        /// </summary>
        public DbParameters()
        {
            _dbParameters = new List<IDbDataParameter>();
            _autoParameterOrder = 0;

        }


        /// <summary>
        /// Auto generate a parameter name
        /// </summary>
        string AutoParameterName
        {
            get
            {
                return "p" + _autoParameterOrder++;
            }
        }
        /// <summary>
        /// Create an auto common data parameter for a parameter value.
        /// </summary>
        /// <param name="value">Sql parameter value. If the value is a IDbDataParameter, it will be return directly with an auto generated name.</param>
        /// <returns>Return a common data parameter based on data value</returns>
        public IDbDataParameter AutoParameter(object value)
        {
            var name = AutoParameterName;
            IDbDataParameter existsParam = value as IDbDataParameter;
            if(existsParam!=null)
            {
                existsParam.ParameterName = name;
            }
            else
            {
                existsParam = new CommonDbParameter
                {
                    ParameterName = name,
                    Direction = ParameterDirection.Input,
                    Value = value //ToDbValue(value),
                };
                //if (value is bool)
                //{
                //    existsParam.DbType = DbType.Boolean;
                //}
                //else if (value != null)
                //{
                //    existsParam.DbType = ToDbType(value.GetType());
                //}
            }
            _currentParameter = existsParam;
            _dbParameters.Add(_currentParameter);
            return _currentParameter;
        }

        /// <summary>
        /// Get all sql common parameters
        /// </summary>
		public IList<IDbDataParameter> Params
		{
			get
			{
				return _dbParameters;
			}
		}

        /// <summary>
        /// Add an input parameter with given name and value
        /// </summary>
        /// <typeparam name="T">The parameter value type.</typeparam>
        /// <param name="name">The parameter name</param>
        /// <param name="value">The parameter value.</param>
        /// <returns>The instance self.</returns>
		public DbParameters Input<T>(string name, T value)
		{
            _currentParameter = new CommonDbParameter
            {
                ParameterName = name,
                Direction = ParameterDirection.Input,
                Value = value// ToDbValue(value),
			};
            //if (value is bool)
            //{
            //    _currentParameter.DbType = DbType.Boolean;
            //}
            //else
            //{
            //    _currentParameter.DbType = ToDbType(typeof(T));
            //}

			_dbParameters.Add(_currentParameter);
			return this;
		}

        /// <summary>
        /// Add and input parameter with given name, type and data type
        /// </summary>
        /// <typeparam name="T">The parameter value type.</typeparam>
        /// <param name="name">The parameter name</param>
        /// <param name="value">The parameter value.</param>
        /// <param name="dbType">The data Db type</param>
        /// <returns>The instance self.</returns>
		public DbParameters Input<T>(string name, T value, DbType dbType)
		{
			Input(name, value);
			_currentParameter.DbType = dbType;
			return this;
		}

        /// <summary>
        /// Add an output parameter with given name and data size. 
        /// </summary>
        /// <typeparam name="T">Type of data to be output from sql result</typeparam>
        /// <param name="name">The parameter name</param>
        /// <param name="size">The parameter data buffer size. The default size is -1. </param>
        /// <returns>The instance self.</returns>
		public DbParameters Output<T>(string name, int size = -1)
		{
			Output(name, ToDbType(typeof(T)), size);
			return this;
		}

        /// <summary>
        /// Add an output parameter with given parameter name, data DB type and size.
        /// </summary>
        /// <param name="name">The parameter name</param>
        /// <param name="dbType">The data common type</param>
        /// <param name="size">The parameter data buffer size. The default size is -1. </param>
        /// <returns>The instance self.</returns>
		public DbParameters Output(string name, DbType dbType, int size=-1)
		{
			_currentParameter = new CommonDbParameter
            {
				ParameterName = name,
				Direction = ParameterDirection.Output,
				DbType = dbType,
			};
			if (size != -1)
			{
				_currentParameter.Size = size;
			}
			_dbParameters.Add(_currentParameter);
			return this;
		}

        /// <summary>
        /// Add a sql return value parameter with given name, data DB type and size.
        /// </summary>
        /// <param name="name">The parameter name</param>
        /// <param name="dbType">The data common type</param>
        /// <param name="size">The parameter data buffer size. The default size is -1. </param>
        /// <returns>The instance self.</returns>
		public DbParameters Return(string name, DbType dbType, int size = -1)
		{
			Output(name, dbType, size);
			_currentParameter.Direction = ParameterDirection.ReturnValue;
			return this;
		}


		private DbType ToDbType(Type type)
		{
			if (type == typeof(int) || type == typeof(uint))
				return DbType.Int32;
			
			if (type == typeof(long) || type == typeof(ulong))
				return DbType.Int64;

			if (type == typeof(double) || type == typeof(float))
				return DbType.Double;

			if (type == typeof(decimal))
				return DbType.Decimal;

			if (type == typeof(string))
				return DbType.String;

			if (type == typeof(DateTime))
				return DbType.DateTime;

            if (type == typeof(Byte[]))
                return DbType.Binary;

            if (type == typeof(Guid))
                return DbType.Guid;

            return DbType.Object;
		}


		/// <summary>
		/// Maps from.
		/// </summary>
		/// <param name="p">P.</param>
		/// <param name="parameter">Parameter.</param>
		internal static void MapFrom(IDbDataParameter p, IDbDataParameter parameter)
		{
			parameter.Direction = p.Direction;
			parameter.Size = p.Size;
			parameter.Precision = p.Precision;
			parameter.Scale = p.Scale;
			parameter.ParameterName = p.ParameterName;

            //Some database do not support certain data type
            //hack native data convert
            if(p.DbType== DbType.Boolean)
            {
                if (IsOracle(parameter.GetType()))
                {
                    parameter.DbType = DbType.Int16;
                    parameter.Value = Convert.ToInt16(p.Value);
                }
            }
            parameter.DbType = p.DbType;
            parameter.Value = p.Value;
        }

        static bool IsOracle(Type type)
        {
            return type.Name.ToLower().Contains("oracle");
        }

        internal static void MapperTo(IDbDataParameter parameter, IDbDataParameter p)
		{
			parameter.Value = p.Value;


            //Some database do not support certain data type
            //hack native data convert
            if (parameter.DbType == DbType.Boolean)
            {
                if (IsOracle(p.GetType()))
                {
                    bool booleanValue = false;
                    if (p.Value != null )
                    {
                        var value = p.Value.ToString().ToLower();
                        if("1,y,t".Contains(value))
                        {
                            booleanValue = true;
                        }
                    }
                    parameter.Value = booleanValue;
                }
            }
        }
	}

    /// <summary>
    /// A generic database parameter.
    /// It simply inherited from DbParameter.
    /// </summary>
    public class CommonDbParameter : DbParameter
    {
        /// <summary>
        /// Data type
        /// </summary>
        public override DbType DbType { get; set; }
        /// <summary>
        /// Parameter direction. In/Out/InOut/Return
        /// </summary>
        public override ParameterDirection Direction { get; set; }
        /// <summary>
        /// IsNullable
        /// </summary>
        public override bool IsNullable { get; set; }
        /// <summary>
        /// name
        /// </summary>
        public override string ParameterName { get; set; }
        /// <summary>
        /// Size
        /// </summary>
        public override int Size { get; set; }
        /// <summary>
        /// SourceColumn
        /// </summary>
        public override string SourceColumn { get; set; }
        /// <summary>
        /// SourceColumnNullMapping
        /// </summary>
        public override bool SourceColumnNullMapping { get; set; }
        /// <summary>
        /// Get/Set value
        /// </summary>
        public override object Value { get; set; }

        /// <summary>
        /// Reset DbType to original.
        /// Not implemented
        /// </summary>
        public override void ResetDbType()
        {
            throw new NotImplementedException();
        }
    }
}
