using System;
namespace qshine.database.idm
{
	/// <summary>
	/// Resource operation defines each securable operation apply to one type of securable resource.
	/// The resource operation could be "Access", "Create entity", "Update entity", "Read entity"
	/// </summary>
	public class ResourceOperation : SqlDDLTable
	{
		public ResourceOperation()
			: base("sec_resource_op", "SECURITY", "Securable resource operation table.", "secData", "secIndex")
		{
			AddPKColumn("id", System.Data.DbType.Int64)
				.AddColumn("name", System.Data.DbType.String, 150, allowNull: false, comments: "resource operation name")
				.AddColumn("description", System.Data.DbType.String, 500, comments: "resource operation description")
				.AddColumn("resource_type_id", System.Data.DbType.Int64, 0, allowNull: false, comments: "Resource type", reference:"sec_resource_type:id")
				.AddColumn("secure_mask", System.Data.DbType.Int64, 0, allowNull: false, comments: "list a possible base operation using binary value. One bit indicates a single base operation.")
				.AddAuditColumn();
		}
	}
}
