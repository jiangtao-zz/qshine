using qshine.database.tables.organization;
using System;
namespace qshine.database.tables.security.iam
{
    /// <summary>
    /// Securable resource group is a container of related secure resources.
    /// It can contain resource or another resource group. 
    /// Using nested resource group to build resource hierarchy.
    /// Example:
    ///     [Region Group]
    ///        |--> [Country Group]
    ///                  |--> [Province Group]
    ///                           |-->[City Group]
    ///                                   |-->[Office Resource]
    /// 
    /// </summary>
    public class SecureResourceGroup : SqlDDLTable
    {
        /// <summary>
        /// Ctor::
        /// </summary>
		public SecureResourceGroup()
            : base("im_resource_group", "Security", "Secure resource group table.", "secData", "secIndex")
        {
            AddPKColumn("id", System.Data.DbType.Int64)

                //Specifies an organization
                .AddColumn("org_id", System.Data.DbType.Int64, 0, allowNull: false, isIndex: true,
                reference: new OrganizationUnit().PkColumn, comments: "Organization id.")

                .AddColumn("name", System.Data.DbType.String, 150, allowNull: false,
                comments: "Resource group name.")

                .AddColumn("description", System.Data.DbType.String, 500,
                comments: "resource group description.")

                .AddAuditColumn();
        }
    }
}
