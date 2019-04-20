using qshine.database.common;
using System;
namespace qshine.database.security.iam
{
	/// <summary>
	/// User table.
	/// A user is a person or automated agent who has a user account (service account) for application access. 
	/// An authenticated user is a user whose credentials have been validated.
	/// An anonymous user is a user whose credentials have not been validated (hence unauthenticated) that is permitted access to only unprotected resources.
	/// </summary>
	public class User:SqlDDLTable
	{
		public User()
			: base("im_user", "Security", "User table.", "secData", "secIndex")
		{
			AddPKColumn("id", System.Data.DbType.Int64)

				.AddColumn("enterprise_id", System.Data.DbType.Int32, 0, allowNull: false, defaultValue:0, 
                comments: "Specifies an enterprise account id.")

                //login name. it could be an email or phone number
                //A unique key is Enterprise id + login name.
				.AddColumn("login_name", System.Data.DbType.String, 150, allowNull: false,  isIndex: true,
                    comments: "user login name.")
                
                //Refer to person table
				.AddColumn("person_id", System.Data.DbType.Int64, 150, allowNull: false, isIndex: true, reference: new Person().PkColumn,
                comments: "user detail person information")

                //A type of user,
				.AddColumn("user_type", System.Data.DbType.String, 50, allowNull: false, 
                comments: "User type and category for business purpose.(Not for security)")

                //A special authentication provider map apply to the user.
                //In general, user authentication provider is set by account policy.
                .AddColumn("auth_provider", System.Data.DbType.String, 250, 
                comments: "The name of user authentication provider.")

                //The date when user get inactivated
                .AddColumn("inactive_date", System.Data.DbType.Date, 1, allowNull: true, 
                comments: "Record inactive date. It also indicates the user account inactive if this field is not null.")

                //Account locked
                .AddColumn("locked_date", System.Data.DbType.Date, 1, allowNull: true,
                comments: "The date user account is locked. This field may not be applicable if use out of box IAM")

                .AddAuditColumn();

            DataVersion = 1; //set system data version

            SetData(100, -1, "admin", -1, "SYSTEM", null, null);
            SetData(101, -1, "service", -1, "SERVICE", null, null);
        }
	}
}
