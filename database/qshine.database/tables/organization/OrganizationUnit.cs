using qshine.database.common;
using System;
namespace qshine.database.idm
{
    /// <summary>
    /// High level enterprise organization for your clients
    /// </summary>
    public class OrganizationUnit : SqlDDLTable
    {
        public OrganizationUnit()
            : base("org_unit", "ORG", "Enterprise organization unit table.", "orgData", "orgIndex")
        {
            AddPKColumn("id", System.Data.DbType.Int64)
                .AddColumn("enterprise_id", System.Data.DbType.Int64, 0, allowNull: false, reference: new Enterprise().PkColumn, comments: "Enterprise organization id.")
                .AddColumn("name", System.Data.DbType.String, 50, allowNull: false, comments: "Organization unit name.")
                .AddColumn("description", System.Data.DbType.String, 150, comments: "description")
                .AddColumn("ou_type", System.Data.DbType.String, 50, comments: "orgnization unit type: Group/Team, Department, Branch, Section,Operation, Division, Company,...")
                .AddColumn("location_id", System.Data.DbType.Int64,0, reference: new Location().PkColumn, comments:"OU geographical location.")
                .AddColumn("effective_date", System.Data.DbType.Date, 0, comments: "effective date.")
                .AddColumn("inactive_date", System.Data.DbType.Date,0,comments:"inactive date.")
                .AddColumn("parent_ou_id", System.Data.DbType.Int64,0, comments:"parent organization unit.")
                .AddAuditColumn();
        }
    }
}
