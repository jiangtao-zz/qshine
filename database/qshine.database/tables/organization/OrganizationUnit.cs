using qshine.database.common;
using System;
namespace qshine.database.organization
{
    /// <summary>
    /// Orgnization unit and it's hierarchy.
    /// It contains each level enterprise organization hierarchy structure.
    /// </summary>
    public class OrganizationUnit : SqlDDLTable
    {
        public OrganizationUnit()
            : base("org_unit", "ORG", "Enterprise organization unit table.", "orgData", "orgIndex")
        {
            AddPKColumn("id", System.Data.DbType.Int64)
                //Enterprise Id
                .AddColumn("enterprise_id", System.Data.DbType.Int64, 0, allowNull: false, reference: new Enterprise().PkColumn, 
                comments: "Enterprise organization id.")

                //Organization name
                .AddColumn("name", System.Data.DbType.String, 50, allowNull: false, 
                comments: "Organization unit name.")

                //Organization description
                .AddColumn("description", System.Data.DbType.String, 150, 
                comments: "description")

                //Common name of organization unit
                .AddColumn("ou_type", System.Data.DbType.String, 50, 
                comments: "orgnization unit type: Group/Team, Department, Branch, Section,Operation, Division, Company,...")

                //hierarchy level, start from 1.
                .AddColumn("ou_level", System.Data.DbType.Int16, 0,
                comments: "Organization unit hierarchy level.")

                //parent unit in organization hierarchy structure
                .AddColumn("parent_ou_id", System.Data.DbType.Int64, 0, 
                comments: "parent organization unit.")

                //location (optional)
                .AddColumn("location_id", System.Data.DbType.Int64,0, reference: new Location().PkColumn, 
                comments:"OU geographical location.")

                //effective date (optional).
                .AddColumn("effective_date", System.Data.DbType.Date, 0, comments: "effective date.")

                //inactivate date (optional)
                .AddColumn("inactive_date", System.Data.DbType.Date,0,comments:"inactive date.")

                //organization unit detail meta data provider. 
                .AddColumn("ou_provider", System.Data.DbType.Date, 0, comments: "detail organization information (metadata) provider.")

                //organization unit detail meta data id. 
                .AddColumn("ou_object_id", System.Data.DbType.Int64, 0, comments: "meta data id.")

                .AddAuditColumn();
        }
    }
}
