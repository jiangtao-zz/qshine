using System;
namespace qshine.database
{
	enum SqlReservedWordValue
	{
		SysDate
	}

	public class SqlReservedWord
	{
		private SqlReservedWordValue _keywordValue;
		public SqlReservedWord() { }

		SqlReservedWord(SqlReservedWordValue value) { _keywordValue = value;}

		public static SqlReservedWord SysDate
		{
			get
			{
				return new SqlReservedWord(SqlReservedWordValue.SysDate);
			}
		}

		public bool IsSysDate
		{
			get
			{
				return _keywordValue == SqlReservedWordValue.SysDate;
			}
		}

		public override string ToString()
		{
			if (_keywordValue == SqlReservedWordValue.SysDate)
			{
				return "SqlReservedWordValue.SysDate";
			}
			return "";
		}
	}

	public class SqlDDL
	{
	}
}
