using System;
using System.Data;
using qshine.Globalization;

namespace qshine
{
	/// <summary>
	/// Data reader extension.
	/// The extension Read method can read DbNull, string or database type value to object type value.
	/// If the column is DBNull it returns type default value.
	/// If the column is text type, it returns a converted type value or throw exception.
	/// </summary>
	public static class DataReaderExtension
	{
        #region Read primitive value by column index
        /// <summary>
        /// Read string type value by index
        /// </summary>
        /// <param name="reader">data reader</param>
        /// <param name="index">Index of the column</param>
        /// <returns>The string value</returns>
        public static string ReadString(this IDataReader reader, int index)
		{
			if (reader.IsDBNull(index))
			{
				return default(string);
			}
			return reader.GetString(index);
		}

		/// <summary>
		/// Reads the int16 by column index.
		/// </summary>
		/// <returns>The int16.</returns>
		/// <param name="reader">Reader.</param>
		/// <param name="index">Index of the column</param>
		public static short ReadInt16(this IDataReader reader, int index)
		{
			if (reader.IsDBNull(index))
			{
				return default(short);
			}

			short value;
			try
			{
				value = reader.GetInt16(index);
			}
			catch
			{
				if (!short.TryParse(reader[index].ToString(), out value))
				{
					throw;
				}
			}

			return value;
		}

		/// <summary>
		/// Reads the int32 by column index.
		/// </summary>
		/// <returns>The int32.</returns>
		/// <param name="reader">Reader.</param>
		/// <param name="index">Index of the column</param>
		public static int ReadInt32(this IDataReader reader, int index)
		{
			if (reader.IsDBNull(index))
			{
				return default(int);
			}

			int value;
			try
			{
				value = reader.GetInt32(index);
			}
			catch
			{
				if (!int.TryParse(reader[index].ToString(), out value))
				{
					throw;
				}
			}

			return value;
		}

		/// <summary>
		/// Reads the int64 by column index.
		/// </summary>
		/// <returns>The int64.</returns>
		/// <param name="reader">Reader.</param>
		/// <param name="index">Index of the column</param>
		public static long ReadInt64(this IDataReader reader, int index)
		{
			if (reader.IsDBNull(index))
			{
				return default(long);
			}

			long value;
			try
			{
				value = reader.GetInt64(index);
			}
			catch
			{
				if (!long.TryParse(reader[index].ToString(), out value))
				{
					throw;
				}
			}

			return value;
		}

		/// <summary>
		/// Reads the decimal by column index.
		/// </summary>
		/// <returns>The decimal value.</returns>
		/// <param name="reader">Reader.</param>
		/// <param name="index">Index of the column</param>
		public static decimal ReadDecimal(this IDataReader reader, int index)
		{
			if (reader.IsDBNull(index))
			{
				return default(decimal);
			}
			decimal value;
			try
			{
				value = reader.GetDecimal(index);
			}
			catch
			{
				if (!decimal.TryParse(reader[index].ToString(), out value))
				{
					throw;
				}
			}
			return value;
		}

		/// <summary>
		/// Reads the float by column index.
		/// </summary>
		/// <returns>The float value.</returns>
		/// <param name="reader">Reader.</param>
		/// <param name="index">Index of the column</param>
		public static float ReadFloat(this IDataReader reader, int index)
		{
			if (reader.IsDBNull(index))
			{
				return default(float);
			}

			float value;
			try
			{
				value = reader.GetFloat(index);
			}
			catch
			{
				if (!float.TryParse(reader[index].ToString(), out value))
				{
					throw;
				}
			}

			return value;
		}

		/// <summary>
		/// Reads the double by column index.
		/// </summary>
		/// <returns>The double value.</returns>
		/// <param name="reader">Reader.</param>
		/// <param name="index">Index of the column</param>
		public static double ReadDouble(this IDataReader reader, int index)
		{
			if (reader.IsDBNull(index))
			{
				return default(double);
			}

			double value;
			try
			{
				value = reader.GetDouble(index);
			}
			catch
			{
				if (!double.TryParse(reader[index].ToString(), out value))
				{
					throw;
				}
			}

			return value;
		}

		/// <summary>
		/// Reads the DateTime by column index.
		/// </summary>
		/// <returns>The DateTime value.</returns>
		/// <param name="reader">Reader.</param>
		/// <param name="index">Index of the column</param>
		public static DateTime ReadDateTime(this IDataReader reader, int index)
		{
			if (reader.IsDBNull(index))
			{
				return default(DateTime);
			}

            DateTime value;

            var type = reader.GetFieldType(index);
            if (type == typeof(DateTime))
            {
                value = reader.GetDateTime(index);
            }else if(type == typeof(TimeSpan))
            {
                value = new DateTime(0, 0, 0)+ (TimeSpan)reader.GetValue(index);
            }
            if (!DateTime.TryParse(reader[index].ToString(), out value))
            {
                throw new InvalidCastException(string.Format("Cannot cast field {0} to DateTime.",index));
            }

			return value;
		}

		/// <summary>
		/// Reads the BLOB data by column index.
		/// </summary>
		/// <returns>The byte array value.</returns>
		/// <param name="reader">Reader.</param>
		/// <param name="index">Index of the column</param>
		public static byte[] ReadBytes(this IDataReader reader, int index)
		{
			if (reader.IsDBNull(index))
			{
				return null;
			}
			return (byte[])reader[index];
		}
        #endregion

        #region Read Nullable vale by column index
        /// <summary>
        /// Read nullable integer value
        /// </summary>
        /// <param name="reader">data reader</param>
        /// <param name="index">column index</param>
        /// <returns></returns>
        public static short? ReadNullableInt16(this IDataReader reader, int index)
		{
			var value = reader.ReadInt16(index);
			if (value == default(short))
			{
				return null;
			}
			return value;
		}

        /// <summary>
        /// Read nullable 32 bits integer value
        /// </summary>
        /// <param name="reader">data reader</param>
        /// <param name="index">column index</param>
        /// <returns></returns>
		public static int? ReadNullableInt32(this IDataReader reader, int index)
		{
			var value = reader.ReadInt32(index);
			if (value == default(int))
			{
				return null;
			}
			return value;
		}

        /// <summary>
        /// Read nullable 64 bits integer value
        /// </summary>
        /// <param name="reader">data reader</param>
        /// <param name="index">column index</param>
        /// <returns></returns>
		public static long? ReadNullableInt64(this IDataReader reader, int index)
		{
			var value = reader.ReadInt64(index);
			if (value == default(long))
			{
				return null;
			}
			return value;
		}

        /// <summary>
        /// Read nullable decimal value
        /// </summary>
        /// <param name="reader">data reader</param>
        /// <param name="index">column index</param>
        /// <returns></returns>
		public static decimal? ReadNullableDecimal(this IDataReader reader, int index)
		{
			var value = reader.ReadDecimal(index);
			if (value == default(decimal))
			{
				return null;
			}
			return value;
		}

        /// <summary>
        /// Read nullable float type value
        /// </summary>
        /// <param name="reader">data reader</param>
        /// <param name="index">column index</param>
        /// <returns></returns>
		public static float? ReadNullableFloat(this IDataReader reader, int index)
		{
			var value = reader.ReadFloat(index);
			if (Math.Abs(value - default(float)) <= 0.0000001)
			{
				return null;
			}
			return value;
		}

        /// <summary>
        /// Read nullable double type value
        /// </summary>
        /// <param name="reader">data reader</param>
        /// <param name="index">column index</param>
        /// <returns></returns>
		public static double? ReadNullableDouble(this IDataReader reader, int index)
		{
			var value = reader.ReadDouble(index);
			if (Math.Abs(value - default(double)) < 0.000001)
			{
				return null;
			}
			return value;
		}

        /// <summary>
        /// Read nullable date and time value
        /// </summary>
        /// <param name="reader">data reader</param>
        /// <param name="index">column index</param>
        /// <returns></returns>
		public static DateTime? ReadNullableDateTime(this IDataReader reader, int index)
		{
			var value = reader.ReadDateTime(index);
			if (value == default(DateTime))
			{
				return null;
			}
			return value;
		}
		#endregion

		#region Read Nullable value by column name

        /// <summary>
        /// Read nullable 16 bits integer value by column name
        /// </summary>
        /// <param name="reader">data reader</param>
        /// <param name="columnName">column name</param>
        /// <returns>nullable value</returns>
		public static short? ReadNullableInt16(this IDataReader reader, string columnName)
		{
			return reader.ReadNullableInt16(reader.GetOrdinal(columnName));
		}

        /// <summary>
        /// Read nullable 32 bits integer value by column name
        /// </summary>
        /// <param name="reader">data reader</param>
        /// <param name="columnName">column name</param>
        /// <returns>nullable value</returns>
		public static int? ReadNullableInt32(this IDataReader reader, string columnName)
		{
			return reader.ReadNullableInt32(reader.GetOrdinal(columnName));
		}

        /// <summary>
        /// Read nullable 64 bits integer value by column name
        /// </summary>
        /// <param name="reader">data reader</param>
        /// <param name="columnName">column name</param>
        /// <returns>nullable value</returns>
		public static long? ReadNullableInt64(this IDataReader reader, string columnName)
		{
			return reader.ReadNullableInt64(reader.GetOrdinal(columnName));
		}

        /// <summary>
        /// Read nullable decimal value by column name
        /// </summary>
        /// <param name="reader">data reader</param>
        /// <param name="columnName">column name</param>
        /// <returns>nullable value</returns>
		public static decimal? ReadNullableDecimal(this IDataReader reader, string columnName)
		{
			return reader.ReadNullableDecimal(reader.GetOrdinal(columnName));
		}

        /// <summary>
        /// Read nullable float value by column name
        /// </summary>
        /// <param name="reader">data reader</param>
        /// <param name="columnName">column name</param>
        /// <returns>nullable value</returns>
		public static float? ReadNullableFloat(this IDataReader reader, string columnName)
		{
			return reader.ReadNullableFloat(reader.GetOrdinal(columnName));
		}

        /// <summary>
        /// Read nullable double value by column name
        /// </summary>
        /// <param name="reader">data reader</param>
        /// <param name="columnName">column name</param>
        /// <returns>nullable value</returns>
		public static double? ReadNullableDouble(this IDataReader reader, string columnName)
		{
			return reader.ReadNullableDouble(reader.GetOrdinal(columnName));
		}

        /// <summary>
        /// Read nullable date and time value by column name
        /// </summary>
        /// <param name="reader">data reader</param>
        /// <param name="columnName">column name</param>
        /// <returns>nullable value</returns>
		public static DateTime? ReadNullableDateTime(this IDataReader reader, string columnName)
		{
			return reader.ReadNullableDateTime(reader.GetOrdinal(columnName));
		}
        #endregion

        #region Read primitive value by column name

        /// <summary>
        /// Read string value by column name
        /// </summary>
        /// <param name="reader">data reader</param>
        /// <param name="columnName">column name</param>
        /// <returns>Column value</returns>
        public static string ReadString(this IDataReader reader, string columnName)
		{
			return reader.ReadString(reader.GetOrdinal(columnName));
		}

        /// <summary>
        /// Read 16 bits integer value by column name
        /// </summary>
        /// <param name="reader">data reader</param>
        /// <param name="columnName">column name</param>
        /// <returns>Column value</returns>
		public static short ReadInt16(this IDataReader reader, string columnName)
		{
			return reader.ReadInt16(reader.GetOrdinal(columnName));
		}

        /// <summary>
        /// Read 32 bits integer value by column name
        /// </summary>
        /// <param name="reader">data reader</param>
        /// <param name="columnName">column name</param>
        /// <returns>Column value</returns>
		public static int ReadInt32(this IDataReader reader, string columnName)
		{
			return reader.ReadInt32(reader.GetOrdinal(columnName));
		}

        /// <summary>
        /// Read 64 bits integer value by column name
        /// </summary>
        /// <param name="reader">data reader</param>
        /// <param name="columnName">column name</param>
        /// <returns>Column value</returns>
		public static long ReadInt64(this IDataReader reader, string columnName)
		{
			return reader.ReadInt64(reader.GetOrdinal(columnName));
		}

        /// <summary>
        /// Read decimal value by column name
        /// </summary>
        /// <param name="reader">data reader</param>
        /// <param name="columnName">column name</param>
        /// <returns>Column value</returns>
		public static decimal ReadDecimal(this IDataReader reader, string columnName)
		{
			return reader.ReadDecimal(reader.GetOrdinal(columnName));
		}

        /// <summary>
        /// Read float value by column name
        /// </summary>
        /// <param name="reader">data reader</param>
        /// <param name="columnName">column name</param>
        /// <returns>Column value</returns>
		public static float ReadFloat(this IDataReader reader, string columnName)
		{
			return reader.ReadFloat(reader.GetOrdinal(columnName));
		}

        /// <summary>
        /// Read double type value by column name
        /// </summary>
        /// <param name="reader">data reader</param>
        /// <param name="columnName">column name</param>
        /// <returns>Column value</returns>
		public static double ReadDouble(this IDataReader reader, string columnName)
		{
			return reader.ReadDouble(reader.GetOrdinal(columnName));
		}

        /// <summary>
        /// Read date and time value by column name
        /// </summary>
        /// <param name="reader">data reader</param>
        /// <param name="columnName">column name</param>
        /// <returns>Column value</returns>
		public static DateTime ReadDateTime(this IDataReader reader, string columnName)
		{
			return reader.ReadDateTime(reader.GetOrdinal(columnName));
		}

        /// <summary>
        /// Read all binary data by column name
        /// </summary>
        /// <param name="reader">data reader</param>
        /// <param name="columnName">column name</param>
        /// <returns>Column value</returns>
		public static byte[] ReadBytes(this IDataReader reader, string columnName)
		{
			return reader.ReadBytes(reader.GetOrdinal(columnName));
		}


        /// <summary>
        /// Read Guid value by column name
        /// </summary>
        /// <param name="reader">data reader</param>
        /// <param name="columnName">column name</param>
        /// <returns>Column value</returns>
        public static Guid ReadGuid(this IDataReader reader, string columnName)
        {
            var value = reader.GetValue(reader.GetOrdinal(columnName));
            if(value is DBNull)
            {
                return Guid.Empty;
            }
            if(value is Guid)
            {
                return (Guid)value;
            }else
            {
                return new Guid(value.ToString());
            }
        }
        #endregion

        #region ReadBoolean

        /// <summary>
        /// Read a boolean value by column index
        /// </summary>
        /// <param name="reader">data reader</param>
        /// <param name="index">index of column</param>
        /// <returns>return boolean value. 
        /// The boolean true could be:
        ///    1, -1, Y, T
        /// </returns>
        public static bool ReadBoolean(this IDataReader reader, int index)
		{
			if (reader.IsDBNull(index))
			{
				return false;
			}
            return DbClient.ToBoolean(reader[index]);
		}

		/// <summary>
		/// Reads the boolean by columnc name.
		/// </summary>
		/// <param name="reader">Reader.</param>
		/// <param name="columnName">Column name.</param>
		/// <returns>return boolean value. 
		/// The boolean true could be:
		///    1, -1, Y, T
		/// </returns>
		public static bool ReadBoolean(this IDataReader reader, string columnName)
		{
			return reader.ReadBoolean(reader.GetOrdinal(columnName));
		}
		#endregion

        /// <summary>
        /// Cast any type converable date and time value to DateTime type value.
        ///     null ==> default(DateTime)
        ///     timespan ==> default(DateTime) + timespan
        ///     datetimeoffset ==> utc time
        ///     string datetime ==> date and time
        ///     integer ==> year.
        /// </summary>
        /// <param name="value">date and type object value</param>
        /// <returns>DateTime type value or throw exception</returns>
        /// 
        public static DateTime ToDateTime(this object value)
        {
            return _toDateTime(value, value.GetType());
        }

        private static DateTime _toDateTime(object value, Type type=null)
        {
            if (value == null)
            {
                return default(DateTime);
            }

            DateTime actualValue;

            if (type == null)
            {
                type = value.GetType();
            }

            if (type == typeof(DateTime))
            {
                actualValue = (DateTime) value;
            }
            else if (type == typeof(TimeSpan))
            {
                actualValue = default(DateTime);
                var time = (TimeSpan)value;
                actualValue += time;
            }else if (type == typeof(DateTimeOffset))
            {
                actualValue = ((DateTimeOffset)value).UtcDateTime;
            }

            if (!DateTime.TryParse(value.ToString(), out actualValue))
            {
                throw new InvalidCastException(string.Format("Cannot cast value '{0}' to DateTime."._G(value.ToString())));
            }
            return actualValue;
        }
    }
}
