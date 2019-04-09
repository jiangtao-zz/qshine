using System;
namespace qshine.database
{
	enum SqlReservedWordValue
	{
		SysDate
	}

    /// <summary>
    /// Sql reserved keyword.
    /// </summary>
	public class SqlReservedWord
	{
		private SqlReservedWordValue _keywordValue;
        /// <summary>
        /// Ctro.
        /// </summary>
		public SqlReservedWord() { }

		SqlReservedWord(SqlReservedWordValue value) { _keywordValue = value;}

        /// <summary>
        /// Get current date
        /// </summary>
		public static SqlReservedWord SysDate
		{
			get
			{
				return new SqlReservedWord(SqlReservedWordValue.SysDate);
			}
		}

        /// <summary>
        /// Is sysdate
        /// </summary>
		public bool IsSysDate
		{
			get
			{
				return _keywordValue == SqlReservedWordValue.SysDate;
			}
		}

        /// <summary>
        /// Get a internal reserved keyword statement.
        /// </summary>
        /// <returns></returns>
		public override string ToString()
		{
			if (_keywordValue == SqlReservedWordValue.SysDate)
			{
				return "SqlReservedWordValue.SysDate";
			}
			return "";
		}
	}

    /// <summary>
    /// 
    /// </summary>
	public class SqlDDL
	{
	}
}
