using System;
namespace qshine.database.security.iam
{
	/// <summary>
	/// Securable resource.
    /// Security resource is a manageable item which need be protected for user access.
    /// It could be a machine, account, application, api, database table, file ,folder or any business valuable object.
	/// </summary>
	public class SecureResource : SqlDDLTable
	{
        /// <summary>
        /// Ctor::
        /// https://docs.microsoft.com/en-us/azure/azure-resource-manager/resource-group-overview
        /// </summary>
		public SecureResource()
			: base("im_resource", "Security", "Securable resource object table.", "secData", "secIndex")
		{
			AddPKColumn("id", System.Data.DbType.Int64)
                //Enterprise id
                .AddColumn("enterprise_id", System.Data.DbType.Int32, 0, allowNull: false, defaultValue: 0,
                comments: "Specifies an enterprise account id.")

                //registered resource type
                .AddColumn("resource_type_id", System.Data.DbType.Int64, 0, allowNull: false, 
                comments: "securable resource type", reference:new SecureResourceType().PkColumn)

                //resource detail metadata
				.AddColumn("object_id", System.Data.DbType.Int64, 0, 
                comments: "A securable resource object source id. The source resource object could be null if the securable resource do not have any extra metadata.")

                //resource name
				.AddColumn("name", System.Data.DbType.String, 250, 
                comments: "A securable resource object name.")

                //resource uri
				.AddColumn("uri", System.Data.DbType.String, 250, 
                comments: "A unique resource location identity.")

				.AddAuditColumn();
		}
	}
}
