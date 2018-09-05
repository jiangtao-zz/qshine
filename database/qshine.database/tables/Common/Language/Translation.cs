using System;
namespace qshine.database.common.language
{
	/// <summary>
	/// Language translation table.
	/// Contains translated native language info.
	/// </summary>
	public class Translation : SqlDDLTable
	{
		public Translation()
			: base("ln_translation", "LANGUAGE", "Contains translated native language info.", "comData", "comIndex")
		{
			AddPKColumn("id", System.Data.DbType.Int64)
				.AddColumn("language_id", System.Data.DbType.Int32, 0, allowNull: false, comments: "Supported language id.")
				.AddColumn("source_id", System.Data.DbType.Int64, 0, allowNull: false, comments: "Refer to source table record which contains a text to be translation. The id refer to [TableName.id]")
				.AddColumn("scope", System.Data.DbType.String, 50, allowNull: false, comments: "Uniquely identify a source table text field. The scope name is [TableName.FieldName].")
				.AddColumn("text", System.Data.DbType.String, 2000, allowNull: false, comments: "Translated native text.")
				.AddAuditColumn();
			AddIndex("language_id,source_id,scope", "ln_translation_idx1");
		}
	}
}
