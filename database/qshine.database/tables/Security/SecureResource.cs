using System;
namespace qshine.database.idm
{
	/// <summary>
	/// Securable resource object.
	/// </summary>
	public class SecureResource : SqlDDLTable
	{
		public SecureResource()
			: base("sec_resource", "SECURITY", "Securable resource object table.", "secData", "secIndex")
		{
			AddPKColumn("id", System.Data.DbType.Int64)
				.AddColumn("resource_type_id", System.Data.DbType.Int64, 0, allowNull: false, comments: "securable resource type", reference:new SecureResourceType().PkColumn)
				.AddColumn("object_id", System.Data.DbType.Int64, 0, comments: "A securable resource object source id. The source resource object could be null if the securable resource can be store in this table.")
				.AddColumn("name", System.Data.DbType.String, 250, comments: "A securable resource object name.")
				.AddColumn("url", System.Data.DbType.String, 250, comments: "A unique resource object location.")
				.AddAuditColumn();
		}
	}
}
