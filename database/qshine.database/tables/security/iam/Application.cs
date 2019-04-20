using System;
namespace qshine.database.security.iam
{
	/// <summary>
	/// Business application table.
	/// A business application is a functional area group by business module. Such as,
    /// General Ledger, Budgeting, Payroll.
	/// </summary>
	public class Application : SqlDDLTable
	{
        /// <summary>
        /// Ctor::
        /// </summary>
        public Application()
			: base("im_application", "Security", "business application.", "secData", "secIndex")
		{
			AddPKColumn("id", System.Data.DbType.Int64)
                //business module the application belong
                .AddColumn("module_id", System.Data.DbType.Int64, 0, allowNull: false, reference: (new Module()).PkColumn,
                comments: "business module id")
                
                //business application unique code
				.AddColumn("code", System.Data.DbType.String, 50, allowNull: false, 
                comments: "application unique code")

                //business application name
				.AddColumn("name", System.Data.DbType.String, 150, allowNull: false, 
                comments: "application name")

				.AddColumn("description", System.Data.DbType.String, 250, 
                comments: "application description")

				.AddColumn("version", System.Data.DbType.String, 20, 
                comments: "application version number. major.minor[build[revision]]")

			.AddAuditColumn();

            DataVersion = 1; //set system data version

            SetData(1000, 1000, "AUTHENTICATION", "Authentication", "User authentication", 1);
            SetData(1000, 1001, "USER", "Account Management", "User account management", 1);
            SetData(1000, 1002, "GROUP", "Group Management", "Group management", 1);
            SetData(1000, 1003, "ROLE", "Role Management", "Role management", 1);

        }
    }
}
