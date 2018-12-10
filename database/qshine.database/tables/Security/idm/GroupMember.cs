using System;
namespace qshine.database.idm
{
	/// <summary>
	/// Group member table.
	/// Group member could be a user or a group.
	/// Note: The group user member could be a direct user or users from a group hierarchy. 
	/// </summary>
	public class GroupMember : SqlDDLTable
	{
		public GroupMember()
			: base("im_group_member", "IDM", "Group member table. A group member could be a sub-group or a user.", "idmData", "idmIndex")
		{
			AddPKColumn("id", System.Data.DbType.Int64)
				.AddColumn("enterprise_id", System.Data.DbType.Int32, 0, allowNull:false, defaultValue:0, comments: "Specifies an enterprise account id.")
				.AddColumn("principal_id", System.Data.DbType.Int64, 0, allowNull: false, comments: "principal id. It only can be user id or group id.", isIndex: true,
                reference:new Principal().PkColumn)
				.AddColumn("group_id", System.Data.DbType.Int64, 0, allowNull: false, comments: "group id", isIndex: true, reference:new Group().PkColumn)
				.AddAuditColumn();
		}
	}
}
