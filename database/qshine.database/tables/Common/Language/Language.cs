using System;
namespace qshine.database.common.language
{
	/// <summary>
	/// Supportted language table.
	/// Contains information for multi-lingual support.
	/// </summary>
	public class Language : SqlDDLTable
	{
		public Language()
					: base("ln_language", "LANGUAGE", "Defines supportted languages.", "comData", "comIndex")
		{
            AddPKColumn("id", System.Data.DbType.Int64)
				.AddColumn("language", System.Data.DbType.String, 10, allowNull:false,comments:"Language name in English.")
				.AddColumn("iso_code", System.Data.DbType.String, 5, allowNull: false, comments: "language ISO 639-1 code (2 letters)")
				.AddColumn("culture_code", System.Data.DbType.String, 10, allowNull: false, comments: "RFC 4646 Culture code");
		}
	}
}
