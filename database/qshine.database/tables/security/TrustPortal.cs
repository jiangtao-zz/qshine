using qshine.database.tables.organization;

namespace qshine.database.tables.Security
{
    /// <summary>
    /// Trust portal configuration table::
    /// Trust portal build a trust relationship with external system.
    /// </summary>
    public class TrustPortal : SqlDDLTable
    {
        /// <summary>
        /// Ctor::
        /// https://dzone.com/refcardz/rest-api-security-1?chapter=1
        /// </summary>
        public TrustPortal()
            : base("sec_trust_portal", "Security", "Trust Portal Table.", "secData", "secIndex")
        {
            AddPKColumn("id", System.Data.DbType.Int64)

            //Specifies an organization
            .AddColumn("org_id", System.Data.DbType.Int64, 0, 
            reference: new OrganizationUnit().PkColumn, comments: "Organization id.")

            //Name
            .AddColumn("name", System.Data.DbType.String, 250, isUnique:true,
            comments: "Trust portal name.")

            //Description
            .AddColumn("description", System.Data.DbType.String, 250,
            comments: "Trust portal description.")

            //Portal URI. 
            .AddColumn("portal_uri", System.Data.DbType.String, 250, isUnique: true,
            comments: "Trust portal URI where user connected to.")

            //Client URI.
            //For inbound request, it could be a referral URL from external request.
            //For outbound request, it is the external system URI.
            .AddColumn("client_uri", System.Data.DbType.String, 250,
            comments: "Trusted external system URI.")

            //Trust portal type.
            .AddColumn("trust_type", System.Data.DbType.String, 50,
            comments: "type of trust portal.")

            .AddColumn("certificate", System.Data.DbType.String, 100,
            comments: "portal certificate.")

            //Communication direction:
            //inbound: Request always from external system to access your APIs, services and applications.
            // Auth type could be SSO, OAUTH2, SAML, BASIC, CERTIFICATE, JWT
            //outbound: Request send to external system to access APIs, service and applications. 
            // Auth type could be SSO, OAUTH2, SAML, BASIC, CERTIFICATE, JWT
            .AddColumn("direction", System.Data.DbType.Int16, 0, defaultValue: 1,
            comments: "communication direction. 0: not defined. 1: inbound direction. 2: outbound direction. 3. bi-direction.")

            //Authentication method.
            //SSO, OAUTH2, SAML, BASIC, CERTIFICATE, JWT.
            //For SSO, authentication provider name must be present
            .AddColumn("auth_method", System.Data.DbType.String, 50, 
            comments: "Authentication method.")

            //Security authentication provider
            .AddColumn("auth_provider", System.Data.DbType.String, 250,
            comments: "The name of authentication provider.")

            //Service account.
            //If the service account presents, all inbound request security will be restricted within this service account permission.
            //If the value is null then the request security control is set by individual user. 
            .AddColumn("service_account", System.Data.DbType.String, 250,
            comments: "A service account name to delegate inbound request user.")

            //Role control
            //External system may claim user roles to access the system resource.
            //Only the listed roles will be accepted by the system.
            //If the value is null, it accepts any non-system role defined in the system.
            .AddColumn("role_control", System.Data.DbType.String, 1000,
            comments: "A list of service roles valid for external system to request.")

            ;
        }
    }
}
