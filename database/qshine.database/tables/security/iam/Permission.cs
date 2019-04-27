using qshine.database.tables.organization;
using System;
namespace qshine.database.tables.security.iam
{
    /// <summary>
    /// Role permission definition table.
    /// User permission is a collection of permission definitions of the role.
    /// Role permission could be a grant permission or deny permission.
    /// The permission evaluation process will be:
    ///     1. Select all granted actions
    ///     2. Exclude all denied actions
    ///     3. Select all granted resources
    ///     4. Exclude all denied resources
    ///     5. Apply permitted resources to the actions
    /// </summary>
    public class Permission : SqlDDLTable
    {
        /// <summary>
        /// Ctor::
        /// https://docs.microsoft.com/en-us/azure/role-based-access-control/overview
        /// https://en.wikipedia.org/wiki/Role-based_access_control
        /// </summary>
		public Permission()
            : base("im_permission", "Security", "Role permission table.", "secData", "secIndex")
        {
            AddPKColumn("id", System.Data.DbType.Int64)

                //Specifies an organization
                .AddColumn("org_id", System.Data.DbType.Int64, 0, allowNull: false, isIndex: true,
                reference: new OrganizationUnit().PkColumn, comments: "Organization id.")


                //Role unique name
                .AddColumn("role_id", System.Data.DbType.Int64, 0, isIndex: true,
                comments: "role id")

                //action or secure resource object path.
                //resource path is a match pattern. Such as "*", "Security/*/Read", "Form/TimeSheet/Approve"
                .AddColumn("resource_path", System.Data.DbType.String, 500,
                comments: "resource path is a match pattern to find permission related secure resources and actions")

                //resource type
                .AddColumn("resource_type", System.Data.DbType.Int16, 0,
                comments: "Indicates action or secure resource. 0: action, 1: secure resource (data resource)")

                //permission grant or deny.
                .AddColumn("deny_flag", System.Data.DbType.Boolean, 0, defaultValue: false,
                comments: "Indicates a deny permission.")

                .AddAuditColumn();
        }
    }
}
