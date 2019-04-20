using System;
namespace qshine.database.security.iam
{
    /// <summary>
    /// Securable resource object type.
    /// A securable resource object is a type of entity which requires access permission control. 
    /// Only permitted user can access the securable resource.
    /// 
    /// The common securable resource object type could be:
    /// business module, application, api, service, task, operation and action
    /// organization unit, department
    /// server, network  
    /// country, region, city, location ...
    /// 
    /// </summary>
    public class SecureResourceType : SqlDDLTable
	{
        /// <summary>
        /// Ctor::
        /// https://docs.oracle.com/cd/E13222_01/wls/docs92/secwlres/types.html
        /// https://docs.aws.amazon.com/config/latest/developerguide/resource-config-reference.html
        /// https://www.ibm.com/support/knowledgecenter/en/SS4GSP_7.0.2/com.ibm.edt.heat.reference.doc/topics/ref_heat_types_azure_ov.html
        /// </summary>
		public SecureResourceType()
			: base("im_resource_type", "Security", "Secure resource object type table.", "secData", "secIndex")
		{
			AddPKColumn("id", System.Data.DbType.Int64)
                
                //Enterprise id
                .AddColumn("enterprise_id", System.Data.DbType.Int32, 0, allowNull: false, defaultValue: 0,
                comments: "Specifies an enterprise account id.")

                .AddColumn("type_name", System.Data.DbType.String, 150, allowNull: false, 
                comments: "resource type name. it is uniquely identifing a secuable resource.")

				.AddColumn("description", System.Data.DbType.String, 500, 
                comments: "resource type description.")

                //
				.AddColumn("category", System.Data.DbType.String, 150, 
                comments: "resource category. It could be used to categorize a group of resource types.")

                //resource hierarchy level start from 0 (top level).
                .AddColumn("level", System.Data.DbType.Int16, 0, defaultValue:0,
                comments: "resource hierarchy level. It is used to build resource hierarchy. Same hierarchy resources should be in same rsource group (category).")

                //resource hierarchy level start from 0 (top level).
                .AddColumn("parent_id", System.Data.DbType.Int64, 0, 
                comments: "parent resourceid.")
                
                //resource provider is used to find a specific resource item for access control
                .AddColumn("resource_provider", System.Data.DbType.String, 150,
                comments: "resource provider name. It can provide actual resource metadata information.")

                .AddAuditColumn();
		}
	}
}
