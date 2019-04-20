using System;
namespace qshine.database.security.iam
{
	/// <summary>
	/// Role table.
	/// A role is a collection of users, groups and other roles. It can be hierarchical, that is, a role can include arbitrarily nested roles (other than itself).
	/// A role is created for various job functions. It has a collection of operations or resources.
	/// </summary>
	public class Role : SqlDDLTable
	{
        /// <summary>
        /// Ctor::
        /// https://en.wikipedia.org/wiki/Role-oriented_programming
        /// https://docs.microsoft.com/en-us/azure/role-based-access-control/rbac-and-directory-admin-roles
        /// </summary>
		public Role()
			: base("im_role", "Security", "Role table.", "secData", "secIndex")
		{
			AddPKColumn("id", System.Data.DbType.Int64)

				.AddColumn("enterprise_id", System.Data.DbType.Int32, 0, allowNull: false, defaultValue:0, 
                comments: "Specifies an enterprise account id.")

                //Role unique name
				.AddColumn("role", System.Data.DbType.String, 150, isIndex: true,
                comments: "role name")

                //
				.AddColumn("description", System.Data.DbType.String, 500, 
                comments: "role description")

                //Role type:
				.AddColumn("role_type", System.Data.DbType.String, 50, 
                comments: "Type of the role. built-in,custom")

				.AddAuditColumn();
		}
	}
}
