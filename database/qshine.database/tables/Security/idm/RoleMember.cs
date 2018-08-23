using System;
namespace qshine.database.idm
{
	/// <summary>
	/// Role member table.
	/// The role member is a principal which could be a user, a group or a role.
	/// Note: The role member is also used to build a role hierarchy. Through a role hierarchy to find all users associaed to this role.
	/// </summary>
	public class RoleMember : SqlDDLTable
	{
		public RoleMember()
			: base("im_role_member", "IDM", "Role member table. Role member could be a user, a group or an role", "idmData", "idmIndex")
		{
			AddPKColumn("id", System.Data.DbType.Int64)
				.AddColumn("enterprise_id", System.Data.DbType.Int32, 0, false, defaultValue:0, comments: "Specifies an enterprise account id.")
				.AddColumn("role_id", System.Data.DbType.Int64, 0, false, comments: "role id", isIndex: true, reference:"im_role:id")
				.AddColumn("principal_id", System.Data.DbType.Int64, 0, false, comments: "principal id", isIndex: true, reference:"im_principal:id")
				.AddAuditColumn();
		}
	}
}
