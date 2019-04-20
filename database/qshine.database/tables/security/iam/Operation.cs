using System;
namespace qshine.database.security.iam
{
    /// <summary>
    /// Critical function of operation/action table.
    /// An operation or action control user access in the lowest function level. Such as,
    /// "approval", "assign task".
    /// </summary>
    public class Operation : SqlDDLTable
    {
        /// <summary>
        /// Ctor::
        /// https://docs.microsoft.com/en-us/azure/role-based-access-control/overview
        /// </summary>
        public Operation()
            : base("im_operation", "Security", "Application operation/action.", "secData", "secIndex")
        {
            AddPKColumn("id", System.Data.DbType.Int64)
                //business module 
                .AddColumn("module_id", System.Data.DbType.Int64, 0, reference: (new Module()).PkColumn,
                comments: "business module")

                //business application
                .AddColumn("application_id", System.Data.DbType.Int64, 0, reference: (new Application()).PkColumn,
                comments: "business application")

                //business application operation/action
                //apps/security/permission/assignment
                .AddColumn("operation", System.Data.DbType.String, 250, allowNull: false,
                comments: "application operation/action uri")

                //action/operation description.
                .AddColumn("description", System.Data.DbType.String, 250,
                comments: "detail description")

            .AddAuditColumn();

        }
    }
}
