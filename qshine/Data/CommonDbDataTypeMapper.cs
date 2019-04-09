using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace qshine
{
    /// <summary>
    /// Common database data type mapper
    /// </summary>
    public class CommonDbDataTypeMapper:IDbTypeMapper
    {
        /// <summary>
        /// Common database data type mapper for all database provider.
        /// This name could be a wildcard (*) or a supportted database provider name.
        /// </summary>
        public string SupportedProviderNames
        {
            get
            {
                return "*";
            }
        }

        /// <summary>
        /// Map common Db parameter type and value to a database specific native type and value
        /// </summary>
        /// <param name="common">common db parameter</param>
        /// <param name="native">native parameter</param>
        public bool MapToNative(IDbDataParameter common, IDbDataParameter native)
        {
            
            native.Value = ToDbValue(common.Value);
            DbType dbType;

            if (common.DbType == DbType.AnsiString)
            {
                dbType = native.DbType;
                common.DbType = dbType;
                return true;
            }
            native.DbType = common.DbType;
            return false;
        }

        /// <summary>
        /// Map supported provider specific native dbtype and value to common data type and value.
        /// </summary>
        /// <param name="native">native parameter</param>
        /// <param name="common">common parameter</param>
        /// <returns>returns true if the parameter get mapped.</returns>
        public bool MapFromNative(IDbDataParameter native, IDbDataParameter common)
        {
            //no specific map required in common mapper.
            return false;
        }


        //public 
        private bool ToDbType(Type type, out DbType dbType)
        {
            if (type == typeof(int) || type == typeof(uint))
            {
                dbType = DbType.Int32;
            }
            else if (type == typeof(long) || type == typeof(ulong))
            {
                dbType = DbType.Int64;
            }
            else if (type == typeof(double) || type == typeof(float))
            {
                dbType = DbType.Double;
            }
            else if (type == typeof(decimal))
            {
                dbType = DbType.Decimal;
            }
            else if (type == typeof(string))
            {
                dbType = DbType.String;
            }
            else if (type == typeof(DateTime))
            {
                dbType = DbType.DateTime;
            }
            else if (type == typeof(DateTimeOffset))
            {
                dbType = DbType.DateTimeOffset;
            }
            else if (type == typeof(TimeSpan))
            {
                dbType = DbType.Time;
            }
            else if (type == typeof(Byte[]))
            {
                dbType = DbType.Binary;
            }
            else if (type == typeof(Guid))
            {
                dbType = DbType.Guid;
            }
            else if (type == typeof(Boolean))
            {
                dbType = DbType.Boolean;
            }
            else
            {
                dbType = DbType.AnsiString;
                return false;
            }

            return true;
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
                if ((ulong)value >= 0x8000000000000000)
                    return Convert.ToDecimal(value);
            }

            if (value is long)
            {
                if ((long)value >= 0x7000000000000000)
                    return Convert.ToDecimal(value);
            }
            return value;
        }

    }
}
