using qshine.database.common;
using qshine.database.common.language;
using qshine.database.organization;
using qshine.database.security.iam;
using qshine.database.tables.common.email;
using qshine.database.tables.Security;


namespace qshine.database
{
    public class MyDatabase:SqlDDLDatabase
    {
        public MyDatabase(Database database)
            :base(database)
        {
            #region Organization/Multi-tenant
            //Organization
            AddTable(new Enterprise());
            AddTable(new OrganizationUnit());
            #endregion

            //Register Common tables
            AddTable(new Location());
            AddTable(new BuildingHour());
            AddTable(new Person());
            AddTable(new LookupType());
            AddTable(new Lookup());
            AddTable(new Language());
            AddTable(new Translation());

            #region Email Messages
            AddTable(new EmailOutQueue());
            AddTable(new EmailSentLog());
            AddTable(new EmailDeadletter());
            AddTable(new EmailTemplate());
            #endregion

            #region Security Tables
            //Identity and Access Management (IAM)
            AddTable(new User());
            AddTable(new UserPreference());
            AddTable(new Group());
            AddTable(new GroupMember());
            AddTable(new Role());
            AddTable(new RoleMember());
            AddTable(new Permission());

            //IAM.Actions
            AddTable(new Module());
            AddTable(new Application());
            AddTable(new ApplicationSetting());
            AddTable(new Operation());

            //IAM.Data Resources
            AddTable(new SecureResourceType());
            AddTable(new SecureResource());
            AddTable(new SecureResourceGroup());
            AddTable(new SecureResourceGroupMember());

            //Security Authentication
            AddTable(new AccountPolicy());
            AddTable(new AuditPolicy());
            AddTable(new Session());
            AddTable(new SessionLog());
            AddTable(new SessionAuditLog());
            AddTable(new TrustPortal());
            AddTable(new LandingPage());
            #endregion
        }
    }
}
