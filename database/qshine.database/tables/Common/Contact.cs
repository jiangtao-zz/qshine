using qshine.database.tables.organization;

namespace qshine.database.tables.common
{
    /// <summary>
    /// Business contacts information
    /// </summary>
    public class Contact : SqlDDLTable
    {
        public Contact()
            : base("cm_contact", "Common", "Business contacts information table.", "orgData", "orgIndex")
        {
            AddPKColumn("id", System.Data.DbType.Int64)

                //Specifies an organization
                .AddColumn("org_id", System.Data.DbType.Int64, 0, allowNull: false, isIndex: true,
                reference: new OrganizationUnit().PkColumn, comments: "Organization id.")

                .AddColumn("person_id", System.Data.DbType.Int64, 0, allowNull: false, reference: new Person().PkColumn, 
                comments: "Contacts person information.")

                .AddColumn("location_id", System.Data.DbType.Int64, 0, reference: new Location().PkColumn, 
                comments: "Contacts location id.")

                .AddColumn("category", System.Data.DbType.String, 250, 
                comments: "Contracts category.Ex:Home, Business, Account Manager, ..")

                .AddColumn("comments", System.Data.DbType.String, 500, 
                comments: "Comments")

                .AddColumn("effective_date", System.Data.DbType.Date, 0, 
                comments: "effective date.")

                .AddColumn("inactive_date", System.Data.DbType.Date, 0, 
                comments: "inactive date.")

                .AddAuditColumn();
        }
    }
}
