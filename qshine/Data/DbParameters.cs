using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;

namespace qshine
{
	public class DbParameters
	{
		List<IDbDataParameter> _dbParameters;
		IDbDataParameter _currentParameter;

		/// <summary>
		/// Gets the new DbParameters.
		/// </summary>
		/// <value>The new.</value>
		static public DbParameters New
		{
			get
			{
				return new DbParameters();
			}
		}

		public DbParameters()
		{
			_dbParameters = new List<IDbDataParameter>();
		}

		public IList<IDbDataParameter> Params
		{
			get
			{
				return _dbParameters;
			}
		}

		public DbParameters Input<T>(string name, T value)
		{
			_currentParameter = new CommonDbParameter
            {
				ParameterName = name,
				Direction = ParameterDirection.Input,
				Value = ToDbValue(value),
			};
            if (value is bool)
            {
                _currentParameter.DbType = DbType.Boolean;
            }
            else
            {
                _currentParameter.DbType = ToDbType(typeof(T));
            }

			_dbParameters.Add(_currentParameter);
			return this;
		}

		public DbParameters Input<T>(string name, T value, DbType dbType)
		{
			Input(name, value);
			_currentParameter.DbType = dbType;
			return this;
		}

		public DbParameters Output<T>(string name, int size = -1)
		{
			Output(name, ToDbType(typeof(T)), size);
			return this;
		}

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

		public DbParameters Return(string name, DbType dbType, int size = -1)
		{
			Output(name, dbType, size);
			_currentParameter.Direction = ParameterDirection.ReturnValue;
			return this;
		}


		private object ToDbValue(object value)
		{
			if (value == null || (value is DateTime && DateTime.MinValue == (DateTime)value)
				|| (value is int && int.MinValue == (int)value)
				|| (value is long && long.MinValue == (long)value)
				|| (value is decimal && decimal.MinValue == (decimal)value)
				)
			{
				return DBNull.Value;
			}
            
			if (value is ulong)
            {
                if((ulong)value>=0x8000000000000000)
                    return Convert.ToDecimal(value);
            }

            if (value is long)
            {
                if ((long)value >=0x7000000000000000)
                    return Convert.ToDecimal(value);
            }
			return value;
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
			
			return DbType.String;
		}


		/// <summary>
		/// Maps from.
		/// </summary>
		/// <param name="p">P.</param>
		/// <param name="parameter">Parameter.</param>
		internal static void MapFrom(IDbDataParameter p, IDbDataParameter parameter)
		{
			parameter.Direction = p.Direction;
			parameter.DbType = p.DbType;
			parameter.Size = p.Size;
			parameter.Precision = p.Precision;
			parameter.Scale = p.Scale;
			parameter.ParameterName = p.ParameterName;
			parameter.Value = p.Value;

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

    public class CommonDbParameter : DbParameter
    {
        public override DbType DbType { get; set; }
        public override ParameterDirection Direction { get; set; }
        public override bool IsNullable { get; set; }
        public override string ParameterName { get; set; }
        public override int Size { get; set; }
        public override string SourceColumn { get; set; }
        public override bool SourceColumnNullMapping { get; set; }
        public override object Value { get; set; }

        public override void ResetDbType()
        {
            throw new NotImplementedException();
        }
    }
}
