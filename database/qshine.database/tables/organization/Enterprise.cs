using System;
namespace qshine.database.idm
{
    /// <summary>
    /// High level enterprise organization for your clients
    /// </summary>
    public class Enterprise : SqlDDLTable
    {
        public Enterprise()
            : base("org_enterprise", "ORG", "Top level enterprise organization table.", "orgData", "orgIndex")
        {
            AddPKColumn("id", System.Data.DbType.Int64)
                .AddColumn("name", System.Data.DbType.String, 50, allowNull: false, comments: "Enterprise name.")
                .AddColumn("description", System.Data.DbType.String, 150, comments: "description")
                .AddAuditColumn();
        }
    }
}
