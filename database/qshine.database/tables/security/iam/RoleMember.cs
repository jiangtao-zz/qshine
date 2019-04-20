using System;
namespace qshine.database.security.iam
{
	/// <summary>
	/// Role member table.
	/// The role member is a principal which could be a user, a group or a role.
	/// Note: The role member is also used to build a role hierarchy. Through a role hierarchy to find all users associaed to this role.
	/// </summary>
	public class RoleMember : SqlDDLTable
	{
        /// <summary>
        /// Ctor::
        /// 
        /// </summary>
		public RoleMember()
			: base("im_role_member", "Security", "Role member table. Role member could be a user, a group or an role", "secData", "secIndex")
		{
			AddPKColumn("id", System.Data.DbType.Int64)

				.AddColumn("enterprise_id", System.Data.DbType.Int32, 0, allowNull: false, defaultValue:0, 
                comments: "Specifies an enterprise account id.")

                //Refer to an role
				.AddColumn("role_id", System.Data.DbType.Int64, 0, allowNull: false, isIndex: true, reference:new Role().PkColumn,
                comments: "role id")

                //refer to a valid role member
				.AddColumn("member_id", System.Data.DbType.Int64, 0, allowNull: false, isIndex: true
                , comments: "principal id")

                //Role member type
                .AddColumn("member_type", System.Data.DbType.Int64, 0, allowNull: false, defaultValue:1,
                comments: "Role member type. 0: user, 1: group, 2:role,")

                .AddAuditColumn();
		}
	}
}
