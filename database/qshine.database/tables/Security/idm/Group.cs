using System;
namespace qshine.database.idm
{
	/// <summary>
	/// Group table.
	/// </summary>
	public class Group : SqlDDLTable
	{
		public Group()
			: base("im_group", "IDM", "User group table.", "idmData", "idmIndex")
		{
			AddPKColumn("id", System.Data.DbType.Int64)
				.AddColumn("enterprise_id", System.Data.DbType.Int32, 0, false, defaultValue:0, comments: "Specifies an enterprise account id.")
				.AddColumn("group_name", System.Data.DbType.String, 150, false, comments: "group name", isUnique: true, isIndex: true)
				.AddColumn("description", System.Data.DbType.String, 500, true, comments: "group detail description")
				.AddAuditColumn();
		}
	}
}
