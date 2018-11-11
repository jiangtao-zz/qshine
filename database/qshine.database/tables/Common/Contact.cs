using System;
namespace qshine.database.idm
{
    /// <summary>
    /// Business contacts information
    /// </summary>
    public class Contact : SqlDDLTable
    {
        public Contact()
            : base("cm_contact", "COMMON", "Business contacts information table.", "orgData", "orgIndex")
        {
            AddPKColumn("id", System.Data.DbType.Int64)
                .AddColumn("enterprise_id", System.Data.DbType.Int64, 0, allowNull: false, reference: "org_enterprise:id", comments: "Enterprise organization id.")
                .AddColumn("person_id", System.Data.DbType.Int64, 0, allowNull: false, reference:"cm_person:id", comments: "Contacts person information.")
                .AddColumn("location_id", System.Data.DbType.Int64, 0, reference: "cm_location:id", comments: "Contacts location id.")
                .AddColumn("category", System.Data.DbType.String, 250, comments: "Contracts category.Ex:Home, Business, Account Manager, ..")
                .AddColumn("comments", System.Data.DbType.String, 500, comments: "Comments")
                .AddColumn("effective_date", System.Data.DbType.Date, 0, comments: "effective date.")
                .AddColumn("inactive_date", System.Data.DbType.Date, 0, comments: "inactive date.")
                .AddAuditColumn();
        }
    }
}
