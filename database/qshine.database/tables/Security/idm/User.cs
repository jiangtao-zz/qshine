using qshine.database.common;
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
				.AddColumn("enterprise_id", System.Data.DbType.Int32, 0, allowNull: false, defaultValue:0, comments: "Specifies an enterprise account id.")
				.AddColumn("login_name", System.Data.DbType.String, 150, allowNull: false, comments: "user login name", isUnique: true, isIndex: true)
				.AddColumn("person_id", System.Data.DbType.Int64, 150, allowNull: false, comments: "user detail person information", isIndex: true, reference:new Person().PkColumn)
				.AddColumn("user_type", System.Data.DbType.String, 50, allowNull: false, comments: "User type and category for business purpose.(Not for security)")
                .AddColumn("auth_provider", System.Data.DbType.String, 250, allowNull: true, comments: "The name of user authentication provider.")
                .AddColumn("inactive_date", System.Data.DbType.Date, 1, allowNull: true, comments: "Record inactive date. It also indicates the user account inactive if this field is not null.")
                .AddAuditColumn();
		}
	}
}
