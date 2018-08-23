using System;
namespace qshine.database.idm
{
	/// <summary>
	/// Principal table.
	/// A principal could be a user, group or role. The principal is used to build user, group and role relationship and hierarchy 
	/// </summary>
	public class Principal : SqlDDLTable
	{
		public Principal()
			: base("im_principal", "IDM", "Principal table.", "idmData", "idmIndex")
		{
			AddPKColumn("id", System.Data.DbType.Int64)
				.AddColumn("enterprise_id", System.Data.DbType.Int32, 0, false, defaultValue:0, comments: "Specifies an enterprise account id.")
				.AddColumn("principal_type", System.Data.DbType.Int16, 0, true, comments: "principal type:1-user,2-group, 3-role")
				.AddColumn("source_id", System.Data.DbType.Int64, 0, true, comments: "principal source table id. It could be user id, group id or role id.")
				.AddAuditColumn();
		}
	}
}
