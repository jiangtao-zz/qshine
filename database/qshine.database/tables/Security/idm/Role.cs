using System;
namespace qshine.database.idm
{
	/// <summary>
	/// Role table.
	/// A role is a collection of users, groups and other roles. It can be hierarchical, that is, a role can include arbitrarily nested roles (other than itself).
	/// A role is created for various job functions. It has a collection of operations or resources.
	/// </summary>
	public class Role : SqlDDLTable
	{
		public Role()
			: base("im_role", "IDM", "Role table.", "idmData", "idmIndex")
		{
			AddPKColumn("id", System.Data.DbType.Int64)
				.AddColumn("enterprise_id", System.Data.DbType.Int32, 0, allowNull: false, defaultValue:0, comments: "Specifies an enterprise account id.")
				.AddColumn("role", System.Data.DbType.String, 150, comments: "role name", isIndex: true)
				.AddColumn("description", System.Data.DbType.String, 500, comments: "role description")
				.AddColumn("role_type", System.Data.DbType.String, 50, allowNull: false, comments: "Type of the role.")
				.AddAuditColumn();
		}
	}
}
