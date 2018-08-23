using System;
namespace qshine.database.idm
{
	/// <summary>
	/// User table.
	/// A user is a person or automated agent who has a user account for application access. 
	/// An authenticated user is a user whose credentials have been validated.
	/// An anonymous user is a user whose credentials have not been validated (hence unauthenticated) that is permitted access to only unprotected resources.
	/// </summary>
	public class User:SqlDDLTable
	{
		public User()
			: base("im_user", "IDM", "User table.", "idmData", "idmIndex")
		{
			AddPKColumn("id", System.Data.DbType.Int64)
				.AddColumn("enterprise_id", System.Data.DbType.Int32, 0, false, defaultValue:0, comments: "Specifies an enterprise account id.")
				.AddColumn("login_name", System.Data.DbType.String, 150, false, comments: "user login name", isUnique: true, isIndex: true)
				.AddColumn("person_id", System.Data.DbType.Int64, 150, false, comments: "user detail person information", isIndex: true, reference:"im_person:id")
				.AddColumn("user_type", System.Data.DbType.String, 50, false, comments: "User type and category.")
				.AddColumn("inactive_flag", System.Data.DbType.Boolean, 1, false, comments: "Indicates record inactive flag.")
				.AddAuditColumn();
		}
	}
}
