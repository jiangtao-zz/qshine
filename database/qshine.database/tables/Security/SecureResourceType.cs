using System;
namespace qshine.database.idm
{
	/// <summary>
	/// Securable resource object type.
	/// A securable resource object is a type of entity which requires access permission control. Only permitted user can access the securable resource.
	/// The common securable resource object type could be:
	/// 1. business module
	/// 2. Module application
	/// 3. Application function, task or operation
	/// 4. Organization, department ... 
	/// </summary>
	public class SecureResourceType : SqlDDLTable
	{
		public SecureResourceType()
			: base("sec_resource_type", "SECURITY", "Secure resource object type table.", "secData", "secIndex")
		{
			AddPKColumn("id", System.Data.DbType.Int64)
				.AddColumn("type_name", System.Data.DbType.String, 150, allowNull: false, comments: "resource type name. it is uniquely identifing a secuable resource.")
				.AddColumn("description", System.Data.DbType.String, 500, comments: "resource type description.")
				.AddColumn("category", System.Data.DbType.String, 150, comments: "resource category. It could be used to categorize a group of resource types.")
				.AddAuditColumn();
		}
	}
}
