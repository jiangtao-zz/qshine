using System;
namespace qshine.database.tables.organization
{
    /// <summary>
    /// Top level organization in enterprise structure hierarchy.
    /// An Enterprise ID is owned for the organization. 
    /// </summary>
    public class Enterprise : SqlDDLTable
    {
        /// <summary>
        /// Ctor::
        /// https://en.wikipedia.org/wiki/Organizational_unit_(computing)
        /// https://azure.microsoft.com/en-us/blog/organizing-subscriptions-and-resource-groups-within-the-enterprise/
        /// 
        /// </summary>
        public Enterprise()
            : base("org_enterprise", "ORG", "Top level enterprise organization table.", "orgData", "orgIndex")
        {
            AddPKColumn("id", System.Data.DbType.Int64)
                
                //Name of enterprise
                .AddColumn("name", System.Data.DbType.String, 50, allowNull: false, 
                comments: "Enterprise name.")
                
                //description
                .AddColumn("description", System.Data.DbType.String, 150, 
                comments: "description")

                .AddAuditColumn();
        }
    }
}
