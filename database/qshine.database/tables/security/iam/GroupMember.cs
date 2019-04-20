using qshine.database.organization;
namespace qshine.database.security.iam
{
	/// <summary>
	/// Group member table.
	/// Group member could be a user or a group.
	/// Note: The group user member could be a direct user or users from a group hierarchy. 
	/// </summary>
	public class GroupMember : SqlDDLTable
	{
        /// <summary>
        /// Ctor::
        /// </summary>
		public GroupMember()
			: base("im_group_member", "Security", "Group member table. A group member could be a sub-group or a user.", "secData", "secIndex")
		{
			AddPKColumn("id", System.Data.DbType.Int64)
                //Specifies an organization
                .AddColumn("enterprise_id", System.Data.DbType.Int64, 0, allowNull: false,
                isIndex: true, reference: new Enterprise().PkColumn,
                comments: "Enterprise organization id. For a global service group, the value is 0.")

                //user id or sub-group id
                .AddColumn("member_id", System.Data.DbType.Int64, 0, allowNull: false, isIndex: true,
                comments: "It can be user id or group id.")

                //member type
                .AddColumn("member_type", System.Data.DbType.Int64, 0, allowNull: false, defaultValue:0,
                comments: "Member type. 0: user, 1: sub-group")

                //refer to a group.
                .AddColumn("group_id", System.Data.DbType.Int64, 0, allowNull: false, 
                comments: "group id", isIndex: true, reference:new Group().PkColumn)

				.AddAuditColumn();
		}
	}
}
