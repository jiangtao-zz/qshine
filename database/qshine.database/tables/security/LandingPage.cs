
using qshine.database.tables.organization;
using qshine.database.tables.security.iam;

namespace qshine.database.tables.Security
{
    /// <summary>
    /// A landing page is a specialized web page that someone lands on after sign up.
    /// The landing page are designed for a focused objective (marketing objective, mobile and desktop device, organization, group and user) for a group of users.
    /// Portal vs Landing page:
    ///     An inbound portal page is very similar as a landing page. The inbound portal page is usually designed for external system connection.
    /// </summary>
    public class LandingPage : SqlDDLTable
    {
        /// <summary>
        /// Ctor::
        /// https://workshops.unbounce.com/lesson/1-what-is-a-landing-page/
        /// </summary>
        public LandingPage()
            : base("sec_landing_page", "Security", "Landing Page Table.", "secData", "secIndex")
        {
            AddPKColumn("id", System.Data.DbType.Int64)
                
                //apply to organization
                .AddColumn("org_id", System.Data.DbType.Int64, 0, 
                reference: new OrganizationUnit().PkColumn, comments: "Organization id.")

                //apply to specific group
                .AddColumn("group_id", System.Data.DbType.Int64, 0, reference: new Group().PkColumn,
                comments: "Group id. It could be null which indicates the page applies to all group accounts")

                //apply to specific type of devices
                .AddColumn("user_device", System.Data.DbType.String, 200,
                comments: "Specifies user device type. The common values are: Mobile, Desktop or UA:<user agent expression>")

                //landing page name
                .AddColumn("name", System.Data.DbType.String, 256,
                comments: "landing page name.")

                //landing page URL
                .AddColumn("url", System.Data.DbType.String, 256,
                comments: "Specifies landing page url.")

                //sequence of the landing page. Always choose lowest landing page if multiple landing pages available for the user.
                .AddColumn("sequence", System.Data.DbType.Int16, 0,
                comments: "Sequence of landing page. Always choose lowest sequence landing page if multiple landing pages available for the user.")

                .AddAuditColumn()
            ;
        }
    }
}


