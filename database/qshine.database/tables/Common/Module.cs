using System;
namespace qshine.database.common
{
	/// <summary>
	/// Module table contains all business area in the system.
	/// Module is a business area which contains many applications for business use.
	/// </summary>
	public class Module : SqlDDLTable
	{
		public Module()
			: base("cm_module", "COMMON", "Defines application business area.", "comData", "comIndex")
		{
			AddPKColumn("id", System.Data.DbType.Int64)
				.AddColumn("code", System.Data.DbType.String, 50, allowNull: false, comments: "Module unique code")
				.AddColumn("name", System.Data.DbType.String, 150, allowNull: false, comments: "Module name")
				.AddColumn("description", System.Data.DbType.String, 250, comments: "Module description")
				.AddColumn("version", System.Data.DbType.String, 20, comments: "Module version number")
			.AddAuditColumn();
		}
	}
}
